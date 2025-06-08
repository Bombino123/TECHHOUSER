using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2SetFSInformationResponse : Transaction2Subcommand
{
	public const int ParametersLength = 0;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_SET_FS_INFORMATION;
}
