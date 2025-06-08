using System.Runtime.InteropServices;
using System.Text;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class NDRUnicodeString : INDRStructure
{
	private bool m_writeNullTerminationCharacter;

	public string Value;

	public NDRUnicodeString()
		: this(string.Empty, writeNullTerminationCharacter: true)
	{
	}

	public NDRUnicodeString(string value)
		: this(value, writeNullTerminationCharacter: true)
	{
	}

	public NDRUnicodeString(string value, bool writeNullTerminationCharacter)
	{
		m_writeNullTerminationCharacter = writeNullTerminationCharacter;
		Value = value;
	}

	public NDRUnicodeString(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.ReadUInt32();
		parser.ReadUInt32();
		uint num = parser.ReadUInt32();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append((char)parser.ReadUInt16());
		}
		Value = stringBuilder.ToString().TrimEnd(new char[1]);
	}

	public void Write(NDRWriter writer)
	{
		string text = string.Empty;
		if (Value != null)
		{
			text = Value;
		}
		if (m_writeNullTerminationCharacter)
		{
			text += "\0";
		}
		uint length = (uint)text.Length;
		writer.WriteUInt32(length);
		uint value = 0u;
		writer.WriteUInt32(value);
		uint length2 = (uint)text.Length;
		writer.WriteUInt32(length2);
		for (int i = 0; i < text.Length; i++)
		{
			writer.WriteUInt16(text[i]);
		}
	}
}
