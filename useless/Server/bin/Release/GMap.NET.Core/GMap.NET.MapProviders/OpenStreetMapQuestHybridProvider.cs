using System;

namespace GMap.NET.MapProviders;

public class OpenStreetMapQuestHybridProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreetMapQuestHybridProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("95E05027-F846-4429-AB7A-9445ABEEFA2A");


	public override string Name { get; } = "OpenStreetMapQuestHybrid";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					OpenStreetMapQuestSatelliteProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private OpenStreetMapQuestHybridProvider()
	{
		Copyright = $"© MapQuest - Map data ©{DateTime.Today.Year} MapQuest, OpenStreetMap";
	}

	static OpenStreetMapQuestHybridProvider()
	{
		UrlFormat = "http://otile{0}.mqcdn.com/tiles/1.0.0/hyb/{1}/{2}/{3}.png";
		Instance = new OpenStreetMapQuestHybridProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
	}
}
