using System;

namespace dnlib.DotNet.MD;

[Flags]
public enum StorageFlags : byte
{
	Normal = 0,
	ExtraData = 1
}
