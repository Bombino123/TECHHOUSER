using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class IOCtlResponse : SMB2Command
{
	public const int FixedLength = 48;

	public const int DeclaredSize = 49;

	private ushort StructureSize;

	public ushort Reserved;

	public uint CtlCode;

	public FileID FileId;

	private uint InputOffset;

	private uint InputCount;

	private uint OutputOffset;

	private uint OutputCount;

	public uint Flags;

	public uint Reserved2;

	public byte[] Input = new byte[0];

	public byte[] Output = new byte[0];

	public override int CommandLength
	{
		get
		{
			int num = (int)Math.Ceiling((double)Input.Length / 8.0) * 8;
			return 48 + num + Output.Length;
		}
	}

	public IOCtlResponse()
		: base(SMB2CommandName.IOCtl)
	{
		Header.IsResponse = true;
		StructureSize = 49;
	}

	public IOCtlResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		CtlCode = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
		InputOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 24);
		InputCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
		OutputOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		OutputCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		Flags = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 40);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 44);
		Input = ByteReader.ReadBytes(buffer, offset + (int)InputOffset, (int)InputCount);
		Output = ByteReader.ReadBytes(buffer, offset + (int)OutputOffset, (int)OutputCount);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		InputOffset = 0u;
		InputCount = (uint)Input.Length;
		OutputOffset = 0u;
		OutputCount = (uint)Output.Length;
		if (Input.Length != 0)
		{
			InputOffset = 112u;
		}
		int num = (int)Math.Ceiling((double)Input.Length / 8.0) * 8;
		if (Output.Length != 0)
		{
			OutputOffset = (uint)(112 + num);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, CtlCode);
		FileId.WriteBytes(buffer, offset + 8);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, InputOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, InputCount);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, OutputOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, OutputCount);
		LittleEndianWriter.WriteUInt32(buffer, offset + 40, Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 44, Reserved2);
		if (Input.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48, Input);
		}
		if (Output.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 48 + num, Output);
		}
	}
}
