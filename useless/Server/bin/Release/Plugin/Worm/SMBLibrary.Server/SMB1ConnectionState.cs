using System.Collections.Generic;

namespace SMBLibrary.Server;

internal class SMB1ConnectionState : ConnectionState
{
	public int MaxBufferSize;

	public bool LargeRead;

	public bool LargeWrite;

	private Dictionary<ushort, SMB1Session> m_sessions = new Dictionary<ushort, SMB1Session>();

	private ushort m_nextUID = 1;

	private ushort m_nextTID = 1;

	private ushort m_nextFID = 1;

	private Dictionary<uint, ProcessStateObject> m_processStateList = new Dictionary<uint, ProcessStateObject>();

	private List<SMB1AsyncContext> m_pendingRequests = new List<SMB1AsyncContext>();

	public SMB1ConnectionState(ConnectionState state)
		: base(state)
	{
	}

	public ushort? AllocateUserID()
	{
		for (ushort num = 0; num < ushort.MaxValue; num++)
		{
			ushort num2 = (ushort)(m_nextUID + num);
			if (num2 != 0 && num2 != 65534 && num2 != ushort.MaxValue && !m_sessions.ContainsKey(num2))
			{
				m_nextUID = (ushort)(num2 + 1);
				return num2;
			}
		}
		return null;
	}

	public SMB1Session CreateSession(ushort userID, string userName, string machineName, byte[] sessionKey, object accessToken)
	{
		SMB1Session sMB1Session = new SMB1Session(this, userID, userName, machineName, sessionKey, accessToken);
		lock (m_sessions)
		{
			m_sessions.Add(userID, sMB1Session);
			return sMB1Session;
		}
	}

	public SMB1Session CreateSession(string userName, string machineName, byte[] sessionKey, object accessToken)
	{
		ushort? num = AllocateUserID();
		if (num.HasValue)
		{
			return CreateSession(num.Value, userName, machineName, sessionKey, accessToken);
		}
		return null;
	}

	public SMB1Session GetSession(ushort userID)
	{
		m_sessions.TryGetValue(userID, out var value);
		return value;
	}

	public void RemoveSession(ushort userID)
	{
		m_sessions.TryGetValue(userID, out var value);
		if (value != null)
		{
			value.Close();
			lock (m_sessions)
			{
				m_sessions.Remove(userID);
			}
		}
	}

	public override void CloseSessions()
	{
		lock (m_sessions)
		{
			foreach (SMB1Session value in m_sessions.Values)
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
			foreach (SMB1Session value in m_sessions.Values)
			{
				list.Add(new SessionInformation(base.ClientEndPoint, Dialect, value.UserName, value.MachineName, value.GetOpenFilesInformation(), value.CreationDT));
			}
			return list;
		}
	}

	public ushort? AllocateTreeID()
	{
		for (ushort num = 0; num < ushort.MaxValue; num++)
		{
			ushort num2 = (ushort)(m_nextTID + num);
			if (num2 != 0 && num2 != ushort.MaxValue && !IsTreeIDAllocated(num2))
			{
				m_nextTID = (ushort)(num2 + 1);
				return num2;
			}
		}
		return null;
	}

	private bool IsTreeIDAllocated(ushort treeID)
	{
		foreach (SMB1Session value in m_sessions.Values)
		{
			if (value.GetConnectedTree(treeID) != null)
			{
				return true;
			}
		}
		return false;
	}

	public ushort? AllocateFileID()
	{
		for (ushort num = 0; num < ushort.MaxValue; num++)
		{
			ushort num2 = (ushort)(m_nextFID + num);
			if (num2 != 0 && num2 != ushort.MaxValue && !IsFileIDAllocated(num2))
			{
				m_nextFID = (ushort)(num2 + 1);
				return num2;
			}
		}
		return null;
	}

	private bool IsFileIDAllocated(ushort fileID)
	{
		foreach (SMB1Session value in m_sessions.Values)
		{
			if (value.GetOpenFileObject(fileID) != null)
			{
				return true;
			}
		}
		return false;
	}

	public ProcessStateObject CreateProcessState(uint processID)
	{
		ProcessStateObject processStateObject = new ProcessStateObject();
		m_processStateList[processID] = processStateObject;
		return processStateObject;
	}

	public ProcessStateObject GetProcessState(uint processID)
	{
		if (m_processStateList.ContainsKey(processID))
		{
			return m_processStateList[processID];
		}
		return null;
	}

	public void RemoveProcessState(uint processID)
	{
		m_processStateList.Remove(processID);
	}

	public SMB1AsyncContext CreateAsyncContext(ushort userID, ushort treeID, uint processID, ushort multiplexID, ushort fileID, SMB1ConnectionState connection)
	{
		SMB1AsyncContext sMB1AsyncContext = new SMB1AsyncContext();
		sMB1AsyncContext.UID = userID;
		sMB1AsyncContext.TID = treeID;
		sMB1AsyncContext.MID = multiplexID;
		sMB1AsyncContext.PID = processID;
		sMB1AsyncContext.FileID = fileID;
		sMB1AsyncContext.Connection = connection;
		lock (m_pendingRequests)
		{
			m_pendingRequests.Add(sMB1AsyncContext);
			return sMB1AsyncContext;
		}
	}

	public SMB1AsyncContext GetAsyncContext(ushort userID, ushort treeID, uint processID, ushort multiplexID)
	{
		lock (m_pendingRequests)
		{
			int num = IndexOfAsyncContext(userID, treeID, processID, multiplexID);
			if (num >= 0)
			{
				return m_pendingRequests[num];
			}
		}
		return null;
	}

	public void RemoveAsyncContext(SMB1AsyncContext context)
	{
		lock (m_pendingRequests)
		{
			int num = IndexOfAsyncContext(context.UID, context.TID, context.PID, context.MID);
			if (num >= 0)
			{
				m_pendingRequests.RemoveAt(num);
			}
		}
	}

	private int IndexOfAsyncContext(ushort userID, ushort treeID, uint processID, ushort multiplexID)
	{
		for (int i = 0; i < m_pendingRequests.Count; i++)
		{
			SMB1AsyncContext sMB1AsyncContext = m_pendingRequests[i];
			if (sMB1AsyncContext.UID == userID && sMB1AsyncContext.TID == treeID && sMB1AsyncContext.PID == processID && sMB1AsyncContext.MID == multiplexID)
			{
				return i;
			}
		}
		return -1;
	}
}
