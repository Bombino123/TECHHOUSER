using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindClose2Request : SMB1Command
{
	public const int ParameterCount = 2;

	public ushort SearchHandle;

	public override CommandName CommandName => CommandName.SMB_COM_FIND_CLOSE2;

	public FindClose2Request()
	{
	}

	public FindClose2Request(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		SearchHandle = LittleEndianConverter.ToUInt16(SMBParameters, 0);
	}
}
