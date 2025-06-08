using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.ResultAssembly;

internal class BridgeDataReaderFactory
{
	private readonly Translator _translator;

	public BridgeDataReaderFactory(Translator translator = null)
	{
		_translator = translator ?? new Translator();
	}

	public virtual DbDataReader Create(DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps)
	{
		KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> keyValuePair = CreateShaperInfo(storeDataReader, columnMap, workspace);
		return new BridgeDataReader(keyValuePair.Key, keyValuePair.Value, 0, GetNextResultShaperInfo(storeDataReader, workspace, nextResultColumnMaps).GetEnumerator());
	}

	private KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> CreateShaperInfo(DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace)
	{
		Shaper<RecordState> shaper = _translator.TranslateColumnMap<RecordState>(columnMap, workspace, null, MergeOption.NoTracking, streaming: true, valueLayer: true).Create(storeDataReader, null, workspace, MergeOption.NoTracking, readerOwned: true, streaming: true);
		return new KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>(shaper, shaper.RootCoordinator.TypedCoordinatorFactory);
	}

	private IEnumerable<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> GetNextResultShaperInfo(DbDataReader storeDataReader, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps)
	{
		return nextResultColumnMaps.Select((ColumnMap nextResultColumnMap) => CreateShaperInfo(storeDataReader, nextResultColumnMap, workspace));
	}
}
