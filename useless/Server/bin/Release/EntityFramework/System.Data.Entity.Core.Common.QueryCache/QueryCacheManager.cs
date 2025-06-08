using System.Collections.Generic;
using System.Data.Entity.Internal;
using System.Threading;

namespace System.Data.Entity.Core.Common.QueryCache;

internal class QueryCacheManager : IDisposable
{
	private sealed class EvictionTimer
	{
		private readonly object _sync = new object();

		private readonly int _period;

		private readonly QueryCacheManager _cacheManager;

		private Timer _timer;

		internal EvictionTimer(QueryCacheManager cacheManager, int recyclePeriod)
		{
			_cacheManager = cacheManager;
			_period = recyclePeriod;
		}

		internal void Start()
		{
			lock (_sync)
			{
				if (_timer == null)
				{
					_timer = new Timer(CacheRecyclerHandler, _cacheManager, _period, _period);
				}
			}
		}

		internal bool Stop()
		{
			lock (_sync)
			{
				if (_timer != null)
				{
					_timer.Dispose();
					_timer = null;
					return true;
				}
				return false;
			}
		}

		internal bool Suspend()
		{
			lock (_sync)
			{
				if (_timer != null)
				{
					_timer.Change(-1, -1);
					return true;
				}
				return false;
			}
		}

		internal void Resume()
		{
			lock (_sync)
			{
				if (_timer != null)
				{
					_timer.Change(_period, _period);
				}
			}
		}
	}

	private readonly object _cacheDataLock = new object();

	private readonly Dictionary<QueryCacheKey, QueryCacheEntry> _cacheData = new Dictionary<QueryCacheKey, QueryCacheEntry>(32);

	private readonly int _maxNumberOfEntries;

	private readonly int _sweepingTriggerHighMark;

	private readonly EvictionTimer _evictionTimer;

	private static readonly int[] _agingFactor = new int[6] { 1, 1, 2, 4, 8, 16 };

	private static readonly int _agingMaxIndex = _agingFactor.Length - 1;

	internal static QueryCacheManager Create()
	{
		QueryCacheConfig queryCache = AppConfig.DefaultInstance.QueryCache;
		int queryCacheSize = queryCache.GetQueryCacheSize();
		int recycleMillis = queryCache.GetCleaningIntervalInSeconds() * 1000;
		return new QueryCacheManager(queryCacheSize, 0.8f, recycleMillis);
	}

	private QueryCacheManager(int maximumSize, float loadFactor, int recycleMillis)
	{
		_maxNumberOfEntries = maximumSize;
		_sweepingTriggerHighMark = (int)((float)_maxNumberOfEntries * loadFactor);
		_evictionTimer = new EvictionTimer(this, recycleMillis);
	}

	internal bool TryLookupAndAdd(QueryCacheEntry inQueryCacheEntry, out QueryCacheEntry outQueryCacheEntry)
	{
		outQueryCacheEntry = null;
		lock (_cacheDataLock)
		{
			if (!_cacheData.TryGetValue(inQueryCacheEntry.QueryCacheKey, out outQueryCacheEntry))
			{
				_cacheData.Add(inQueryCacheEntry.QueryCacheKey, inQueryCacheEntry);
				if (_cacheData.Count > _sweepingTriggerHighMark)
				{
					_evictionTimer.Start();
				}
				return false;
			}
			outQueryCacheEntry.QueryCacheKey.UpdateHit();
			return true;
		}
	}

	internal bool TryCacheLookup<TK, TE>(TK key, out TE value) where TK : QueryCacheKey
	{
		value = default(TE);
		QueryCacheEntry queryCacheEntry = null;
		bool num = TryInternalCacheLookup(key, out queryCacheEntry);
		if (num)
		{
			value = (TE)queryCacheEntry.GetTarget();
		}
		return num;
	}

	internal void Clear()
	{
		lock (_cacheDataLock)
		{
			_cacheData.Clear();
		}
	}

	private bool TryInternalCacheLookup(QueryCacheKey queryCacheKey, out QueryCacheEntry queryCacheEntry)
	{
		queryCacheEntry = null;
		bool flag = false;
		lock (_cacheDataLock)
		{
			flag = _cacheData.TryGetValue(queryCacheKey, out queryCacheEntry);
		}
		if (flag)
		{
			queryCacheEntry.QueryCacheKey.UpdateHit();
		}
		return flag;
	}

	private static void CacheRecyclerHandler(object state)
	{
		((QueryCacheManager)state).SweepCache();
	}

	private void SweepCache()
	{
		if (!_evictionTimer.Suspend())
		{
			return;
		}
		bool flag = false;
		lock (_cacheDataLock)
		{
			if (_cacheData.Count > _sweepingTriggerHighMark)
			{
				uint num = 0u;
				List<QueryCacheKey> list = new List<QueryCacheKey>(_cacheData.Count);
				list.AddRange(_cacheData.Keys);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].HitCount == 0)
					{
						_cacheData.Remove(list[i]);
						num++;
						continue;
					}
					int num2 = list[i].AgingIndex + 1;
					if (num2 > _agingMaxIndex)
					{
						num2 = _agingMaxIndex;
					}
					list[i].AgingIndex = num2;
					list[i].HitCount = list[i].HitCount >> _agingFactor[num2];
				}
			}
			else
			{
				_evictionTimer.Stop();
				flag = true;
			}
		}
		if (!flag)
		{
			_evictionTimer.Resume();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (_evictionTimer.Stop())
		{
			Clear();
		}
	}
}
