using System;
using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public class ZipErrorEventArgs : ZipProgressEventArgs
{
	private Exception _exc;

	public Exception Exception => _exc;

	public string FileName => base.CurrentEntry.LocalFileName;

	private ZipErrorEventArgs()
	{
	}

	internal static ZipErrorEventArgs Saving(string archiveName, ZipEntry entry, Exception exception)
	{
		ZipErrorEventArgs zipErrorEventArgs = new ZipErrorEventArgs();
		zipErrorEventArgs.EventType = ZipProgressEventType.Error_Saving;
		zipErrorEventArgs.ArchiveName = archiveName;
		zipErrorEventArgs.CurrentEntry = entry;
		zipErrorEventArgs._exc = exception;
		return zipErrorEventArgs;
	}
}
