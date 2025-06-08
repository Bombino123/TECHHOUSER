using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo : INDRStructure
{
	public uint Level;

	public IShareInfoEntry Info;

	public ShareInfo()
	{
	}

	public ShareInfo(uint level)
	{
		Level = level;
	}

	public ShareInfo(IShareInfoEntry info)
	{
		Level = info.Level;
		Info = info;
	}

	public ShareInfo(NDRParser parser)
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
			ShareInfo0Entry structure2 = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure2);
			Info = structure2;
			break;
		}
		case 101u:
		{
			ShareInfo1Entry structure = null;
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
