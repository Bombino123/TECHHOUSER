namespace System.Net.Http;

public class MultipartFormDataContent : MultipartContent
{
	public extern MultipartFormDataContent();

	public extern MultipartFormDataContent(string boundary);

	public override extern void Add(HttpContent content);

	public extern void Add(HttpContent content, string name);

	public extern void Add(HttpContent content, string name, string fileName);
}
