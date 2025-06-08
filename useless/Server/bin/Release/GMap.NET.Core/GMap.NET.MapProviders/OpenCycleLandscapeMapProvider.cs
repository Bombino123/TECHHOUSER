using System;

namespace GMap.NET.MapProviders;

public class OpenCycleLandscapeMapProvider : OpenStreetMapProviderBase
{
	public static readonly OpenCycleLandscapeMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("BDBAA939-6597-4D87-8F4F-261C49E35F56");


	public override string Name { get; } = "OpenCycleLandscapeMap";


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

	private OpenCycleLandscapeMapProvider()
	{
		RefererUrl = "http://www.opencyclemap.org/";
	}

	static OpenCycleLandscapeMapProvider()
	{
		UrlFormat = "http://{0}.tile3.opencyclemap.org/landscape/{1}/{2}/{3}.png";
		Instance = new OpenCycleLandscapeMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		char c = ServerLetters[GMapProvider.GetServerNum(pos, 3)];
		return string.Format(UrlFormat, c, zoom, pos.X, pos.Y);
	}
}
