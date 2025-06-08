using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class CustomMapProvider : GMapProvider
{
	public static readonly CustomMapProvider Instance;

	private GMapProvider[] _overlays;

	public string CustomServerUrl = string.Empty;

	public string CustomServerLetters = string.Empty;

	public override Guid Id { get; } = new Guid("BEAB409B-6ED0-443F-B8E3-E6CC6F019F66");


	public override string Name { get; } = "CustomMapProvider";


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

	private CustomMapProvider()
	{
		RefererUrl = CustomServerUrl;
	}

	static CustomMapProvider()
	{
		Instance = new CustomMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		string format = CustomServerUrl.Replace("{l}", "{0}").Replace("{z}", "{1}").Replace("{x}", "{2}")
			.Replace("{y}", "{3}");
		string text = (string.IsNullOrEmpty(CustomServerLetters) ? "" : CustomServerLetters[GMapProvider.GetServerNum(pos, 3)].ToString());
		return string.Format(format, text, zoom, pos.X, pos.Y);
	}
}
