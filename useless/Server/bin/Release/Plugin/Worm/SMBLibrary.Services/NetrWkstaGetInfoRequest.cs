using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrWkstaGetInfoRequest
{
	public string ServerName;

	public uint Level;

	public NetrWkstaGetInfoRequest()
	{
	}

	public NetrWkstaGetInfoRequest(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		ServerName = nDRParser.ReadTopLevelUnicodeStringPointer();
		Level = nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteTopLevelUnicodeStringPointer(ServerName);
		nDRWriter.WriteUInt32(Level);
		return nDRWriter.GetBytes();
	}
}
