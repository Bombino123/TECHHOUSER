using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class LatviaMapProviderBase : GMapProvider
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

	public override PureProjection Projection => LKS92Projection.Instance;

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

	public LatviaMapProviderBase()
	{
		RefererUrl = "http://www.ikarte.lv/default.aspx?lang=en";
		Copyright = string.Format("©{0} Hnit-Baltic - Map data ©{0} LR Valsts zemes dieniests, SIA Envirotech", DateTime.Today.Year);
		MaxZoom = 11;
		Area = new RectLatLng(58.0794870805093, 20.3286067123543, 7.90883164336887, 2.506129113082);
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
