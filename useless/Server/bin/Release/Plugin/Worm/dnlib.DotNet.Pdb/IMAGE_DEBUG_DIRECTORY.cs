using System.Runtime.InteropServices;
using dnlib.PE;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public struct IMAGE_DEBUG_DIRECTORY
{
	public uint Characteristics;

	public uint TimeDateStamp;

	public ushort MajorVersion;

	public ushort MinorVersion;

	public ImageDebugType Type;

	public uint SizeOfData;

	public uint AddressOfRawData;

	public uint PointerToRawData;
}
