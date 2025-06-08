using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class WorkstationInfo101 : WorkstationInfoLevel
{
	public uint PlatformID;

	public NDRUnicodeString ComputerName;

	public NDRUnicodeString LanGroup;

	public uint VerMajor;

	public uint VerMinor;

	public NDRUnicodeString LanRoot;

	public override uint Level => 101u;

	public WorkstationInfo101()
	{
		ComputerName = new NDRUnicodeString();
		LanGroup = new NDRUnicodeString();
		LanRoot = new NDRUnicodeString();
	}

	public WorkstationInfo101(NDRParser parser)
	{
		Read(parser);
	}

	public override void Read(NDRParser parser)
	{
		parser.BeginStructure();
		PlatformID = parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref ComputerName);
		parser.ReadEmbeddedStructureFullPointer(ref LanGroup);
		VerMajor = parser.ReadUInt32();
		VerMinor = parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref LanRoot);
		parser.EndStructure();
	}

	public override void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteUInt32(PlatformID);
		writer.WriteEmbeddedStructureFullPointer(ComputerName);
		writer.WriteEmbeddedStructureFullPointer(LanGroup);
		writer.WriteUInt32(VerMajor);
		writer.WriteUInt32(VerMinor);
		writer.WriteEmbeddedStructureFullPointer(LanRoot);
		writer.EndStructure();
	}
}
