using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2QueryFSInformationResponse : Transaction2Subcommand
{
	public const int ParametersLength = 0;

	public byte[] InformationBytes;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_QUERY_FS_INFORMATION;

	public Transaction2QueryFSInformationResponse()
	{
	}

	public Transaction2QueryFSInformationResponse(byte[] parameters, byte[] data, bool isUnicode)
	{
		InformationBytes = data;
	}

	public override byte[] GetData(bool isUnicode)
	{
		return InformationBytes;
	}

	public QueryFSInformation GetQueryFSInformation(QueryFSInformationLevel informationLevel, bool isUnicode)
	{
		return QueryFSInformation.GetQueryFSInformation(InformationBytes, informationLevel, isUnicode);
	}

	public void SetQueryFSInformation(QueryFSInformation queryFSInformation, bool isUnicode)
	{
		InformationBytes = queryFSInformation.GetBytes(isUnicode);
	}

	public FileSystemInformation GetFileSystemInformation(FileSystemInformationClass informationClass)
	{
		return FileSystemInformation.GetFileSystemInformation(InformationBytes, 0, informationClass);
	}

	public void SetFileSystemInformation(FileSystemInformation information)
	{
		InformationBytes = information.GetBytes();
	}
}
