using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

internal class NextResultGenerator
{
	private readonly EntityCommand _entityCommand;

	private readonly ReadOnlyCollection<EntitySet> _entitySets;

	private readonly ObjectContext _context;

	private readonly EdmType[] _edmTypes;

	private readonly int _resultSetIndex;

	private readonly bool _streaming;

	private readonly MergeOption _mergeOption;

	internal NextResultGenerator(ObjectContext context, EntityCommand entityCommand, EdmType[] edmTypes, ReadOnlyCollection<EntitySet> entitySets, MergeOption mergeOption, bool streaming, int resultSetIndex)
	{
		_context = context;
		_entityCommand = entityCommand;
		_entitySets = entitySets;
		_edmTypes = edmTypes;
		_resultSetIndex = resultSetIndex;
		_streaming = streaming;
		_mergeOption = mergeOption;
	}

	internal ObjectResult<TElement> GetNextResult<TElement>(DbDataReader storeReader)
	{
		bool flag = false;
		try
		{
			flag = storeReader.NextResult();
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, ex);
			}
			throw;
		}
		if (flag)
		{
			MetadataHelper.CheckFunctionImportReturnType<TElement>(_edmTypes[_resultSetIndex], _context.MetadataWorkspace);
			return _context.MaterializedDataRecord<TElement>(_entityCommand, storeReader, _resultSetIndex, _entitySets, _edmTypes, null, _mergeOption, _streaming);
		}
		return null;
	}
}
