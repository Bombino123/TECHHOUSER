using SMBLibrary.SMB2;

namespace SMBLibrary.Server.SMB2;

internal class EchoHelper
{
	internal static EchoResponse GetUnsolicitedEchoResponse()
	{
		return new EchoResponse
		{
			Header = 
			{
				MessageID = ulong.MaxValue
			}
		};
	}
}
