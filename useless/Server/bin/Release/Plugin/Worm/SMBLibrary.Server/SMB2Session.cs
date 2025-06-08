using System;
using System.Collections.Generic;
using System.IO;
using SMBLibrary.SMB2;

namespace SMBLibrary.Server;

internal class SMB2Session
{
	private SMB2ConnectionState m_connection;

	private ulong m_sessionID;

	private byte[] m_sessionKey;

	private SecurityContext m_securityContext;

	private DateTime m_creationDT;

	private bool m_signingRequired;

	private byte[] m_signingKey;

	private Dictionary<uint, ISMBShare> m_connectedTrees = new Dictionary<uint, ISMBShare>();

	private uint m_nextTreeID = 1u;

	private Dictionary<ulong, OpenFileObject> m_openFiles = new Dictionary<ulong, OpenFileObject>();

	private ulong m_nextVolatileFileID = 1uL;

	private Dictionary<ulong, OpenSearch> m_openSearches = new Dictionary<ulong, OpenSearch>();

	public byte[] SessionKey => m_sessionKey;

	public SecurityContext SecurityContext => m_securityContext;

	public string UserName => m_securityContext.UserName;

	public string MachineName => m_securityContext.MachineName;

	public DateTime CreationDT => m_creationDT;

	public bool SigningRequired => m_signingRequired;

	public byte[] SigningKey => m_signingKey;

	public SMB2Session(SMB2ConnectionState connection, ulong sessionID, string userName, string machineName, byte[] sessionKey, object accessToken, bool signingRequired, byte[] signingKey)
	{
		m_connection = connection;
		m_sessionID = sessionID;
		m_sessionKey = sessionKey;
		m_securityContext = new SecurityContext(userName, machineName, connection.ClientEndPoint, connection.AuthenticationContext, accessToken);
		m_creationDT = DateTime.UtcNow;
		m_signingRequired = signingRequired;
		m_signingKey = signingKey;
	}

	private uint? AllocateTreeID()
	{
		for (uint num = 0u; num < uint.MaxValue; num++)
		{
			uint num2 = m_nextTreeID + num;
			if (num2 != 0 && num2 != uint.MaxValue && !m_connectedTrees.ContainsKey(num2))
			{
				m_nextTreeID = num2 + 1;
				return num2;
			}
		}
		return null;
	}

	public uint? AddConnectedTree(ISMBShare share)
	{
		lock (m_connectedTrees)
		{
			uint? result = AllocateTreeID();
			if (result.HasValue)
			{
				m_connectedTrees.Add(result.Value, share);
			}
			return result;
		}
	}

	public ISMBShare GetConnectedTree(uint treeID)
	{
		m_connectedTrees.TryGetValue(treeID, out var value);
		return value;
	}

	public void DisconnectTree(uint treeID)
	{
		m_connectedTrees.TryGetValue(treeID, out var value);
		if (value == null)
		{
			return;
		}
		lock (m_openFiles)
		{
			foreach (ulong item in new List<ulong>(m_openFiles.Keys))
			{
				OpenFileObject openFileObject = m_openFiles[item];
				if (openFileObject.TreeID == treeID)
				{
					value.FileStore.CloseFile(openFileObject.Handle);
					m_openFiles.Remove(item);
				}
			}
		}
		lock (m_connectedTrees)
		{
			m_connectedTrees.Remove(treeID);
		}
	}

	public bool IsTreeConnected(uint treeID)
	{
		return m_connectedTrees.ContainsKey(treeID);
	}

	private ulong? AllocateVolatileFileID()
	{
		for (ulong num = 0uL; num < ulong.MaxValue; num++)
		{
			ulong num2 = m_nextVolatileFileID + num;
			if (num2 != 0L && num2 != ulong.MaxValue && !m_openFiles.ContainsKey(num2))
			{
				m_nextVolatileFileID = num2 + 1;
				return num2;
			}
		}
		return null;
	}

	public FileID? AddOpenFile(uint treeID, string shareName, string relativePath, object handle, FileAccess fileAccess)
	{
		lock (m_openFiles)
		{
			ulong? num = AllocateVolatileFileID();
			if (num.HasValue)
			{
				FileID value = default(FileID);
				value.Volatile = num.Value;
				value.Persistent = num.Value;
				m_openFiles.Add(num.Value, new OpenFileObject(treeID, shareName, relativePath, handle, fileAccess));
				return value;
			}
		}
		return null;
	}

	public OpenFileObject GetOpenFileObject(FileID fileID)
	{
		m_openFiles.TryGetValue(fileID.Volatile, out var value);
		return value;
	}

	public void RemoveOpenFile(FileID fileID)
	{
		lock (m_openFiles)
		{
			m_openFiles.Remove(fileID.Volatile);
		}
		m_openSearches.Remove(fileID.Volatile);
	}

	public List<OpenFileInformation> GetOpenFilesInformation()
	{
		List<OpenFileInformation> list = new List<OpenFileInformation>();
		lock (m_openFiles)
		{
			foreach (OpenFileObject value in m_openFiles.Values)
			{
				list.Add(new OpenFileInformation(value.ShareName, value.Path, value.FileAccess, value.OpenedDT));
			}
			return list;
		}
	}

	public OpenSearch AddOpenSearch(FileID fileID, List<QueryDirectoryFileInformation> entries, int enumerationLocation)
	{
		OpenSearch openSearch = new OpenSearch(entries, enumerationLocation);
		m_openSearches.Add(fileID.Volatile, openSearch);
		return openSearch;
	}

	public OpenSearch GetOpenSearch(FileID fileID)
	{
		m_openSearches.TryGetValue(fileID.Volatile, out var value);
		return value;
	}

	public void RemoveOpenSearch(FileID fileID)
	{
		m_openSearches.Remove(fileID.Volatile);
	}

	public void Close()
	{
		foreach (uint item in new List<uint>(m_connectedTrees.Keys))
		{
			DisconnectTree(item);
		}
	}
}
