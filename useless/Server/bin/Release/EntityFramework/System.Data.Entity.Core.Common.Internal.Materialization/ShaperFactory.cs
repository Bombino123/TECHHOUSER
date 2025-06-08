using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal abstract class ShaperFactory
{
}
internal class ShaperFactory<T> : ShaperFactory
{
	private readonly int _stateCount;

	private readonly CoordinatorFactory<T> _rootCoordinatorFactory;

	private readonly MergeOption _mergeOption;

	public Type[] ColumnTypes { get; private set; }

	public bool[] NullableColumns { get; private set; }

	internal ShaperFactory(int stateCount, CoordinatorFactory<T> rootCoordinatorFactory, Type[] columnTypes, bool[] nullableColumns, MergeOption mergeOption)
	{
		_stateCount = stateCount;
		_rootCoordinatorFactory = rootCoordinatorFactory;
		ColumnTypes = columnTypes;
		NullableColumns = nullableColumns;
		_mergeOption = mergeOption;
	}

	internal Shaper<T> Create(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, bool readerOwned, bool streaming)
	{
		return new Shaper<T>(reader, context, workspace, mergeOption, _stateCount, _rootCoordinatorFactory, readerOwned, streaming);
	}
}
