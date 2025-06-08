using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class OpenFileInformation
{
	public string ShareName;

	public string Path;

	public FileAccess FileAccess;

	public DateTime OpenedDT;

	public OpenFileInformation(string shareName, string path, FileAccess fileAccess, DateTime openedDT)
	{
		ShareName = shareName;
		Path = path;
		FileAccess = fileAccess;
		OpenedDT = openedDT;
	}
}
