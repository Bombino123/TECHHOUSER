using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ServerInfo101 : ServerInfoLevel
{
	public PlatformName PlatformID;

	public NDRUnicodeString ServerName;

	public uint VerMajor;

	public uint VerMinor;

	public ServerType Type;

	public NDRUnicodeString Comment;

	public override uint Level => 101u;

	public ServerInfo101()
	{
		ServerName = new NDRUnicodeString();
		Comment = new NDRUnicodeString();
	}

	public ServerInfo101(NDRParser parser)
	{
		Read(parser);
	}

	public override void Read(NDRParser parser)
	{
		parser.BeginStructure();
		PlatformID = (PlatformName)parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref ServerName);
		VerMajor = parser.ReadUInt32();
		VerMinor = parser.ReadUInt32();
		Type = (ServerType)parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref Comment);
		parser.EndStructure();
	}

	public override void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteUInt32((uint)PlatformID);
		writer.WriteEmbeddedStructureFullPointer(ServerName);
		writer.WriteUInt32(VerMajor);
		writer.WriteUInt32(VerMinor);
		writer.WriteUInt32((uint)Type);
		writer.WriteEmbeddedStructureFullPointer(Comment);
		writer.EndStructure();
	}
}
