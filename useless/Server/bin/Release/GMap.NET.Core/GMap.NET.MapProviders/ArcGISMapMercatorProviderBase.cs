using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class ArcGISMapMercatorProviderBase : GMapProvider
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

	public ArcGISMapMercatorProviderBase()
	{
		MaxZoom = null;
		Copyright = string.Format("©{0} ESRI - Map data ©{0} ArcGIS", DateTime.Today.Year);
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
