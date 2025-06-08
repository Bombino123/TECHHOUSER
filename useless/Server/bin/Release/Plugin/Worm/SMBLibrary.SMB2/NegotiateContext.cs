using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class NegotiateContext
{
	public const int FixedLength = 8;

	public NegotiateContextType ContextType;

	private ushort DataLength;

	public uint Reserved;

	public byte[] Data = new byte[0];

	public int Length => 8 + Data.Length;

	public NegotiateContext()
	{
	}

	public NegotiateContext(byte[] buffer, int offset)
	{
		ContextType = (NegotiateContextType)LittleEndianConverter.ToUInt16(buffer, offset);
		DataLength = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		ByteReader.ReadBytes(buffer, offset + 8, DataLength);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		DataLength = (ushort)Data.Length;
		LittleEndianWriter.WriteUInt16(buffer, offset, (ushort)ContextType);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, DataLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, Reserved);
		ByteWriter.WriteBytes(buffer, offset + 8, Data);
	}

	public static List<NegotiateContext> ReadNegotiateContextList(byte[] buffer, int offset, int count)
	{
		List<NegotiateContext> list = new List<NegotiateContext>();
		for (int i = 0; i < count; i++)
		{
			NegotiateContext negotiateContext = new NegotiateContext(buffer, offset);
			list.Add(negotiateContext);
			offset += negotiateContext.Length;
		}
		return list;
	}

	public static void WriteNegotiateContextList(byte[] buffer, int offset, List<NegotiateContext> negotiateContextList)
	{
		for (int i = 0; i < negotiateContextList.Count; i++)
		{
			NegotiateContext negotiateContext = negotiateContextList[i];
			int num = (int)Math.Ceiling((double)negotiateContext.Length / 8.0) * 8;
			negotiateContext.WriteBytes(buffer, offset);
			offset += num;
		}
	}

	public static int GetNegotiateContextListLength(List<NegotiateContext> negotiateContextList)
	{
		int num = 0;
		for (int i = 0; i < negotiateContextList.Count; i++)
		{
			int length = negotiateContextList[i].Length;
			if (i < negotiateContextList.Count - 1)
			{
				int num2 = (int)Math.Ceiling((double)length / 8.0) * 8;
				num += num2;
			}
			else
			{
				num += length;
			}
		}
		return num;
	}
}
