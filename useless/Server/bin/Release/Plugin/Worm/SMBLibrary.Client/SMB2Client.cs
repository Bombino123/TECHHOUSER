using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using SMBLibrary.Client.Authentication;
using SMBLibrary.NetBios;
using SMBLibrary.SMB2;
using SMBLibrary.Services;
using Utilities;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class SMB2Client : ISMBClient
{
	public static readonly int NetBiosOverTCPPort = 139;

	public static readonly int DirectTCPPort = 445;

	public static readonly uint ClientMaxTransactSize = 1048576u;

	public static readonly uint ClientMaxReadSize = 1048576u;

	public static readonly uint ClientMaxWriteSize = 1048576u;

	private static readonly ushort DesiredCredits = 16;

	public static readonly int DefaultResponseTimeoutInMilliseconds = 5000;

	private string m_serverName;

	private SMBTransportType m_transport;

	private bool m_isConnected;

	private bool m_isLoggedIn;

	private Socket m_clientSocket;

	private ConnectionState m_connectionState;

	private int m_responseTimeoutInMilliseconds;

	private object m_incomingQueueLock = new object();

	private List<SMB2Command> m_incomingQueue = new List<SMB2Command>();

	private EventWaitHandle m_incomingQueueEventHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private SessionPacket m_sessionResponsePacket;

	private EventWaitHandle m_sessionResponseEventHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private uint m_messageID;

	private SMB2Dialect m_dialect;

	private bool m_signingRequired;

	private byte[] m_signingKey;

	private bool m_encryptSessionData;

	private byte[] m_encryptionKey;

	private byte[] m_decryptionKey;

	private uint m_maxTransactSize;

	private uint m_maxReadSize;

	private uint m_maxWriteSize;

	private ulong m_sessionID;

	private byte[] m_securityBlob;

	private byte[] m_sessionKey;

	private ushort m_availableCredits = 1;

	public uint MaxTransactSize => m_maxTransactSize;

	public uint MaxReadSize => m_maxReadSize;

	public uint MaxWriteSize => m_maxWriteSize;

	public bool IsConnected => m_isConnected;

	public bool Connect(string serverName, SMBTransportType transport)
	{
		return Connect(serverName, transport, DefaultResponseTimeoutInMilliseconds);
	}

	public bool Connect(string serverName, SMBTransportType transport, int responseTimeoutInMilliseconds)
	{
		m_serverName = serverName;
		IPAddress[] hostAddresses = Dns.GetHostAddresses(serverName);
		if (hostAddresses.Length == 0)
		{
			throw new Exception($"Cannot resolve host name {serverName} to an IP address");
		}
		IPAddress serverAddress = IPAddressHelper.SelectAddressPreferIPv4(hostAddresses);
		return Connect(serverAddress, transport, responseTimeoutInMilliseconds);
	}

	public bool Connect(IPAddress serverAddress, SMBTransportType transport)
	{
		return Connect(serverAddress, transport, DefaultResponseTimeoutInMilliseconds);
	}

	public bool Connect(IPAddress serverAddress, SMBTransportType transport, int responseTimeoutInMilliseconds)
	{
		int port = ((transport == SMBTransportType.DirectTCPTransport) ? DirectTCPPort : NetBiosOverTCPPort);
		return Connect(serverAddress, transport, port, responseTimeoutInMilliseconds);
	}

	internal bool Connect(IPAddress serverAddress, SMBTransportType transport, int port, int responseTimeoutInMilliseconds)
	{
		if (m_serverName == null)
		{
			m_serverName = serverAddress.ToString();
		}
		m_transport = transport;
		if (!m_isConnected)
		{
			m_responseTimeoutInMilliseconds = responseTimeoutInMilliseconds;
			if (!ConnectSocket(serverAddress, port))
			{
				return false;
			}
			if (transport == SMBTransportType.NetBiosOverTCP)
			{
				SessionRequestPacket sessionRequestPacket = new SessionRequestPacket();
				sessionRequestPacket.CalledName = NetBiosUtils.GetMSNetBiosName("*SMBSERVER", NetBiosSuffix.FileServiceService);
				sessionRequestPacket.CallingName = NetBiosUtils.GetMSNetBiosName(Environment.MachineName, NetBiosSuffix.WorkstationService);
				TrySendPacket(m_clientSocket, sessionRequestPacket);
				if (!(WaitForSessionResponsePacket() is PositiveSessionResponsePacket))
				{
					m_clientSocket.Disconnect(reuseSocket: false);
					if (!ConnectSocket(serverAddress, port))
					{
						return false;
					}
					string serverName = new NameServiceClient(serverAddress).GetServerName();
					if (serverName == null)
					{
						return false;
					}
					sessionRequestPacket.CalledName = serverName;
					TrySendPacket(m_clientSocket, sessionRequestPacket);
					if (!(WaitForSessionResponsePacket() is PositiveSessionResponsePacket))
					{
						return false;
					}
				}
			}
			if (!NegotiateDialect())
			{
				m_clientSocket.Close();
			}
			else
			{
				m_isConnected = true;
			}
		}
		return m_isConnected;
	}

	private bool ConnectSocket(IPAddress serverAddress, int port)
	{
		m_clientSocket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			m_clientSocket.Connect(serverAddress, port);
		}
		catch (SocketException)
		{
			return false;
		}
		m_connectionState = new ConnectionState(m_clientSocket);
		NBTConnectionReceiveBuffer receiveBuffer = m_connectionState.ReceiveBuffer;
		m_clientSocket.BeginReceive(receiveBuffer.Buffer, receiveBuffer.WriteOffset, receiveBuffer.AvailableLength, SocketFlags.None, OnClientSocketReceive, m_connectionState);
		return true;
	}

	public void Disconnect()
	{
		if (m_isConnected)
		{
			m_clientSocket.Disconnect(reuseSocket: false);
			m_clientSocket.Close();
			m_connectionState.ReceiveBuffer.Dispose();
			m_isConnected = false;
			m_messageID = 0u;
			m_sessionID = 0uL;
			m_availableCredits = 1;
		}
	}

	private bool NegotiateDialect()
	{
		NegotiateRequest negotiateRequest = new NegotiateRequest();
		negotiateRequest.SecurityMode = SecurityMode.SigningEnabled;
		negotiateRequest.Capabilities = Capabilities.Encryption;
		negotiateRequest.ClientGuid = Guid.NewGuid();
		negotiateRequest.ClientStartTime = DateTime.Now;
		negotiateRequest.Dialects.Add(SMB2Dialect.SMB202);
		negotiateRequest.Dialects.Add(SMB2Dialect.SMB210);
		negotiateRequest.Dialects.Add(SMB2Dialect.SMB300);
		TrySendCommand(negotiateRequest);
		if (WaitForCommand(negotiateRequest.MessageID) is NegotiateResponse negotiateResponse && negotiateResponse.Header.Status == NTStatus.STATUS_SUCCESS)
		{
			m_dialect = negotiateResponse.DialectRevision;
			m_signingRequired = (int)(negotiateResponse.SecurityMode & SecurityMode.SigningRequired) > 0;
			m_maxTransactSize = Math.Min(negotiateResponse.MaxTransactSize, ClientMaxTransactSize);
			m_maxReadSize = Math.Min(negotiateResponse.MaxReadSize, ClientMaxReadSize);
			m_maxWriteSize = Math.Min(negotiateResponse.MaxWriteSize, ClientMaxWriteSize);
			m_securityBlob = negotiateResponse.SecurityBuffer;
			return true;
		}
		return false;
	}

	public NTStatus Login(string domainName, string userName, string password)
	{
		return Login(domainName, userName, password, AuthenticationMethod.NTLMv2);
	}

	public NTStatus Login(string domainName, string userName, string password, AuthenticationMethod authenticationMethod)
	{
		string spn = $"cifs/{m_serverName}";
		NTLMAuthenticationClient authenticationClient = new NTLMAuthenticationClient(domainName, userName, password, spn, authenticationMethod);
		return Login(authenticationClient);
	}

	public NTStatus Login(IAuthenticationClient authenticationClient)
	{
		if (!m_isConnected)
		{
			throw new InvalidOperationException("A connection must be successfully established before attempting login");
		}
		byte[] array = authenticationClient.InitializeSecurityContext(m_securityBlob);
		if (array == null)
		{
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		SessionSetupRequest sessionSetupRequest = new SessionSetupRequest();
		sessionSetupRequest.SecurityMode = SecurityMode.SigningEnabled;
		sessionSetupRequest.SecurityBuffer = array;
		TrySendCommand(sessionSetupRequest);
		SMB2Command sMB2Command = WaitForCommand(sessionSetupRequest.MessageID);
		if (sMB2Command != null)
		{
			if (sMB2Command.Header.Status != NTStatus.STATUS_MORE_PROCESSING_REQUIRED || !(sMB2Command is SessionSetupResponse))
			{
				return sMB2Command.Header.Status;
			}
			byte[] array2 = authenticationClient.InitializeSecurityContext(((SessionSetupResponse)sMB2Command).SecurityBuffer);
			if (array2 == null)
			{
				return NTStatus.SEC_E_INVALID_TOKEN;
			}
			m_sessionKey = authenticationClient.GetSessionKey();
			m_sessionID = sMB2Command.Header.SessionID;
			sessionSetupRequest = new SessionSetupRequest();
			sessionSetupRequest.SecurityMode = SecurityMode.SigningEnabled;
			sessionSetupRequest.SecurityBuffer = array2;
			TrySendCommand(sessionSetupRequest);
			sMB2Command = WaitForCommand(sessionSetupRequest.MessageID);
			if (sMB2Command != null)
			{
				m_isLoggedIn = sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS;
				if (m_isLoggedIn)
				{
					m_signingKey = SMB2Cryptography.GenerateSigningKey(m_sessionKey, m_dialect, null);
					if (m_dialect == SMB2Dialect.SMB300)
					{
						m_encryptSessionData = (int)(((SessionSetupResponse)sMB2Command).SessionFlags & SessionFlags.EncryptData) > 0;
						m_encryptionKey = SMB2Cryptography.GenerateClientEncryptionKey(m_sessionKey, SMB2Dialect.SMB300, null);
						m_decryptionKey = SMB2Cryptography.GenerateClientDecryptionKey(m_sessionKey, SMB2Dialect.SMB300, null);
					}
				}
				return sMB2Command.Header.Status;
			}
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public NTStatus Logoff()
	{
		if (!m_isConnected)
		{
			throw new InvalidOperationException("A login session must be successfully established before attempting logoff");
		}
		LogoffRequest logoffRequest = new LogoffRequest();
		TrySendCommand(logoffRequest);
		SMB2Command sMB2Command = WaitForCommand(logoffRequest.MessageID);
		if (sMB2Command != null)
		{
			m_isLoggedIn = sMB2Command.Header.Status != NTStatus.STATUS_SUCCESS;
			return sMB2Command.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public List<string> ListShares(out NTStatus status)
	{
		if (!m_isConnected || !m_isLoggedIn)
		{
			throw new InvalidOperationException("A login session must be successfully established before retrieving share list");
		}
		ISMBFileStore iSMBFileStore = TreeConnect("IPC$", out status);
		if (iSMBFileStore == null)
		{
			return null;
		}
		List<string> result = ServerServiceHelper.ListShares(iSMBFileStore, m_serverName, SMBLibrary.Services.ShareType.DiskDrive, out status);
		iSMBFileStore.Disconnect();
		return result;
	}

	public ISMBFileStore TreeConnect(string shareName, out NTStatus status)
	{
		if (!m_isConnected || !m_isLoggedIn)
		{
			throw new InvalidOperationException("A login session must be successfully established before connecting to a share");
		}
		string path = $"\\\\{m_serverName}\\{shareName}";
		TreeConnectRequest treeConnectRequest = new TreeConnectRequest();
		treeConnectRequest.Path = path;
		TrySendCommand(treeConnectRequest);
		SMB2Command sMB2Command = WaitForCommand(treeConnectRequest.MessageID);
		if (sMB2Command != null)
		{
			status = sMB2Command.Header.Status;
			if (sMB2Command.Header.Status == NTStatus.STATUS_SUCCESS && sMB2Command is TreeConnectResponse)
			{
				bool flag = (((TreeConnectResponse)sMB2Command).ShareFlags & ShareFlags.EncryptData) != 0;
				return new SMB2FileStore(this, sMB2Command.Header.TreeID, m_encryptSessionData || flag);
			}
		}
		else
		{
			status = NTStatus.STATUS_INVALID_SMB;
		}
		return null;
	}

	private void OnClientSocketReceive(IAsyncResult ar)
	{
		ConnectionState connectionState = (ConnectionState)ar.AsyncState;
		Socket clientSocket = connectionState.ClientSocket;
		if (!clientSocket.Connected)
		{
			connectionState.ReceiveBuffer.Dispose();
			return;
		}
		int num = 0;
		try
		{
			num = clientSocket.EndReceive(ar);
		}
		catch (ArgumentException)
		{
			connectionState.ReceiveBuffer.Dispose();
			return;
		}
		catch (ObjectDisposedException)
		{
			Log("[ReceiveCallback] EndReceive ObjectDisposedException");
			connectionState.ReceiveBuffer.Dispose();
			return;
		}
		catch (SocketException ex3)
		{
			Log("[ReceiveCallback] EndReceive SocketException: " + ex3.Message);
			connectionState.ReceiveBuffer.Dispose();
			return;
		}
		if (num == 0)
		{
			m_isConnected = false;
			connectionState.ReceiveBuffer.Dispose();
			return;
		}
		NBTConnectionReceiveBuffer receiveBuffer = connectionState.ReceiveBuffer;
		receiveBuffer.SetNumberOfBytesReceived(num);
		ProcessConnectionBuffer(connectionState);
		if (!clientSocket.Connected)
		{
			return;
		}
		try
		{
			clientSocket.BeginReceive(receiveBuffer.Buffer, receiveBuffer.WriteOffset, receiveBuffer.AvailableLength, SocketFlags.None, OnClientSocketReceive, connectionState);
		}
		catch (ObjectDisposedException)
		{
			m_isConnected = false;
			Log("[ReceiveCallback] BeginReceive ObjectDisposedException");
			receiveBuffer.Dispose();
		}
		catch (SocketException ex5)
		{
			m_isConnected = false;
			Log("[ReceiveCallback] BeginReceive SocketException: " + ex5.Message);
			receiveBuffer.Dispose();
		}
	}

	private void ProcessConnectionBuffer(ConnectionState state)
	{
		NBTConnectionReceiveBuffer receiveBuffer = state.ReceiveBuffer;
		while (receiveBuffer.HasCompletePacket())
		{
			SessionPacket sessionPacket = null;
			try
			{
				sessionPacket = receiveBuffer.DequeuePacket();
			}
			catch (Exception)
			{
				Log("[ProcessConnectionBuffer] Invalid packet");
				state.ClientSocket.Close();
				state.ReceiveBuffer.Dispose();
				break;
			}
			if (sessionPacket != null)
			{
				ProcessPacket(sessionPacket, state);
			}
		}
	}

	private void ProcessPacket(SessionPacket packet, ConnectionState state)
	{
		if (packet is SessionMessagePacket)
		{
			byte[] buffer;
			if (m_dialect == SMB2Dialect.SMB300 && SMB2TransformHeader.IsTransformHeader(packet.Trailer, 0))
			{
				SMB2TransformHeader sMB2TransformHeader = new SMB2TransformHeader(packet.Trailer, 0);
				byte[] encryptedMessage = ByteReader.ReadBytes(packet.Trailer, 52, (int)sMB2TransformHeader.OriginalMessageSize);
				buffer = SMB2Cryptography.DecryptMessage(m_decryptionKey, sMB2TransformHeader, encryptedMessage);
			}
			else
			{
				buffer = packet.Trailer;
			}
			SMB2Command sMB2Command;
			try
			{
				sMB2Command = SMB2Command.ReadResponse(buffer, 0);
			}
			catch (Exception ex)
			{
				Log("Invalid SMB2 response: " + ex.Message);
				state.ClientSocket.Close();
				m_isConnected = false;
				state.ReceiveBuffer.Dispose();
				return;
			}
			m_availableCredits += sMB2Command.Header.Credits;
			if (m_transport == SMBTransportType.DirectTCPTransport && sMB2Command is NegotiateResponse)
			{
				NegotiateResponse negotiateResponse = (NegotiateResponse)sMB2Command;
				if ((negotiateResponse.Capabilities & Capabilities.LargeMTU) != 0)
				{
					int num = (int)Math.Max(negotiateResponse.MaxTransactSize, negotiateResponse.MaxReadSize);
					int num2 = 4 + (int)Math.Min(num, ClientMaxTransactSize) + 256;
					if (num2 > state.ReceiveBuffer.Buffer.Length)
					{
						state.ReceiveBuffer.IncreaseBufferSize(num2);
					}
				}
			}
			if (sMB2Command.Header.MessageID != ulong.MaxValue || sMB2Command.Header.Command == SMB2CommandName.OplockBreak)
			{
				lock (m_incomingQueueLock)
				{
					m_incomingQueue.Add(sMB2Command);
					m_incomingQueueEventHandle.Set();
				}
			}
		}
		else if ((packet is PositiveSessionResponsePacket || packet is NegativeSessionResponsePacket) && m_transport == SMBTransportType.NetBiosOverTCP)
		{
			m_sessionResponsePacket = packet;
			m_sessionResponseEventHandle.Set();
		}
		else if (!(packet is SessionKeepAlivePacket) || m_transport != 0)
		{
			Log("Inappropriate NetBIOS session packet");
			state.ClientSocket.Close();
			state.ReceiveBuffer.Dispose();
		}
	}

	internal SMB2Command WaitForCommand(ulong messageID)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (stopwatch.ElapsedMilliseconds < m_responseTimeoutInMilliseconds)
		{
			lock (m_incomingQueueLock)
			{
				for (int i = 0; i < m_incomingQueue.Count; i++)
				{
					SMB2Command sMB2Command = m_incomingQueue[i];
					if (sMB2Command.Header.MessageID == messageID)
					{
						m_incomingQueue.RemoveAt(i);
						if (!sMB2Command.Header.IsAsync || sMB2Command.Header.Status != NTStatus.STATUS_PENDING)
						{
							return sMB2Command;
						}
						i--;
					}
				}
			}
			m_incomingQueueEventHandle.WaitOne(100);
		}
		return null;
	}

	internal SessionPacket WaitForSessionResponsePacket()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (stopwatch.ElapsedMilliseconds < 5000)
		{
			if (m_sessionResponsePacket != null)
			{
				SessionPacket sessionResponsePacket = m_sessionResponsePacket;
				m_sessionResponsePacket = null;
				return sessionResponsePacket;
			}
			m_sessionResponseEventHandle.WaitOne(100);
		}
		return null;
	}

	private void Log(string message)
	{
	}

	internal void TrySendCommand(SMB2Command request)
	{
		TrySendCommand(request, m_encryptSessionData);
	}

	internal void TrySendCommand(SMB2Command request, bool encryptData)
	{
		if (m_dialect == SMB2Dialect.SMB202 || m_transport == SMBTransportType.NetBiosOverTCP)
		{
			request.Header.CreditCharge = 0;
			request.Header.Credits = 1;
			m_availableCredits--;
		}
		else
		{
			if (request.Header.CreditCharge == 0)
			{
				request.Header.CreditCharge = 1;
			}
			if (m_availableCredits < request.Header.CreditCharge)
			{
				throw new Exception("Not enough credits");
			}
			m_availableCredits -= request.Header.CreditCharge;
			if (m_availableCredits < DesiredCredits)
			{
				request.Header.Credits += (ushort)(DesiredCredits - m_availableCredits);
			}
		}
		request.Header.MessageID = m_messageID;
		request.Header.SessionID = m_sessionID;
		if (m_signingRequired && !encryptData)
		{
			request.Header.IsSigned = m_sessionID != 0L && (request.CommandName == SMB2CommandName.TreeConnect || request.Header.TreeID != 0 || (m_dialect == SMB2Dialect.SMB300 && request.CommandName == SMB2CommandName.Logoff));
			if (request.Header.IsSigned)
			{
				request.Header.Signature = new byte[16];
				byte[] bytes = request.GetBytes();
				byte[] buffer = SMB2Cryptography.CalculateSignature(m_signingKey, m_dialect, bytes, 0, bytes.Length);
				request.Header.Signature = ByteReader.ReadBytes(buffer, 0, 16);
			}
		}
		TrySendCommand(m_clientSocket, request, encryptData ? m_encryptionKey : null);
		if (m_dialect == SMB2Dialect.SMB202 || m_transport == SMBTransportType.NetBiosOverTCP)
		{
			m_messageID++;
		}
		else
		{
			m_messageID += request.Header.CreditCharge;
		}
	}

	private void TrySendCommand(Socket socket, SMB2Command request, byte[] encryptionKey)
	{
		SessionMessagePacket sessionMessagePacket = new SessionMessagePacket();
		if (encryptionKey != null)
		{
			byte[] bytes = request.GetBytes();
			sessionMessagePacket.Trailer = SMB2Cryptography.TransformMessage(encryptionKey, bytes, request.Header.SessionID);
		}
		else
		{
			sessionMessagePacket.Trailer = request.GetBytes();
		}
		TrySendPacket(socket, sessionMessagePacket);
	}

	private void TrySendPacket(Socket socket, SessionPacket packet)
	{
		try
		{
			byte[] bytes = packet.GetBytes();
			socket.Send(bytes);
		}
		catch (SocketException)
		{
			m_isConnected = false;
		}
		catch (ObjectDisposedException)
		{
			m_isConnected = false;
		}
	}
}
