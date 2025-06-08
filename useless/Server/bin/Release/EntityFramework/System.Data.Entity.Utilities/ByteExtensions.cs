using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Utilities;

internal static class ByteExtensions
{
	public static string ToHexString(this IEnumerable<byte> bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte @byte in bytes)
		{
			stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}
}
