using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class ResourceRecord
{
	public string Name;

	public NameRecordType Type;

	public ResourceRecordClass Class;

	public uint TTL;

	public byte[] Data;

	public ResourceRecord(NameRecordType type)
	{
		Name = string.Empty;
		Type = type;
		Class = ResourceRecordClass.In;
		TTL = (uint)new TimeSpan(7, 0, 0, 0).TotalSeconds;
		Data = new byte[0];
	}

	public ResourceRecord(byte[] buffer, ref int offset)
	{
		Name = NetBiosUtils.DecodeName(buffer, ref offset);
		Type = (NameRecordType)BigEndianReader.ReadUInt16(buffer, ref offset);
		Class = (ResourceRecordClass)BigEndianReader.ReadUInt16(buffer, ref offset);
		TTL = BigEndianReader.ReadUInt32(buffer, ref offset);
		ushort length = BigEndianReader.ReadUInt16(buffer, ref offset);
		Data = ByteReader.ReadBytes(buffer, ref offset, length);
	}

	public void WriteBytes(Stream stream)
	{
		WriteBytes(stream, null);
	}

	public void WriteBytes(Stream stream, int? nameOffset)
	{
		if (nameOffset.HasValue)
		{
			NetBiosUtils.WriteNamePointer(stream, nameOffset.Value);
		}
		else
		{
			byte[] bytes = NetBiosUtils.EncodeName(Name, string.Empty);
			ByteWriter.WriteBytes(stream, bytes);
		}
		BigEndianWriter.WriteUInt16(stream, (ushort)Type);
		BigEndianWriter.WriteUInt16(stream, (ushort)Class);
		BigEndianWriter.WriteUInt32(stream, TTL);
		BigEndianWriter.WriteUInt16(stream, (ushort)Data.Length);
		ByteWriter.WriteBytes(stream, Data);
	}
}
