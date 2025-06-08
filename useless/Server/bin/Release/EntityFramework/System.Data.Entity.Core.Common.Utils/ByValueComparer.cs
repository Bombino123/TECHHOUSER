using System.Collections;
using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils;

internal class ByValueComparer : IComparer
{
	internal static readonly IComparer Default = new ByValueComparer(Comparer<object>.Default);

	private readonly IComparer nonByValueComparer;

	private ByValueComparer(IComparer comparer)
	{
		nonByValueComparer = comparer;
	}

	int IComparer.Compare(object x, object y)
	{
		if (x == y)
		{
			return 0;
		}
		if (x == DBNull.Value)
		{
			x = null;
		}
		if (y == DBNull.Value)
		{
			y = null;
		}
		if (x != null && y != null)
		{
			byte[] array = x as byte[];
			byte[] array2 = y as byte[];
			if (array != null && array2 != null)
			{
				int num = array.Length - array2.Length;
				if (num == 0)
				{
					int num2 = 0;
					while (num == 0 && num2 < array.Length)
					{
						byte b = array[num2];
						byte b2 = array2[num2];
						if (b != b2)
						{
							num = b - b2;
						}
						num2++;
					}
				}
				return num;
			}
		}
		return nonByValueComparer.Compare(x, y);
	}
}
