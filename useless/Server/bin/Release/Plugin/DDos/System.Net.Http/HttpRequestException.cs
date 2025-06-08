namespace System.Net.Http;

[Serializable]
public class HttpRequestException : Exception
{
	public extern HttpRequestException();

	public extern HttpRequestException(string message);

	public extern HttpRequestException(string message, Exception inner);
}
