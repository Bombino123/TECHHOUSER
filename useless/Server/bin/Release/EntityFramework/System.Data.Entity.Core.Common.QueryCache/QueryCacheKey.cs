namespace System.Data.Entity.Core.Common.QueryCache;

internal abstract class QueryCacheKey
{
	protected const int EstimatedParameterStringSize = 20;

	private uint _hitCount;

	protected static StringComparison _stringComparison = StringComparison.Ordinal;

	internal uint HitCount
	{
		get
		{
			return _hitCount;
		}
		set
		{
			_hitCount = value;
		}
	}

	internal int AgingIndex { get; set; }

	protected QueryCacheKey()
	{
		_hitCount = 1u;
	}

	public abstract override bool Equals(object obj);

	public abstract override int GetHashCode();

	internal void UpdateHit()
	{
		if (-1 != (int)_hitCount)
		{
			_hitCount++;
		}
	}

	protected virtual bool Equals(string s, string t)
	{
		return string.Equals(s, t, _stringComparison);
	}
}
