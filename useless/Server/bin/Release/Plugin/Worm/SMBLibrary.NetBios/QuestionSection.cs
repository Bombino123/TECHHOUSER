using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class QuestionSection
{
	public string Name;

	public NameRecordType Type;

	public QuestionClass Class;

	public QuestionSection()
	{
		Class = QuestionClass.In;
	}

	public QuestionSection(byte[] buffer, ref int offset)
	{
		Name = NetBiosUtils.DecodeName(buffer, ref offset);
		Type = (NameRecordType)BigEndianReader.ReadUInt16(buffer, ref offset);
		Class = (QuestionClass)BigEndianReader.ReadUInt16(buffer, ref offset);
	}

	public void WriteBytes(Stream stream)
	{
		byte[] bytes = NetBiosUtils.EncodeName(Name, string.Empty);
		ByteWriter.WriteBytes(stream, bytes);
		BigEndianWriter.WriteUInt16(stream, (ushort)Type);
		BigEndianWriter.WriteUInt16(stream, (ushort)Class);
	}
}
