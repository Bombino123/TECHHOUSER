using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum AssemblyNameComparerFlags
{
	Name = 1,
	Version = 2,
	PublicKeyToken = 4,
	Culture = 8,
	ContentType = 0x10,
	All = 0x1F
}
