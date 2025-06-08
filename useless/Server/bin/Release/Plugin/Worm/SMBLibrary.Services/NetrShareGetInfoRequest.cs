using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class NetrShareGetInfoRequest
{
	public string ServerName;

	public string NetName;

	public uint Level;

	public NetrShareGetInfoRequest(byte[] buffer)
	{
		NDRParser nDRParser = new NDRParser(buffer);
		ServerName = nDRParser.ReadTopLevelUnicodeStringPointer();
		NetName = nDRParser.ReadUnicodeString();
		Level = nDRParser.ReadUInt32();
	}

	public byte[] GetBytes()
	{
		NDRWriter nDRWriter = new NDRWriter();
		nDRWriter.WriteTopLevelUnicodeStringPointer(ServerName);
		nDRWriter.WriteUnicodeString(NetName);
		nDRWriter.WriteUInt32(Level);
		return nDRWriter.GetBytes();
	}
}
