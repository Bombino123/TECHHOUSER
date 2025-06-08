using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactSetSecurityDescriptorRequest : NTTransactSubcommand
{
	public const int ParametersLength = 8;

	public ushort FID;

	public ushort Reserved;

	public SecurityInformation SecurityInformation;

	public SecurityDescriptor SecurityDescriptor;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_SET_SECURITY_DESC;

	public NTTransactSetSecurityDescriptorRequest()
	{
	}

	public NTTransactSetSecurityDescriptorRequest(byte[] parameters, byte[] data)
	{
		FID = LittleEndianConverter.ToUInt16(parameters, 0);
		Reserved = LittleEndianConverter.ToUInt16(parameters, 2);
		SecurityInformation = (SecurityInformation)LittleEndianConverter.ToUInt32(parameters, 4);
		SecurityDescriptor = new SecurityDescriptor(data, 0);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[8];
		LittleEndianWriter.WriteUInt16(array, 0, FID);
		LittleEndianWriter.WriteUInt16(array, 2, Reserved);
		LittleEndianWriter.WriteUInt32(array, 4, (uint)SecurityInformation);
		return array;
	}

	public override byte[] GetData()
	{
		return SecurityDescriptor.GetBytes();
	}
}
