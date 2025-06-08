using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTTransactCreateRequest : NTTransactSubcommand
{
	public const int ParametersFixedLength = 53;

	public NTCreateFlags Flags;

	public uint RootDirectoryFID;

	public AccessMask DesiredAccess;

	public long AllocationSize;

	public ExtendedFileAttributes ExtFileAttributes;

	public ShareAccess ShareAccess;

	public CreateDisposition CreateDisposition;

	public CreateOptions CreateOptions;

	public ImpersonationLevel ImpersonationLevel;

	public SecurityFlags SecurityFlags;

	public string Name;

	public SecurityDescriptor SecurityDescriptor;

	public List<FileFullEAEntry> ExtendedAttributes;

	public override NTTransactSubcommandName SubcommandName => NTTransactSubcommandName.NT_TRANSACT_CREATE;

	public NTTransactCreateRequest()
	{
	}

	public NTTransactCreateRequest(byte[] parameters, byte[] data, bool isUnicode)
	{
		int offset = 0;
		Flags = (NTCreateFlags)LittleEndianReader.ReadUInt32(parameters, ref offset);
		RootDirectoryFID = LittleEndianReader.ReadUInt32(parameters, ref offset);
		DesiredAccess = (AccessMask)LittleEndianReader.ReadUInt32(parameters, ref offset);
		AllocationSize = LittleEndianReader.ReadInt64(parameters, ref offset);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(parameters, ref offset);
		ShareAccess = (ShareAccess)LittleEndianReader.ReadUInt32(parameters, ref offset);
		CreateDisposition = (CreateDisposition)LittleEndianReader.ReadUInt32(parameters, ref offset);
		CreateOptions = (CreateOptions)LittleEndianReader.ReadUInt32(parameters, ref offset);
		uint num = LittleEndianReader.ReadUInt32(parameters, ref offset);
		LittleEndianReader.ReadUInt32(parameters, ref offset);
		uint byteCount = LittleEndianReader.ReadUInt32(parameters, ref offset);
		ImpersonationLevel = (ImpersonationLevel)LittleEndianReader.ReadUInt32(parameters, ref offset);
		SecurityFlags = (SecurityFlags)ByteReader.ReadByte(parameters, ref offset);
		if (isUnicode)
		{
			offset++;
		}
		Name = SMB1Helper.ReadFixedLengthString(parameters, ref offset, isUnicode, (int)byteCount);
		if (num != 0)
		{
			SecurityDescriptor = new SecurityDescriptor(data, 0);
		}
		ExtendedAttributes = FileFullEAInformation.ReadList(data, (int)num);
	}

	public override byte[] GetParameters(bool isUnicode)
	{
		throw new NotImplementedException();
	}

	public override byte[] GetData()
	{
		throw new NotImplementedException();
	}
}
