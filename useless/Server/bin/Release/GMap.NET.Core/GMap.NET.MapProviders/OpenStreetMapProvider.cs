using System;
using System.Net;

namespace GMap.NET.MapProviders;

public class OpenStreetMapProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreetMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("0521335C-92EC-47A8-98A5-6FD333DDA9C0");


	public override string Name { get; } = "OpenStreetMap";


	public string YoursClientName { get; set; }

	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[1] { this };
			}
			return _overlays;
		}
	}

	private OpenStreetMapProvider()
	{
	}

	static OpenStreetMapProvider()
	{
		UrlFormat = "https://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png";
		Instance = new OpenStreetMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	protected override void InitializeWebRequest(WebRequest request)
	{
		base.InitializeWebRequest(request);
		if (!string.IsNullOrEmpty(YoursClientName))
		{
			request.Headers.Add("X-Yours-client", YoursClientName);
		}
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		char c = ServerLetters[GMapProvider.GetServerNum(pos, 3)];
		return string.Format(UrlFormat, c, zoom, pos.X, pos.Y);
	}
}
