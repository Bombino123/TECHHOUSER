using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactNotifyChangeResponse : NTTransactSubcommand
{
	public byte[] FileNotifyInformationBytes;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_NOTIFY_CHANGE;

	public NTTransactNotifyChangeResponse()
	{
	}

	public NTTransactNotifyChangeResponse(byte[] parameters)
	{
		FileNotifyInformationBytes = parameters;
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		return FileNotifyInformationBytes;
	}

	public List<FileNotifyInformation> GetFileNotifyInformation()
	{
		return FileNotifyInformation.ReadList(FileNotifyInformationBytes, 0);
	}

	public void SetFileNotifyInformation(List<FileNotifyInformation> notifyInformationList)
	{
		FileNotifyInformationBytes = FileNotifyInformation.GetBytes(notifyInformationList);
	}
}
