using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class NegotiateRequest : SMB2Command
{
	public const int DeclaredSize = 36;

	private ushort StructureSize;

	public SecurityMode SecurityMode;

	public ushort Reserved;

	public Capabilities Capabilities;

	public Guid ClientGuid;

	public DateTime ClientStartTime;

	public List<SMB2Dialect> Dialects = new List<SMB2Dialect>();

	public override int CommandLength => 36 + Dialects.Count * 2;

	public NegotiateRequest()
		: base(SMB2CommandName.Negotiate)
	{
		StructureSize = 36;
	}

	public NegotiateRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		SecurityMode = (SecurityMode)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 4);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 6);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 8);
		ClientGuid = LittleEndianConverter.ToGuid(buffer, offset + 64 + 12);
		ClientStartTime = DateTime.FromFileTimeUtc(LittleEndianConverter.ToInt64(buffer, offset + 64 + 28));
		for (int i = 0; i < num; i++)
		{
			SMB2Dialect item = (SMB2Dialect)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 36 + i * 2);
			Dialects.Add(item);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Dialects.Count);
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, (ushort)SecurityMode);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, (uint)Capabilities);
		LittleEndianWriter.WriteGuid(buffer, offset + 12, ClientGuid);
		LittleEndianWriter.WriteInt64(buffer, offset + 28, ClientStartTime.ToFileTimeUtc());
		for (int i = 0; i < Dialects.Count; i++)
		{
			SMB2Dialect value = Dialects[i];
			LittleEndianWriter.WriteUInt16(buffer, offset + 36 + i * 2, (ushort)value);
		}
	}
}
