using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class SMBShareCollection : List<FileSystemShare>
{
	public bool Contains(string shareName, StringComparison comparisonType)
	{
		return IndexOf(shareName, comparisonType) != -1;
	}

	public int IndexOf(string shareName, StringComparison comparisonType)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].Name.Equals(shareName, comparisonType))
			{
				return i;
			}
		}
		return -1;
	}

	public List<string> ListShares()
	{
		List<string> list = new List<string>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			FileSystemShare current = enumerator.Current;
			list.Add(current.Name);
		}
		return list;
	}

	public FileSystemShare GetShareFromName(string shareName)
	{
		int num = IndexOf(shareName, StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			return base[num];
		}
		return null;
	}
}
