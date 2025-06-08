using System;

namespace GMap.NET.MapProviders;

public class OpenSeaMapHybridProvider : OpenStreetMapProviderBase
{
	public static readonly OpenSeaMapHybridProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("FAACDE73-4B90-4AE6-BB4A-ADE4F3545592");


	public override string Name { get; } = "OpenSeaMapHybrid";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					OpenStreetMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private OpenSeaMapHybridProvider()
	{
		RefererUrl = "http://openseamap.org/";
	}

	static OpenSeaMapHybridProvider()
	{
		UrlFormat = "http://tiles.openseamap.org/seamark/{0}/{1}/{2}.png";
		Instance = new OpenSeaMapHybridProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, zoom, pos.X, pos.Y);
	}
}
