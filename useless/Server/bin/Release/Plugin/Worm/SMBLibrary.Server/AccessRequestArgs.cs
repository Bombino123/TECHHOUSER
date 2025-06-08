using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class AccessRequestArgs : EventArgs
{
	public string UserName;

	public string Path;

	public FileAccess RequestedAccess;

	public string MachineName;

	public IPEndPoint ClientEndPoint;

	public bool Allow = true;

	public AccessRequestArgs(string userName, string path, FileAccess requestedAccess, string machineName, IPEndPoint clientEndPoint)
	{
		UserName = userName;
		Path = path;
		RequestedAccess = requestedAccess;
		MachineName = machineName;
		ClientEndPoint = clientEndPoint;
	}
}
