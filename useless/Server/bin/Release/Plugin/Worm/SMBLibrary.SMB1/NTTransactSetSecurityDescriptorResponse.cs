using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactSetSecurityDescriptorResponse : NTTransactSubcommand
{
	public const int ParametersLength = 0;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_SET_SECURITY_DESC;
}
