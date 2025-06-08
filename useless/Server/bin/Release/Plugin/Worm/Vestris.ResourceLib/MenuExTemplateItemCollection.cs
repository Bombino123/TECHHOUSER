using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class MenuExTemplateItemCollection : List<MenuExTemplateItem>
{
	internal IntPtr Read(IntPtr lpRes)
	{
		User32.MENUEXITEMTEMPLATE obj;
		do
		{
			lpRes = ResourceUtil.Align(lpRes.ToInt64());
			obj = (User32.MENUEXITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUEXITEMTEMPLATE));
			MenuExTemplateItem menuExTemplateItem = null;
			menuExTemplateItem = (((obj.bResInfo & 1) == 0) ? ((MenuExTemplateItem)new MenuExTemplateItemCommand()) : ((MenuExTemplateItem)new MenuExTemplateItemPopup()));
			lpRes = menuExTemplateItem.Read(lpRes);
			Add(menuExTemplateItem);
		}
		while ((obj.bResInfo & 0x80) == 0);
		return lpRes;
	}

	internal void Write(BinaryWriter w)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			MenuExTemplateItem current = enumerator.Current;
			ResourceUtil.PadToDWORD(w);
			current.Write(w);
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
					MenuExTemplateItem current = enumerator.Current;
					stringBuilder.Append(current.ToString(indent + 1));
				}
			}
			stringBuilder.AppendLine($"{new string(' ', indent)}END");
		}
		return stringBuilder.ToString();
	}
}
