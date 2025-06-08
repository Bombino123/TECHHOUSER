using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrServerGetInfoResponse
{
	public ServerInfo InfoStruct;

	public Win32Error Result;

	public NetrServerGetInfoResponse()
	{
	}

	public NetrServerGetInfoResponse(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		InfoStruct = new ServerInfo(nDRParser);
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
