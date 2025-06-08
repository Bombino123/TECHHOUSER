using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using GMap.NET.Internals;

namespace GMap.NET.MapProviders;

public abstract class GMapProvider
{
	private static readonly List<GMapProvider> MapProviders;

	public readonly int DbId;

	public RectLatLng? Area;

	public int MinZoom;

	public int? MaxZoom = 17;

	public static IWebProxy WebProxy;

	public static bool IsSocksProxy;

	public static Func<GMapProvider, string, WebRequest> WebRequestFactory;

	public static ICredentials Credential;

	public static string UserAgent;

	public static int TimeoutMs;

	public static int TTLCache;

	public string RefererUrl = string.Empty;

	public string Copyright = string.Empty;

	public bool InvertedAxisY;

	private static LanguageType _language;

	public bool BypassCache;

	internal static PureImageProxy TileImageProxy;

	private static readonly string requestAccept;

	private static readonly string responseContentType;

	private string _authorization = string.Empty;

	public abstract Guid Id { get; }

	public abstract string Name { get; }

	public abstract PureProjection Projection { get; }

	public abstract GMapProvider[] Overlays { get; }

	public bool IsInitialized { get; internal set; }

	public static string LanguageStr { get; private set; }

	public static LanguageType Language
	{
		get
		{
			return _language;
		}
		set
		{
			_language = value;
			LanguageStr = Stuff.EnumToString(Language);
		}
	}

	public abstract PureImage GetTileImage(GPoint pos, int zoom);

	protected GMapProvider()
	{
		using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
		{
			DbId = Math.Abs(BitConverter.ToInt32(sHA1CryptoServiceProvider.ComputeHash(Id.ToByteArray()), 0));
		}
		if (MapProviders.Exists((GMapProvider p) => p.Id == Id || p.DbId == DbId))
		{
			throw new Exception("such provider id already exists, try regenerate your provider guid...");
		}
		MapProviders.Add(this);
	}

	static GMapProvider()
	{
		MapProviders = new List<GMapProvider>();
		IsSocksProxy = false;
		WebRequestFactory = null;
		UserAgent = string.Format("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:{0}.0) Gecko/20100101 Firefox/{0}.0", Stuff.Random.Next((DateTime.Today.Year - 2012) * 10 - 10, (DateTime.Today.Year - 2012) * 10));
		TimeoutMs = 5000;
		TTLCache = 240;
		LanguageStr = "en";
		_language = LanguageType.English;
		TileImageProxy = DefaultImageProxy.Instance;
		requestAccept = "*/*";
		responseContentType = "image";
		WebProxy = EmptyWebProxy.Instance;
	}

	public virtual void OnInitialized()
	{
	}

	protected virtual bool CheckTileImageHttpResponse(WebResponse response)
	{
		return response.ContentType.Contains(responseContentType);
	}

	public void ForceBasicHttpAuthentication(string userName, string userPassword)
	{
		_authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + userPassword));
	}

	protected virtual void InitializeWebRequest(WebRequest request)
	{
	}

	protected PureImage GetTileImageUsingHttp(string url)
	{
		PureImage pureImage = null;
		WebRequest webRequest = (IsSocksProxy ? SocksHttpWebRequest.Create(url) : ((WebRequestFactory != null) ? WebRequestFactory(this, url) : WebRequest.Create(url)));
		if (WebProxy != null)
		{
			webRequest.Proxy = WebProxy;
		}
		if (Credential != null)
		{
			webRequest.PreAuthenticate = true;
			webRequest.Credentials = Credential;
		}
		if (!string.IsNullOrEmpty(_authorization))
		{
			webRequest.Headers.Set("Authorization", _authorization);
		}
		if (webRequest is HttpWebRequest httpWebRequest)
		{
			httpWebRequest.UserAgent = UserAgent;
			httpWebRequest.ReadWriteTimeout = TimeoutMs * 6;
			httpWebRequest.Accept = requestAccept;
			if (!string.IsNullOrEmpty(RefererUrl))
			{
				httpWebRequest.Referer = RefererUrl;
			}
			httpWebRequest.Timeout = TimeoutMs;
		}
		else
		{
			if (!string.IsNullOrEmpty(UserAgent))
			{
				webRequest.Headers.Add("User-Agent", UserAgent);
			}
			if (!string.IsNullOrEmpty(requestAccept))
			{
				webRequest.Headers.Add("Accept", requestAccept);
			}
			if (!string.IsNullOrEmpty(RefererUrl))
			{
				webRequest.Headers.Add("Referer", RefererUrl);
			}
		}
		InitializeWebRequest(webRequest);
		using WebResponse webResponse = webRequest.GetResponse();
		if (CheckTileImageHttpResponse(webResponse))
		{
			using Stream inputStream = webResponse.GetResponseStream();
			MemoryStream memoryStream = Stuff.CopyStream(inputStream, seekOriginBegin: false);
			if (memoryStream.Length > 0)
			{
				pureImage = TileImageProxy.FromStream(memoryStream);
				if (pureImage != null)
				{
					pureImage.Data = memoryStream;
					pureImage.Data.Position = 0L;
				}
				else
				{
					memoryStream.Dispose();
				}
			}
		}
		webResponse.Close();
		return pureImage;
	}

	protected string GetContentUsingHttp(string url)
	{
		string result = string.Empty;
		WebRequest webRequest = (IsSocksProxy ? SocksHttpWebRequest.Create(url) : ((WebRequestFactory != null) ? WebRequestFactory(this, url) : WebRequest.Create(url)));
		if (WebProxy != null)
		{
			webRequest.Proxy = WebProxy;
		}
		if (Credential != null)
		{
			webRequest.PreAuthenticate = true;
			webRequest.Credentials = Credential;
		}
		if (!string.IsNullOrEmpty(_authorization))
		{
			webRequest.Headers.Set("Authorization", _authorization);
		}
		if (webRequest is HttpWebRequest httpWebRequest)
		{
			httpWebRequest.UserAgent = UserAgent;
			httpWebRequest.ReadWriteTimeout = TimeoutMs * 6;
			httpWebRequest.Accept = requestAccept;
			httpWebRequest.Referer = RefererUrl;
			httpWebRequest.Timeout = TimeoutMs;
		}
		else
		{
			if (!string.IsNullOrEmpty(UserAgent))
			{
				webRequest.Headers.Add("User-Agent", UserAgent);
			}
			if (!string.IsNullOrEmpty(requestAccept))
			{
				webRequest.Headers.Add("Accept", requestAccept);
			}
			if (!string.IsNullOrEmpty(RefererUrl))
			{
				webRequest.Headers.Add("Referer", RefererUrl);
			}
		}
		InitializeWebRequest(webRequest);
		WebResponse webResponse;
		try
		{
			webResponse = webRequest.GetResponse();
		}
		catch (WebException ex)
		{
			webResponse = (HttpWebResponse)ex.Response;
		}
		catch (Exception)
		{
			webResponse = null;
		}
		if (webResponse != null)
		{
			using (Stream stream = webResponse.GetResponseStream())
			{
				using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
				result = streamReader.ReadToEnd();
			}
			webResponse.Close();
		}
		return result;
	}

	protected virtual PureImage GetTileImageFromFile(string fileName)
	{
		return GetTileImageFromArray(File.ReadAllBytes(fileName));
	}

	protected virtual PureImage GetTileImageFromArray(byte[] data)
	{
		return TileImageProxy.FromArray(data);
	}

	protected static int GetServerNum(GPoint pos, int max)
	{
		return (int)(pos.X + 2 * pos.Y) % max;
	}

	public override int GetHashCode()
	{
		return DbId;
	}

	public override bool Equals(object obj)
	{
		if (obj is GMapProvider)
		{
			return Id.Equals((obj as GMapProvider).Id);
		}
		return false;
	}

	public override string ToString()
	{
		return Name;
	}
}
