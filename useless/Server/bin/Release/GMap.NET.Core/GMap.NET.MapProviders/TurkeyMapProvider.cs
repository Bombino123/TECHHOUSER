using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class TurkeyMapProvider : GMapProvider
{
	public static readonly TurkeyMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string Zeros;

	private static readonly string Slash;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("EDE895BD-756D-4BE4-8D03-D54DD8856F1D");


	public override string Name { get; } = "TurkeyMap";


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

	public override PureProjection Projection => MercatorProjection.Instance;

	private TurkeyMapProvider()
	{
		Copyright = string.Format("©{0} Pergo - Map data ©{0} Fideltus Advanced Technology", DateTime.Today.Year);
		Area = new RectLatLng(42.5830078125, 25.48828125, 19.05029296875, 6.83349609375);
		InvertedAxisY = true;
	}

	static TurkeyMapProvider()
	{
		Zeros = "000000000";
		Slash = "/";
		UrlFormat = "http://map{0}.pergo.com.tr/publish/tile/tile9913/{1:00}/{2}/{3}.png";
		Instance = new TurkeyMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		string text = pos.X.ToString(Zeros).Insert(3, Slash).Insert(7, Slash);
		string text2 = pos.Y.ToString(Zeros).Insert(3, Slash).Insert(7, Slash);
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3), zoom, text, text2);
	}
}
