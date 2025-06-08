using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NodeStatusResponse
{
	public NameServicePacketHeader Header;

	public ResourceRecord Resource;

	public KeyValuePairList<string, NameFlags> Names = new KeyValuePairList<string, NameFlags>();

	public NodeStatistics Statistics;

	public NodeStatusResponse()
	{
		Header = new NameServicePacketHeader();
		Header.OpCode = NameServiceOperation.QueryResponse;
		Header.Flags = OperationFlags.RecursionAvailable | OperationFlags.AuthoritativeAnswer;
		Header.ANCount = 1;
		Resource = new ResourceRecord(NameRecordType.NBStat);
		Statistics = new NodeStatistics();
	}

	public NodeStatusResponse(byte[] buffer, int offset)
	{
		Header = new NameServicePacketHeader(buffer, ref offset);
		Resource = new ResourceRecord(buffer, ref offset);
		int offset2 = 0;
		byte b = ByteReader.ReadByte(Resource.Data, ref offset2);
		for (int i = 0; i < b; i++)
		{
			string key = ByteReader.ReadAnsiString(Resource.Data, ref offset2, 16);
			NameFlags value = (NameFlags)BigEndianReader.ReadUInt16(Resource.Data, ref offset2);
			Names.Add(key, value);
		}
		Statistics = new NodeStatistics(Resource.Data, ref offset2);
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
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.WriteByte((byte)Names.Count);
		foreach (KeyValuePair<string, NameFlags> name in Names)
		{
			ByteWriter.WriteAnsiString(memoryStream, name.Key);
			BigEndianWriter.WriteUInt16(memoryStream, (ushort)name.Value);
		}
		ByteWriter.WriteBytes(memoryStream, Statistics.GetBytes());
		return memoryStream.ToArray();
	}
}
