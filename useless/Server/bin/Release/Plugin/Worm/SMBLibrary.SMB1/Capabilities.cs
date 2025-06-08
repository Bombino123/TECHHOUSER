using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum Capabilities : uint
{
	RawMode = 1u,
	MPXMode = 2u,
	Unicode = 4u,
	LargeFiles = 8u,
	NTSMB = 0x10u,
	RpcRemoteApi = 0x20u,
	NTStatusCode = 0x40u,
	Level2Oplocks = 0x80u,
	LockAndRead = 0x100u,
	NTFind = 0x200u,
	DFS = 0x1000u,
	InfoLevelPassthrough = 0x2000u,
	LargeRead = 0x4000u,
	LargeWrite = 0x8000u,
	LightWeightIO = 0x10000u,
	Unix = 0x800000u,
	DynamicReauthentication = 0x20000000u,
	ExtendedSecurity = 0x80000000u
}
