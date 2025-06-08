using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class NegotiateResponse : SMB2Command
{
	public const int FixedSize = 64;

	public const int DeclaredSize = 65;

	private ushort StructureSize;

	public SecurityMode SecurityMode;

	public SMB2Dialect DialectRevision;

	private ushort NegotiateContextCount;

	public Guid ServerGuid;

	public Capabilities Capabilities;

	public uint MaxTransactSize;

	public uint MaxReadSize;

	public uint MaxWriteSize;

	public DateTime SystemTime;

	public DateTime ServerStartTime;

	private ushort SecurityBufferOffset;

	private ushort SecurityBufferLength;

	private uint NegotiateContextOffset;

	public byte[] SecurityBuffer = new byte[0];

	public List<NegotiateContext> NegotiateContextList = new List<NegotiateContext>();

	public override int CommandLength
	{
		get
		{
			if (NegotiateContextList.Count == 0)
			{
				return 64 + SecurityBuffer.Length;
			}
			int num = (int)Math.Ceiling((double)(int)SecurityBufferLength / 8.0) * 8;
			return 64 + num + NegotiateContext.GetNegotiateContextListLength(NegotiateContextList);
		}
	}

	public NegotiateResponse()
		: base(SMB2CommandName.Negotiate)
	{
		Header.IsResponse = true;
		StructureSize = 65;
	}

	public NegotiateResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		SecurityMode = (SecurityMode)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		DialectRevision = (SMB2Dialect)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 4);
		NegotiateContextCount = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 6);
		ServerGuid = LittleEndianConverter.ToGuid(buffer, offset + 64 + 8);
		Capabilities = (Capabilities)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 24);
		MaxTransactSize = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
		MaxReadSize = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		MaxWriteSize = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		SystemTime = DateTime.FromFileTimeUtc(LittleEndianConverter.ToInt64(buffer, offset + 64 + 40));
		ServerStartTime = DateTime.FromFileTimeUtc(LittleEndianConverter.ToInt64(buffer, offset + 64 + 48));
		SecurityBufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 56);
		SecurityBufferLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 58);
		NegotiateContextOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 60);
		SecurityBuffer = ByteReader.ReadBytes(buffer, offset + SecurityBufferOffset, SecurityBufferLength);
		NegotiateContextList = NegotiateContext.ReadNegotiateContextList(buffer, (int)NegotiateContextOffset, NegotiateContextCount);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		SecurityBufferOffset = 0;
		SecurityBufferLength = (ushort)SecurityBuffer.Length;
		int num = (int)Math.Ceiling((double)(int)SecurityBufferLength / 8.0) * 8;
		if (SecurityBuffer.Length != 0)
		{
			SecurityBufferOffset = 128;
		}
		NegotiateContextOffset = 0u;
		NegotiateContextCount = (ushort)NegotiateContextList.Count;
		if (NegotiateContextList.Count > 0)
		{
			NegotiateContextOffset = (uint)(128 + num);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)SecurityMode);
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, (ushort)DialectRevision);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, NegotiateContextCount);
		LittleEndianWriter.WriteGuid(buffer, offset + 8, ServerGuid);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, (uint)Capabilities);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, MaxTransactSize);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, MaxReadSize);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, MaxWriteSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 40, SystemTime.ToFileTimeUtc());
		LittleEndianWriter.WriteInt64(buffer, offset + 48, ServerStartTime.ToFileTimeUtc());
		LittleEndianWriter.WriteUInt16(buffer, offset + 56, SecurityBufferOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 58, SecurityBufferLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 60, NegotiateContextOffset);
		ByteWriter.WriteBytes(buffer, offset + 64, SecurityBuffer);
		NegotiateContext.WriteNegotiateContextList(buffer, offset + 64 + num, NegotiateContextList);
	}
}
