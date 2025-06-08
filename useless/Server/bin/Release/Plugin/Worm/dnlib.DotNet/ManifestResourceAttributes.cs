using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[Flags]
[ComVisible(true)]
public enum ManifestResourceAttributes : uint
{
	VisibilityMask = 7u,
	Public = 1u,
	Private = 2u
}
