using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class FileHandle
{
	public string Path;

	public bool IsDirectory;

	public Stream Stream;

	public bool DeleteOnClose;

	public FileHandle(string path, bool isDirectory, Stream stream, bool deleteOnClose)
	{
		Path = path;
		IsDirectory = isDirectory;
		Stream = stream;
		DeleteOnClose = deleteOnClose;
	}
}
