using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal static class NumericReader
{
	public static bool TryReadNumeric(ref DataReader reader, ulong end, out object value)
	{
		value = null;
		ulong num = reader.Position;
		if (num + 2 > end)
		{
			return false;
		}
		NumericLeaf numericLeaf = (NumericLeaf)reader.ReadUInt16();
		if ((int)numericLeaf < 32768)
		{
			value = (short)numericLeaf;
			return true;
		}
		switch (numericLeaf)
		{
		case NumericLeaf.LF_NUMERIC:
			if (num > end)
			{
				return false;
			}
			value = reader.ReadSByte();
			return true;
		case NumericLeaf.LF_SHORT:
			if (num + 2 > end)
			{
				return false;
			}
			value = reader.ReadInt16();
			return true;
		case NumericLeaf.LF_USHORT:
			if (num + 2 > end)
			{
				return false;
			}
			value = reader.ReadUInt16();
			return true;
		case NumericLeaf.LF_LONG:
			if (num + 4 > end)
			{
				return false;
			}
			value = reader.ReadInt32();
			return true;
		case NumericLeaf.LF_ULONG:
			if (num + 4 > end)
			{
				return false;
			}
			value = reader.ReadUInt32();
			return true;
		case NumericLeaf.LF_REAL32:
			if (num + 4 > end)
			{
				return false;
			}
			value = reader.ReadSingle();
			return true;
		case NumericLeaf.LF_REAL64:
			if (num + 8 > end)
			{
				return false;
			}
			value = reader.ReadDouble();
			return true;
		case NumericLeaf.LF_QUADWORD:
			if (num + 8 > end)
			{
				return false;
			}
			value = reader.ReadInt64();
			return true;
		case NumericLeaf.LF_UQUADWORD:
			if (num + 8 > end)
			{
				return false;
			}
			value = reader.ReadUInt64();
			return true;
		case NumericLeaf.LF_VARSTRING:
		{
			if (num + 2 > end)
			{
				return false;
			}
			int num3 = reader.ReadUInt16();
			if (num + (uint)num3 > end)
			{
				return false;
			}
			value = reader.ReadUtf8String(num3);
			return true;
		}
		case NumericLeaf.LF_VARIANT:
		{
			if (num + 16 > end)
			{
				return false;
			}
			int num2 = reader.ReadInt32();
			int hi = reader.ReadInt32();
			int lo = reader.ReadInt32();
			int mid = reader.ReadInt32();
			byte b = (byte)(num2 >> 16);
			if (b <= 28)
			{
				value = new decimal(lo, mid, hi, num2 < 0, b);
			}
			else
			{
				value = null;
			}
			return true;
		}
		default:
			return false;
		}
	}
}
