using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NodeStatistics
{
	public const int Length = 46;

	public byte[] UnitID;

	public byte Jumpers;

	public byte TestResult;

	public ushort VersionNumber;

	public ushort PeriodOfStatistics;

	public ushort NumberOfCRCs;

	public ushort NumberOfAlignmentErrors;

	public ushort NumberOfCollisions;

	public ushort NumberOfSendAborts;

	public uint NumberOfGoodSends;

	public uint NumberOfGoodReceives;

	public ushort NumberOfRetransmits;

	public ushort NumberOfNoResourceConditions;

	public ushort NumberOfFreeCommandBlocks;

	public ushort TotalNumberOfCommandBlocks;

	public ushort MaxTotalNumberOfCommandBlocks;

	public ushort NumberOfPendingSessions;

	public ushort MaxNumberOfPendingSessions;

	public ushort MaxTotalsSessionsPossible;

	public ushort SessionDataPacketSize;

	public NodeStatistics()
	{
		UnitID = new byte[6];
	}

	public NodeStatistics(byte[] buffer, ref int offset)
	{
		UnitID = ByteReader.ReadBytes(buffer, ref offset, 6);
		Jumpers = ByteReader.ReadByte(buffer, ref offset);
		TestResult = ByteReader.ReadByte(buffer, ref offset);
		VersionNumber = BigEndianReader.ReadUInt16(buffer, ref offset);
		PeriodOfStatistics = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfCRCs = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfAlignmentErrors = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfCollisions = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfSendAborts = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfGoodSends = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfGoodReceives = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfRetransmits = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfNoResourceConditions = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfFreeCommandBlocks = BigEndianReader.ReadUInt16(buffer, ref offset);
		TotalNumberOfCommandBlocks = BigEndianReader.ReadUInt16(buffer, ref offset);
		MaxTotalNumberOfCommandBlocks = BigEndianReader.ReadUInt16(buffer, ref offset);
		NumberOfPendingSessions = BigEndianReader.ReadUInt16(buffer, ref offset);
		MaxNumberOfPendingSessions = BigEndianReader.ReadUInt16(buffer, ref offset);
		MaxTotalsSessionsPossible = BigEndianReader.ReadUInt16(buffer, ref offset);
		SessionDataPacketSize = BigEndianReader.ReadUInt16(buffer, ref offset);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteBytes(buffer, ref offset, UnitID, 6);
		ByteWriter.WriteByte(buffer, ref offset, Jumpers);
		ByteWriter.WriteByte(buffer, ref offset, TestResult);
		BigEndianWriter.WriteUInt16(buffer, ref offset, VersionNumber);
		BigEndianWriter.WriteUInt16(buffer, ref offset, PeriodOfStatistics);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfCRCs);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfAlignmentErrors);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfCollisions);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfSendAborts);
		BigEndianWriter.WriteUInt32(buffer, ref offset, NumberOfGoodSends);
		BigEndianWriter.WriteUInt32(buffer, ref offset, NumberOfGoodReceives);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfRetransmits);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfNoResourceConditions);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfFreeCommandBlocks);
		BigEndianWriter.WriteUInt16(buffer, ref offset, TotalNumberOfCommandBlocks);
		BigEndianWriter.WriteUInt16(buffer, ref offset, MaxTotalNumberOfCommandBlocks);
		BigEndianWriter.WriteUInt16(buffer, ref offset, NumberOfPendingSessions);
		BigEndianWriter.WriteUInt16(buffer, ref offset, MaxNumberOfPendingSessions);
		BigEndianWriter.WriteUInt16(buffer, ref offset, MaxTotalsSessionsPossible);
		BigEndianWriter.WriteUInt16(buffer, ref offset, SessionDataPacketSize);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[46];
		WriteBytes(array, 0);
		return array;
	}
}
