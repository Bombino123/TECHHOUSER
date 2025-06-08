using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactQuerySecurityDescriptorResponse : NTTransactSubcommand
{
	public const int ParametersLength = 4;

	public uint LengthNeeded;

	public SecurityDescriptor SecurityDescriptor;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_QUERY_SECURITY_DESC;

	public NTTransactQuerySecurityDescriptorResponse()
	{
	}

	public NTTransactQuerySecurityDescriptorResponse(byte[] parameters, byte[] data)
	{
		LengthNeeded = LittleEndianConverter.ToUInt32(parameters, 0);
		if (data.Length == LengthNeeded)
		{
			SecurityDescriptor = new SecurityDescriptor(data, 0);
		}
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[4];
		LittleEndianWriter.WriteUInt32(array, 0, LengthNeeded);
		return array;
	}

	public override byte[] GetData()
	{
		if (SecurityDescriptor != null)
		{
			return SecurityDescriptor.GetBytes();
		}
		return new byte[0];
	}
}
