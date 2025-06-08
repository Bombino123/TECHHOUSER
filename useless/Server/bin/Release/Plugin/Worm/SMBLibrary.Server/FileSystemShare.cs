using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class FileSystemShare : ISMBShare
{
	private string m_name;

	private INTFileStore m_fileSystem;

	private CachingPolicy m_cachingPolicy;

	public string Name => m_name;

	public INTFileStore FileStore => m_fileSystem;

	public CachingPolicy CachingPolicy => m_cachingPolicy;

	public event EventHandler<AccessRequestArgs> AccessRequested;

	public FileSystemShare(string shareName, INTFileStore fileSystem)
		: this(shareName, fileSystem, CachingPolicy.ManualCaching)
	{
	}

	public FileSystemShare(string shareName, INTFileStore fileSystem, CachingPolicy cachingPolicy)
	{
		m_name = shareName;
		m_fileSystem = fileSystem;
		m_cachingPolicy = cachingPolicy;
	}

	public bool HasReadAccess(SecurityContext securityContext, string path)
	{
		return HasAccess(securityContext, path, FileAccess.Read);
	}

	public bool HasWriteAccess(SecurityContext securityContext, string path)
	{
		return HasAccess(securityContext, path, FileAccess.Write);
	}

	public bool HasAccess(SecurityContext securityContext, string path, FileAccess requestedAccess)
	{
		EventHandler<AccessRequestArgs> accessRequested = this.AccessRequested;
		if (accessRequested != null)
		{
			AccessRequestArgs accessRequestArgs = new AccessRequestArgs(securityContext.UserName, path, requestedAccess, securityContext.MachineName, securityContext.ClientEndPoint);
			accessRequested(this, accessRequestArgs);
			return accessRequestArgs.Allow;
		}
		return true;
	}
}
