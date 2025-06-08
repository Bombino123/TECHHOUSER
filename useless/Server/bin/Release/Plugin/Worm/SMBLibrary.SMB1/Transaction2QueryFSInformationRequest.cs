using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2QueryFSInformationRequest : Transaction2Subcommand
{
	private const ushort SMB_INFO_PASSTHROUGH = 1000;

	public const int ParametersLength = 2;

	public ushort InformationLevel;

	public bool IsPassthroughInformationLevel => InformationLevel >= 1000;

	public QueryFSInformationLevel QueryFSInformationLevel
	{
		get
		{
			return (QueryFSInformationLevel)InformationLevel;
		}
		set
		{
			InformationLevel = (ushort)value;
		}
	}

	public FileSystemInformationClass FileSystemInformationClass
	{
		get
		{
			return (FileSystemInformationClass)(InformationLevel - 1000);
		}
		set
		{
			InformationLevel = (ushort)((uint)value + 1000u);
		}
	}

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_QUERY_FS_INFORMATION;

	public Transaction2QueryFSInformationRequest()
	{
	}

	public Transaction2QueryFSInformationRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		InformationLevel = LittleEndianConverter.ToUInt16(parameters, 0);
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		byte[] array = new byte[2];
		LittleEndianWriter.WriteUInt16(array, 0, InformationLevel);
		return array;
	}
}
