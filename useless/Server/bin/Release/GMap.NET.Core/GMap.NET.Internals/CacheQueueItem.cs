namespace GMap.NET.Internals;

internal struct CacheQueueItem
{
	public RawTile Tile;

	public byte[] Img;

	public CacheUsage CacheType;

	public CacheQueueItem(RawTile tile, byte[] img, CacheUsage cacheType)
	{
		Tile = tile;
		Img = img;
		CacheType = cacheType;
	}

	public override string ToString()
	{
		RawTile tile = Tile;
		return tile.ToString() + ", CacheType:" + CacheType;
	}

	public void Clear()
	{
		Img = null;
	}
}
