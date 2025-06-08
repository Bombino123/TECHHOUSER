using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CreateRequest : SMB2Command
{
	public const int FixedLength = 56;

	public const int DeclaredSize = 57;

	private ushort StructureSize;

	public byte SecurityFlags;

	public OplockLevel RequestedOplockLevel;

	public ImpersonationLevel ImpersonationLevel;

	public ulong SmbCreateFlags;

	public ulong Reserved;

	public AccessMask DesiredAccess;

	public FileAttributes FileAttributes;

	public ShareAccess ShareAccess;

	public CreateDisposition CreateDisposition;

	public CreateOptions CreateOptions;

	private ushort NameOffset;

	private ushort NameLength;

	private uint CreateContextsOffset;

	private uint CreateContextsLength;

	public string Name;

	public List<CreateContext> CreateContexts = new List<CreateContext>();

	public override int CommandLength
	{
		get
		{
			int val = ((CreateContexts.Count != 0) ? ((int)Math.Ceiling((double)(Name.Length * 2) / 8.0) * 8 + CreateContext.GetCreateContextListLength(CreateContexts)) : (Name.Length * 2));
			return 56 + Math.Max(val, 1);
		}
	}

	public CreateRequest()
		: base(SMB2CommandName.Create)
	{
		StructureSize = 57;
	}

	public CreateRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		SecurityFlags = ByteReader.ReadByte(buffer, offset + 64 + 2);
		RequestedOplockLevel = (OplockLevel)ByteReader.ReadByte(buffer, offset + 64 + 3);
		ImpersonationLevel = (ImpersonationLevel)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		SmbCreateFlags = LittleEndianConverter.ToUInt64(buffer, offset + 64 + 8);
		Reserved = LittleEndianConverter.ToUInt64(buffer, offset + 64 + 16);
		DesiredAccess = (AccessMask)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 24);
		FileAttributes = (FileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
		ShareAccess = (ShareAccess)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		CreateDisposition = (CreateDisposition)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		CreateOptions = (CreateOptions)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 40);
		NameOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 44);
		NameLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 46);
		CreateContextsOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 48);
		CreateContextsLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 52);
		Name = ByteReader.ReadUTF16String(buffer, offset + NameOffset, NameLength / 2);
		if (CreateContextsLength != 0)
		{
			CreateContexts = CreateContext.ReadCreateContextList(buffer, (int)CreateContextsOffset);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		NameOffset = 120;
		NameLength = (ushort)(Name.Length * 2);
		CreateContextsOffset = 0u;
		CreateContextsLength = 0u;
		int num = (int)Math.Ceiling((double)(Name.Length * 2) / 8.0) * 8;
		if (CreateContexts.Count > 0)
		{
			CreateContextsOffset = (uint)(120 + num);
			CreateContextsLength = (uint)CreateContext.GetCreateContextListLength(CreateContexts);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, SecurityFlags);
		ByteWriter.WriteByte(buffer, offset + 3, (byte)RequestedOplockLevel);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)ImpersonationLevel);
		LittleEndianWriter.WriteUInt64(buffer, offset + 8, SmbCreateFlags);
		LittleEndianWriter.WriteUInt64(buffer, offset + 16, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, (uint)DesiredAccess);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, (uint)FileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, (uint)ShareAccess);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, (uint)CreateDisposition);
		LittleEndianWriter.WriteUInt32(buffer, offset + 40, (uint)CreateOptions);
		LittleEndianWriter.WriteUInt16(buffer, offset + 44, NameOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 46, NameLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 48, CreateContextsOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 52, CreateContextsLength);
		ByteWriter.WriteUTF16String(buffer, offset + 56, Name);
		CreateContext.WriteCreateContextList(buffer, offset + 56 + num, CreateContexts);
	}
}
