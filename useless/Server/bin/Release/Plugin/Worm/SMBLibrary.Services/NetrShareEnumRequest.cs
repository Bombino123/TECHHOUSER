using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrShareEnumRequest
{
	public string ServerName;

	public ShareEnum InfoStruct;

	public uint PreferedMaximumLength;

	public uint ResumeHandle;

	public NetrShareEnumRequest()
	{
	}

	public NetrShareEnumRequest(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		ServerName = nDRParser.ReadTopLevelUnicodeStringPointer();
		InfoStruct = new ShareEnum(nDRParser);
		PreferedMaximumLength = nDRParser.ReadUInt32();
		ResumeHandle = nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteTopLevelUnicodeStringPointer(ServerName);
		nDRWriter.WriteStructure(InfoStruct);
		nDRWriter.WriteUInt32(PreferedMaximumLength);
		nDRWriter.WriteUInt32(ResumeHandle);
		return nDRWriter.GetBytes();
	}
}
