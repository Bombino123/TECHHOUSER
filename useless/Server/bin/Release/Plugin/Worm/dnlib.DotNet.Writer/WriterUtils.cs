namespace dnlib.DotNet.Writer;

internal static class WriterUtils
{
	public static uint WriteCompressedUInt32(this DataWriter writer, IWriterError helper, uint value)
	{
		if (value > 536870911)
		{
			helper.Error("UInt32 value is too big and can't be compressed");
			value = 536870911u;
		}
		writer.WriteCompressedUInt32(value);
		return value;
	}

	public static int WriteCompressedInt32(this DataWriter writer, IWriterError helper, int value)
	{
		if (value < -268435456)
		{
			helper.Error("Int32 value is too small and can't be compressed.");
			value = -268435456;
		}
		else if (value > 268435455)
		{
			helper.Error("Int32 value is too big and can't be compressed.");
			value = 268435455;
		}
		writer.WriteCompressedInt32(value);
		return value;
	}

	public static void Write(this DataWriter writer, IWriterError helper, UTF8String s)
	{
		if (UTF8String.IsNull(s))
		{
			helper.Error("UTF8String is null");
			s = UTF8String.Empty;
		}
		writer.WriteCompressedUInt32(helper, (uint)s.DataLength);
		writer.WriteBytes(s.Data);
	}
}
