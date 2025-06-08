using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class NDRWriter
{
	private MemoryStream m_stream = new MemoryStream();

	private int m_depth;

	private List<INDRStructure> m_deferredStructures = new List<INDRStructure>();

	private Dictionary<uint, INDRStructure> m_referentToInstance = new Dictionary<uint, INDRStructure>();

	private uint m_nextReferentID = 131072u;

	public void BeginStructure()
	{
		m_depth++;
	}

	private void AddDeferredStructure(INDRStructure structure)
	{
		m_deferredStructures.Add(structure);
	}

	public void EndStructure()
	{
		m_depth--;
		if (m_depth != 0)
		{
			return;
		}
		List<INDRStructure> list = new List<INDRStructure>(m_deferredStructures);
		m_deferredStructures.Clear();
		foreach (INDRStructure item in list)
		{
			item.Write(this);
		}
	}

	public void WriteUnicodeString(string value)
	{
		new NDRUnicodeString(value).Write(this);
	}

	public void WriteStructure(INDRStructure structure)
	{
		structure.Write(this);
	}

	public void WriteTopLevelUnicodeStringPointer(string value)
	{
		if (value == null)
		{
			WriteUInt32(0u);
			return;
		}
		uint nextReferentID = GetNextReferentID();
		WriteUInt32(nextReferentID);
		NDRUnicodeString nDRUnicodeString = new NDRUnicodeString(value);
		nDRUnicodeString.Write(this);
		m_referentToInstance.Add(nextReferentID, nDRUnicodeString);
	}

	public void WriteEmbeddedStructureFullPointer(INDRStructure structure)
	{
		if (structure == null)
		{
			WriteUInt32(0u);
			return;
		}
		uint nextReferentID = GetNextReferentID();
		WriteUInt32(nextReferentID);
		AddDeferredStructure(structure);
		m_referentToInstance.Add(nextReferentID, structure);
	}

	public void WriteUInt16(ushort value)
	{
		uint num = (uint)(2 - m_stream.Position % 2) % 2;
		m_stream.Position += num;
		LittleEndianWriter.WriteUInt16(m_stream, value);
	}

	public void WriteUInt32(uint value)
	{
		uint num = (uint)(4 - m_stream.Position % 4) % 4;
		m_stream.Position += num;
		LittleEndianWriter.WriteUInt32(m_stream, value);
	}

	public void WriteBytes(byte[] value)
	{
		ByteWriter.WriteBytes(m_stream, value);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[m_stream.Length];
		m_stream.Seek(0L, SeekOrigin.Begin);
		m_stream.Read(array, 0, array.Length);
		return array;
	}

	private uint GetNextReferentID()
	{
		uint nextReferentID = m_nextReferentID;
		m_nextReferentID++;
		return nextReferentID;
	}
}
