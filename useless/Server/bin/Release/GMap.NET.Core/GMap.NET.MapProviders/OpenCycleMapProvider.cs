using System;

namespace GMap.NET.MapProviders;

public class OpenCycleMapProvider : OpenStreetMapProviderBase
{
	public static readonly OpenCycleMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("D7E1826E-EE1E-4441-9F15-7C2DE0FE0B0A");


	public override string Name { get; } = "OpenCycleMap";


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

	private OpenCycleMapProvider()
	{
		RefererUrl = "http://www.opencyclemap.org/";
	}

	static OpenCycleMapProvider()
	{
		UrlFormat = "http://{0}.tile.opencyclemap.org/cycle/{1}/{2}/{3}.png";
		Instance = new OpenCycleMapProvider();
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
