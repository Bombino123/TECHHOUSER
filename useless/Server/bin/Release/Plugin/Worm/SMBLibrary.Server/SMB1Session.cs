using System;
using System.Collections.Generic;
using System.IO;

namespace SMBLibrary.Server;

internal class SMB1Session
{
	private const int MaxSearches = 2048;

	private SMB1ConnectionState m_connection;

	private ushort m_userID;

	private byte[] m_sessionKey;

	private SecurityContext m_securityContext;

	private DateTime m_creationDT;

	private Dictionary<ushort, ISMBShare> m_connectedTrees = new Dictionary<ushort, ISMBShare>();

	private Dictionary<ushort, OpenFileObject> m_openFiles = new Dictionary<ushort, OpenFileObject>();

	private Dictionary<ushort, OpenSearch> m_openSearches = new Dictionary<ushort, OpenSearch>();

	private ushort m_nextSearchHandle = 1;

	public ushort UserID => m_userID;

	public SecurityContext SecurityContext => m_securityContext;

	public string UserName => m_securityContext.UserName;

	public string MachineName => m_securityContext.MachineName;

	public DateTime CreationDT => m_creationDT;

	public SMB1Session(SMB1ConnectionState connection, ushort userID, string userName, string machineName, byte[] sessionKey, object accessToken)
	{
		m_connection = connection;
		m_userID = userID;
		m_sessionKey = sessionKey;
		m_securityContext = new SecurityContext(userName, machineName, connection.ClientEndPoint, connection.AuthenticationContext, accessToken);
		m_creationDT = DateTime.UtcNow;
	}

	public ushort? AddConnectedTree(ISMBShare share)
	{
		lock (m_connection)
		{
			ushort? result = m_connection.AllocateTreeID();
			if (result.HasValue)
			{
				m_connectedTrees.Add(result.Value, share);
			}
			return result;
		}
	}

	public ISMBShare GetConnectedTree(ushort treeID)
	{
		m_connectedTrees.TryGetValue(treeID, out var value);
		return value;
	}

	public void DisconnectTree(ushort treeID)
	{
		m_connectedTrees.TryGetValue(treeID, out var value);
		if (value == null)
		{
			return;
		}
		lock (m_connection)
		{
			foreach (ushort item in new List<ushort>(m_openFiles.Keys))
			{
				OpenFileObject openFileObject = m_openFiles[item];
				if (openFileObject.TreeID == treeID)
				{
					value.FileStore.CloseFile(openFileObject.Handle);
					m_openFiles.Remove(item);
				}
			}
			m_connectedTrees.Remove(treeID);
		}
	}

	public bool IsTreeConnected(ushort treeID)
	{
		return m_connectedTrees.ContainsKey(treeID);
	}

	public ushort? AddOpenFile(ushort treeID, string shareName, string relativePath, object handle, FileAccess fileAccess)
	{
		lock (m_connection)
		{
			ushort? result = m_connection.AllocateFileID();
			if (result.HasValue)
			{
				m_openFiles.Add(result.Value, new OpenFileObject(treeID, shareName, relativePath, handle, fileAccess));
			}
			return result;
		}
	}

	public OpenFileObject GetOpenFileObject(ushort fileID)
	{
		m_openFiles.TryGetValue(fileID, out var value);
		return value;
	}

	public void RemoveOpenFile(ushort fileID)
	{
		lock (m_connection)
		{
			m_openFiles.Remove(fileID);
		}
	}

	public List<OpenFileInformation> GetOpenFilesInformation()
	{
		List<OpenFileInformation> list = new List<OpenFileInformation>();
		lock (m_connection)
		{
			foreach (OpenFileObject value in m_openFiles.Values)
			{
				list.Add(new OpenFileInformation(value.ShareName, value.Path, value.FileAccess, value.OpenedDT));
			}
			return list;
		}
	}

	private ushort? AllocateSearchHandle()
	{
		for (ushort num = 0; num < ushort.MaxValue; num++)
		{
			ushort num2 = (ushort)(m_nextSearchHandle + num);
			if (num2 != 0 && num2 != ushort.MaxValue && !m_openSearches.ContainsKey(num2))
			{
				m_nextSearchHandle = (ushort)(num2 + 1);
				return num2;
			}
		}
		return null;
	}

	public ushort? AddOpenSearch(List<QueryDirectoryFileInformation> entries, int enumerationLocation)
	{
		ushort? result = AllocateSearchHandle();
		if (result.HasValue)
		{
			OpenSearch value = new OpenSearch(entries, enumerationLocation);
			m_openSearches.Add(result.Value, value);
		}
		return result;
	}

	public OpenSearch GetOpenSearch(ushort searchHandle)
	{
		m_openSearches.TryGetValue(searchHandle, out var value);
		return value;
	}

	public void RemoveOpenSearch(ushort searchHandle)
	{
		m_openSearches.Remove(searchHandle);
	}

	public void Close()
	{
		foreach (ushort item in new List<ushort>(m_connectedTrees.Keys))
		{
			DisconnectTree(item);
		}
	}
}
