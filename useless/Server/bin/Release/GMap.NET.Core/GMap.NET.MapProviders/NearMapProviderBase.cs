using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public abstract class NearMapProviderBase : GMapProvider
{
	private GMapProvider[] _overlays;

	private static readonly string SecureStr = "Vk52edzNRYKbGjF8Ur0WhmQlZs4wgipDETyL1oOMXIAvqtxJBuf7H36acCnS9P";

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

	public NearMapProviderBase()
	{
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		throw new NotImplementedException();
	}

	public new static int GetServerNum(GPoint pos, int max)
	{
		return (int)(pos.X & 2) % max;
	}

	public string GetSafeString(GPoint pos)
	{
		string text = pos.X.ToString() + pos.Y + (3 * pos.X + pos.Y);
		string text2 = "&s=";
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			num += int.Parse(text[i].ToString());
			text2 += SecureStr[num % SecureStr.Length];
		}
		return text2;
	}
}
