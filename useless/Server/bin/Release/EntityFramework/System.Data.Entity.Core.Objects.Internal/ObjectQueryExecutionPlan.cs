using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ObjectQueryExecutionPlan
{
	internal readonly DbCommandDefinition CommandDefinition;

	internal readonly bool Streaming;

	internal readonly ShaperFactory ResultShaperFactory;

	internal readonly TypeUsage ResultType;

	internal readonly MergeOption MergeOption;

	internal readonly IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> CompiledQueryParameters;

	private readonly EntitySet _singleEntitySet;

	public ObjectQueryExecutionPlan(DbCommandDefinition commandDefinition, ShaperFactory resultShaperFactory, TypeUsage resultType, MergeOption mergeOption, bool streaming, EntitySet singleEntitySet, IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters)
	{
		CommandDefinition = commandDefinition;
		ResultShaperFactory = resultShaperFactory;
		ResultType = resultType;
		MergeOption = mergeOption;
		Streaming = streaming;
		_singleEntitySet = singleEntitySet;
		CompiledQueryParameters = compiledQueryParameters;
	}

	internal string ToTraceString()
	{
		if (!(CommandDefinition is EntityCommandDefinition entityCommandDefinition))
		{
			return string.Empty;
		}
		return entityCommandDefinition.ToTraceString();
	}

	internal virtual ObjectResult<TResultType> Execute<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues)
	{
		DbDataReader dbDataReader = null;
		BufferedDataReader bufferedDataReader = null;
		try
		{
			using (EntityCommand entityCommand = PrepareEntityCommand(context, parameterValues))
			{
				dbDataReader = entityCommand.GetCommandDefinition().ExecuteStoreCommands(entityCommand, (!Streaming) ? CommandBehavior.SequentialAccess : CommandBehavior.Default);
			}
			ShaperFactory<TResultType> shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
			Shaper<TResultType> shaper;
			if (Streaming)
			{
				shaper = shaperFactory.Create(dbDataReader, context, context.MetadataWorkspace, MergeOption, readerOwned: true, Streaming);
			}
			else
			{
				StoreItemCollection storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
				DbProviderServices service = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);
				bufferedDataReader = new BufferedDataReader(dbDataReader);
				bufferedDataReader.Initialize(storeItemCollection.ProviderManifestToken, service, shaperFactory.ColumnTypes, shaperFactory.NullableColumns);
				shaper = shaperFactory.Create(bufferedDataReader, context, context.MetadataWorkspace, MergeOption, readerOwned: true, Streaming);
			}
			return new ObjectResult<TResultType>(resultItemType: (ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.CollectionType) ? ResultType : ((CollectionType)ResultType.EdmType).TypeUsage, shaper: shaper, singleEntitySet: _singleEntitySet);
		}
		catch (Exception)
		{
			if (Streaming)
			{
				dbDataReader?.Dispose();
			}
			if (!Streaming)
			{
				bufferedDataReader?.Dispose();
			}
			throw;
		}
	}

	internal virtual async Task<ObjectResult<TResultType>> ExecuteAsync<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		DbDataReader storeReader = null;
		BufferedDataReader bufferedReader = null;
		try
		{
			using (EntityCommand entityCommand = PrepareEntityCommand(context, parameterValues))
			{
				storeReader = await entityCommand.GetCommandDefinition().ExecuteStoreCommandsAsync(entityCommand, (!Streaming) ? CommandBehavior.SequentialAccess : CommandBehavior.Default, cancellationToken).WithCurrentCulture();
			}
			ShaperFactory<TResultType> shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
			Shaper<TResultType> shaper;
			if (Streaming)
			{
				shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, readerOwned: true, Streaming);
			}
			else
			{
				StoreItemCollection storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
				DbProviderServices service = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);
				bufferedReader = new BufferedDataReader(storeReader);
				await bufferedReader.InitializeAsync(storeItemCollection.ProviderManifestToken, service, shaperFactory.ColumnTypes, shaperFactory.NullableColumns, cancellationToken).WithCurrentCulture();
				shaper = shaperFactory.Create(bufferedReader, context, context.MetadataWorkspace, MergeOption, readerOwned: true, Streaming);
			}
			return new ObjectResult<TResultType>(resultItemType: (ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.CollectionType) ? ResultType : ((CollectionType)ResultType.EdmType).TypeUsage, shaper: shaper, singleEntitySet: _singleEntitySet);
		}
		catch (Exception)
		{
			if (Streaming)
			{
				storeReader?.Dispose();
			}
			if (!Streaming)
			{
				bufferedReader?.Dispose();
			}
			throw;
		}
	}

	private EntityCommand PrepareEntityCommand(ObjectContext context, ObjectParameterCollection parameterValues)
	{
		EntityCommandDefinition entityCommandDefinition = (EntityCommandDefinition)CommandDefinition;
		EntityConnection entityConnection = (EntityConnection)context.Connection;
		EntityCommand entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, context.InterceptionContext);
		if (context.CommandTimeout.HasValue)
		{
			entityCommand.CommandTimeout = context.CommandTimeout.Value;
		}
		if (parameterValues != null)
		{
			foreach (ObjectParameter parameterValue in parameterValues)
			{
				int num = entityCommand.Parameters.IndexOf(parameterValue.Name);
				if (num != -1)
				{
					entityCommand.Parameters[num].Value = parameterValue.Value ?? DBNull.Value;
				}
			}
		}
		if (entityConnection.CurrentTransaction != null)
		{
			entityCommand.Transaction = entityConnection.CurrentTransaction;
		}
		return entityCommand;
	}
}
