namespace GMap.NET.Internals;

internal struct RawTile
{
	public int Type;

	public GPoint Pos;

	public int Zoom;

	public RawTile(int type, GPoint pos, int zoom)
	{
		Type = type;
		Pos = pos;
		Zoom = zoom;
	}

	public override string ToString()
	{
		string[] obj = new string[5]
		{
			Type.ToString(),
			" at zoom ",
			Zoom.ToString(),
			", pos: ",
			null
		};
		GPoint pos = Pos;
		obj[4] = pos.ToString();
		return string.Concat(obj);
	}
}
