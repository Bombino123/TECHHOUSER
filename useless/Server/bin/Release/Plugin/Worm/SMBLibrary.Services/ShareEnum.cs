using System;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareEnum : INDRStructure
{
	public uint Level;

	public IShareInfoContainer Info;

	public ShareEnum()
	{
	}

	public ShareEnum(uint level)
	{
		Level = level;
	}

	public ShareEnum(IShareInfoContainer info)
	{
		Level = info.Level;
		Info = info;
	}

	public ShareEnum(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		Level = parser.ReadUInt32();
		parser.BeginStructure();
		uint num = parser.ReadUInt32();
		switch (num)
		{
		case 0u:
		{
			ShareInfo0Container structure2 = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure2);
			Info = structure2;
			break;
		}
		case 1u:
		{
			ShareInfo1Container structure = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure);
			Info = structure;
			break;
		}
		case 2u:
		case 501u:
		case 502u:
		case 503u:
			throw new UnsupportedLevelException(num);
		default:
			throw new InvalidLevelException(num);
		}
		parser.EndStructure();
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		if (Info != null && Level != Info.Level)
		{
			throw new ArgumentException("SHARE_ENUM_STRUCT Level mismatch");
		}
		writer.BeginStructure();
		writer.WriteUInt32(Level);
		writer.BeginStructure();
		writer.WriteUInt32(Level);
		writer.WriteEmbeddedStructureFullPointer(Info);
		writer.EndStructure();
		writer.EndStructure();
	}
}
