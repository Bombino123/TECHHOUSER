using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class HereMapProviderBase : GMapProvider
{
	public string AppId = string.Empty;

	public string AppCode = string.Empty;

	private GMapProvider[] _overlays;

	protected static readonly string UrlServerLetters = "1234";

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

	public HereMapProviderBase()
	{
		MaxZoom = null;
		RefererUrl = "http://wego.here.com/";
		Copyright = string.Format("©{0} Here - Map data ©{0} NAVTEQ, Imagery ©{0} DigitalGlobe", DateTime.Today.Year);
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
