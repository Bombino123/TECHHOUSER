using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class SessionInformation
{
	public IPEndPoint ClientEndPoint;

	public SMBDialect Dialect;

	public string UserName;

	public string MachineName;

	public List<OpenFileInformation> OpenFiles;

	public DateTime CreationDT;

	public SessionInformation(IPEndPoint clientEndPoint, SMBDialect dialect, string userName, string machineName, List<OpenFileInformation> openFiles, DateTime creationDT)
	{
		ClientEndPoint = clientEndPoint;
		Dialect = dialect;
		UserName = userName;
		MachineName = machineName;
		OpenFiles = openFiles;
		CreationDT = creationDT;
	}
}
