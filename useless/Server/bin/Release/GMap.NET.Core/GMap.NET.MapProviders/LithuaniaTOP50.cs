using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class LithuaniaTOP50 : GMapProvider
{
	public static readonly LithuaniaTOP50 Instance;

	private GMapProvider[] _overlays;

	public override Guid Id { get; } = new Guid("2920B1AF-6D57-4895-9A21-D5837CBF1049");


	public override string Name => "LithuaniaTOP50";

	public override PureProjection Projection => MercatorProjection.Instance;

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

	private LithuaniaTOP50()
	{
		MaxZoom = 15;
	}

	static LithuaniaTOP50()
	{
		Instance = new LithuaniaTOP50();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		return null;
	}
}
