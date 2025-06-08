using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class SwedenMapProviderBase : GMapProvider
{
	private GMapProvider[] _overlays;

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

	public override PureProjection Projection => SWEREF99_TMProjection.Instance;

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

	public SwedenMapProviderBase()
	{
		RefererUrl = "https://kso.etjanster.lantmateriet.se/?lang=en";
		Copyright = $"©{DateTime.Today.Year} Lantmäteriet";
		MaxZoom = 11;
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
