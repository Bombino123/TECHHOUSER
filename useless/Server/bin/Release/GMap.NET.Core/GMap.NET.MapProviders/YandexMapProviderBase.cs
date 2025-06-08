using System;
using GMap.NET.Internals;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class YandexMapProviderBase : GMapProvider
{
	private GMapProvider[] _overlays;

	protected string Version = "4.6.9";

	public readonly string Server = Stuff.GString("MECxW6okUK3Ir7a9ue/vIA==");

	public readonly string ServerRu = Stuff.GString("MECxW6okUK0FRlRPbF0BQg==");

	public readonly string ServerCom = Stuff.GString("MECxW6okUK2JNHOW5AuimA==");

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

	public override PureProjection Projection => MercatorProjectionYandex.Instance;

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

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}
}
