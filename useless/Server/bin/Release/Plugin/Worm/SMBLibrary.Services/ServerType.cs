using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[Flags]
[ComVisible(true)]
public enum ServerType : uint
{
	Workstation = 1u,
	Server = 2u,
	SqlServer = 4u,
	DomainController = 8u,
	BackupDomainController = 0x10u,
	NetworkTimeSource = 0x20u,
	AppleFileProtocolServer = 0x40u,
	NovellServer = 0x80u,
	DomainMember = 0x100u,
	PrintQueueServer = 0x200u,
	DialInServer = 0x400u,
	XenixServer = 0x800u,
	WindowsNT = 0x1000u,
	WindowsForWorkgroupServer = 0x2000u,
	FileAndPrintForNetware = 0x4000u,
	ServerNT = 0x8000u,
	PotentialBrowser = 0x10000u,
	BackupBrowser = 0x20000u,
	MasterBrowser = 0x40000u,
	DomainMaster = 0x80000u,
	Windows = 0x400000u,
	DfsServer = 0x800000u,
	TerminalServer = 0x2000000u,
	ClusterVirtualServer = 0x4000000u,
	NTCluster = 0x10000000u,
	LocalListOnly = 0x40000000u,
	PrimaryDomain = 0x80000000u,
	All = uint.MaxValue
}
