using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2QueryPathInformationResponse : Transaction2Subcommand
{
	public const int ParametersLength = 2;

	public ushort EaErrorOffset;

	public byte[] InformationBytes;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_QUERY_PATH_INFORMATION;

	public Transaction2QueryPathInformationResponse()
	{
	}

	public Transaction2QueryPathInformationResponse(byte[] parameters, byte[] data, bool isUnicode)
	{
		EaErrorOffset = LittleEndianConverter.ToUInt16(parameters, 0);
		InformationBytes = data;
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		return LittleEndianConverter.GetBytes(EaErrorOffset);
	}

	public override byte[] GetData(bool isUnicode)
	{
		return InformationBytes;
	}

	public QueryInformation GetQueryInformation(QueryInformationLevel queryInformationLevel)
	{
		return QueryInformation.GetQueryInformation(InformationBytes, queryInformationLevel);
	}

	public void SetQueryInformation(QueryInformation queryInformation)
	{
		InformationBytes = queryInformation.GetBytes();
	}

	public FileInformation GetFileInformation(FileInformationClass informationClass)
	{
		return FileInformation.GetFileInformation(InformationBytes, 0, informationClass);
	}

	public void SetFileInformation(FileInformation information)
	{
		InformationBytes = information.GetBytes();
	}
}
