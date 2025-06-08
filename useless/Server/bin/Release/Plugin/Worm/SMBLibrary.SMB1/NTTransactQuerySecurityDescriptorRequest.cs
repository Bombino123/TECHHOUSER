using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactQuerySecurityDescriptorRequest : NTTransactSubcommand
{
	public const int ParametersLength = 8;

	public ushort FID;

	public ushort Reserved;

	public SecurityInformation SecurityInfoFields;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_QUERY_SECURITY_DESC;

	public NTTransactQuerySecurityDescriptorRequest()
	{
	}

	public NTTransactQuerySecurityDescriptorRequest(byte[] parameters)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		Reserved = LittleEndianConverter.ToUInt16(parameters, 2);
		SecurityInfoFields = (SecurityInformation)LittleEndianConverter.ToUInt32(parameters, 4);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, Reserved);
		LittleEndianWriter.WriteUInt32(array, 4, (uint)SecurityInfoFields);
		return array;
	}
}
