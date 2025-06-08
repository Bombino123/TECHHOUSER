using System;
using System.IO;

namespace SMBLibrary.Server;

internal class OpenFileObject
{
	private uint m_treeID;

	private string m_shareName;

	private string m_path;

	private object m_handle;

	private FileAccess m_fileAccess;

	private DateTime m_openedDT;

	public uint TreeID => m_treeID;

	public string ShareName => m_shareName;

	public string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	public object Handle => m_handle;

	public FileAccess FileAccess => m_fileAccess;

	public DateTime OpenedDT => m_openedDT;

	public OpenFileObject(uint treeID, string shareName, string path, object handle, FileAccess fileAccess)
	{
		m_treeID = treeID;
		m_shareName = shareName;
		m_path = path;
		m_handle = handle;
		m_fileAccess = fileAccess;
		m_openedDT = DateTime.UtcNow;
	}
}
