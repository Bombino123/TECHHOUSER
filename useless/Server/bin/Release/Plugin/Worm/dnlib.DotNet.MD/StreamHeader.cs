using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("O:{offset} L:{streamSize} {name}")]
[ComVisible(true)]
public sealed class StreamHeader : FileSection
{
	private readonly uint offset;

	private readonly uint streamSize;

	private readonly string name;

	public uint Offset => offset;

	public uint StreamSize => streamSize;

	public string Name => name;

	public StreamHeader(ref DataReader reader, bool verify)
		: this(ref reader, verify, verify, CLRRuntimeReaderKind.CLR, out var _)
	{
	}

	internal StreamHeader(ref DataReader reader, bool throwOnError, bool verify, CLRRuntimeReaderKind runtime, out bool failedVerification)
	{
		failedVerification = false;
		SetStartOffset(ref reader);
		offset = reader.ReadUInt32();
		streamSize = reader.ReadUInt32();
		name = ReadString(ref reader, 32, verify, ref failedVerification);
		SetEndoffset(ref reader);
		if (runtime == CLRRuntimeReaderKind.Mono)
		{
			if (offset > reader.Length)
			{
				offset = reader.Length;
			}
			streamSize = reader.Length - offset;
		}
		if (verify && offset + size < offset)
		{
			failedVerification = true;
		}
		if (throwOnError & failedVerification)
		{
			throw new BadImageFormatException("Invalid stream header");
		}
	}

	internal StreamHeader(uint offset, uint streamSize, string name)
	{
		this.offset = offset;
		this.streamSize = streamSize;
		this.name = name ?? throw new ArgumentNullException("name");
	}

	private static string ReadString(ref DataReader reader, int maxLen, bool verify, ref bool failedVerification)
	{
		uint position = reader.Position;
		StringBuilder stringBuilder = new StringBuilder(maxLen);
		int i;
		for (i = 0; i < maxLen; i++)
		{
			byte b = reader.ReadByte();
			if (b == 0)
			{
				break;
			}
			stringBuilder.Append((char)b);
		}
		if (verify && i == maxLen)
		{
			failedVerification = true;
		}
		if (i != maxLen)
		{
			reader.Position = position + (uint)((i + 1 + 3) & -4);
		}
		return stringBuilder.ToString();
	}
}
