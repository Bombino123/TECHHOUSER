using System;
using System.Collections.Generic;
using System.IO;

namespace Leb128;

public class LEB128
{
	public static object[] Read(byte[] data)
	{
		List<object> list = new List<object>();
		using MemoryStream memoryStream = new MemoryStream(data);
		while (true)
		{
			int num = memoryStream.ReadByte();
			switch (num)
			{
			case 0:
				list.Add(LEB128Decoder.ReadLEB128(memoryStream));
				break;
			case 1:
				list.Add(LEB128Decoder.ReadLEB128Bool(memoryStream));
				break;
			case 2:
				list.Add(LEB128Decoder.ReadLEB128Bytes(memoryStream));
				break;
			case 3:
				list.Add(LEB128Decoder.ReadLEB128Double(memoryStream));
				break;
			case 4:
				list.Add(LEB128Decoder.ReadLEB128Float(memoryStream));
				break;
			case 5:
				list.Add(LEB128Decoder.ReadLEB128Int32(memoryStream));
				break;
			case 6:
				list.Add(LEB128Decoder.ReadLEB128Int64(memoryStream));
				break;
			case 7:
				list.Add(LEB128Decoder.ReadLEB128String(memoryStream));
				break;
			case 8:
				list.Add((byte)memoryStream.ReadByte());
				break;
			case -1:
				return list.ToArray();
			default:
				throw new Exception(num.ToString());
			}
		}
	}

	public static byte[] Write(object[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		foreach (object obj in data)
		{
			if (obj is ulong)
			{
				memoryStream.WriteByte(0);
				LEB128Encoder.WriteLEB128(memoryStream, (ulong)obj);
				continue;
			}
			if (obj is bool)
			{
				memoryStream.WriteByte(1);
				LEB128Encoder.WriteLEB128(memoryStream, (bool)obj);
				continue;
			}
			if (obj is byte[])
			{
				memoryStream.WriteByte(2);
				LEB128Encoder.WriteLEB128(memoryStream, (byte[])obj);
				continue;
			}
			if (obj is double)
			{
				memoryStream.WriteByte(3);
				LEB128Encoder.WriteLEB128(memoryStream, (double)obj);
				continue;
			}
			if (obj is float)
			{
				memoryStream.WriteByte(4);
				LEB128Encoder.WriteLEB128(memoryStream, (float)obj);
				continue;
			}
			if (obj is int)
			{
				memoryStream.WriteByte(5);
				LEB128Encoder.WriteLEB128(memoryStream, (int)obj);
				continue;
			}
			if (obj is long)
			{
				memoryStream.WriteByte(6);
				LEB128Encoder.WriteLEB128(memoryStream, (long)obj);
				continue;
			}
			if (obj is string)
			{
				memoryStream.WriteByte(7);
				LEB128Encoder.WriteLEB128(memoryStream, (string)obj);
				continue;
			}
			if (obj is byte)
			{
				memoryStream.WriteByte(8);
				memoryStream.WriteByte((byte)obj);
				continue;
			}
			throw new Exception(obj.GetType().Name);
		}
		return memoryStream.ToArray();
	}
}
