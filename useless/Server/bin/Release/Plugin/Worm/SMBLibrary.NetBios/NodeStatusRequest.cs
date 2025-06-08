using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NodeStatusRequest
{
	public NameServicePacketHeader Header;

	public QuestionSection Question;

	public NodeStatusRequest()
	{
		Header = new NameServicePacketHeader();
		Header.OpCode = NameServiceOperation.QueryRequest;
		Question = new QuestionSection();
		Question.Type = NameRecordType.NBStat;
	}

	public NodeStatusRequest(byte[] buffer, int offset)
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
