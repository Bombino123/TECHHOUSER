using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public sealed class MetadataHeader : FileSection
{
	private readonly uint signature;

	private readonly ushort majorVersion;

	private readonly ushort minorVersion;

	private readonly uint reserved1;

	private readonly uint stringLength;

	private readonly string versionString;

	private readonly FileOffset offset2ndPart;

	private readonly StorageFlags flags;

	private readonly byte reserved2;

	private readonly ushort streams;

	private readonly IList<StreamHeader> streamHeaders;

	public uint Signature => signature;

	public ushort MajorVersion => majorVersion;

	public ushort MinorVersion => minorVersion;

	public uint Reserved1 => reserved1;

	public uint StringLength => stringLength;

	public string VersionString => versionString;

	public FileOffset StorageHeaderOffset => offset2ndPart;

	public StorageFlags Flags => flags;

	public byte Reserved2 => reserved2;

	public ushort Streams => streams;

	public IList<StreamHeader> StreamHeaders => streamHeaders;

	public MetadataHeader(ref DataReader reader, bool verify)
		: this(ref reader, CLRRuntimeReaderKind.CLR, verify)
	{
	}

	public MetadataHeader(ref DataReader reader, CLRRuntimeReaderKind runtime, bool verify)
	{
		SetStartOffset(ref reader);
		signature = reader.ReadUInt32();
		if (verify && signature != 1112167234)
		{
			throw new BadImageFormatException("Invalid metadata header signature");
		}
		majorVersion = reader.ReadUInt16();
		minorVersion = reader.ReadUInt16();
		reserved1 = reader.ReadUInt32();
		stringLength = reader.ReadUInt32();
		versionString = ReadString(ref reader, stringLength, runtime);
		offset2ndPart = (FileOffset)reader.CurrentOffset;
		flags = (StorageFlags)reader.ReadByte();
		reserved2 = reader.ReadByte();
		streams = reader.ReadUInt16();
		streamHeaders = new StreamHeader[streams];
		for (int i = 0; i < streamHeaders.Count; i++)
		{
			bool failedVerification;
			StreamHeader streamHeader = new StreamHeader(ref reader, throwOnError: false, verify, runtime, out failedVerification);
			if (failedVerification || (ulong)((long)streamHeader.Offset + (long)streamHeader.StreamSize) > (ulong)reader.EndOffset)
			{
				streamHeader = new StreamHeader(0u, 0u, "<invalid>");
			}
			streamHeaders[i] = streamHeader;
		}
		SetEndoffset(ref reader);
	}

	private static string ReadString(ref DataReader reader, uint maxLength, CLRRuntimeReaderKind runtime)
	{
		ulong num = (ulong)reader.CurrentOffset + (ulong)maxLength;
		if (runtime == CLRRuntimeReaderKind.Mono)
		{
			num = (num + 3) / 4 * 4;
		}
		if (num > reader.EndOffset)
		{
			throw new BadImageFormatException("Invalid MD version string");
		}
		byte[] array = new byte[maxLength];
		uint num2;
		for (num2 = 0u; num2 < maxLength; num2++)
		{
			byte b = reader.ReadByte();
			if (b == 0)
			{
				break;
			}
			array[num2] = b;
		}
		reader.CurrentOffset = (uint)num;
		return Encoding.UTF8.GetString(array, 0, (int)num2);
	}
}
