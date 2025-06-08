using System;

namespace GMap.NET;

public interface PureImageCache
{
	bool PutImageToCache(byte[] tile, int type, GPoint pos, int zoom);

	PureImage GetImageFromCache(int type, GPoint pos, int zoom);

	int DeleteOlderThan(DateTime date, int? type);
}
