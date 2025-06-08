using System;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class WorkstationInfo : INDRStructure
{
	public uint Level;

	public WorkstationInfoLevel Info;

	public WorkstationInfo()
	{
	}

	public WorkstationInfo(uint level)
	{
		Level = level;
	}

	public WorkstationInfo(WorkstationInfoLevel info)
	{
		Level = info.Level;
		Info = info;
	}

	public WorkstationInfo(NDRParser parser)
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
			WorkstationInfo100 structure2 = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure2);
			Info = structure2;
			break;
		}
		case 101u:
		{
			WorkstationInfo101 structure = null;
			parser.ReadEmbeddedStructureFullPointer(ref structure);
			Info = structure;
			break;
		}
		default:
			throw new NotImplementedException();
		}
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		if (Info != null && Level != Info.Level)
		{
			throw new ArgumentException("Invalid WKSTA_INFO Level");
		}
		writer.BeginStructure();
		writer.WriteUInt32(Level);
		writer.WriteEmbeddedStructureFullPointer(Info);
		writer.EndStructure();
	}
}
