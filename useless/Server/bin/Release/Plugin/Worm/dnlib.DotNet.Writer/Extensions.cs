using System.IO;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public static class Extensions
{
	public static void WriteZeroes(this DataWriter writer, int count)
	{
		while (count >= 8)
		{
			writer.WriteUInt64(0uL);
			count -= 8;
		}
		for (int i = 0; i < count; i++)
		{
			writer.WriteByte(0);
		}
	}

	public static void VerifyWriteTo(this IChunk chunk, DataWriter writer)
	{
		long position = writer.Position;
		chunk.WriteTo(writer);
		if (writer.Position - position != chunk.GetFileLength())
		{
			VerifyWriteToThrow(chunk);
		}
	}

	private static void VerifyWriteToThrow(IChunk chunk)
	{
		throw new IOException("Did not write all bytes: " + chunk.GetType().FullName);
	}

	internal static void WriteDataDirectory(this DataWriter writer, IChunk chunk)
	{
		if (chunk == null || chunk.GetVirtualSize() == 0)
		{
			writer.WriteUInt64(0uL);
			return;
		}
		writer.WriteUInt32((uint)chunk.RVA);
		writer.WriteUInt32(chunk.GetVirtualSize());
	}

	internal static void WriteDebugDirectory(this DataWriter writer, DebugDirectory chunk)
	{
		if (chunk == null || chunk.GetVirtualSize() == 0)
		{
			writer.WriteUInt64(0uL);
			return;
		}
		writer.WriteUInt32((uint)chunk.RVA);
		writer.WriteUInt32((uint)(chunk.Count * 28));
	}

	internal static void Error2(this IWriterError helper, string message, params object[] args)
	{
		if (helper is IWriterError2 writerError)
		{
			writerError.Error(message, args);
		}
		else
		{
			helper.Error(string.Format(message, args));
		}
	}
}
