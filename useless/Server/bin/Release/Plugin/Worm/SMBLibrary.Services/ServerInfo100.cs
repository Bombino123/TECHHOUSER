using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ServerInfo100 : ServerInfoLevel
{
	public PlatformName PlatformID;

	public NDRUnicodeString ServerName;

	public override uint Level => 100u;

	public ServerInfo100()
	{
		ServerName = new NDRUnicodeString();
	}

	public ServerInfo100(NDRParser parser)
	{
		Read(parser);
	}

	public override void Read(NDRParser parser)
	{
		parser.BeginStructure();
		PlatformID = (PlatformName)parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref ServerName);
		parser.EndStructure();
	}

	public override void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteUInt32((uint)PlatformID);
		writer.WriteEmbeddedStructureFullPointer(ServerName);
		writer.EndStructure();
	}
}
