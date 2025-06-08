using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NameQueryRequest
{
	public NameServicePacketHeader Header;

	public QuestionSection Question;

	public NameQueryRequest()
	{
		Header = new NameServicePacketHeader();
		Header.OpCode = NameServiceOperation.QueryRequest;
		Header.Flags = OperationFlags.RecursionDesired;
		Question = new QuestionSection();
		Question.Type = NameRecordType.NB;
	}

	public NameQueryRequest(byte[] buffer, int offset)
	{
		Header = new NameServicePacketHeader(buffer, ref offset);
		Question = new QuestionSection(buffer, ref offset);
	}

	public byte[] GetBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		Header.WriteBytes(memoryStream);
		Question.WriteBytes(memoryStream);
		return memoryStream.ToArray();
	}
}
