using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum ServiceName
{
	DiskShare,
	PrinterShare,
	NamedPipe,
	SerialCommunicationsDevice,
	AnyType
}
