using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public struct OpenResults
{
	public const int Length = 2;

	public OpenResult OpenResult;

	public bool OpLockGranted;

	public OpenResults(byte[] buffer, int offset)
	{
		OpenResult = (OpenResult)(buffer[offset] & 3u);
		OpLockGranted = (buffer[offset + 1] & 0x80) > 0;
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = (byte)OpenResult;
		if (OpLockGranted)
		{
			buffer[offset + 1] = 128;
		}
		else
		{
			buffer[offset + 1] = 0;
		}
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += 2;
	}

	public static OpenResults Read(byte[] buffer, ref int offset)
	{
		offset += 2;
		return new OpenResults(buffer, offset - 2);
	}
}
