namespace dnlib.IO;

public static class IOExtensions
{
	public static FileOffset AlignUp(this FileOffset offset, uint alignment)
	{
		return (FileOffset)((uint)(offset + alignment - 1) & ~(alignment - 1));
	}

	public static FileOffset AlignUp(this FileOffset offset, int alignment)
	{
		return (FileOffset)(((long)offset + (long)alignment - 1) & ~(alignment - 1));
	}
}
