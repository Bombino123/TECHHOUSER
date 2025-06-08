using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public enum NamedPipeType : byte
{
	ByteModePipe,
	MessageModePipe
}
