using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum SMBTransportType
{
	NetBiosOverTCP,
	DirectTCPTransport
}
