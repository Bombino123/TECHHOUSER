using System.Text;

namespace System.Net.Http;

public class StringContent : ByteArrayContent
{
	public extern StringContent(string content);

	public extern StringContent(string content, Encoding encoding);

	public extern StringContent(string content, Encoding encoding, string mediaType);
}
