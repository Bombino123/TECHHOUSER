using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects;

public abstract class ObjectResult : IEnumerable, IDisposable, IListSource, IDbAsyncEnumerable
{
	bool IListSource.ContainsListCollection => false;

	public abstract Type ElementType { get; }

	protected internal ObjectResult()
	{
	}

	IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
	{
		return GetAsyncEnumeratorInternal();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorInternal();
	}

	IList IListSource.GetList()
	{
		return GetIListSourceListInternal();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected abstract void Dispose(bool disposing);

	public virtual ObjectResult<TElement> GetNextResult<TElement>()
	{
		return GetNextResultInternal<TElement>();
	}

	internal abstract IDbAsyncEnumerator GetAsyncEnumeratorInternal();

	internal abstract IEnumerator GetEnumeratorInternal();

	internal abstract IList GetIListSourceListInternal();

	internal abstract ObjectResult<TElement> GetNextResultInternal<TElement>();
}
public class ObjectResult<T> : ObjectResult, IEnumerable<T>, IEnumerable, IDbAsyncEnumerable<T>, IDbAsyncEnumerable
{
	private Shaper<T> _shaper;

	private DbDataReader _reader;

	private DbCommand _command;

	private readonly EntitySet _singleEntitySet;

	private readonly TypeUsage _resultItemType;

	private readonly bool _readerOwned;

	private readonly bool _shouldReleaseConnection;

	private IBindingList _cachedBindingList;

	private NextResultGenerator _nextResultGenerator;

	private Action<object, EventArgs> _onReaderDispose;

	public override Type ElementType => typeof(T);

	protected ObjectResult()
	{
	}

	internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType)
		: this(shaper, singleEntitySet, resultItemType, readerOwned: true, shouldReleaseConnection: true, (DbCommand)null)
	{
	}

	internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned, bool shouldReleaseConnection, DbCommand command = null)
		: this(shaper, singleEntitySet, resultItemType, readerOwned, shouldReleaseConnection, (NextResultGenerator)null, (Action<object, EventArgs>)null, command)
	{
	}

	internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned, bool shouldReleaseConnection, NextResultGenerator nextResultGenerator, Action<object, EventArgs> onReaderDispose, DbCommand command = null)
	{
		_shaper = shaper;
		_reader = _shaper.Reader;
		_command = command;
		_singleEntitySet = singleEntitySet;
		_resultItemType = resultItemType;
		_readerOwned = readerOwned;
		_shouldReleaseConnection = shouldReleaseConnection;
		_nextResultGenerator = nextResultGenerator;
		_onReaderDispose = onReaderDispose;
	}

	private void EnsureCanEnumerateResults()
	{
		if (_shaper == null)
		{
			throw new InvalidOperationException(Strings.Materializer_CannotReEnumerateQueryResults);
		}
	}

	public virtual IEnumerator<T> GetEnumerator()
	{
		return GetDbEnumerator();
	}

	internal virtual IDbEnumerator<T> GetDbEnumerator()
	{
		EnsureCanEnumerateResults();
		Shaper<T> shaper = _shaper;
		_shaper = null;
		return shaper.GetEnumerator();
	}

	IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
	{
		return GetDbEnumerator();
	}

	protected override void Dispose(bool disposing)
	{
		DbDataReader reader = _reader;
		_reader = null;
		_nextResultGenerator = null;
		if (reader != null && _readerOwned)
		{
			reader.Dispose();
			if (_onReaderDispose != null)
			{
				_onReaderDispose(this, new EventArgs());
				_onReaderDispose = null;
			}
		}
		if (_shaper != null)
		{
			if (_shaper.Context != null && _readerOwned && _shouldReleaseConnection)
			{
				_shaper.Context.ReleaseConnection();
			}
			_shaper = null;
		}
		if (_command != null)
		{
			_command.Dispose();
			_command = null;
		}
	}

	internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
	{
		return GetDbEnumerator();
	}

	internal override IEnumerator GetEnumeratorInternal()
	{
		return GetDbEnumerator();
	}

	internal override IList GetIListSourceListInternal()
	{
		if (_cachedBindingList == null)
		{
			EnsureCanEnumerateResults();
			bool forceReadOnly = _shaper.MergeOption == MergeOption.NoTracking;
			_cachedBindingList = ObjectViewFactory.CreateViewForQuery(_resultItemType, this, _shaper.Context, forceReadOnly, _singleEntitySet);
		}
		return _cachedBindingList;
	}

	internal override ObjectResult<TElement> GetNextResultInternal<TElement>()
	{
		if (_nextResultGenerator == null)
		{
			return null;
		}
		return _nextResultGenerator.GetNextResult<TElement>(_reader);
	}
}
