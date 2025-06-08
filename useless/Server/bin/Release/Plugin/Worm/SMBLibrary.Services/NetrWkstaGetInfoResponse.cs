using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrWkstaGetInfoResponse
{
	public WorkstationInfo WkstaInfo;

	public Win32Error Result;

	public NetrWkstaGetInfoResponse()
	{
	}

	public NetrWkstaGetInfoResponse(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		WkstaInfo = new WorkstationInfo(nDRParser);
		Result = (Win32Error)nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteStructure(WkstaInfo);
		nDRWriter.WriteUInt32((uint)Result);
		return nDRWriter.GetBytes();
	}
}
