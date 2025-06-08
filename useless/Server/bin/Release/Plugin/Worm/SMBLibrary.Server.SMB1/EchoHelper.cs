using System.Collections.Generic;
using SMBLibrary.SMB1;

namespace SMBLibrary.Server.SMB1;

internal class EchoHelper
{
	internal static List<SMB1Command> GetEchoResponse(EchoRequest request)
	{
		List<SMB1Command> list = new List<SMB1Command>();
		for (int i = 0; i < request.EchoCount; i++)
		{
			EchoResponse echoResponse = new EchoResponse();
			echoResponse.SequenceNumber = (ushort)i;
			echoResponse.Data = request.Data;
			list.Add(echoResponse);
		}
		return list;
	}

	internal static SMB1Message GetUnsolicitedEchoReply()
	{
		SMB1Header sMB1Header = new SMB1Header();
		sMB1Header.Command = CommandName.SMB_COM_ECHO;
		sMB1Header.Status = NTStatus.STATUS_SUCCESS;
		sMB1Header.Flags = HeaderFlags.CaseInsensitive | HeaderFlags.CanonicalizedPaths | HeaderFlags.Reply;
		sMB1Header.Flags2 = HeaderFlags2.LongNamesAllowed | HeaderFlags2.NTStatusCode | HeaderFlags2.Unicode;
		sMB1Header.UID = ushort.MaxValue;
		sMB1Header.TID = ushort.MaxValue;
		sMB1Header.PID = uint.MaxValue;
		sMB1Header.MID = ushort.MaxValue;
		EchoResponse item = new EchoResponse();
		return new SMB1Message
		{
			Header = sMB1Header,
			Commands = { (SMB1Command)item }
		};
	}
}
