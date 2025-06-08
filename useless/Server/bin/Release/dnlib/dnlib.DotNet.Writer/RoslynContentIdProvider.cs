using System;

namespace dnlib.DotNet.Writer;

internal static class RoslynContentIdProvider
{
	public static void GetContentId(byte[] hash, out Guid guid, out uint timestamp)
	{
		if (hash.Length < 20)
		{
			throw new InvalidOperationException();
		}
		byte[] array = new byte[16];
		Array.Copy(hash, 0, array, 0, array.Length);
		array[7] = (byte)((array[7] & 0xFu) | 0x40u);
		array[8] = (byte)((array[8] & 0x3Fu) | 0x80u);
		guid = new Guid(array);
		timestamp = 0x80000000u | (uint)((hash[19] << 24) | (hash[18] << 16) | (hash[17] << 8) | hash[16]);
	}
}
