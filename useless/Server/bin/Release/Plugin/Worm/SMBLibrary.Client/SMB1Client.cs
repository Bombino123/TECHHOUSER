using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using SMBLibrary.Authentication.NTLM;
using SMBLibrary.Client.Authentication;
using SMBLibrary.NetBios;
using SMBLibrary.SMB1;
using SMBLibrary.Services;
using Utilities;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class SMB1Client : ISMBClient
{
	private const string NTLanManagerDialect = "NT LM 0.12";

	public static readonly int NetBiosOverTCPPort = 139;

	public static readonly int DirectTCPPort = 445;

	private static readonly ushort ClientMaxBufferSize = ushort.MaxValue;

	private static readonly ushort ClientMaxMpxCount = 1;

	private static readonly int DefaultResponseTimeoutInMilliseconds = 5000;

	private SMBTransportType m_transport;

	private bool m_isConnected;

	private bool m_isLoggedIn;

	private Socket m_clientSocket;

	private ConnectionState m_connectionState;

	private bool m_forceExtendedSecurity;

	private bool m_unicode;

	private bool m_largeFiles;

	private bool m_infoLevelPassthrough;

	private bool m_largeRead;

	private bool m_largeWrite;

	private uint m_serverMaxBufferSize;

	private ushort m_maxMpxCount;

	private int m_responseTimeoutInMilliseconds;

	private object m_incomingQueueLock = new object();

	private List<SMB1Message> m_incomingQueue = new List<SMB1Message>();

	private EventWaitHandle m_incomingQueueEventHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private SessionPacket m_sessionResponsePacket;

	private EventWaitHandle m_sessionResponseEventHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private ushort m_userID;

	private byte[] m_serverChallenge;

	private byte[] m_securityBlob;

	private byte[] m_sessionKey;

	public bool Unicode => m_unicode;

	public bool LargeFiles => m_largeFiles;

	public bool InfoLevelPassthrough => m_infoLevelPassthrough;

	public bool LargeRead => m_largeRead;

	public bool LargeWrite => m_largeWrite;

	public uint ServerMaxBufferSize => m_serverMaxBufferSize;

	public int MaxMpxCount => m_maxMpxCount;

	public uint MaxReadSize => (uint)(ClientMaxBufferSize - 59);

	public uint MaxWriteSize
	{
		get
		{
			uint num = ServerMaxBufferSize - 63;
			if (m_unicode)
			{
				num--;
			}
			return num;
		}
	}

	public bool IsConnected => m_isConnected;

	public bool Connect(string serverName, SMBTransportType transport)
	{
		IPAddress[] hostAddresses = Dns.GetHostAddresses(serverName);
		if (hostAddresses.Length == 0)
		{
			throw new Exception($"Cannot resolve host name {serverName} to an IP address");
		}
		IPAddress serverAddress = IPAddressHelper.SelectAddressPreferIPv4(hostAddresses);
		return Connect(serverAddress, transport);
	}

	public bool Connect(IPAddress serverAddress, SMBTransportType transport)
	{
		return Connect(serverAddress, transport, forceExtendedSecurity: true);
	}

	public bool Connect(IPAddress serverAddress, SMBTransportType transport, bool forceExtendedSecurity)
	{
		return Connect(serverAddress, transport, forceExtendedSecurity, DefaultResponseTimeoutInMilliseconds);
	}

	public bool Connect(IPAddress serverAddress, SMBTransportType transport, bool forceExtendedSecurity, int responseTimeoutInMilliseconds)
	{
		int port = ((transport == SMBTransportType.DirectTCPTransport) ? DirectTCPPort : NetBiosOverTCPPort);
		return Connect(serverAddress, transport, port, forceExtendedSecurity, responseTimeoutInMilliseconds);
	}

	internal bool Connect(IPAddress serverAddress, SMBTransportType transport, int port, bool forceExtendedSecurity, int responseTimeoutInMilliseconds)
	{
		m_transport = transport;
		if (!m_isConnected)
		{
			m_forceExtendedSecurity = forceExtendedSecurity;
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
			if (!NegotiateDialect(m_forceExtendedSecurity))
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
			m_userID = 0;
		}
	}

	private bool NegotiateDialect(bool forceExtendedSecurity)
	{
		NegotiateRequest negotiateRequest = new NegotiateRequest();
		negotiateRequest.Dialects.Add("NT LM 0.12");
		TrySendMessage(negotiateRequest);
		SMB1Message sMB1Message = WaitForMessage(CommandName.SMB_COM_NEGOTIATE);
		if (sMB1Message == null)
		{
			return false;
		}
		if (sMB1Message.Commands[0] is NegotiateResponse && !forceExtendedSecurity)
		{
			NegotiateResponse negotiateResponse = (NegotiateResponse)sMB1Message.Commands[0];
			m_unicode = (negotiateResponse.Capabilities & Capabilities.Unicode) != 0;
			m_largeFiles = (negotiateResponse.Capabilities & Capabilities.LargeFiles) != 0;
			bool num = (negotiateResponse.Capabilities & Capabilities.NTSMB) != 0;
			bool flag = (negotiateResponse.Capabilities & Capabilities.RpcRemoteApi) != 0;
			bool flag2 = (negotiateResponse.Capabilities & Capabilities.NTStatusCode) != 0;
			m_infoLevelPassthrough = (negotiateResponse.Capabilities & Capabilities.InfoLevelPassthrough) != 0;
			m_largeRead = (negotiateResponse.Capabilities & Capabilities.LargeRead) != 0;
			m_largeWrite = (negotiateResponse.Capabilities & Capabilities.LargeWrite) != 0;
			m_serverMaxBufferSize = negotiateResponse.MaxBufferSize;
			m_maxMpxCount = Math.Min(negotiateResponse.MaxMpxCount, ClientMaxMpxCount);
			m_serverChallenge = negotiateResponse.Challenge;
			return num && flag && flag2;
		}
		if (sMB1Message.Commands[0] is NegotiateResponseExtended)
		{
			NegotiateResponseExtended negotiateResponseExtended = (NegotiateResponseExtended)sMB1Message.Commands[0];
			m_unicode = (negotiateResponseExtended.Capabilities & Capabilities.Unicode) != 0;
			m_largeFiles = (negotiateResponseExtended.Capabilities & Capabilities.LargeFiles) != 0;
			bool num2 = (negotiateResponseExtended.Capabilities & Capabilities.NTSMB) != 0;
			bool flag3 = (negotiateResponseExtended.Capabilities & Capabilities.RpcRemoteApi) != 0;
			bool flag4 = (negotiateResponseExtended.Capabilities & Capabilities.NTStatusCode) != 0;
			m_infoLevelPassthrough = (negotiateResponseExtended.Capabilities & Capabilities.InfoLevelPassthrough) != 0;
			m_largeRead = (negotiateResponseExtended.Capabilities & Capabilities.LargeRead) != 0;
			m_largeWrite = (negotiateResponseExtended.Capabilities & Capabilities.LargeWrite) != 0;
			m_serverMaxBufferSize = negotiateResponseExtended.MaxBufferSize;
			m_maxMpxCount = Math.Min(negotiateResponseExtended.MaxMpxCount, ClientMaxMpxCount);
			m_securityBlob = negotiateResponseExtended.SecurityBlob;
			return num2 && flag3 && flag4;
		}
		return false;
	}

	public NTStatus Login(string domainName, string userName, string password)
	{
		return Login(domainName, userName, password, AuthenticationMethod.NTLMv2);
	}

	public NTStatus Login(string domainName, string userName, string password, AuthenticationMethod authenticationMethod)
	{
		if (!m_isConnected)
		{
			throw new InvalidOperationException("A connection must be successfully established before attempting login");
		}
		Capabilities capabilities = Capabilities.NTSMB | Capabilities.RpcRemoteApi | Capabilities.NTStatusCode | Capabilities.NTFind;
		if (m_unicode)
		{
			capabilities |= Capabilities.Unicode;
		}
		if (m_largeFiles)
		{
			capabilities |= Capabilities.LargeFiles;
		}
		if (m_largeRead)
		{
			capabilities |= Capabilities.LargeRead;
		}
		if (m_serverChallenge != null)
		{
			SessionSetupAndXRequest sessionSetupAndXRequest = new SessionSetupAndXRequest();
			sessionSetupAndXRequest.MaxBufferSize = ClientMaxBufferSize;
			sessionSetupAndXRequest.MaxMpxCount = m_maxMpxCount;
			sessionSetupAndXRequest.Capabilities = capabilities;
			sessionSetupAndXRequest.AccountName = userName;
			sessionSetupAndXRequest.PrimaryDomain = domainName;
			byte[] array = new byte[8];
			new Random().NextBytes(array);
			switch (authenticationMethod)
			{
			case AuthenticationMethod.NTLMv1:
				sessionSetupAndXRequest.OEMPassword = NTLMCryptography.ComputeLMv1Response(m_serverChallenge, password);
				sessionSetupAndXRequest.UnicodePassword = NTLMCryptography.ComputeNTLMv1Response(m_serverChallenge, password);
				break;
			case AuthenticationMethod.NTLMv1ExtendedSessionSecurity:
				throw new ArgumentException("SMB Extended Security must be negotiated in order for NTLMv1 Extended Session Security to be used");
			default:
			{
				sessionSetupAndXRequest.OEMPassword = NTLMCryptography.ComputeLMv2Response(m_serverChallenge, array, password, userName, domainName);
				byte[] bytesPadded = new NTLMv2ClientChallenge(DateTime.UtcNow, array, AVPairUtils.GetAVPairSequence(domainName, Environment.MachineName)).GetBytesPadded();
				byte[] a = NTLMCryptography.ComputeNTLMv2Proof(m_serverChallenge, bytesPadded, password, userName, domainName);
				sessionSetupAndXRequest.UnicodePassword = ByteUtils.Concatenate(a, bytesPadded);
				break;
			}
			}
			TrySendMessage(sessionSetupAndXRequest);
			SMB1Message sMB1Message = WaitForMessage(CommandName.SMB_COM_SESSION_SETUP_ANDX);
			if (sMB1Message != null)
			{
				m_isLoggedIn = sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS;
				return sMB1Message.Header.Status;
			}
			return NTStatus.STATUS_INVALID_SMB;
		}
		NTLMAuthenticationClient nTLMAuthenticationClient = new NTLMAuthenticationClient(domainName, userName, password, null, authenticationMethod);
		byte[] array2 = nTLMAuthenticationClient.InitializeSecurityContext(m_securityBlob);
		if (array2 == null)
		{
			return NTStatus.SEC_E_INVALID_TOKEN;
		}
		SessionSetupAndXRequestExtended sessionSetupAndXRequestExtended = new SessionSetupAndXRequestExtended();
		sessionSetupAndXRequestExtended.MaxBufferSize = ClientMaxBufferSize;
		sessionSetupAndXRequestExtended.MaxMpxCount = m_maxMpxCount;
		sessionSetupAndXRequestExtended.Capabilities = capabilities;
		sessionSetupAndXRequestExtended.SecurityBlob = array2;
		TrySendMessage(sessionSetupAndXRequestExtended);
		SMB1Message sMB1Message2 = WaitForMessage(CommandName.SMB_COM_SESSION_SETUP_ANDX);
		if (sMB1Message2 != null)
		{
			if (sMB1Message2.Header.Status != NTStatus.STATUS_MORE_PROCESSING_REQUIRED || !(sMB1Message2.Commands[0] is SessionSetupAndXResponseExtended))
			{
				return sMB1Message2.Header.Status;
			}
			SessionSetupAndXResponseExtended sessionSetupAndXResponseExtended = (SessionSetupAndXResponseExtended)sMB1Message2.Commands[0];
			byte[] array3 = nTLMAuthenticationClient.InitializeSecurityContext(sessionSetupAndXResponseExtended.SecurityBlob);
			if (array3 == null)
			{
				return NTStatus.SEC_E_INVALID_TOKEN;
			}
			m_sessionKey = nTLMAuthenticationClient.GetSessionKey();
			m_userID = sMB1Message2.Header.UID;
			sessionSetupAndXRequestExtended = new SessionSetupAndXRequestExtended();
			sessionSetupAndXRequestExtended.MaxBufferSize = ClientMaxBufferSize;
			sessionSetupAndXRequestExtended.MaxMpxCount = m_maxMpxCount;
			sessionSetupAndXRequestExtended.Capabilities = capabilities;
			sessionSetupAndXRequestExtended.SecurityBlob = array3;
			TrySendMessage(sessionSetupAndXRequestExtended);
			sMB1Message2 = WaitForMessage(CommandName.SMB_COM_SESSION_SETUP_ANDX);
			if (sMB1Message2 != null)
			{
				m_isLoggedIn = sMB1Message2.Header.Status == NTStatus.STATUS_SUCCESS;
				return sMB1Message2.Header.Status;
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
		LogoffAndXRequest request = new LogoffAndXRequest();
		TrySendMessage(request);
		SMB1Message sMB1Message = WaitForMessage(CommandName.SMB_COM_LOGOFF_ANDX);
		if (sMB1Message != null)
		{
			m_isLoggedIn = sMB1Message.Header.Status != NTStatus.STATUS_SUCCESS;
			return sMB1Message.Header.Status;
		}
		return NTStatus.STATUS_INVALID_SMB;
	}

	public List<string> ListShares(out NTStatus status)
	{
		if (!m_isConnected || !m_isLoggedIn)
		{
			throw new InvalidOperationException("A login session must be successfully established before retrieving share list");
		}
		SMB1FileStore sMB1FileStore = TreeConnect("IPC$", ServiceName.NamedPipe, out status);
		if (sMB1FileStore == null)
		{
			return null;
		}
		List<string> result = ServerServiceHelper.ListShares(sMB1FileStore, ShareType.DiskDrive, out status);
		sMB1FileStore.Disconnect();
		return result;
	}

	public ISMBFileStore TreeConnect(string shareName, out NTStatus status)
	{
		return TreeConnect(shareName, ServiceName.AnyType, out status);
	}

	public SMB1FileStore TreeConnect(string shareName, ServiceName serviceName, out NTStatus status)
	{
		if (!m_isConnected || !m_isLoggedIn)
		{
			throw new InvalidOperationException("A login session must be successfully established before connecting to a share");
		}
		TreeConnectAndXRequest treeConnectAndXRequest = new TreeConnectAndXRequest();
		treeConnectAndXRequest.Path = shareName;
		treeConnectAndXRequest.Service = serviceName;
		TrySendMessage(treeConnectAndXRequest);
		SMB1Message sMB1Message = WaitForMessage(CommandName.SMB_COM_TREE_CONNECT_ANDX);
		if (sMB1Message != null)
		{
			status = sMB1Message.Header.Status;
			if (sMB1Message.Header.Status == NTStatus.STATUS_SUCCESS && sMB1Message.Commands[0] is TreeConnectAndXResponse)
			{
				_ = (TreeConnectAndXResponse)sMB1Message.Commands[0];
				return new SMB1FileStore(this, sMB1Message.Header.TID);
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
			receiveBuffer.Dispose();
			Log("[ReceiveCallback] BeginReceive ObjectDisposedException");
		}
		catch (SocketException ex5)
		{
			m_isConnected = false;
			receiveBuffer.Dispose();
			Log("[ReceiveCallback] BeginReceive SocketException: " + ex5.Message);
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
			SMB1Message sMB1Message;
			try
			{
				sMB1Message = SMB1Message.GetSMB1Message(packet.Trailer);
			}
			catch (Exception ex)
			{
				Log("Invalid SMB1 message: " + ex.Message);
				state.ClientSocket.Close();
				state.ReceiveBuffer.Dispose();
				m_isConnected = false;
				return;
			}
			if ((sMB1Message.Header.MID == ushort.MaxValue && sMB1Message.Header.Command == CommandName.SMB_COM_LOCKING_ANDX) || (sMB1Message.Header.PID == 0 && sMB1Message.Header.MID == 0))
			{
				lock (m_incomingQueueLock)
				{
					m_incomingQueue.Add(sMB1Message);
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

	internal SMB1Message WaitForMessage(CommandName commandName)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (stopwatch.ElapsedMilliseconds < m_responseTimeoutInMilliseconds)
		{
			lock (m_incomingQueueLock)
			{
				for (int i = 0; i < m_incomingQueue.Count; i++)
				{
					SMB1Message sMB1Message = m_incomingQueue[i];
					if (sMB1Message.Commands[0].CommandName == commandName)
					{
						m_incomingQueue.RemoveAt(i);
						return sMB1Message;
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

	internal void TrySendMessage(SMB1Command request)
	{
		TrySendMessage(request, 0);
	}

	internal void TrySendMessage(SMB1Command request, ushort treeID)
	{
		SMB1Message sMB1Message = new SMB1Message();
		sMB1Message.Header.UnicodeFlag = m_unicode;
		sMB1Message.Header.ExtendedSecurityFlag = m_forceExtendedSecurity;
		sMB1Message.Header.Flags2 |= HeaderFlags2.LongNamesAllowed | HeaderFlags2.LongNameUsed | HeaderFlags2.NTStatusCode;
		sMB1Message.Header.UID = m_userID;
		sMB1Message.Header.TID = treeID;
		sMB1Message.Commands.Add(request);
		TrySendMessage(m_clientSocket, sMB1Message);
	}

	private void TrySendMessage(Socket socket, SMB1Message message)
	{
		SessionMessagePacket sessionMessagePacket = new SessionMessagePacket();
		sessionMessagePacket.Trailer = message.GetBytes();
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
