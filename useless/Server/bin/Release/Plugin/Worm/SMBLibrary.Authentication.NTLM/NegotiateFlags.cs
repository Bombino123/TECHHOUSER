using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.NTLM;

[Flags]
[ComVisible(true)]
public enum NegotiateFlags : uint
{
	UnicodeEncoding = 1u,
	OEMEncoding = 2u,
	TargetNameSupplied = 4u,
	Sign = 0x10u,
	Seal = 0x20u,
	Datagram = 0x40u,
	LanManagerSessionKey = 0x80u,
	NTLMSessionSecurity = 0x200u,
	Anonymous = 0x800u,
	DomainNameSupplied = 0x1000u,
	WorkstationNameSupplied = 0x2000u,
	AlwaysSign = 0x8000u,
	TargetTypeDomain = 0x10000u,
	TargetTypeServer = 0x20000u,
	ExtendedSessionSecurity = 0x80000u,
	Identify = 0x100000u,
	RequestLMSessionKey = 0x400000u,
	TargetInfo = 0x800000u,
	Version = 0x2000000u,
	Use128BitEncryption = 0x20000000u,
	KeyExchange = 0x40000000u,
	Use56BitEncryption = 0x80000000u
}
