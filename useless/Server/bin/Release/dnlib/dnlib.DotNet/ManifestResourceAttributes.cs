using System;

namespace dnlib.DotNet;

[Flags]
public enum ManifestResourceAttributes : uint
{
	VisibilityMask = 7u,
	Public = 1u,
	Private = 2u
}
