using dnlib.IO;

namespace dnlib.DotNet;

internal sealed class ConstantMD : Constant, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly uint origRid;

	public uint OrigRid => origRid;

	public ConstantMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		readerModule.TablesStream.TryReadConstantRow(origRid, out var row);
		type = (ElementType)row.Type;
		DataReader reader = readerModule.BlobStream.CreateReader(row.Value);
		value = GetValue(type, ref reader);
	}

	private static object GetValue(ElementType etype, ref DataReader reader)
	{
		switch (etype)
		{
		case ElementType.Boolean:
			if (reader.Length < 1)
			{
				return false;
			}
			return reader.ReadBoolean();
		case ElementType.Char:
			if (reader.Length < 2)
			{
				return '\0';
			}
			return reader.ReadChar();
		case ElementType.I1:
			if (reader.Length < 1)
			{
				return (sbyte)0;
			}
			return reader.ReadSByte();
		case ElementType.U1:
			if (reader.Length < 1)
			{
				return (byte)0;
			}
			return reader.ReadByte();
		case ElementType.I2:
			if (reader.Length < 2)
			{
				return (short)0;
			}
			return reader.ReadInt16();
		case ElementType.U2:
			if (reader.Length < 2)
			{
				return (ushort)0;
			}
			return reader.ReadUInt16();
		case ElementType.I4:
			if (reader.Length < 4)
			{
				return 0;
			}
			return reader.ReadInt32();
		case ElementType.U4:
			if (reader.Length < 4)
			{
				return 0u;
			}
			return reader.ReadUInt32();
		case ElementType.I8:
			if (reader.Length < 8)
			{
				return 0L;
			}
			return reader.ReadInt64();
		case ElementType.U8:
			if (reader.Length < 8)
			{
				return 0uL;
			}
			return reader.ReadUInt64();
		case ElementType.R4:
			if (reader.Length < 4)
			{
				return 0f;
			}
			return reader.ReadSingle();
		case ElementType.R8:
			if (reader.Length < 8)
			{
				return 0.0;
			}
			return reader.ReadDouble();
		case ElementType.String:
			return reader.ReadUtf16String((int)(reader.BytesLeft / 2));
		case ElementType.Class:
			return null;
		default:
			return null;
		}
	}
}
