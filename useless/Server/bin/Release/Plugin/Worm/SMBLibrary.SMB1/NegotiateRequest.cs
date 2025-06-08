using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NegotiateRequest : SMB1Command
{
	public const int SupportedBufferFormat = 2;

	public List<string> Dialects = new List<string>();

	public override CommandName CommandName => CommandName.SMB_COM_NEGOTIATE;

	public NegotiateRequest()
	{
	}

	public NegotiateRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		string text;
		for (int i = 0; i < SMBData.Length; i += text.Length + 1)
		{
			if (ByteReader.ReadByte(SMBData, ref i) != 2)
			{
				throw new InvalidDataException("Unsupported Buffer Format");
			}
			text = ByteReader.ReadNullTerminatedAnsiString(SMBData, i);
			Dialects.Add(text);
		}
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		int num = 0;
		foreach (string dialect in Dialects)
		{
			num += 1 + dialect.Length + 1;
		}
		SMBParameters = new byte[0];
		SMBData = new byte[num];
		int num2 = 0;
		foreach (string dialect2 in Dialects)
		{
			ByteWriter.WriteByte(SMBData, num2, 2);
			ByteWriter.WriteAnsiString(SMBData, num2 + 1, dialect2, dialect2.Length);
			ByteWriter.WriteByte(SMBData, num2 + 1 + dialect2.Length, 0);
			num2 += 1 + dialect2.Length + 1;
		}
		return base.GetBytes(isUnicode);
	}
}
