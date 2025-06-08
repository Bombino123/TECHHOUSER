using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class WikiMapiaMapProviderBase : GMapProvider
{
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
			throw new NotImplementedException();
		}
	}

	public WikiMapiaMapProviderBase()
	{
		MaxZoom = 22;
		RefererUrl = "http://wikimapia.org/";
		Copyright = $"© WikiMapia.org - Map data ©{DateTime.Today.Year} WikiMapia";
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public static int GetServerNum(GPoint pos)
	{
		return (int)(pos.X % 4 + pos.Y % 4 * 4);
	}
}
