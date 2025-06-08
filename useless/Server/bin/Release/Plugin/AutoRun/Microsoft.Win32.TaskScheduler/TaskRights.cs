using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[Flags]
[ComVisible(true)]
public enum TaskRights
{
	FullControl = 0x1F01FF,
	Write = 0x120116,
	Read = 0x120089,
	Execute = 0x120089,
	Synchronize = 0x100000,
	TakeOwnership = 0x80000,
	ChangePermissions = 0x40000,
	ReadPermissions = 0x20000,
	Delete = 0x10000,
	WriteAttributes = 0x100,
	ReadAttributes = 0x80,
	DeleteChild = 0x40,
	ExecuteFile = 0x20,
	WriteExtendedAttributes = 0x10,
	ReadExtendedAttributes = 8,
	AppendData = 4,
	WriteData = 2,
	ReadData = 1
}
