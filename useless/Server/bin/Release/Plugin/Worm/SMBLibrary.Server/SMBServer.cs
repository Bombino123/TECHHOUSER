using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.NetBios;
using SMBLibrary.SMB1;
using SMBLibrary.SMB2;
using SMBLibrary.Server.SMB1;
using SMBLibrary.Server.SMB2;
using Utilities;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class SMBServer
{
	public static readonly int NetBiosOverTCPPort = 139;

	public static readonly int DirectTCPPort = 445;

	public const string NTLanManagerDialect = "NT LM 0.12";

	public static readonly bool EnableExtendedSecurity = true;

	private static readonly int InactivityMonitoringInterval = 30000;

	private SMBShareCollection m_shares;

	private GSSProvider m_securityProvider;

	private NamedPipeShare m_services;

	private Guid m_serverGuid;

	private ConnectionManager m_connectionManager;

	private Thread m_sendSMBKeepAliveThread;

	private IPAddress m_serverAddress;

	private SMBTransportType m_transport;

	private bool m_enableSMB1;

	private bool m_enableSMB2;

	private bool m_enableSMB3;

	private Socket m_listenerSocket;

	private bool m_listening;

	private DateTime m_serverStartTime;

	public event EventHandler<ConnectionRequestEventArgs> ConnectionRequested;

	public event EventHandler<LogEntry> LogEntryAdded;

	public SMBServer(SMBShareCollection shares, GSSProvider securityProvider)
	{
		m_shares = shares;
		m_securityProvider = securityProvider;
		m_services = new NamedPipeShare(shares.ListShares());
		m_serverGuid = Guid.NewGuid();
		m_connectionManager = new ConnectionManager();
	}

	public void Start(IPAddress serverAddress, SMBTransportType transport)
	{
		Start(serverAddress, transport, enableSMB1: true, enableSMB2: true);
	}

	public void Start(IPAddress serverAddress, SMBTransportType transport, bool enableSMB1, bool enableSMB2)
	{
		Start(serverAddress, transport, enableSMB1, enableSMB2, enableSMB3: false);
	}

	public void Start(IPAddress serverAddress, SMBTransportType transport, bool enableSMB1, bool enableSMB2, bool enableSMB3)
	{
		Start(serverAddress, transport, enableSMB1, enableSMB2, enableSMB3, null);
	}

	public void Start(IPAddress serverAddress, SMBTransportType transport, bool enableSMB1, bool enableSMB2, bool enableSMB3, TimeSpan? connectionInactivityTimeout)
	{
		int port = ((transport == SMBTransportType.DirectTCPTransport) ? DirectTCPPort : NetBiosOverTCPPort);
		Start(serverAddress, transport, port, enableSMB1, enableSMB2, enableSMB3, connectionInactivityTimeout);
	}

	internal void Start(IPAddress serverAddress, SMBTransportType transport, int port, bool enableSMB1, bool enableSMB2, bool enableSMB3, TimeSpan? connectionInactivityTimeout)
	{
		if (m_listening)
		{
			return;
		}
		if (enableSMB3 && !enableSMB2)
		{
			throw new ArgumentException("SMB2 must be enabled for SMB3 to be enabled");
		}
		Log(Severity.Information, "Starting server");
		m_serverAddress = serverAddress;
		m_transport = transport;
		m_enableSMB1 = enableSMB1;
		m_enableSMB2 = enableSMB2;
		m_enableSMB3 = enableSMB3;
		m_listening = true;
		m_serverStartTime = DateTime.Now;
		m_listenerSocket = new Socket(m_serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		m_listenerSocket.Bind(new IPEndPoint(m_serverAddress, port));
		m_listenerSocket.Listen(int.MaxValue);
		m_listenerSocket.BeginAccept(ConnectRequestCallback, m_listenerSocket);
		if (!connectionInactivityTimeout.HasValue)
		{
			return;
		}
		m_sendSMBKeepAliveThread = new Thread((ThreadStart)delegate
		{
			while (m_listening)
			{
				Thread.Sleep(InactivityMonitoringInterval);
				m_connectionManager.SendSMBKeepAlive(connectionInactivityTimeout.Value);
			}
		});
		m_sendSMBKeepAliveThread.IsBackground = true;
		m_sendSMBKeepAliveThread.Start();
	}

	public void Stop()
	{
		Log(Severity.Information, "Stopping server");
		m_listening = false;
		if (m_sendSMBKeepAliveThread != null)
		{
			m_sendSMBKeepAliveThread.Abort();
		}
		SocketUtils.ReleaseSocket(m_listenerSocket);
		m_connectionManager.ReleaseAllConnections();
	}

	private void ConnectRequestCallback(IAsyncResult ar)
	{
		Socket socket = (Socket)ar.AsyncState;
		Socket socket2;
		try
		{
			socket2 = socket.EndAccept(ar);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		catch (SocketException ex2)
		{
			if (ex2.ErrorCode == 10054 || ex2.ErrorCode == 10060)
			{
				socket.BeginAccept(ConnectRequestCallback, socket);
			}
			Log(Severity.Debug, "Connection request error {0}", ex2.ErrorCode);
			return;
		}
		SocketUtils.SetKeepAlive(socket2, TimeSpan.FromMinutes(2.0));
		socket2.NoDelay = true;
		IPEndPoint iPEndPoint = (IPEndPoint)socket2.RemoteEndPoint;
		EventHandler<ConnectionRequestEventArgs> connectionRequested = this.ConnectionRequested;
		bool flag = true;
		if (connectionRequested != null)
		{
			ConnectionRequestEventArgs connectionRequestEventArgs = new ConnectionRequestEventArgs(iPEndPoint);
			connectionRequested(this, connectionRequestEventArgs);
			flag = connectionRequestEventArgs.Accept;
		}
		if (flag)
		{
			ConnectionState state = new ConnectionState(socket2, iPEndPoint, Log);
			state.LogToServer(Severity.Verbose, "New connection request accepted");
			Thread thread = new Thread((ThreadStart)delegate
			{
				ProcessSendQueue(state);
			});
			thread.IsBackground = true;
			thread.Start();
			try
			{
				socket2.BeginReceive(state.ReceiveBuffer.Buffer, state.ReceiveBuffer.WriteOffset, state.ReceiveBuffer.AvailableLength, SocketFlags.None, ReceiveCallback, state);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException)
			{
			}
		}
		else
		{
			Log(Severity.Verbose, "[{0}:{1}] New connection request rejected", iPEndPoint.Address, iPEndPoint.Port);
			socket2.Close();
		}
		socket.BeginAccept(ConnectRequestCallback, socket);
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		ConnectionState state = (ConnectionState)result.AsyncState;
		Socket clientSocket = state.ClientSocket;
		if (!m_listening)
		{
			clientSocket.Close();
			return;
		}
		int num;
		try
		{
			num = clientSocket.EndReceive(result);
		}
		catch (ObjectDisposedException)
		{
			state.LogToServer(Severity.Debug, "The connection was terminated");
			m_connectionManager.ReleaseConnection(state);
			return;
		}
		catch (SocketException ex2)
		{
			if (ex2.ErrorCode == 10054)
			{
				state.LogToServer(Severity.Debug, "The connection was forcibly closed by the remote host");
			}
			else
			{
				state.LogToServer(Severity.Debug, "The connection was terminated, Socket error code: {0}", ex2.ErrorCode);
			}
			m_connectionManager.ReleaseConnection(state);
			return;
		}
		if (num == 0)
		{
			state.LogToServer(Severity.Debug, "The client closed the connection");
			m_connectionManager.ReleaseConnection(state);
			return;
		}
		state.UpdateLastReceiveDT();
		state.ReceiveBuffer.SetNumberOfBytesReceived(num);
		ProcessConnectionBuffer(ref state);
		if (!clientSocket.Connected)
		{
			return;
		}
		try
		{
			clientSocket.BeginReceive(state.ReceiveBuffer.Buffer, state.ReceiveBuffer.WriteOffset, state.ReceiveBuffer.AvailableLength, SocketFlags.None, ReceiveCallback, state);
		}
		catch (ObjectDisposedException)
		{
			m_connectionManager.ReleaseConnection(state);
		}
		catch (SocketException)
		{
			m_connectionManager.ReleaseConnection(state);
		}
	}

	private void ProcessConnectionBuffer(ref ConnectionState state)
	{
		_ = state.ClientSocket;
		NBTConnectionReceiveBuffer receiveBuffer = state.ReceiveBuffer;
		while (receiveBuffer.HasCompletePacket())
		{
			SessionPacket sessionPacket = null;
			try
			{
				sessionPacket = receiveBuffer.DequeuePacket();
			}
			catch (Exception ex)
			{
				state.ClientSocket.Close();
				state.ReceiveBuffer.Dispose();
				state.LogToServer(Severity.Warning, "Rejected Invalid NetBIOS session packet: {0}", ex.Message);
				break;
			}
			if (sessionPacket != null)
			{
				ProcessPacket(sessionPacket, ref state);
			}
		}
	}

	private void ProcessPacket(SessionPacket packet, ref ConnectionState state)
	{
		if (packet is SessionMessagePacket)
		{
			bool flag = state.Dialect == SMBDialect.NotSet || state.Dialect == SMBDialect.NTLM012;
			bool flag2 = m_enableSMB2 && (state.Dialect == SMBDialect.NotSet || state.Dialect == SMBDialect.SMB202 || state.Dialect == SMBDialect.SMB210 || state.Dialect == SMBDialect.SMB300);
			if (SMB1Header.IsValidSMB1Header(packet.Trailer))
			{
				if (!flag)
				{
					state.LogToServer(Severity.Verbose, "Rejected SMB1 message");
					state.ClientSocket.Close();
					state.ReceiveBuffer.Dispose();
					return;
				}
				SMB1Message sMB1Message = null;
				try
				{
					sMB1Message = SMB1Message.GetSMB1Message(packet.Trailer);
				}
				catch (Exception ex)
				{
					state.LogToServer(Severity.Warning, "Invalid SMB1 message: " + ex.Message);
					state.ClientSocket.Close();
					state.ReceiveBuffer.Dispose();
					return;
				}
				state.LogToServer(Severity.Verbose, "SMB1 message received: {0} requests, First request: {1}, Packet length: {2}", sMB1Message.Commands.Count, sMB1Message.Commands[0].CommandName.ToString(), packet.Length);
				if (state.Dialect == SMBDialect.NotSet && m_enableSMB2)
				{
					List<string> list = SMBLibrary.Server.SMB2.NegotiateHelper.FindSMB2Dialects(sMB1Message);
					if (list.Count > 0)
					{
						SMB2Command negotiateResponse = SMBLibrary.Server.SMB2.NegotiateHelper.GetNegotiateResponse(list, m_securityProvider, state, m_transport, m_serverGuid, m_serverStartTime);
						if (state.Dialect != 0)
						{
							state = new SMB2ConnectionState(state);
							m_connectionManager.AddConnection(state);
						}
						EnqueueResponse(state, negotiateResponse);
						return;
					}
				}
				if (m_enableSMB1)
				{
					ProcessSMB1Message(sMB1Message, ref state);
					return;
				}
				state.LogToServer(Severity.Verbose, "Rejected SMB1 message");
				state.ClientSocket.Close();
			}
			else if (SMB2Header.IsValidSMB2Header(packet.Trailer))
			{
				if (!flag2)
				{
					state.LogToServer(Severity.Verbose, "Rejected SMB2 message");
					state.ClientSocket.Close();
					state.ReceiveBuffer.Dispose();
					return;
				}
				List<SMB2Command> list2;
				try
				{
					list2 = SMB2Command.ReadRequestChain(packet.Trailer, 0);
				}
				catch (Exception ex2)
				{
					state.LogToServer(Severity.Warning, "Invalid SMB2 request chain: " + ex2.Message);
					state.ClientSocket.Close();
					state.ReceiveBuffer.Dispose();
					return;
				}
				state.LogToServer(Severity.Verbose, "SMB2 request chain received: {0} requests, First request: {1}, Packet length: {2}", list2.Count, list2[0].CommandName.ToString(), packet.Length);
				ProcessSMB2RequestChain(list2, ref state);
			}
			else
			{
				state.LogToServer(Severity.Warning, "Invalid SMB message");
				state.ClientSocket.Close();
			}
		}
		else if (packet is SessionRequestPacket && m_transport == SMBTransportType.NetBiosOverTCP)
		{
			PositiveSessionResponsePacket item = new PositiveSessionResponsePacket();
			state.SendQueue.Enqueue(item);
		}
		else if (!(packet is SessionKeepAlivePacket) || m_transport != 0)
		{
			state.LogToServer(Severity.Warning, "Inappropriate NetBIOS session packet");
			state.ClientSocket.Close();
			state.ReceiveBuffer.Dispose();
		}
	}

	private void ProcessSendQueue(ConnectionState state)
	{
		state.LogToServer(Severity.Trace, "Entering ProcessSendQueue");
		SessionPacket item;
		while (state.SendQueue.TryDequeue(out item))
		{
			Socket clientSocket = state.ClientSocket;
			try
			{
				byte[] bytes = item.GetBytes();
				clientSocket.Send(bytes);
			}
			catch (SocketException ex)
			{
				state.LogToServer(Severity.Debug, "Failed to send packet. SocketException: {0}", ex.Message);
				m_connectionManager.ReleaseConnection(state.ClientEndPoint);
				break;
			}
			catch (ObjectDisposedException)
			{
				state.LogToServer(Severity.Debug, "Failed to send packet. ObjectDisposedException.");
				m_connectionManager.ReleaseConnection(state.ClientEndPoint);
				break;
			}
			state.UpdateLastSendDT();
		}
	}

	public List<SessionInformation> GetSessionsInformation()
	{
		return m_connectionManager.GetSessionsInformation();
	}

	public void TerminateConnection(IPEndPoint clientEndPoint)
	{
		m_connectionManager.ReleaseConnection(clientEndPoint);
	}

	private void Log(Severity severity, string message)
	{
		this.LogEntryAdded?.Invoke(this, new LogEntry(DateTime.Now, severity, "SMB Server", message));
	}

	private void Log(Severity severity, string message, params object[] args)
	{
		Log(severity, string.Format(message, args));
	}

	private void ProcessSMB1Message(SMB1Message message, ref ConnectionState state)
	{
		SMB1Header sMB1Header = new SMB1Header();
		PrepareResponseHeader(sMB1Header, message.Header);
		List<SMB1Command> list = new List<SMB1Command>();
		bool flag = message.Commands.Count > 1;
		foreach (SMB1Command command in message.Commands)
		{
			List<SMB1Command> collection = ProcessSMB1Command(sMB1Header, command, ref state);
			list.AddRange(collection);
			if (sMB1Header.Status != 0)
			{
				break;
			}
		}
		if (flag && list.Count > 0)
		{
			SMB1Message sMB1Message = new SMB1Message();
			sMB1Message.Header = sMB1Header;
			int num = 0;
			while (num < list.Count && (sMB1Message.Commands.Count == 0 || sMB1Message.Commands[sMB1Message.Commands.Count - 1] is SMBAndXCommand))
			{
				sMB1Message.Commands.Add(list[num]);
				list.RemoveAt(num);
				num--;
				num++;
			}
			EnqueueMessage(state, sMB1Message);
		}
		foreach (SMB1Command item in list)
		{
			SMB1Message sMB1Message2 = new SMB1Message();
			sMB1Message2.Header = sMB1Header;
			sMB1Message2.Commands.Add(item);
			EnqueueMessage(state, sMB1Message2);
		}
	}

	private List<SMB1Command> ProcessSMB1Command(SMB1Header header, SMB1Command command, ref ConnectionState state)
	{
		if (state.Dialect == SMBDialect.NotSet)
		{
			if (command is SMBLibrary.SMB1.NegotiateRequest)
			{
				SMBLibrary.SMB1.NegotiateRequest negotiateRequest = (SMBLibrary.SMB1.NegotiateRequest)command;
				if (negotiateRequest.Dialects.Contains("NT LM 0.12"))
				{
					state = new SMB1ConnectionState(state);
					state.Dialect = SMBDialect.NTLM012;
					m_connectionManager.AddConnection(state);
					if (EnableExtendedSecurity && header.ExtendedSecurityFlag)
					{
						return SMBLibrary.Server.SMB1.NegotiateHelper.GetNegotiateResponseExtended(negotiateRequest, m_serverGuid);
					}
					return SMBLibrary.Server.SMB1.NegotiateHelper.GetNegotiateResponse(header, negotiateRequest, m_securityProvider, state);
				}
				return new NegotiateResponseNotSupported();
			}
			header.Status = NTStatus.STATUS_INVALID_SMB;
			return new SMBLibrary.SMB1.ErrorResponse(command.CommandName);
		}
		if (command is SMBLibrary.SMB1.NegotiateRequest)
		{
			header.Status = NTStatus.STATUS_INVALID_SMB;
			return new SMBLibrary.SMB1.ErrorResponse(command.CommandName);
		}
		return ProcessSMB1Command(header, command, (SMB1ConnectionState)state);
	}

	private List<SMB1Command> ProcessSMB1Command(SMB1Header header, SMB1Command command, SMB1ConnectionState state)
	{
		if (command is SessionSetupAndXRequest)
		{
			SessionSetupAndXRequest sessionSetupAndXRequest = (SessionSetupAndXRequest)command;
			state.MaxBufferSize = sessionSetupAndXRequest.MaxBufferSize;
			return SMBLibrary.Server.SMB1.SessionSetupHelper.GetSessionSetupResponse(header, sessionSetupAndXRequest, m_securityProvider, state);
		}
		if (command is SessionSetupAndXRequestExtended)
		{
			SessionSetupAndXRequestExtended sessionSetupAndXRequestExtended = (SessionSetupAndXRequestExtended)command;
			state.MaxBufferSize = sessionSetupAndXRequestExtended.MaxBufferSize;
			return SMBLibrary.Server.SMB1.SessionSetupHelper.GetSessionSetupResponseExtended(header, sessionSetupAndXRequestExtended, m_securityProvider, state);
		}
		if (command is SMBLibrary.SMB1.EchoRequest)
		{
			return SMBLibrary.Server.SMB1.EchoHelper.GetEchoResponse((SMBLibrary.SMB1.EchoRequest)command);
		}
		SMB1Session session = state.GetSession(header.UID);
		if (session == null)
		{
			header.Status = NTStatus.STATUS_USER_SESSION_DELETED;
			return new SMBLibrary.SMB1.ErrorResponse(command.CommandName);
		}
		if (command is TreeConnectAndXRequest)
		{
			return SMBLibrary.Server.SMB1.TreeConnectHelper.GetTreeConnectResponse(header, (TreeConnectAndXRequest)command, state, m_services, m_shares);
		}
		if (command is LogoffAndXRequest)
		{
			state.LogToServer(Severity.Information, "Logoff: User '{0}' logged off. (UID: {1})", session.UserName, header.UID);
			m_securityProvider.DeleteSecurityContext(ref session.SecurityContext.AuthenticationContext);
			state.RemoveSession(header.UID);
			return new LogoffAndXResponse();
		}
		ISMBShare connectedTree = session.GetConnectedTree(header.TID);
		if (connectedTree == null)
		{
			state.LogToServer(Severity.Verbose, "{0} failed. Invalid TID (UID: {1}, TID: {2}).", command.CommandName, header.UID, header.TID);
			header.Status = NTStatus.STATUS_SMB_BAD_TID;
			return new SMBLibrary.SMB1.ErrorResponse(command.CommandName);
		}
		if (command is CreateDirectoryRequest)
		{
			return FileStoreResponseHelper.GetCreateDirectoryResponse(header, (CreateDirectoryRequest)command, connectedTree, state);
		}
		if (command is DeleteDirectoryRequest)
		{
			return FileStoreResponseHelper.GetDeleteDirectoryResponse(header, (DeleteDirectoryRequest)command, connectedTree, state);
		}
		if (command is SMBLibrary.SMB1.CloseRequest)
		{
			return SMBLibrary.Server.SMB1.CloseHelper.GetCloseResponse(header, (SMBLibrary.SMB1.CloseRequest)command, connectedTree, state);
		}
		if (command is SMBLibrary.SMB1.FlushRequest)
		{
			return SMBLibrary.Server.SMB1.ReadWriteResponseHelper.GetFlushResponse(header, (SMBLibrary.SMB1.FlushRequest)command, connectedTree, state);
		}
		if (command is DeleteRequest)
		{
			return FileStoreResponseHelper.GetDeleteResponse(header, (DeleteRequest)command, connectedTree, state);
		}
		if (command is RenameRequest)
		{
			return FileStoreResponseHelper.GetRenameResponse(header, (RenameRequest)command, connectedTree, state);
		}
		if (command is QueryInformationRequest)
		{
			return FileStoreResponseHelper.GetQueryInformationResponse(header, (QueryInformationRequest)command, connectedTree, state);
		}
		if (command is SetInformationRequest)
		{
			return FileStoreResponseHelper.GetSetInformationResponse(header, (SetInformationRequest)command, connectedTree, state);
		}
		if (command is SMBLibrary.SMB1.ReadRequest)
		{
			return SMBLibrary.Server.SMB1.ReadWriteResponseHelper.GetReadResponse(header, (SMBLibrary.SMB1.ReadRequest)command, connectedTree, state);
		}
		if (command is SMBLibrary.SMB1.WriteRequest)
		{
			return SMBLibrary.Server.SMB1.ReadWriteResponseHelper.GetWriteResponse(header, (SMBLibrary.SMB1.WriteRequest)command, connectedTree, state);
		}
		if (command is CheckDirectoryRequest)
		{
			return FileStoreResponseHelper.GetCheckDirectoryResponse(header, (CheckDirectoryRequest)command, connectedTree, state);
		}
		if (command is WriteRawRequest)
		{
			return new WriteRawFinalResponse();
		}
		if (command is SetInformation2Request)
		{
			return FileStoreResponseHelper.GetSetInformation2Response(header, (SetInformation2Request)command, connectedTree, state);
		}
		if (command is LockingAndXRequest)
		{
			return LockingHelper.GetLockingAndXResponse(header, (LockingAndXRequest)command, connectedTree, state);
		}
		if (command is OpenAndXRequest)
		{
			return OpenAndXHelper.GetOpenAndXResponse(header, (OpenAndXRequest)command, connectedTree, state);
		}
		if (command is ReadAndXRequest)
		{
			return SMBLibrary.Server.SMB1.ReadWriteResponseHelper.GetReadResponse(header, (ReadAndXRequest)command, connectedTree, state);
		}
		if (command is WriteAndXRequest)
		{
			return SMBLibrary.Server.SMB1.ReadWriteResponseHelper.GetWriteResponse(header, (WriteAndXRequest)command, connectedTree, state);
		}
		if (command is FindClose2Request)
		{
			return SMBLibrary.Server.SMB1.CloseHelper.GetFindClose2Response(header, (FindClose2Request)command, state);
		}
		if (command is SMBLibrary.SMB1.TreeDisconnectRequest)
		{
			return SMBLibrary.Server.SMB1.TreeConnectHelper.GetTreeDisconnectResponse(header, (SMBLibrary.SMB1.TreeDisconnectRequest)command, connectedTree, state);
		}
		if (command is TransactionRequest)
		{
			return TransactionHelper.GetTransactionResponse(header, (TransactionRequest)command, connectedTree, state);
		}
		if (command is TransactionSecondaryRequest)
		{
			return TransactionHelper.GetTransactionResponse(header, (TransactionSecondaryRequest)command, connectedTree, state);
		}
		if (command is NTTransactRequest)
		{
			return NTTransactHelper.GetNTTransactResponse(header, (NTTransactRequest)command, connectedTree, state);
		}
		if (command is NTTransactSecondaryRequest)
		{
			return NTTransactHelper.GetNTTransactResponse(header, (NTTransactSecondaryRequest)command, connectedTree, state);
		}
		if (command is NTCreateAndXRequest)
		{
			return NTCreateHelper.GetNTCreateResponse(header, (NTCreateAndXRequest)command, connectedTree, state);
		}
		if (command is NTCancelRequest)
		{
			SMBLibrary.Server.SMB1.CancelHelper.ProcessNTCancelRequest(header, (NTCancelRequest)command, connectedTree, state);
			return new List<SMB1Command>();
		}
		header.Status = NTStatus.STATUS_SMB_BAD_COMMAND;
		return new SMBLibrary.SMB1.ErrorResponse(command.CommandName);
	}

	internal static void EnqueueMessage(ConnectionState state, SMB1Message response)
	{
		SessionMessagePacket sessionMessagePacket = new SessionMessagePacket();
		sessionMessagePacket.Trailer = response.GetBytes();
		state.SendQueue.Enqueue(sessionMessagePacket);
		state.LogToServer(Severity.Verbose, "SMB1 message queued: {0} responses, First response: {1}, Packet length: {2}", response.Commands.Count, response.Commands[0].CommandName.ToString(), sessionMessagePacket.Length);
	}

	private static void PrepareResponseHeader(SMB1Header responseHeader, SMB1Header requestHeader)
	{
		responseHeader.Status = NTStatus.STATUS_SUCCESS;
		responseHeader.Flags = HeaderFlags.CaseInsensitive | HeaderFlags.CanonicalizedPaths | HeaderFlags.Reply;
		responseHeader.Flags2 = HeaderFlags2.NTStatusCode;
		if ((int)(requestHeader.Flags2 & HeaderFlags2.LongNamesAllowed) > 0)
		{
			responseHeader.Flags2 |= HeaderFlags2.LongNamesAllowed | HeaderFlags2.LongNameUsed;
		}
		if ((int)(requestHeader.Flags2 & HeaderFlags2.ExtendedAttributes) > 0)
		{
			responseHeader.Flags2 |= HeaderFlags2.ExtendedAttributes;
		}
		if ((int)(requestHeader.Flags2 & HeaderFlags2.ExtendedSecurity) > 0)
		{
			responseHeader.Flags2 |= HeaderFlags2.ExtendedSecurity;
		}
		if ((int)(requestHeader.Flags2 & HeaderFlags2.Unicode) > 0)
		{
			responseHeader.Flags2 |= HeaderFlags2.Unicode;
		}
		responseHeader.MID = requestHeader.MID;
		responseHeader.PID = requestHeader.PID;
		responseHeader.UID = requestHeader.UID;
		responseHeader.TID = requestHeader.TID;
	}

	private void ProcessSMB2RequestChain(List<SMB2Command> requestChain, ref ConnectionState state)
	{
		List<SMB2Command> list = new List<SMB2Command>();
		FileID? fileID = null;
		NTStatus? nTStatus = null;
		foreach (SMB2Command item in requestChain)
		{
			SMB2Command sMB2Command;
			if (item.Header.IsRelatedOperations && RequestContainsFileID(item))
			{
				if (nTStatus.HasValue && nTStatus != NTStatus.STATUS_SUCCESS && nTStatus != (NTStatus?)NTStatus.STATUS_BUFFER_OVERFLOW)
				{
					state.LogToServer(Severity.Verbose, "Compunded related request {0} failed because FileId generation failed.", item.CommandName);
					sMB2Command = new SMBLibrary.SMB2.ErrorResponse(item.CommandName, nTStatus.Value);
				}
				else if (fileID.HasValue)
				{
					SetRequestFileID(item, fileID.Value);
					sMB2Command = ProcessSMB2Command(item, ref state);
				}
				else
				{
					state.LogToServer(Severity.Verbose, "Compunded related request {0} failed, the previous request neither contains nor generates a FileId.", item.CommandName);
					sMB2Command = new SMBLibrary.SMB2.ErrorResponse(item.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
				}
			}
			else
			{
				fileID = GetRequestFileID(item);
				sMB2Command = ProcessSMB2Command(item, ref state);
			}
			if (sMB2Command != null)
			{
				UpdateSMB2Header(sMB2Command, item, state);
				list.Add(sMB2Command);
				if (GeneratesFileID(sMB2Command))
				{
					fileID = GetResponseFileID(sMB2Command);
					nTStatus = sMB2Command.Header.Status;
				}
				else if (RequestContainsFileID(item))
				{
					nTStatus = sMB2Command.Header.Status;
				}
			}
		}
		if (list.Count > 0)
		{
			EnqueueResponseChain(state, list);
		}
	}

	private SMB2Command ProcessSMB2Command(SMB2Command command, ref ConnectionState state)
	{
		if (state.Dialect == SMBDialect.NotSet)
		{
			if (command is SMBLibrary.SMB2.NegotiateRequest)
			{
				SMB2Command negotiateResponse = SMBLibrary.Server.SMB2.NegotiateHelper.GetNegotiateResponse((SMBLibrary.SMB2.NegotiateRequest)command, m_securityProvider, state, m_transport, m_serverGuid, m_serverStartTime, m_enableSMB3);
				if (state.Dialect != 0)
				{
					state = new SMB2ConnectionState(state);
					m_connectionManager.AddConnection(state);
				}
				return negotiateResponse;
			}
			state.LogToServer(Severity.Debug, "Invalid Connection State for command {0}", command.CommandName.ToString());
			state.ClientSocket.Close();
			return null;
		}
		if (command is SMBLibrary.SMB2.NegotiateRequest)
		{
			state.LogToServer(Severity.Debug, "Rejecting NegotiateRequest. NegotiateDialect is already set");
			state.ClientSocket.Close();
			return null;
		}
		return ProcessSMB2Command(command, (SMB2ConnectionState)state);
	}

	private SMB2Command ProcessSMB2Command(SMB2Command command, SMB2ConnectionState state)
	{
		if (command is SessionSetupRequest)
		{
			return SMBLibrary.Server.SMB2.SessionSetupHelper.GetSessionSetupResponse((SessionSetupRequest)command, m_securityProvider, state);
		}
		if (command is SMBLibrary.SMB2.EchoRequest)
		{
			return new SMBLibrary.SMB2.EchoResponse();
		}
		SMB2Session session = state.GetSession(command.Header.SessionID);
		if (session == null)
		{
			return new SMBLibrary.SMB2.ErrorResponse(command.CommandName, NTStatus.STATUS_USER_SESSION_DELETED);
		}
		if (command is TreeConnectRequest)
		{
			return SMBLibrary.Server.SMB2.TreeConnectHelper.GetTreeConnectResponse((TreeConnectRequest)command, state, m_services, m_shares);
		}
		if (command is LogoffRequest)
		{
			state.LogToServer(Severity.Information, "Logoff: User '{0}' logged off. (SessionID: {1})", session.UserName, command.Header.SessionID);
			m_securityProvider.DeleteSecurityContext(ref session.SecurityContext.AuthenticationContext);
			state.RemoveSession(command.Header.SessionID);
			return new LogoffResponse();
		}
		if (command.Header.IsAsync)
		{
			if (command is CancelRequest)
			{
				return SMBLibrary.Server.SMB2.CancelHelper.GetCancelResponse((CancelRequest)command, state);
			}
		}
		else
		{
			ISMBShare connectedTree = session.GetConnectedTree(command.Header.TreeID);
			if (connectedTree == null)
			{
				state.LogToServer(Severity.Verbose, "{0} failed. Invalid TreeID (SessionID: {1}, TreeID: {2}).", command.CommandName, command.Header.SessionID, command.Header.TreeID);
				return new SMBLibrary.SMB2.ErrorResponse(command.CommandName, NTStatus.STATUS_NETWORK_NAME_DELETED);
			}
			if (command is SMBLibrary.SMB2.TreeDisconnectRequest)
			{
				return SMBLibrary.Server.SMB2.TreeConnectHelper.GetTreeDisconnectResponse((SMBLibrary.SMB2.TreeDisconnectRequest)command, connectedTree, state);
			}
			if (command is CreateRequest)
			{
				return CreateHelper.GetCreateResponse((CreateRequest)command, connectedTree, state);
			}
			if (command is QueryInfoRequest)
			{
				return QueryInfoHelper.GetQueryInfoResponse((QueryInfoRequest)command, connectedTree, state);
			}
			if (command is SetInfoRequest)
			{
				return SetInfoHelper.GetSetInfoResponse((SetInfoRequest)command, connectedTree, state);
			}
			if (command is QueryDirectoryRequest)
			{
				return QueryDirectoryHelper.GetQueryDirectoryResponse((QueryDirectoryRequest)command, connectedTree, state);
			}
			if (command is SMBLibrary.SMB2.ReadRequest)
			{
				return SMBLibrary.Server.SMB2.ReadWriteResponseHelper.GetReadResponse((SMBLibrary.SMB2.ReadRequest)command, connectedTree, state);
			}
			if (command is SMBLibrary.SMB2.WriteRequest)
			{
				return SMBLibrary.Server.SMB2.ReadWriteResponseHelper.GetWriteResponse((SMBLibrary.SMB2.WriteRequest)command, connectedTree, state);
			}
			if (command is LockRequest)
			{
				return LockHelper.GetLockResponse((LockRequest)command, connectedTree, state);
			}
			if (command is SMBLibrary.SMB2.FlushRequest)
			{
				return SMBLibrary.Server.SMB2.ReadWriteResponseHelper.GetFlushResponse((SMBLibrary.SMB2.FlushRequest)command, connectedTree, state);
			}
			if (command is SMBLibrary.SMB2.CloseRequest)
			{
				return SMBLibrary.Server.SMB2.CloseHelper.GetCloseResponse((SMBLibrary.SMB2.CloseRequest)command, connectedTree, state);
			}
			if (command is IOCtlRequest)
			{
				return IOCtlHelper.GetIOCtlResponse((IOCtlRequest)command, connectedTree, state);
			}
			if (command is CancelRequest)
			{
				return SMBLibrary.Server.SMB2.CancelHelper.GetCancelResponse((CancelRequest)command, state);
			}
			if (command is ChangeNotifyRequest)
			{
				return ChangeNotifyHelper.GetChangeNotifyInterimResponse((ChangeNotifyRequest)command, connectedTree, state);
			}
		}
		return new SMBLibrary.SMB2.ErrorResponse(command.CommandName, NTStatus.STATUS_NOT_SUPPORTED);
	}

	internal static void EnqueueResponse(ConnectionState state, SMB2Command response)
	{
		List<SMB2Command> list = new List<SMB2Command>();
		list.Add(response);
		EnqueueResponseChain(state, list);
	}

	private static void EnqueueResponseChain(ConnectionState state, List<SMB2Command> responseChain)
	{
		byte[] array = null;
		if (state is SMB2ConnectionState)
		{
			ulong sessionID = responseChain[0].Header.SessionID;
			if (sessionID != 0L)
			{
				SMB2Session session = ((SMB2ConnectionState)state).GetSession(sessionID);
				if (session != null)
				{
					array = session.SigningKey;
				}
			}
		}
		SessionMessagePacket sessionMessagePacket = new SessionMessagePacket();
		SMB2Dialect dialect = ((array != null) ? ToSMB2Dialect(state.Dialect) : SMB2Dialect.SMB2xx);
		sessionMessagePacket.Trailer = SMB2Command.GetCommandChainBytes(responseChain, array, dialect);
		state.SendQueue.Enqueue(sessionMessagePacket);
		state.LogToServer(Severity.Verbose, "SMB2 response chain queued: Response count: {0}, First response: {1}, Packet length: {2}", responseChain.Count, responseChain[0].CommandName.ToString(), sessionMessagePacket.Length);
	}

	internal static SMB2Dialect ToSMB2Dialect(SMBDialect smbDialect)
	{
		return smbDialect switch
		{
			SMBDialect.SMB202 => SMB2Dialect.SMB202, 
			SMBDialect.SMB210 => SMB2Dialect.SMB210, 
			SMBDialect.SMB300 => SMB2Dialect.SMB300, 
			_ => throw new ArgumentException("Unsupported SMB2 Dialect: " + smbDialect), 
		};
	}

	private static void UpdateSMB2Header(SMB2Command response, SMB2Command request, ConnectionState state)
	{
		response.Header.MessageID = request.Header.MessageID;
		response.Header.CreditCharge = request.Header.CreditCharge;
		response.Header.Credits = Math.Max((ushort)1, request.Header.Credits);
		response.Header.IsRelatedOperations = request.Header.IsRelatedOperations;
		response.Header.Reserved = request.Header.Reserved;
		if (response.Header.SessionID == 0L)
		{
			response.Header.SessionID = request.Header.SessionID;
		}
		if (response.Header.TreeID == 0)
		{
			response.Header.TreeID = request.Header.TreeID;
		}
		bool flag = false;
		if (state is SMB2ConnectionState)
		{
			SMB2Session session = ((SMB2ConnectionState)state).GetSession(response.Header.SessionID);
			if (session != null && session.SigningRequired)
			{
				flag = true;
			}
		}
		bool flag2 = response.Header.IsAsync && response.Header.Status == NTStatus.STATUS_PENDING;
		response.Header.IsSigned = (request.Header.IsSigned || flag) && !flag2;
	}

	private static bool RequestContainsFileID(SMB2Command command)
	{
		if (!(command is ChangeNotifyRequest) && !(command is SMBLibrary.SMB2.CloseRequest) && !(command is SMBLibrary.SMB2.FlushRequest) && !(command is IOCtlRequest) && !(command is LockRequest) && !(command is QueryDirectoryRequest) && !(command is QueryInfoRequest) && !(command is SMBLibrary.SMB2.ReadRequest) && !(command is SetInfoRequest))
		{
			return command is SMBLibrary.SMB2.WriteRequest;
		}
		return true;
	}

	private static FileID? GetRequestFileID(SMB2Command command)
	{
		if (command is ChangeNotifyRequest)
		{
			return ((ChangeNotifyRequest)command).FileId;
		}
		if (command is SMBLibrary.SMB2.CloseRequest)
		{
			return ((SMBLibrary.SMB2.CloseRequest)command).FileId;
		}
		if (command is SMBLibrary.SMB2.FlushRequest)
		{
			return ((SMBLibrary.SMB2.FlushRequest)command).FileId;
		}
		if (command is IOCtlRequest)
		{
			return ((IOCtlRequest)command).FileId;
		}
		if (command is LockRequest)
		{
			return ((LockRequest)command).FileId;
		}
		if (command is QueryDirectoryRequest)
		{
			return ((QueryDirectoryRequest)command).FileId;
		}
		if (command is QueryInfoRequest)
		{
			return ((QueryInfoRequest)command).FileId;
		}
		if (command is SMBLibrary.SMB2.ReadRequest)
		{
			return ((SMBLibrary.SMB2.ReadRequest)command).FileId;
		}
		if (command is SetInfoRequest)
		{
			return ((SetInfoRequest)command).FileId;
		}
		if (command is SMBLibrary.SMB2.WriteRequest)
		{
			return ((SMBLibrary.SMB2.WriteRequest)command).FileId;
		}
		return null;
	}

	private static void SetRequestFileID(SMB2Command command, FileID fileID)
	{
		if (command is ChangeNotifyRequest)
		{
			((ChangeNotifyRequest)command).FileId = fileID;
		}
		else if (command is SMBLibrary.SMB2.CloseRequest)
		{
			((SMBLibrary.SMB2.CloseRequest)command).FileId = fileID;
		}
		else if (command is SMBLibrary.SMB2.FlushRequest)
		{
			((SMBLibrary.SMB2.FlushRequest)command).FileId = fileID;
		}
		else if (command is IOCtlRequest)
		{
			((IOCtlRequest)command).FileId = fileID;
		}
		else if (command is LockRequest)
		{
			((LockRequest)command).FileId = fileID;
		}
		else if (command is QueryDirectoryRequest)
		{
			((QueryDirectoryRequest)command).FileId = fileID;
		}
		else if (command is QueryInfoRequest)
		{
			((QueryInfoRequest)command).FileId = fileID;
		}
		else if (command is SMBLibrary.SMB2.ReadRequest)
		{
			((SMBLibrary.SMB2.ReadRequest)command).FileId = fileID;
		}
		else if (command is SetInfoRequest)
		{
			((SetInfoRequest)command).FileId = fileID;
		}
		else if (command is SMBLibrary.SMB2.WriteRequest)
		{
			((SMBLibrary.SMB2.WriteRequest)command).FileId = fileID;
		}
	}

	private static bool GeneratesFileID(SMB2Command command)
	{
		if (command.CommandName != SMB2CommandName.Create)
		{
			return command.CommandName == SMB2CommandName.IOCtl;
		}
		return true;
	}

	private static FileID? GetResponseFileID(SMB2Command command)
	{
		if (command is CreateResponse)
		{
			return ((CreateResponse)command).FileId;
		}
		if (command is IOCtlResponse)
		{
			return ((IOCtlResponse)command).FileId;
		}
		return null;
	}
}
