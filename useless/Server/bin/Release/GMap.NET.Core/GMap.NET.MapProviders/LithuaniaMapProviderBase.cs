using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class LithuaniaMapProviderBase : GMapProvider
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

	public override PureProjection Projection => LKS94Projection.Instance;

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

	public LithuaniaMapProviderBase()
	{
		RefererUrl = "http://www.maps.lt/map/";
		Copyright = string.Format("©{0} Hnit-Baltic - Map data ©{0} ESRI", DateTime.Today.Year);
		MaxZoom = 12;
		Area = new RectLatLng(56.431489960361, 20.8962105239809, 5.8924169643369, 2.58940626652217);
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
