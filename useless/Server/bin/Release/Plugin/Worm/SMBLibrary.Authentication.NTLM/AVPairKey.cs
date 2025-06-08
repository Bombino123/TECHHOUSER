using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public enum AVPairKey : ushort
{
	EOL = 0,
	NbComputerName = 1,
	NbDomainName = 2,
	DnsComputerName = 3,
	DnsDomainName = 4,
	DnsTreeName = 5,
	Flags = 6,
	Timestamp = 6,
	SingleHost = 8,
	TargetName = 9,
	ChannelBindings = 10
}
