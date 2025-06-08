using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public class HttpClientHandler : HttpMessageHandler
{
	public virtual extern bool SupportsAutomaticDecompression { get; }

	public virtual extern bool SupportsProxy { get; }

	public virtual extern bool SupportsRedirectConfiguration { get; }

	public extern bool UseCookies { get; set; }

	public extern CookieContainer CookieContainer { get; set; }

	public extern ClientCertificateOption ClientCertificateOptions { get; set; }

	public extern DecompressionMethods AutomaticDecompression { get; set; }

	public extern bool UseProxy { get; set; }

	public extern IWebProxy Proxy
	{
		get; [SecuritySafeCritical]
		set;
	}

	public extern bool PreAuthenticate { get; set; }

	public extern bool UseDefaultCredentials { get; set; }

	public extern ICredentials Credentials { get; set; }

	public extern bool AllowAutoRedirect { get; set; }

	public extern int MaxAutomaticRedirections { get; set; }

	public extern long MaxRequestContentBufferSize { get; set; }

	public extern HttpClientHandler();

	protected override extern void Dispose(bool disposing);

	internal virtual extern void InitializeWebRequest(HttpRequestMessage request, HttpWebRequest webRequest);

	protected internal override extern Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

	internal extern void CheckDisposedOrStarted();
}
