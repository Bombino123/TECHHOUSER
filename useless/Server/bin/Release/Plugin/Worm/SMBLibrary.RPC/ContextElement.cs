using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class ContextElement
{
	public ushort ContextID;

	public byte Reserved;

	public SyntaxID AbstractSyntax;

	public List<SyntaxID> TransferSyntaxList = new List<SyntaxID>();

	public int Length => 4 + 20 * (TransferSyntaxList.Count + 1);

	public ContextElement()
	{
	}

	public ContextElement(byte[] buffer, int offset)
	{
		ContextID = LittleEndianConverter.ToUInt16(buffer, offset);
		byte b = ByteReader.ReadByte(buffer, offset + 2);
		Reserved = ByteReader.ReadByte(buffer, offset + 3);
		AbstractSyntax = new SyntaxID(buffer, offset + 4);
		offset += 24;
		for (int i = 0; i < b; i++)
		{
			SyntaxID item = new SyntaxID(buffer, offset);
			TransferSyntaxList.Add(item);
			offset += 20;
		}
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		byte b = (byte)TransferSyntaxList.Count;
		LittleEndianWriter.WriteUInt16(buffer, offset, ContextID);
		ByteWriter.WriteByte(buffer, offset + 2, b);
		ByteWriter.WriteByte(buffer, offset + 3, Reserved);
		AbstractSyntax.WriteBytes(buffer, offset + 4);
		offset += 24;
		for (int i = 0; i < b; i++)
		{
			TransferSyntaxList[i].WriteBytes(buffer, offset);
			offset += 20;
		}
	}
}
