using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[Flags]
[ComVisible(true)]
public enum StorageFlags : byte
{
	Normal = 0,
	ExtraData = 1
}
