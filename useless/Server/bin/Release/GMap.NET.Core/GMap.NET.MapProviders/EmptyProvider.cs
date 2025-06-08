using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class EmptyProvider : GMapProvider
{
	public static readonly EmptyProvider Instance;

	private readonly MercatorProjection _projection = MercatorProjection.Instance;

	public override Guid Id => Guid.Empty;

	public override string Name { get; } = "None";


	public override PureProjection Projection => _projection;

	public override GMapProvider[] Overlays => null;

	private EmptyProvider()
	{
		MaxZoom = null;
	}

	static EmptyProvider()
	{
		Instance = new EmptyProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		return null;
	}
}
