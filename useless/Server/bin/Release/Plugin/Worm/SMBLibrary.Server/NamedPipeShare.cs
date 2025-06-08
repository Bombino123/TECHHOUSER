using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.Services;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class NamedPipeShare : ISMBShare
{
	public const string NamedPipeShareName = "IPC$";

	private NamedPipeStore m_store;

	public string Name => "IPC$";

	public INTFileStore FileStore => m_store;

	public NamedPipeShare(List<string> shareList)
	{
		m_store = new NamedPipeStore(new List<RemoteService>
		{
			new ServerService(Environment.MachineName, shareList),
			new WorkstationService(Environment.MachineName, Environment.MachineName)
		});
	}
}
