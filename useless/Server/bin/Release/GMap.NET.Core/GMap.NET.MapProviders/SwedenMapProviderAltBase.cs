using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class SwedenMapProviderAltBase : GMapProvider
{
	private GMapProvider[] overlays;

	protected static readonly string UrlServerLetters = "bcde";

	public override Guid Id
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string Name
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override PureProjection Projection => MercatorProjection.Instance;

	public override GMapProvider[] Overlays
	{
		get
		{
			if (overlays == null)
			{
				overlays = new GMapProvider[1] { this };
			}
			return overlays;
		}
	}

	public SwedenMapProviderAltBase()
	{
		RefererUrl = "https://kso.etjanster.lantmateriet.se/?lang=en";
		Copyright = $"©{DateTime.Today.Year} Lantmäteriet";
		MaxZoom = 15;
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
