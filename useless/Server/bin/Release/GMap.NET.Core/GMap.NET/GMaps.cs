using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using GMap.NET.CacheProviders;
using GMap.NET.Internals;
using GMap.NET.MapProviders;

namespace GMap.NET;

public class GMaps
{
	private class StringWriterExt : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public StringWriterExt(IFormatProvider info)
			: base(info)
		{
		}
	}

	public AccessMode Mode = AccessMode.ServerAndCache;

	public bool UseRouteCache = true;

	public bool UseGeocoderCache = true;

	public bool UseDirectionsCache = true;

	public bool UsePlacemarkCache = true;

	public bool UseUrlCache = true;

	public bool UseMemoryCache = true;

	public readonly MemoryCache MemoryCache = new MemoryCache();

	public bool ShuffleTilesOnLoad;

	private readonly Queue<CacheQueueItem> _tileCacheQueue = new Queue<CacheQueueItem>();

	private bool? _isRunningOnMono;

	private Thread _cacheEngine;

	internal readonly AutoResetEvent WaitForCache = new AutoResetEvent(initialState: false);

	private volatile bool _abortCacheLoop;

	internal volatile bool NoMapInstances;

	public TileCacheComplete OnTileCacheComplete;

	public TileCacheStart OnTileCacheStart;

	public TileCacheProgress OnTileCacheProgress;

	private int _readingCache;

	private volatile bool _cacheOnIdleRead = true;

	private volatile bool _boostCacheEngine;

	private readonly Exception _noDataException = new Exception("No data in local tile cache...");

	private TileHttpHost _host;

	public PureImageCache PrimaryCache
	{
		get
		{
			return Cache.Instance.ImageCache;
		}
		set
		{
			Cache.Instance.ImageCache = value;
		}
	}

	public PureImageCache SecondaryCache
	{
		get
		{
			return Cache.Instance.ImageCacheSecond;
		}
		set
		{
			Cache.Instance.ImageCacheSecond = value;
		}
	}

	public bool IsRunningOnMono
	{
		get
		{
			if (!_isRunningOnMono.HasValue)
			{
				try
				{
					_isRunningOnMono = Type.GetType("Mono.Runtime") != null;
					return _isRunningOnMono.Value;
				}
				catch
				{
				}
				return false;
			}
			return _isRunningOnMono.Value;
		}
	}

	public static GMaps Instance { get; }

	public bool CacheOnIdleRead
	{
		get
		{
			return _cacheOnIdleRead;
		}
		set
		{
			_cacheOnIdleRead = value;
		}
	}

	public bool BoostCacheEngine
	{
		get
		{
			return _boostCacheEngine;
		}
		set
		{
			_boostCacheEngine = value;
		}
	}

	static GMaps()
	{
		Instance = new GMaps();
		if (GMapProvider.TileImageProxy != null)
		{
			return;
		}
		try
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Assembly assembly = null;
			Assembly[] array = assemblies;
			foreach (Assembly assembly2 in array)
			{
				if (assembly2.FullName.Contains("GMap.NET.WindowsForms") || assembly2.FullName.Contains("GMap.NET.WindowsPresentation"))
				{
					assembly = assembly2;
					break;
				}
			}
			if (assembly == null)
			{
				string? directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				string path = directoryName + directorySeparatorChar + "GMap.NET.WindowsForms.dll";
				directorySeparatorChar = Path.DirectorySeparatorChar;
				string path2 = directoryName + directorySeparatorChar + "GMap.NET.WindowsPresentation.dll";
				if (File.Exists(path))
				{
					assembly = Assembly.LoadFile(path);
				}
				else if (File.Exists(path2))
				{
					assembly = Assembly.LoadFile(path2);
				}
			}
			if (assembly != null)
			{
				Type type = null;
				if (assembly.FullName.Contains("GMap.NET.WindowsForms"))
				{
					type = assembly.GetType("GMap.NET.WindowsForms.GMapImageProxy");
				}
				else if (assembly.FullName.Contains("GMap.NET.WindowsPresentation"))
				{
					type = assembly.GetType("GMap.NET.WindowsPresentation.GMapImageProxy");
				}
				if (type != null)
				{
					type.InvokeMember("Enable", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, null);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private GMaps()
	{
		ServicePointManager.DefaultConnectionLimit = 5;
	}

	public void SQLitePing()
	{
		SQLitePureImageCache.Ping();
	}

	public bool ExportToGMDB(string file)
	{
		if (PrimaryCache is SQLitePureImageCache)
		{
			StringBuilder stringBuilder = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);
			return SQLitePureImageCache.ExportMapDataToDB(stringBuilder.ToString(), file);
		}
		return false;
	}

	public bool ImportFromGMDB(string file)
	{
		if (PrimaryCache is SQLitePureImageCache)
		{
			StringBuilder stringBuilder = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);
			return SQLitePureImageCache.ExportMapDataToDB(file, stringBuilder.ToString());
		}
		return false;
	}

	public bool OptimizeMapDb(string file)
	{
		if (PrimaryCache is SQLitePureImageCache)
		{
			if (string.IsNullOrEmpty(file))
			{
				StringBuilder stringBuilder = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);
				return SQLitePureImageCache.VacuumDb(stringBuilder.ToString());
			}
			return SQLitePureImageCache.VacuumDb(file);
		}
		return false;
	}

	private void EnqueueCacheTask(CacheQueueItem task)
	{
		lock (_tileCacheQueue)
		{
			if (!_tileCacheQueue.Contains(task))
			{
				_tileCacheQueue.Enqueue(task);
				if (_cacheEngine != null && _cacheEngine.IsAlive)
				{
					WaitForCache.Set();
				}
				else if (_cacheEngine == null || _cacheEngine.ThreadState == ThreadState.Stopped || _cacheEngine.ThreadState == ThreadState.Unstarted)
				{
					_cacheEngine = null;
					_cacheEngine = new Thread(CacheEngineLoop);
					_cacheEngine.Name = "CacheEngine";
					_cacheEngine.IsBackground = false;
					_cacheEngine.Priority = ThreadPriority.Lowest;
					_abortCacheLoop = false;
					_cacheEngine.Start();
				}
			}
		}
	}

	public void CancelTileCaching()
	{
		_abortCacheLoop = true;
		lock (_tileCacheQueue)
		{
			_tileCacheQueue.Clear();
			WaitForCache.Set();
		}
	}

	private void CacheEngineLoop()
	{
		if (OnTileCacheStart != null)
		{
			OnTileCacheStart();
		}
		bool flag = false;
		while (!_abortCacheLoop)
		{
			try
			{
				CacheQueueItem? cacheQueueItem = null;
				int count;
				lock (_tileCacheQueue)
				{
					count = _tileCacheQueue.Count;
					if (count > 0)
					{
						cacheQueueItem = _tileCacheQueue.Dequeue();
					}
				}
				if (cacheQueueItem.HasValue)
				{
					if (flag)
					{
						flag = false;
						if (OnTileCacheStart != null)
						{
							OnTileCacheStart();
						}
					}
					if (OnTileCacheProgress != null)
					{
						OnTileCacheProgress(count);
					}
					if (cacheQueueItem.Value.Img == null)
					{
						continue;
					}
					if ((cacheQueueItem.Value.CacheType & CacheUsage.First) == CacheUsage.First && PrimaryCache != null)
					{
						if (_cacheOnIdleRead)
						{
							while (Interlocked.Decrement(ref _readingCache) > 0)
							{
								Thread.Sleep(1000);
							}
						}
						PrimaryCache.PutImageToCache(cacheQueueItem.Value.Img, cacheQueueItem.Value.Tile.Type, cacheQueueItem.Value.Tile.Pos, cacheQueueItem.Value.Tile.Zoom);
					}
					if ((cacheQueueItem.Value.CacheType & CacheUsage.Second) == CacheUsage.Second && SecondaryCache != null)
					{
						if (_cacheOnIdleRead)
						{
							while (Interlocked.Decrement(ref _readingCache) > 0)
							{
								Thread.Sleep(1000);
							}
						}
						SecondaryCache.PutImageToCache(cacheQueueItem.Value.Img, cacheQueueItem.Value.Tile.Type, cacheQueueItem.Value.Tile.Pos, cacheQueueItem.Value.Tile.Zoom);
					}
					cacheQueueItem.Value.Clear();
					if (!_boostCacheEngine)
					{
						Thread.Sleep(333);
					}
					continue;
				}
				if (!flag)
				{
					flag = true;
					if (OnTileCacheComplete != null)
					{
						OnTileCacheComplete();
					}
				}
				if (_abortCacheLoop || NoMapInstances || !WaitForCache.WaitOne(33333, exitContext: false) || NoMapInstances)
				{
					break;
				}
				continue;
			}
			catch (AbandonedMutexException)
			{
			}
			catch (Exception)
			{
				continue;
			}
			break;
		}
		if (!flag && OnTileCacheComplete != null)
		{
			OnTileCacheComplete();
		}
	}

	public string SerializeGPX(gpxType targetInstance)
	{
		using StringWriterExt stringWriterExt = new StringWriterExt(CultureInfo.InvariantCulture);
		new XmlSerializer(targetInstance.GetType()).Serialize(stringWriterExt, targetInstance);
		return stringWriterExt.ToString();
	}

	public gpxType DeserializeGPX(string objectXml)
	{
		using StringReader input = new StringReader(objectXml);
		XmlTextReader xmlTextReader = new XmlTextReader(input);
		gpxType result = new XmlSerializer(typeof(gpxType)).Deserialize(xmlTextReader) as gpxType;
		xmlTextReader.Close();
		return result;
	}

	public bool ExportGPX(IEnumerable<List<GpsLog>> log, string gpxFile)
	{
		try
		{
			gpxType gpxType2 = new gpxType();
			gpxType2.creator = "GMap.NET - http://greatmaps.codeplex.com";
			gpxType2.trk = new trkType[1];
			gpxType2.trk[0] = new trkType();
			List<List<GpsLog>> list = new List<List<GpsLog>>(log);
			gpxType2.trk[0].trkseg = new trksegType[list.Count];
			int num = 0;
			foreach (List<GpsLog> item in list)
			{
				trksegType trksegType2 = new trksegType();
				trksegType2.trkpt = new wptType[item.Count];
				gpxType2.trk[0].trkseg[num++] = trksegType2;
				for (int i = 0; i < item.Count; i++)
				{
					GpsLog gpsLog = item[i];
					wptType wptType2 = new wptType();
					wptType2.lat = new decimal(gpsLog.Position.Lat);
					wptType2.lon = new decimal(gpsLog.Position.Lng);
					wptType2.time = gpsLog.TimeUTC;
					wptType2.timeSpecified = true;
					if (gpsLog.FixType != 0)
					{
						wptType2.fix = ((gpsLog.FixType == FixType.XyD) ? fixType.Item2d : fixType.Item3d);
						wptType2.fixSpecified = true;
					}
					if (gpsLog.SeaLevelAltitude.HasValue)
					{
						wptType2.ele = new decimal(gpsLog.SeaLevelAltitude.Value);
						wptType2.eleSpecified = true;
					}
					if (gpsLog.EllipsoidAltitude.HasValue)
					{
						wptType2.geoidheight = new decimal(gpsLog.EllipsoidAltitude.Value);
						wptType2.geoidheightSpecified = true;
					}
					if (gpsLog.VerticalDilutionOfPrecision.HasValue)
					{
						wptType2.vdopSpecified = true;
						wptType2.vdop = new decimal(gpsLog.VerticalDilutionOfPrecision.Value);
					}
					if (gpsLog.HorizontalDilutionOfPrecision.HasValue)
					{
						wptType2.hdopSpecified = true;
						wptType2.hdop = new decimal(gpsLog.HorizontalDilutionOfPrecision.Value);
					}
					if (gpsLog.PositionDilutionOfPrecision.HasValue)
					{
						wptType2.pdopSpecified = true;
						wptType2.pdop = new decimal(gpsLog.PositionDilutionOfPrecision.Value);
					}
					if (gpsLog.SatelliteCount.HasValue)
					{
						wptType2.sat = gpsLog.SatelliteCount.Value.ToString();
					}
					trksegType2.trkpt[i] = wptType2;
				}
			}
			list.Clear();
			File.WriteAllText(gpxFile, SerializeGPX(gpxType2), Encoding.UTF8);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public PureImage GetImageFrom(GMapProvider provider, GPoint pos, int zoom, out Exception result)
	{
		PureImage pureImage = null;
		result = null;
		try
		{
			RawTile tile = new RawTile(provider.DbId, pos, zoom);
			if (UseMemoryCache)
			{
				byte[] tileFromMemoryCache = MemoryCache.GetTileFromMemoryCache(tile);
				if (tileFromMemoryCache != null && GMapProvider.TileImageProxy != null)
				{
					pureImage = GMapProvider.TileImageProxy.FromArray(tileFromMemoryCache);
				}
			}
			if (pureImage == null)
			{
				if (Mode != 0 && !provider.BypassCache)
				{
					if (PrimaryCache != null)
					{
						if (_cacheOnIdleRead)
						{
							Interlocked.Exchange(ref _readingCache, 5);
						}
						pureImage = PrimaryCache.GetImageFromCache(provider.DbId, pos, zoom);
						if (pureImage != null)
						{
							if (UseMemoryCache)
							{
								MemoryCache.AddTileToMemoryCache(tile, pureImage.Data.GetBuffer());
							}
							return pureImage;
						}
					}
					if (SecondaryCache != null)
					{
						if (_cacheOnIdleRead)
						{
							Interlocked.Exchange(ref _readingCache, 5);
						}
						pureImage = SecondaryCache.GetImageFromCache(provider.DbId, pos, zoom);
						if (pureImage != null)
						{
							if (UseMemoryCache)
							{
								MemoryCache.AddTileToMemoryCache(tile, pureImage.Data.GetBuffer());
							}
							EnqueueCacheTask(new CacheQueueItem(tile, pureImage.Data.GetBuffer(), CacheUsage.First));
							return pureImage;
						}
					}
				}
				if (Mode != AccessMode.CacheOnly)
				{
					pureImage = provider.GetTileImage(pos, zoom);
					if (pureImage != null)
					{
						if (UseMemoryCache)
						{
							MemoryCache.AddTileToMemoryCache(tile, pureImage.Data.GetBuffer());
						}
						if (Mode != 0 && !provider.BypassCache)
						{
							EnqueueCacheTask(new CacheQueueItem(tile, pureImage.Data.GetBuffer(), CacheUsage.Both));
						}
					}
				}
				else
				{
					result = _noDataException;
				}
			}
		}
		catch (Exception ex)
		{
			result = ex;
			pureImage = null;
		}
		return pureImage;
	}

	public void EnableTileHost(int port)
	{
		if (_host == null)
		{
			_host = new TileHttpHost();
		}
		_host.Start(port);
	}

	public void DisableTileHost()
	{
		if (_host != null)
		{
			_host.Stop();
		}
	}
}
