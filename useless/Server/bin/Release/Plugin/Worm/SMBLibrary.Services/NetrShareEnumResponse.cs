using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrShareEnumResponse
{
	public ShareEnum InfoStruct;

	public uint TotalEntries;

	public uint ResumeHandle;

	public Win32Error Result;

	public NetrShareEnumResponse()
	{
	}

	public NetrShareEnumResponse(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		InfoStruct = new ShareEnum(nDRParser);
		TotalEntries = nDRParser.ReadUInt32();
		ResumeHandle = nDRParser.ReadUInt32();
		Result = (Win32Error)nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteStructure(InfoStruct);
		nDRWriter.WriteUInt32(TotalEntries);
		nDRWriter.WriteUInt32(ResumeHandle);
		nDRWriter.WriteUInt32((uint)Result);
		return nDRWriter.GetBytes();
	}
}
