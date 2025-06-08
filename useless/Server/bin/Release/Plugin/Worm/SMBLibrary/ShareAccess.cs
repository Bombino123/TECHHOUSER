using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum ShareAccess : uint
{
	None = 0u,
	Read = 1u,
	Write = 2u,
	Delete = 4u
}
