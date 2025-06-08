using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[Flags]
[ComVisible(true)]
public enum MDStreamFlags : byte
{
	BigStrings = 1,
	BigGUID = 2,
	BigBlob = 4,
	Padding = 8,
	DeltaOnly = 0x20,
	ExtraData = 0x40,
	HasDelete = 0x80
}
