using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class WorkstationInfo100 : WorkstationInfoLevel
{
	public uint PlatformID;

	public NDRUnicodeString ComputerName;

	public NDRUnicodeString LanGroup;

	public uint VerMajor;

	public uint VerMinor;

	public override uint Level => 100u;

	public WorkstationInfo100()
	{
		ComputerName = new NDRUnicodeString();
		LanGroup = new NDRUnicodeString();
	}

	public WorkstationInfo100(NDRParser parser)
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
		writer.EndStructure();
	}
}
