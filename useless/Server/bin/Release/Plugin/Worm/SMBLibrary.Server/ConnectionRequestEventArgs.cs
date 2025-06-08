using System;
using System.Net;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class ConnectionRequestEventArgs : EventArgs
{
	public IPEndPoint IPEndPoint;

	public bool Accept = true;

	public ConnectionRequestEventArgs(IPEndPoint ipEndPoint)
	{
		IPEndPoint = ipEndPoint;
	}
}
