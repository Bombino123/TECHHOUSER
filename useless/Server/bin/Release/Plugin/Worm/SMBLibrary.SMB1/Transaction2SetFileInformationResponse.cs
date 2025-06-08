using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SetFileInformationResponse : Transaction2Subcommand
{
	public const int ParametersLength = 2;

	public ushort EaErrorOffset;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_SET_FILE_INFORMATION;

	public Transaction2SetFileInformationResponse()
	{
	}

	public Transaction2SetFileInformationResponse(byte[] parameters, byte[] data, bool isUnicode)
	{
		EaErrorOffset = LittleEndianConverter.ToUInt16(parameters, 0);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[2];
		LittleEndianWriter.WriteUInt16(array, 0, EaErrorOffset);
		return array;
	}
}
