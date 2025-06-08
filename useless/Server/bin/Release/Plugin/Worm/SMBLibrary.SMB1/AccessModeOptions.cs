using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public struct AccessModeOptions
{
	public const int Length = 2;

	public AccessMode AccessMode;

	public SharingMode SharingMode;

	public ReferenceLocality ReferenceLocality;

	public CachedMode CachedMode;

	public WriteThroughMode WriteThroughMode;

	public AccessModeOptions(byte[] buffer, int offset)
	{
		AccessMode = (AccessMode)(buffer[offset] & 7u);
		SharingMode = (SharingMode)((buffer[offset] & 0x70) >> 4);
		ReferenceLocality = (ReferenceLocality)(buffer[offset + 1] & 7u);
		CachedMode = (CachedMode)((buffer[offset + 1] & 0x10) >> 4);
		WriteThroughMode = (WriteThroughMode)((buffer[offset + 1] & 0x40) >> 6);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = (byte)(AccessMode & (AccessMode)7);
		buffer[offset] |= (byte)(((uint)SharingMode << 4) & 0x70);
		buffer[offset + 1] = (byte)(ReferenceLocality & (ReferenceLocality)7);
		buffer[offset + 1] |= (byte)(((uint)CachedMode << 4) & 0x10);
		buffer[offset + 1] |= (byte)(((uint)WriteThroughMode << 6) & 0x40);
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += 2;
	}

	public static AccessModeOptions Read(byte[] buffer, ref int offset)
	{
		offset += 2;
		return new AccessModeOptions(buffer, offset - 2);
	}
}
