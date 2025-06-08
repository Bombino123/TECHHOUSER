using System;
using System.Collections.Generic;
using System.Net;
using SMBLibrary.SMB1;
using SMBLibrary.SMB2;
using SMBLibrary.Server.SMB1;
using SMBLibrary.Server.SMB2;
using Utilities;

namespace SMBLibrary.Server;

internal class ConnectionManager
{
	private List<ConnectionState> m_activeConnections = new List<ConnectionState>();

	public void AddConnection(ConnectionState connection)
	{
		lock (m_activeConnections)
		{
			m_activeConnections.Add(connection);
		}
	}

	public bool RemoveConnection(ConnectionState connection)
	{
		lock (m_activeConnections)
		{
			int num = m_activeConnections.IndexOf(connection);
			if (num >= 0)
			{
				m_activeConnections.RemoveAt(num);
				return true;
			}
			return false;
		}
	}

	public void ReleaseConnection(ConnectionState connection)
	{
		connection.SendQueue.Stop();
		SocketUtils.ReleaseSocket(connection.ClientSocket);
		connection.CloseSessions();
		connection.ReceiveBuffer.Dispose();
		RemoveConnection(connection);
	}

	public void ReleaseConnection(IPEndPoint clientEndPoint)
	{
		ConnectionState connectionState = FindConnection(clientEndPoint);
		if (connectionState != null)
		{
			ReleaseConnection(connectionState);
		}
	}

	public void SendSMBKeepAlive(TimeSpan inactivityDuration)
	{
		foreach (ConnectionState item in new List<ConnectionState>(m_activeConnections))
		{
			if (item.LastReceiveDT.Add(inactivityDuration) < DateTime.UtcNow && item.LastSendDT.Add(inactivityDuration) < DateTime.UtcNow)
			{
				if (item is SMB1ConnectionState)
				{
					SMB1Message unsolicitedEchoReply = SMBLibrary.Server.SMB1.EchoHelper.GetUnsolicitedEchoReply();
					SMBServer.EnqueueMessage(item, unsolicitedEchoReply);
				}
				else if (item is SMB2ConnectionState)
				{
					SMBLibrary.SMB2.EchoResponse unsolicitedEchoResponse = SMBLibrary.Server.SMB2.EchoHelper.GetUnsolicitedEchoResponse();
					SMBServer.EnqueueResponse(item, unsolicitedEchoResponse);
				}
			}
		}
	}

	public void ReleaseAllConnections()
	{
		foreach (ConnectionState item in new List<ConnectionState>(m_activeConnections))
		{
			ReleaseConnection(item);
		}
	}

	private ConnectionState FindConnection(IPEndPoint clientEndPoint)
	{
		lock (m_activeConnections)
		{
			for (int i = 0; i < m_activeConnections.Count; i++)
			{
				if (m_activeConnections[i].ClientEndPoint.Equals(clientEndPoint))
				{
					return m_activeConnections[i];
				}
			}
		}
		return null;
	}

	public List<SessionInformation> GetSessionsInformation()
	{
		List<SessionInformation> list = new List<SessionInformation>();
		lock (m_activeConnections)
		{
			foreach (ConnectionState activeConnection in m_activeConnections)
			{
				List<SessionInformation> sessionsInformation = activeConnection.GetSessionsInformation();
				list.AddRange(sessionsInformation);
			}
			return list;
		}
	}
}
