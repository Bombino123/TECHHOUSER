using System.Collections.Generic;

namespace System.Net.Http;

public class FormUrlEncodedContent : ByteArrayContent
{
	public extern FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection);
}
