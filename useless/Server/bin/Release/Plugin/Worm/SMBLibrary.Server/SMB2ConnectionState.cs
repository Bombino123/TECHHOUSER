using System.Collections.Generic;
using SMBLibrary.SMB2;

namespace SMBLibrary.Server;

internal class SMB2ConnectionState : ConnectionState
{
	private Dictionary<ulong, SMB2Session> m_sessions = new Dictionary<ulong, SMB2Session>();

	private ulong m_nextSessionID = 1uL;

	private Dictionary<ulong, SMB2AsyncContext> m_pendingRequests = new Dictionary<ulong, SMB2AsyncContext>();

	private ulong m_nextAsyncID = 1uL;

	public SMB2ConnectionState(ConnectionState state)
		: base(state)
	{
	}

	public ulong? AllocateSessionID()
	{
		for (ulong num = 0uL; num < ulong.MaxValue; num++)
		{
			ulong num2 = m_nextSessionID + num;
			if (num2 != 0L && num2 != uint.MaxValue && !m_sessions.ContainsKey(num2))
			{
				m_nextSessionID = num2 + 1;
				return num2;
			}
		}
		return null;
	}

	public SMB2Session CreateSession(ulong sessionID, string userName, string machineName, byte[] sessionKey, object accessToken, bool signingRequired, byte[] signingKey)
	{
		SMB2Session sMB2Session = new SMB2Session(this, sessionID, userName, machineName, sessionKey, accessToken, signingRequired, signingKey);
		lock (m_sessions)
		{
			m_sessions.Add(sessionID, sMB2Session);
			return sMB2Session;
		}
	}

	public SMB2Session GetSession(ulong sessionID)
	{
		m_sessions.TryGetValue(sessionID, out var value);
		return value;
	}

	public void RemoveSession(ulong sessionID)
	{
		m_sessions.TryGetValue(sessionID, out var value);
		if (value != null)
		{
			value.Close();
			lock (m_sessions)
			{
				m_sessions.Remove(sessionID);
			}
		}
	}

	public override void CloseSessions()
	{
		lock (m_sessions)
		{
			foreach (SMB2Session value in m_sessions.Values)
			{
				value.Close();
			}
			m_sessions.Clear();
		}
	}

	public override List<SessionInformation> GetSessionsInformation()
	{
		List<SessionInformation> list = new List<SessionInformation>();
		lock (m_sessions)
		{
			foreach (SMB2Session value in m_sessions.Values)
			{
				list.Add(new SessionInformation(base.ClientEndPoint, Dialect, value.UserName, value.MachineName, value.GetOpenFilesInformation(), value.CreationDT));
			}
			return list;
		}
	}

	private ulong? AllocateAsyncID()
	{
		for (ulong num = 0uL; num < ulong.MaxValue; num++)
		{
			ulong num2 = m_nextAsyncID + num;
			if (num2 != 0L && num2 != uint.MaxValue && !m_pendingRequests.ContainsKey(num2))
			{
				m_nextAsyncID = num2 + 1;
				return num2;
			}
		}
		return null;
	}

	public SMB2AsyncContext CreateAsyncContext(FileID fileID, SMB2ConnectionState connection, ulong sessionID, uint treeID)
	{
		ulong? num = AllocateAsyncID();
		if (!num.HasValue)
		{
			return null;
		}
		SMB2AsyncContext sMB2AsyncContext = new SMB2AsyncContext();
		sMB2AsyncContext.AsyncID = num.Value;
		sMB2AsyncContext.FileID = fileID;
		sMB2AsyncContext.Connection = connection;
		sMB2AsyncContext.SessionID = sessionID;
		sMB2AsyncContext.TreeID = treeID;
		lock (m_pendingRequests)
		{
			m_pendingRequests.Add(num.Value, sMB2AsyncContext);
			return sMB2AsyncContext;
		}
	}

	public SMB2AsyncContext GetAsyncContext(ulong asyncID)
	{
		lock (m_pendingRequests)
		{
			m_pendingRequests.TryGetValue(asyncID, out var value);
			return value;
		}
	}

	public void RemoveAsyncContext(SMB2AsyncContext context)
	{
		lock (m_pendingRequests)
		{
			m_pendingRequests.Remove(context.AsyncID);
		}
	}
}
