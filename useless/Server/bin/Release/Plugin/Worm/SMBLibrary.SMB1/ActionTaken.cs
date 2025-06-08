using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public struct ActionTaken
{
	public OpenResult OpenResult;

	public LockStatus LockStatus;

	public ActionTaken(byte[] buffer, int offset)
	{
		OpenResult = (OpenResult)(buffer[offset] & 3u);
		LockStatus = (LockStatus)(buffer[offset + 1] >> 7);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = (byte)(OpenResult & OpenResult.FileExistedAndWasTruncated);
		buffer[offset + 1] = (byte)((uint)LockStatus << 7);
	}
}
