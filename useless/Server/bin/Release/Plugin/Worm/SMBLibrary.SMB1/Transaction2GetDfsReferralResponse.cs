using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class Transaction2GetDfsReferralResponse : Transaction2Subcommand
{
	public const int ParametersLength = 0;

	public ResponseGetDfsReferral ReferralResponse;

	public override Transaction2SubcommandName SubcommandName => Transaction2SubcommandName.TRANS2_GET_DFS_REFERRAL;

	public Transaction2GetDfsReferralResponse()
	{
	}

	public Transaction2GetDfsReferralResponse(byte[] parameters, byte[] data)
	{
		ReferralResponse = new ResponseGetDfsReferral(data);
	}

	public override byte[] GetData(bool isUnicode)
	{
		return ReferralResponse.GetBytes();
	}
}
