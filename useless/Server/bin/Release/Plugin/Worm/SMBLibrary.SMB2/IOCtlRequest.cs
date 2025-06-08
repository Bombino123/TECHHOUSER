using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class IOCtlRequest : SMB2Command
{
	public const int FixedLength = 56;

	public const int DeclaredSize = 57;

	private ushort StructureSize;

	public ushort Reserved;

	public uint CtlCode;

	public FileID FileId;

	private uint InputOffset;

	private uint InputCount;

	public uint MaxInputResponse;

	private uint OutputOffset;

	private uint OutputCount;

	public uint MaxOutputResponse;

	public IOCtlRequestFlags Flags;

	public uint Reserved2;

	public byte[] Input = new byte[0];

	public byte[] Output = new byte[0];

	public bool IsFSCtl
	{
		get
		{
			return (Flags & IOCtlRequestFlags.IsFSCtl) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= IOCtlRequestFlags.IsFSCtl;
			}
			else
			{
				Flags &= ~IOCtlRequestFlags.IsFSCtl;
			}
		}
	}

	public override int CommandLength => 56 + Input.Length + Output.Length;

	public IOCtlRequest()
		: base(SMB2CommandName.IOCtl)
	{
		StructureSize = 57;
	}

	public IOCtlRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		CtlCode = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
		InputOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 24);
		InputCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
		MaxInputResponse = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 32);
		OutputOffset = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 36);
		OutputCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 40);
		MaxOutputResponse = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 44);
		Flags = (IOCtlRequestFlags)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 48);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 52);
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
			InputOffset = 120u;
		}
		if (Output.Length != 0)
		{
			OutputOffset = (uint)(120 + Input.Length);
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, CtlCode);
		FileId.WriteBytes(buffer, offset + 8);
		LittleEndianWriter.WriteUInt32(buffer, offset + 24, InputOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, InputCount);
		LittleEndianWriter.WriteUInt32(buffer, offset + 32, MaxInputResponse);
		LittleEndianWriter.WriteUInt32(buffer, offset + 36, OutputOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 40, OutputCount);
		LittleEndianWriter.WriteUInt32(buffer, offset + 44, MaxOutputResponse);
		LittleEndianWriter.WriteUInt32(buffer, offset + 48, (uint)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 52, Reserved2);
		if (Input.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 56, Input);
		}
		if (Output.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 56 + Input.Length, Output);
		}
	}
}
