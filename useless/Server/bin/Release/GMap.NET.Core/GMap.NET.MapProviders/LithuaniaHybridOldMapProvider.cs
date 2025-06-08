using System;

namespace GMap.NET.MapProviders;

public class LithuaniaHybridOldMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaHybridOldMapProvider Instance;

	private GMapProvider[] _overlays;

	public override Guid Id { get; } = new Guid("35C5C685-E868-4AC7-97BE-10A9A37A81B5");


	public override string Name { get; } = "LithuaniaHybridMapOld";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					LithuaniaOrtoFotoOldMapProvider.Instance,
					LithuaniaHybridMapProvider.Instance
				};
			}
			return _overlays;
		}
	}

	private LithuaniaHybridOldMapProvider()
	{
	}

	static LithuaniaHybridOldMapProvider()
	{
		Instance = new LithuaniaHybridOldMapProvider();
	}
}
