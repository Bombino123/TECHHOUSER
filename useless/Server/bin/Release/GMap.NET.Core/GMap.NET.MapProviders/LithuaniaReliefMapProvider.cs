using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class LithuaniaReliefMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaReliefMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("85F89205-1062-4F10-B536-90CD8B2F1B7D");


	public override string Name { get; } = "LithuaniaReliefMap";


	public override PureProjection Projection => LKS94rProjection.Instance;

	private LithuaniaReliefMapProvider()
	{
	}

	static LithuaniaReliefMapProvider()
	{
		UrlFormat = "http://dc5.maps.lt/cache/mapslt_relief_vector/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png";
		Instance = new LithuaniaReliefMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, zoom, pos.Y, pos.X);
	}
}
