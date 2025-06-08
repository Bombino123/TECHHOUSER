using System;

namespace GMap.NET.MapProviders;

public class OpenCycleTransportMapProvider : OpenStreetMapProviderBase
{
	public static readonly OpenCycleTransportMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("AF66DF88-AD25-43A9-8F82-56FCA49A748A");


	public override string Name { get; } = "OpenCycleTransportMap";


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

	private OpenCycleTransportMapProvider()
	{
		RefererUrl = "http://www.opencyclemap.org/";
	}

	static OpenCycleTransportMapProvider()
	{
		UrlFormat = "http://{0}.tile2.opencyclemap.org/transport/{1}/{2}/{3}.png";
		Instance = new OpenCycleTransportMapProvider();
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
