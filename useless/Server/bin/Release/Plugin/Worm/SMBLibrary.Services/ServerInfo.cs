using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ServerInfo : INDRStructure
{
	public uint Level;

	public ServerInfoLevel Info;

	public ServerInfo()
	{
	}

	public ServerInfo(uint level)
	{
		Level = level;
	}

	public ServerInfo(ServerInfoLevel info)
	{
		Level = info.Level;
		Info = info;
	}

	public ServerInfo(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		Level = parser.ReadUInt32();
		switch (Level)
		{
		case 100u:
		{
			ServerInfo100 structure2 = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure2);
			Info = structure2;
			break;
		}
		case 101u:
		{
			ServerInfo101 structure = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure);
			Info = structure;
			break;
		}
		default:
			throw new InvalidLevelException(Level);
		}
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteUInt32(Level);
		writer.WriteEmbeddedStructureFullPointer(Info);
		writer.EndStructure();
	}
}
