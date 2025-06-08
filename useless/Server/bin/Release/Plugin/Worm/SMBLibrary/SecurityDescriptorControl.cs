using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum SecurityDescriptorControl : ushort
{
	OwnerDefaulted = 1,
	GroupDefaulted = 2,
	DaclPresent = 4,
	DaclDefaulted = 8,
	SaclPresent = 0x10,
	SaclDefaulted = 0x20,
	DaclUntrusted = 0x40,
	ServerSecurity = 0x80,
	DaclAutoInheritedReq = 0x100,
	SaclAutoInheritedReq = 0x200,
	DaclAutoInherited = 0x400,
	SaclAutoInherited = 0x800,
	DaclProtected = 0x1000,
	SaclProtected = 0x2000,
	RMControlValid = 0x4000,
	SelfRelative = 0x8000
}
