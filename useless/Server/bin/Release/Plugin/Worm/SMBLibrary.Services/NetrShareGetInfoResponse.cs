using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrShareGetInfoResponse
{
	public ShareInfo InfoStruct;

	public Win32Error Result;

	public NetrShareGetInfoResponse()
	{
	}

	public NetrShareGetInfoResponse(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		InfoStruct = new ShareInfo(nDRParser);
		Result = (Win32Error)nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteStructure(InfoStruct);
		nDRWriter.WriteUInt32((uint)Result);
		return nDRWriter.GetBytes();
	}
}
