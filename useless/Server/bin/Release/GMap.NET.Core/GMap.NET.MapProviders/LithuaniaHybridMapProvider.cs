using System;

namespace GMap.NET.MapProviders;

public class LithuaniaHybridMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaHybridMapProvider Instance;

	private GMapProvider[] _overlays;

	internal static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("279AB0E0-4704-4AA6-86AD-87D13B1F8975");


	public override string Name { get; } = "LithuaniaHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					LithuaniaOrtoFotoMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private LithuaniaHybridMapProvider()
	{
	}

	static LithuaniaHybridMapProvider()
	{
		UrlFormat = "http://dc5.maps.lt/cache/mapslt_ortofoto_overlay/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png";
		Instance = new LithuaniaHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, zoom, pos.Y, pos.X);
	}
}
