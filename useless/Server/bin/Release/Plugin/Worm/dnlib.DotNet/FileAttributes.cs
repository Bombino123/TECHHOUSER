using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum FileAttributes : uint
{
	ContainsMetadata = 0u,
	ContainsNoMetadata = 1u
}
