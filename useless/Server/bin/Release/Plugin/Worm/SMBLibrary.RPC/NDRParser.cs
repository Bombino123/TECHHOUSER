using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class NDRParser
{
	private byte[] m_buffer;

	private int m_offset;

	private int m_depth;

	private List<INDRStructure> m_deferredStructures = new List<INDRStructure>();

	private Dictionary<uint, INDRStructure> m_referentToInstance = new Dictionary<uint, INDRStructure>();

	public NDRParser(byte[] buffer)
	{
		m_buffer = buffer;
		m_offset = 0;
		m_depth = 0;
	}

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
			item.Read(this);
		}
	}

	public string ReadUnicodeString()
	{
		return new NDRUnicodeString(this).Value;
	}

	public void ReadStructure(INDRStructure structure)
	{
		structure.Read(this);
	}

	public string ReadTopLevelUnicodeStringPointer()
	{
		uint num = ReadUInt32();
		if (num == 0)
		{
			return null;
		}
		if (m_referentToInstance.ContainsKey(num))
		{
			return ((NDRUnicodeString)m_referentToInstance[num]).Value;
		}
		NDRUnicodeString nDRUnicodeString = new NDRUnicodeString(this);
		m_referentToInstance.Add(num, nDRUnicodeString);
		return nDRUnicodeString.Value;
	}

	public void ReadEmbeddedStructureFullPointer(ref NDRUnicodeString structure)
	{
		this.ReadEmbeddedStructureFullPointer<NDRUnicodeString>(ref structure);
	}

	public void ReadEmbeddedStructureFullPointer<T>(ref T structure) where T : INDRStructure, new()
	{
		if (ReadUInt32() != 0)
		{
			if (structure == null)
			{
				structure = new T();
			}
			AddDeferredStructure(structure);
		}
		else
		{
			structure = default(T);
		}
	}

	public uint ReadUInt16()
	{
		m_offset += (2 - m_offset % 2) % 2;
		return LittleEndianReader.ReadUInt16(m_buffer, ref m_offset);
	}

	public uint ReadUInt32()
	{
		m_offset += (4 - m_offset % 4) % 4;
		return LittleEndianReader.ReadUInt32(m_buffer, ref m_offset);
	}

	public byte[] ReadBytes(int count)
	{
		return ByteReader.ReadBytes(m_buffer, ref m_offset, count);
	}
}
