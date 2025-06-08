using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class PositiveNameQueryResponse
{
	public const int EntryLength = 6;

	public NameServicePacketHeader Header;

	public ResourceRecord Resource;

	public KeyValuePairList<byte[], NameFlags> Addresses = new KeyValuePairList<byte[], NameFlags>();

	public PositiveNameQueryResponse()
	{
		Header = new NameServicePacketHeader();
		Header.Flags = OperationFlags.RecursionDesired | OperationFlags.AuthoritativeAnswer;
		Header.OpCode = NameServiceOperation.QueryResponse;
		Header.ANCount = 1;
		Resource = new ResourceRecord(NameRecordType.NB);
	}

	public PositiveNameQueryResponse(byte[] buffer, int offset)
	{
		Header = new NameServicePacketHeader(buffer, ref offset);
		Resource = new ResourceRecord(buffer, ref offset);
		int offset2 = 0;
		while (offset2 < Resource.Data.Length)
		{
			NameFlags value = (NameFlags)BigEndianReader.ReadUInt16(Resource.Data, ref offset2);
			byte[] key = ByteReader.ReadBytes(Resource.Data, ref offset2, 4);
			Addresses.Add(key, value);
		}
	}

	public byte[] GetBytes()
	{
		Resource.Data = GetData();
		MemoryStream memoryStream = new MemoryStream();
		Header.WriteBytes(memoryStream);
		Resource.WriteBytes(memoryStream);
		return memoryStream.ToArray();
	}

	private byte[] GetData()
	{
		byte[] array = new byte[6 * Addresses.Count];
		int offset = 0;
		foreach (KeyValuePair<byte[], NameFlags> address in Addresses)
		{
			BigEndianWriter.WriteUInt16(array, ref offset, (ushort)address.Value);
			ByteWriter.WriteBytes(array, ref offset, address.Key, 4);
		}
		return array;
	}
}
