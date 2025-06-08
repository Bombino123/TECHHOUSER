using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SMBLibrary.Authentication.GSSAPI;
using SMBLibrary.NetBios;
using Utilities;

namespace SMBLibrary.Server;

internal class ConnectionState
{
	private Socket m_clientSocket;

	private IPEndPoint m_clientEndPoint;

	private NBTConnectionReceiveBuffer m_receiveBuffer;

	private BlockingQueue<SessionPacket> m_sendQueue;

	private DateTime m_creationDT;

	private DateTime m_lastReceiveDT;

	private Reference<DateTime> m_lastSendDTRef;

	private LogDelegate LogToServerHandler;

	public SMBDialect Dialect;

	public GSSContext AuthenticationContext;

	public Socket ClientSocket => m_clientSocket;

	public IPEndPoint ClientEndPoint => m_clientEndPoint;

	public NBTConnectionReceiveBuffer ReceiveBuffer => m_receiveBuffer;

	public BlockingQueue<SessionPacket> SendQueue => m_sendQueue;

	public DateTime CreationDT => m_creationDT;

	public DateTime LastReceiveDT => m_lastReceiveDT;

	public DateTime LastSendDT => LastSendDTRef.Value;

	internal Reference<DateTime> LastSendDTRef => m_lastSendDTRef;

	public string ConnectionIdentifier
	{
		get
		{
			if (ClientEndPoint != null)
			{
				return ClientEndPoint.Address?.ToString() + ":" + ClientEndPoint.Port;
			}
			return string.Empty;
		}
	}

	public ConnectionState(Socket clientSocket, IPEndPoint clientEndPoint, LogDelegate logToServerHandler)
	{
		m_clientSocket = clientSocket;
		m_clientEndPoint = clientEndPoint;
		m_receiveBuffer = new NBTConnectionReceiveBuffer();
		m_sendQueue = new BlockingQueue<SessionPacket>();
		m_creationDT = DateTime.UtcNow;
		m_lastReceiveDT = DateTime.UtcNow;
		m_lastSendDTRef = DateTime.UtcNow;
		LogToServerHandler = logToServerHandler;
		Dialect = SMBDialect.NotSet;
	}

	public ConnectionState(ConnectionState state)
	{
		m_clientSocket = state.ClientSocket;
		m_clientEndPoint = state.ClientEndPoint;
		m_receiveBuffer = state.ReceiveBuffer;
		m_sendQueue = state.SendQueue;
		m_creationDT = state.CreationDT;
		m_lastReceiveDT = state.LastReceiveDT;
		m_lastSendDTRef = state.LastSendDTRef;
		LogToServerHandler = state.LogToServerHandler;
		Dialect = state.Dialect;
	}

	public virtual void CloseSessions()
	{
	}

	public virtual List<SessionInformation> GetSessionsInformation()
	{
		return new List<SessionInformation>();
	}

	public void LogToServer(Severity severity, string message)
	{
		message = $"[{ConnectionIdentifier}] {message}";
		if (LogToServerHandler != null)
		{
			LogToServerHandler(severity, message);
		}
	}

	public void LogToServer(Severity severity, string message, params object[] args)
	{
		LogToServer(severity, string.Format(message, args));
	}

	public void UpdateLastReceiveDT()
	{
		m_lastReceiveDT = DateTime.UtcNow;
	}

	public void UpdateLastSendDT()
	{
		m_lastSendDTRef.Value = DateTime.UtcNow;
	}
}
