using System;

namespace GMap.NET.MapProviders;

public class NearHybridMapProvider : NearMapProviderBase
{
	public static readonly NearHybridMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("4BF8819A-635D-4A94-8DC7-94C0E0F04BFD");


	public override string Name { get; } = "NearHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					NearSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private NearHybridMapProvider()
	{
	}

	static NearHybridMapProvider()
	{
		UrlFormat = "http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=MapT&nmg=1";
		Instance = new NearHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, NearMapProviderBase.GetServerNum(pos, 3), pos.X, pos.Y, zoom);
	}
}
