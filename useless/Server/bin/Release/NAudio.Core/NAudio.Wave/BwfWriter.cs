using System;
using System.IO;
using System.Text;

namespace NAudio.Wave;

public class BwfWriter : IDisposable
{
	private readonly WaveFormat format;

	private readonly BinaryWriter writer;

	private readonly long dataChunkSizePosition;

	private long dataLength;

	private bool isDisposed;

	public BwfWriter(string filename, WaveFormat format, BextChunkInfo bextChunkInfo)
	{
		this.format = format;
		writer = new BinaryWriter(File.OpenWrite(filename));
		writer.Write(Encoding.UTF8.GetBytes("RIFF"));
		writer.Write(0);
		writer.Write(Encoding.UTF8.GetBytes("WAVE"));
		writer.Write(Encoding.UTF8.GetBytes("JUNK"));
		writer.Write(28);
		writer.Write(0L);
		writer.Write(0L);
		writer.Write(0L);
		writer.Write(0);
		writer.Write(Encoding.UTF8.GetBytes("bext"));
		byte[] bytes = Encoding.ASCII.GetBytes(bextChunkInfo.CodingHistory ?? "");
		int num = 602 + bytes.Length;
		if (num % 2 != 0)
		{
			num++;
		}
		writer.Write(num);
		_ = writer.BaseStream.Position;
		writer.Write(GetAsBytes(bextChunkInfo.Description, 256));
		writer.Write(GetAsBytes(bextChunkInfo.Originator, 32));
		writer.Write(GetAsBytes(bextChunkInfo.OriginatorReference, 32));
		writer.Write(GetAsBytes(bextChunkInfo.OriginationDate, 10));
		writer.Write(GetAsBytes(bextChunkInfo.OriginationTime, 8));
		writer.Write(bextChunkInfo.TimeReference);
		writer.Write(bextChunkInfo.Version);
		writer.Write(GetAsBytes(bextChunkInfo.UniqueMaterialIdentifier, 64));
		writer.Write(bextChunkInfo.Reserved);
		writer.Write(bytes);
		if (bytes.Length % 2 != 0)
		{
			writer.Write((byte)0);
		}
		writer.Write(Encoding.UTF8.GetBytes("fmt "));
		format.Serialize(writer);
		writer.Write(Encoding.UTF8.GetBytes("data"));
		dataChunkSizePosition = writer.BaseStream.Position;
		writer.Write(-1);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		if (isDisposed)
		{
			throw new ObjectDisposedException("This BWF Writer already disposed");
		}
		writer.Write(buffer, offset, count);
		dataLength += count;
	}

	public void Flush()
	{
		if (isDisposed)
		{
			throw new ObjectDisposedException("This BWF Writer already disposed");
		}
		writer.Flush();
		FixUpChunkSizes(restorePosition: true);
	}

	private void FixUpChunkSizes(bool restorePosition)
	{
		long position = writer.BaseStream.Position;
		bool num = dataLength > int.MaxValue;
		long num2 = writer.BaseStream.Length - 8;
		if (num)
		{
			int num3 = format.BitsPerSample / 8 * format.Channels;
			writer.BaseStream.Position = 0L;
			writer.Write(Encoding.UTF8.GetBytes("RF64"));
			writer.Write(-1);
			writer.BaseStream.Position += 4L;
			writer.Write(Encoding.UTF8.GetBytes("ds64"));
			writer.BaseStream.Position += 4L;
			writer.Write(num2);
			writer.Write(dataLength);
			writer.Write(dataLength / num3);
		}
		else
		{
			writer.BaseStream.Position = 4L;
			writer.Write((uint)num2);
			writer.BaseStream.Position = dataChunkSizePosition;
			writer.Write((uint)dataLength);
		}
		if (restorePosition)
		{
			writer.BaseStream.Position = position;
		}
	}

	public void Dispose()
	{
		if (!isDisposed)
		{
			FixUpChunkSizes(restorePosition: false);
			writer.Dispose();
			isDisposed = true;
		}
	}

	private static byte[] GetAsBytes(string message, int byteSize)
	{
		byte[] array = new byte[byteSize];
		byte[] bytes = Encoding.ASCII.GetBytes(message ?? "");
		Array.Copy(bytes, array, Math.Min(bytes.Length, byteSize));
		return array;
	}
}
