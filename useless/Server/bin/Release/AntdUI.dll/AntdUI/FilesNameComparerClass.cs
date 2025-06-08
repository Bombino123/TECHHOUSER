namespace AntdUI;

internal class FilesNameComparerClass
{
	public static int Compare(string x, string y)
	{
		if (int.TryParse(x, out var result) && int.TryParse(y, out var result2))
		{
			if (result == result2)
			{
				return 0;
			}
			if (result > result2)
			{
				return 1;
			}
			return -1;
		}
		char[] array = x.ToCharArray();
		char[] array2 = y.ToCharArray();
		int i = 0;
		int j = 0;
		while (i < array.Length && j < array2.Length)
		{
			if (char.IsDigit(array[i]) && char.IsDigit(array2[j]))
			{
				string text = "";
				string text2 = "";
				for (; i < array.Length && char.IsDigit(array[i]); i++)
				{
					text += array[i];
				}
				for (; j < array2.Length && char.IsDigit(array2[j]); j++)
				{
					text2 += array2[j];
				}
				if (int.TryParse(text, out var result3) && int.TryParse(text2, out var result4))
				{
					if (result3 > result4)
					{
						return 1;
					}
					if (result3 < result4)
					{
						return -1;
					}
				}
			}
			else
			{
				if (array[i] > array2[j])
				{
					return 1;
				}
				if (array[i] < array2[j])
				{
					return -1;
				}
				i++;
				j++;
			}
		}
		if (array.Length == array2.Length)
		{
			return 0;
		}
		if (array.Length <= array2.Length)
		{
			return -1;
		}
		return 1;
	}
}
