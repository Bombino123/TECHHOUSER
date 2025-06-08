using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2GetDfsReferralRequest : Transaction2Subcommand
{
	public RequestGetDfsReferral ReferralRequest;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_GET_DFS_REFERRAL;

	public Transaction2GetDfsReferralRequest()
	{
	}

	public Transaction2GetDfsReferralRequest(byte[] parameters, byte[] data)
	{
		ReferralRequest = new RequestGetDfsReferral(parameters);
	}

	public override byte[] GetSetup()
	{
		return LittleEndianConverter.GetBytes((ushort)SubcommandName);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		return ReferralRequest.GetBytes();
	}
}
