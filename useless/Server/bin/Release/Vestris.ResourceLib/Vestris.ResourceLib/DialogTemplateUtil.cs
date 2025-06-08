using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

internal abstract class DialogTemplateUtil
{
	internal static IntPtr ReadResourceId(IntPtr lpRes, out ResourceId rc)
	{
		rc = null;
		switch ((ushort)Marshal.ReadInt16(lpRes))
		{
		case 0:
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
			break;
		case ushort.MaxValue:
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
			rc = new ResourceId((ushort)Marshal.ReadInt16(lpRes));
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
			break;
		default:
			rc = new ResourceId(Marshal.PtrToStringUni(lpRes));
			lpRes = new IntPtr(lpRes.ToInt64() + (rc.Name.Length + 1) * Marshal.SystemDefaultCharSize);
			break;
		}
		return lpRes;
	}

	internal static void WriteResourceId(BinaryWriter w, ResourceId rc)
	{
		if (rc == null)
		{
			w.Write((ushort)0);
		}
		else if (rc.IsIntResource())
		{
			w.Write(ushort.MaxValue);
			w.Write((ushort)(int)rc.Id);
		}
		else
		{
			ResourceUtil.PadToWORD(w);
			w.Write(Encoding.Unicode.GetBytes(rc.Name));
			w.Write((ushort)0);
		}
	}

	internal static string StyleToString<W, D>(uint style)
	{
		List<string> list = new List<string>();
		list.AddRange(ResourceUtil.FlagsToList<W>(style));
		list.AddRange(ResourceUtil.FlagsToList<D>(style));
		return string.Join(" | ", list.ToArray());
	}

	internal static string StyleToString<W, D>(uint style, uint exstyle)
	{
		List<string> list = new List<string>();
		list.AddRange(ResourceUtil.FlagsToList<W>(style));
		list.AddRange(ResourceUtil.FlagsToList<D>(exstyle));
		return string.Join(" | ", list.ToArray());
	}

	internal static string StyleToString<W>(uint style)
	{
		List<string> list = new List<string>();
		list.AddRange(ResourceUtil.FlagsToList<W>(style));
		return string.Join(" | ", list.ToArray());
	}
}
