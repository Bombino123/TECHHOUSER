using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class MenuTemplateItemCollection : List<MenuTemplateItem>
{
	internal IntPtr Read(IntPtr lpRes)
	{
		User32.MENUITEMTEMPLATE obj;
		do
		{
			obj = (User32.MENUITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUITEMTEMPLATE));
			MenuTemplateItem menuTemplateItem = null;
			menuTemplateItem = (((obj.mtOption & 0x10) == 0) ? ((MenuTemplateItem)new MenuTemplateItemCommand()) : ((MenuTemplateItem)new MenuTemplateItemPopup()));
			lpRes = menuTemplateItem.Read(lpRes);
			Add(menuTemplateItem);
		}
		while ((obj.mtOption & 0x80) == 0);
		return lpRes;
	}

	internal void Write(BinaryWriter w)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Write(w);
		}
	}

	public override string ToString()
	{
		return ToString(0);
	}

	public string ToString(int indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.Count > 0)
		{
			stringBuilder.AppendLine($"{new string(' ', indent)}BEGIN");
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MenuTemplateItem current = enumerator.Current;
					stringBuilder.Append(current.ToString(indent + 1));
				}
			}
			stringBuilder.AppendLine($"{new string(' ', indent)}END");
		}
		return stringBuilder.ToString();
	}
}
