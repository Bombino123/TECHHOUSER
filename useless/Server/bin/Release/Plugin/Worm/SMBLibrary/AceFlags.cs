using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum AceFlags : byte
{
	OBJECT_INHERIT_ACE = 1,
	CONTAINER_INHERIT_ACE = 2,
	NO_PROPAGATE_INHERIT_ACE = 4,
	INHERIT_ONLY_ACE = 8,
	INHERITED_ACE = 0x10,
	SUCCESSFUL_ACCESS_ACE_FLAG = 0x40,
	FAILED_ACCESS_ACE_FLAG = 0x80
}
