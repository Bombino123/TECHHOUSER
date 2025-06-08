using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public struct OpenMode
{
	public const int Length = 2;

	public FileExistsOpts FileExistsOpts;

	public CreateFile CreateFile;

	public OpenMode(byte[] buffer, int offset)
	{
		FileExistsOpts = (FileExistsOpts)(buffer[offset] & 3u);
		CreateFile = (CreateFile)((buffer[offset] & 0x10) >> 4);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = (byte)FileExistsOpts;
		buffer[offset] |= (byte)((uint)CreateFile << 4);
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += 2;
	}

	public static OpenMode Read(byte[] buffer, ref int offset)
	{
		offset += 2;
		return new OpenMode(buffer, offset - 2);
	}
}
