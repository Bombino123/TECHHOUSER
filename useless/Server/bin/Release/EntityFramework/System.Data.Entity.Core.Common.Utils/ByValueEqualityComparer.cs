using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils;

internal sealed class ByValueEqualityComparer : IEqualityComparer<object>
{
	internal static readonly ByValueEqualityComparer Default = new ByValueEqualityComparer();

	private ByValueEqualityComparer()
	{
	}

	public new bool Equals(object x, object y)
	{
		if (object.Equals(x, y))
		{
			return true;
		}
		byte[] array = x as byte[];
		byte[] array2 = y as byte[];
		if (array != null && array2 != null)
		{
			return CompareBinaryValues(array, array2);
		}
		return false;
	}

	public int GetHashCode(object obj)
	{
		if (obj != null)
		{
			if (obj is byte[] bytes)
			{
				return ComputeBinaryHashCode(bytes);
			}
			return obj.GetHashCode();
		}
		return 0;
	}

	internal static int ComputeBinaryHashCode(byte[] bytes)
	{
		int num = 0;
		int i = 0;
		for (int num2 = Math.Min(bytes.Length, 7); i < num2; i++)
		{
			num = (num << 5) ^ bytes[i];
		}
		return num;
	}

	internal static bool CompareBinaryValues(byte[] first, byte[] second)
	{
		if (first.Length != second.Length)
		{
			return false;
		}
		for (int i = 0; i < first.Length; i++)
		{
			if (first[i] != second[i])
			{
				return false;
			}
		}
		return true;
	}
}
