using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class Transaction2Subcommand
{
	public abstract Transaction2SubcommandName SubcommandName { get; }

	public Transaction2Subcommand()
	{
	}

	public virtual byte[] GetSetup()
	{
		return new byte[0];
	}

	public virtual byte[] GetParameters(bool isUnicode)
	{
		return new byte[0];
	}

	public virtual byte[] GetData(bool isUnicode)
	{
		return new byte[0];
	}

	public static Transaction2Subcommand GetSubcommandRequest(byte[] setup, byte[] parameters, byte[] data, bool isUnicode)
	{
		if (setup.Length == 2)
		{
			switch ((Transaction2SubcommandName)LittleEndianConverter.ToUInt16(setup, 0))
			{
			case Transaction2SubcommandName.TRANS2_OPEN2:
				return new Transaction2Open2Request(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_FIND_FIRST2:
				return new Transaction2FindFirst2Request(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_FIND_NEXT2:
				return new Transaction2FindNext2Request(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_QUERY_FS_INFORMATION:
				return new Transaction2QueryFSInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_SET_FS_INFORMATION:
				return new Transaction2SetFSInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_QUERY_PATH_INFORMATION:
				return new Transaction2QueryPathInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_SET_PATH_INFORMATION:
				return new Transaction2SetPathInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_QUERY_FILE_INFORMATION:
				return new Transaction2QueryFileInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_SET_FILE_INFORMATION:
				return new Transaction2SetFileInformationRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_CREATE_DIRECTORY:
				return new Transaction2CreateDirectoryRequest(parameters, data, isUnicode);
			case Transaction2SubcommandName.TRANS2_GET_DFS_REFERRAL:
				return new Transaction2GetDfsReferralRequest(parameters, data);
			}
		}
		throw new InvalidDataException();
	}
}
