using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class SwissTopoProvider : GMapProvider
{
	private readonly Guid _id = new Guid("0F1F1EC5-B297-4B5B-8EB4-27AA403D1860");

	private readonly string _name = "SwissTopo";

	private readonly Random _randomGen;

	public static readonly SwissTopoProvider Instance;

	private GMapProvider[] _overlays;

	public override Guid Id => _id;

	public override string Name => _name;

	public override PureProjection Projection => SwissTopoProjection.Instance;

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

	private SwissTopoProvider()
	{
		MaxZoom = null;
		_randomGen = new Random();
	}

	private string MakeTileImageUrl(GPoint pos, int zoom)
	{
		int num = 10;
		int num2 = _randomGen.Next() % num;
		string text = "ch.swisstopo.pixelkarte-farbe";
		string text2 = "2056";
		string text3 = "current";
		return $"https://wmts{num2}.geo.admin.ch/1.0.0/{text}/default/{text3}/{text2}/{zoom}/{pos.X}/{pos.Y}.jpeg";
	}

	static SwissTopoProvider()
	{
		Instance = new SwissTopoProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom);
		return GetTileImageUsingHttp(url);
	}
}
