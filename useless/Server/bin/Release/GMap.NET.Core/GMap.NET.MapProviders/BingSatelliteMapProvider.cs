using System;

namespace GMap.NET.MapProviders;

public class BingSatelliteMapProvider : BingMapProviderBase
{
	public static readonly BingSatelliteMapProvider Instance;

	private string _urlDynamicFormat = string.Empty;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("3AC742DD-966B-4CFB-B67D-33E7F82F2D37");


	public override string Name { get; } = "BingSatelliteMap";


	private BingSatelliteMapProvider()
	{
	}

	static BingSatelliteMapProvider()
	{
		UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}&n=z{4}";
		Instance = new BingSatelliteMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	public override void OnInitialized()
	{
		base.OnInitialized();
		if (!DisableDynamicTileUrlFormat)
		{
			_urlDynamicFormat = GetTileUrl("Aerial");
			if (!string.IsNullOrEmpty(_urlDynamicFormat))
			{
				_urlDynamicFormat = _urlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}");
			}
		}
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		string text = TileXYToQuadKey(pos.X, pos.Y, zoom);
		if (!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(_urlDynamicFormat))
		{
			return string.Format(_urlDynamicFormat, GMapProvider.GetServerNum(pos, 4), text);
		}
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 4), text, Version, language, ForceSessionIdOnTileAccess ? ("&key=" + SessionId) : string.Empty);
	}
}
