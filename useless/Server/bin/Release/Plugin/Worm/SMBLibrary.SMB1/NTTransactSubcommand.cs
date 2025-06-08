using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class NTTransactSubcommand
{
	public abstract NTTransactSubcommandName SubcommandName { get; }

	public NTTransactSubcommand()
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

	public virtual byte[] GetData()
	{
		return new byte[0];
	}

	public static NTTransactSubcommand GetSubcommandRequest(NTTransactSubcommandName subcommandName, byte[] setup, byte[] parameters, byte[] data, bool isUnicode)
	{
		return subcommandName switch
		{
			NTTransactSubcommandName.NT_TRANSACT_CREATE => new NTTransactCreateRequest(parameters, data, isUnicode), 
			NTTransactSubcommandName.NT_TRANSACT_IOCTL => new NTTransactIOCTLRequest(setup, data), 
			NTTransactSubcommandName.NT_TRANSACT_SET_SECURITY_DESC => new NTTransactSetSecurityDescriptorRequest(parameters, data), 
			NTTransactSubcommandName.NT_TRANSACT_NOTIFY_CHANGE => new NTTransactNotifyChangeRequest(setup), 
			NTTransactSubcommandName.NT_TRANSACT_QUERY_SECURITY_DESC => new NTTransactQuerySecurityDescriptorRequest(parameters), 
			_ => throw new InvalidDataException(), 
		};
	}
}
