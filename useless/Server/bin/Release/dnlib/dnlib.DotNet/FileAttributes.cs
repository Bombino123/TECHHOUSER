using System;

namespace dnlib.DotNet;

[Flags]
public enum FileAttributes : uint
{
	ContainsMetadata = 0u,
	ContainsNoMetadata = 1u
}
