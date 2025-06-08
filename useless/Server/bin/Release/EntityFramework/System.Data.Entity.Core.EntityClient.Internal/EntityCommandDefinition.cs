using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Core.Query.PlanCompiler;
using System.Data.Entity.Core.Query.ResultAssembly;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.EntityClient.Internal;

internal class EntityCommandDefinition : DbCommandDefinition
{
	private interface IColumnMapGenerator
	{
		ColumnMap CreateColumnMap(DbDataReader reader);
	}

	private sealed class ConstantColumnMapGenerator : IColumnMapGenerator
	{
		private readonly ColumnMap _columnMap;

		private readonly int _fieldsRequired;

		internal ConstantColumnMapGenerator(ColumnMap columnMap, int fieldsRequired)
		{
			_columnMap = columnMap;
			_fieldsRequired = fieldsRequired;
		}

		ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader)
		{
			if (reader != null && reader.FieldCount < _fieldsRequired)
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_TooFewColumns);
			}
			return _columnMap;
		}
	}

	private sealed class FunctionColumnMapGenerator : IColumnMapGenerator
	{
		private readonly FunctionImportMappingNonComposable _mapping;

		private readonly EntitySet _entitySet;

		private readonly StructuralType _baseStructuralType;

		private readonly int _resultSetIndex;

		private readonly ColumnMapFactory _columnMapFactory;

		internal FunctionColumnMapGenerator(FunctionImportMappingNonComposable mapping, int resultSetIndex, EntitySet entitySet, StructuralType baseStructuralType, ColumnMapFactory columnMapFactory)
		{
			_mapping = mapping;
			_entitySet = entitySet;
			_baseStructuralType = baseStructuralType;
			_resultSetIndex = resultSetIndex;
			_columnMapFactory = columnMapFactory;
		}

		ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader)
		{
			return _columnMapFactory.CreateFunctionImportStructuralTypeColumnMap(reader, _mapping, _resultSetIndex, _entitySet, _baseStructuralType);
		}
	}

	private readonly List<DbCommandDefinition> _mappedCommandDefinitions;

	private readonly IColumnMapGenerator[] _columnMapGenerators;

	private readonly ReadOnlyCollection<EntityParameter> _parameters;

	private readonly Set<EntitySet> _entitySets;

	private readonly BridgeDataReaderFactory _bridgeDataReaderFactory;

	private readonly ColumnMapFactory _columnMapFactory;

	private readonly DbProviderServices _storeProviderServices;

	internal virtual IEnumerable<EntityParameter> Parameters => _parameters;

	internal virtual Set<EntitySet> EntitySets => _entitySets;

	internal EntityCommandDefinition()
	{
	}

	internal EntityCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree, DbInterceptionContext interceptionContext, IDbDependencyResolver resolver = null, BridgeDataReaderFactory bridgeDataReaderFactory = null, ColumnMapFactory columnMapFactory = null)
	{
		_bridgeDataReaderFactory = bridgeDataReaderFactory ?? new BridgeDataReaderFactory();
		_columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
		_storeProviderServices = resolver?.GetService<DbProviderServices>(storeProviderFactory.GetProviderInvariantName()) ?? storeProviderFactory.GetProviderServices();
		try
		{
			if (commandTree.CommandTreeKind == DbCommandTreeKind.Query)
			{
				List<ProviderCommandInfo> providerCommands = new List<ProviderCommandInfo>();
				PlanCompiler.Compile(commandTree, out providerCommands, out var resultColumnMap, out var columnCount, out _entitySets);
				_columnMapGenerators = new IColumnMapGenerator[1]
				{
					new ConstantColumnMapGenerator(resultColumnMap, columnCount)
				};
				_mappedCommandDefinitions = new List<DbCommandDefinition>(providerCommands.Count);
				foreach (ProviderCommandInfo item3 in providerCommands)
				{
					DbCommandDefinition dbCommandDefinition = _storeProviderServices.CreateCommandDefinition(item3.CommandTree, interceptionContext);
					if (dbCommandDefinition == null)
					{
						throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForCreateCommandDefinition);
					}
					_mappedCommandDefinitions.Add(dbCommandDefinition);
				}
			}
			else
			{
				DbFunctionCommandTree dbFunctionCommandTree = (DbFunctionCommandTree)commandTree;
				FunctionImportMappingNonComposable targetFunctionMapping = GetTargetFunctionMapping(dbFunctionCommandTree);
				IList<FunctionParameter> returnParameters = dbFunctionCommandTree.EdmFunction.ReturnParameters;
				int num = ((returnParameters.Count <= 1) ? 1 : returnParameters.Count);
				_columnMapGenerators = new IColumnMapGenerator[num];
				TypeUsage resultType = DetermineStoreResultType(targetFunctionMapping, 0, out _columnMapGenerators[0]);
				for (int i = 1; i < num; i++)
				{
					DetermineStoreResultType(targetFunctionMapping, i, out _columnMapGenerators[i]);
				}
				List<KeyValuePair<string, TypeUsage>> list = new List<KeyValuePair<string, TypeUsage>>();
				foreach (KeyValuePair<string, TypeUsage> parameter in dbFunctionCommandTree.Parameters)
				{
					list.Add(parameter);
				}
				DbFunctionCommandTree commandTree2 = new DbFunctionCommandTree(dbFunctionCommandTree.MetadataWorkspace, DataSpace.SSpace, targetFunctionMapping.TargetFunction, resultType, list);
				DbCommandDefinition item = _storeProviderServices.CreateCommandDefinition(commandTree2);
				_mappedCommandDefinitions = new List<DbCommandDefinition>(1) { item };
				if (targetFunctionMapping.FunctionImport.EntitySets.FirstOrDefault() != null)
				{
					_entitySets = new Set<EntitySet>();
					_entitySets.Add(targetFunctionMapping.FunctionImport.EntitySets.FirstOrDefault());
					_entitySets.MakeReadOnly();
				}
			}
			List<EntityParameter> list2 = new List<EntityParameter>();
			foreach (KeyValuePair<string, TypeUsage> parameter2 in commandTree.Parameters)
			{
				EntityParameter item2 = CreateEntityParameterFromQueryParameter(parameter2);
				list2.Add(item2);
			}
			_parameters = new ReadOnlyCollection<EntityParameter>(list2);
		}
		catch (EntityCommandCompilationException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			if (ex2.IsCatchableExceptionType())
			{
				throw new EntityCommandCompilationException(Strings.EntityClient_CommandDefinitionPreparationFailed, ex2);
			}
			throw;
		}
	}

	protected EntityCommandDefinition(BridgeDataReaderFactory factory = null, ColumnMapFactory columnMapFactory = null, List<DbCommandDefinition> mappedCommandDefinitions = null)
	{
		_bridgeDataReaderFactory = factory ?? new BridgeDataReaderFactory();
		_columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
		_mappedCommandDefinitions = mappedCommandDefinitions;
	}

	private TypeUsage DetermineStoreResultType(FunctionImportMappingNonComposable mapping, int resultSetIndex, out IColumnMapGenerator columnMapGenerator)
	{
		EdmFunction functionImport = mapping.FunctionImport;
		TypeUsage typeUsage;
		if (MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(functionImport, resultSetIndex, out var returnType))
		{
			ValidateEdmResultType(returnType, functionImport);
			EntitySet entitySet = ((functionImport.EntitySets.Count > resultSetIndex) ? functionImport.EntitySets[resultSetIndex] : null);
			columnMapGenerator = new FunctionColumnMapGenerator(mapping, resultSetIndex, entitySet, returnType, _columnMapFactory);
			typeUsage = mapping.GetExpectedTargetResultType(resultSetIndex);
		}
		else
		{
			FunctionParameter returnParameter = MetadataHelper.GetReturnParameter(functionImport, resultSetIndex);
			if (returnParameter != null && returnParameter.TypeUsage != null)
			{
				typeUsage = returnParameter.TypeUsage;
				ScalarColumnMap elementMap = new ScalarColumnMap(((CollectionType)typeUsage.EdmType).TypeUsage, string.Empty, 0, 0);
				SimpleCollectionColumnMap columnMap = new SimpleCollectionColumnMap(typeUsage, string.Empty, elementMap, null, null);
				columnMapGenerator = new ConstantColumnMapGenerator(columnMap, 1);
			}
			else
			{
				typeUsage = null;
				columnMapGenerator = new ConstantColumnMapGenerator(null, 0);
			}
		}
		return typeUsage;
	}

	private static void ValidateEdmResultType(EdmType resultType, EdmFunction functionImport)
	{
		if (!Helper.IsComplexType(resultType))
		{
			return;
		}
		ComplexType complexType = resultType as ComplexType;
		foreach (EdmProperty property in complexType.Properties)
		{
			if (property.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
			{
				throw new NotSupportedException(Strings.ComplexTypeAsReturnTypeAndNestedComplexProperty(property.Name, complexType.Name, functionImport.FullName));
			}
		}
	}

	private static FunctionImportMappingNonComposable GetTargetFunctionMapping(DbFunctionCommandTree functionCommandTree)
	{
		if (!functionCommandTree.MetadataWorkspace.TryGetFunctionImportMapping(functionCommandTree.EdmFunction, out var targetFunctionMapping))
		{
			throw new InvalidOperationException(Strings.EntityClient_UnmappedFunctionImport(functionCommandTree.EdmFunction.FullName));
		}
		return (FunctionImportMappingNonComposable)targetFunctionMapping;
	}

	public override DbCommand CreateCommand()
	{
		return new EntityCommand(this, new DbInterceptionContext());
	}

	internal ColumnMap CreateColumnMap(DbDataReader storeDataReader)
	{
		return CreateColumnMap(storeDataReader, 0);
	}

	internal virtual ColumnMap CreateColumnMap(DbDataReader storeDataReader, int resultSetIndex)
	{
		return _columnMapGenerators[resultSetIndex].CreateColumnMap(storeDataReader);
	}

	private static EntityParameter CreateEntityParameterFromQueryParameter(KeyValuePair<string, TypeUsage> queryParameter)
	{
		EntityParameter obj = new EntityParameter
		{
			ParameterName = queryParameter.Key
		};
		PopulateParameterFromTypeUsage(obj, queryParameter.Value, isOutParam: false);
		return obj;
	}

	internal static void PopulateParameterFromTypeUsage(EntityParameter parameter, TypeUsage type, bool isOutParam)
	{
		if (type != null)
		{
			PrimitiveTypeKind spatialType;
			if (Helper.IsEnumType(type.EdmType))
			{
				type = TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(type.EdmType));
			}
			else if (Helper.IsSpatialType(type, out spatialType))
			{
				parameter.EdmType = EdmProviderManifest.Instance.GetPrimitiveType(spatialType);
			}
		}
		DbCommandDefinition.PopulateParameterFromTypeUsage(parameter, type, isOutParam);
	}

	internal virtual DbDataReader Execute(EntityCommand entityCommand, CommandBehavior behavior)
	{
		if (CommandBehavior.SequentialAccess != (behavior & CommandBehavior.SequentialAccess))
		{
			throw new InvalidOperationException(Strings.ADP_MustUseSequentialAccess);
		}
		DbDataReader dbDataReader = ExecuteStoreCommands(entityCommand, behavior & ~CommandBehavior.SequentialAccess);
		DbDataReader result = null;
		if (dbDataReader != null)
		{
			try
			{
				ColumnMap columnMap = CreateColumnMap(dbDataReader, 0);
				if (columnMap == null)
				{
					CommandHelper.ConsumeReader(dbDataReader);
					result = dbDataReader;
				}
				else
				{
					MetadataWorkspace metadataWorkspace = entityCommand.Connection.GetMetadataWorkspace();
					IEnumerable<ColumnMap> nextResultColumnMaps = GetNextResultColumnMaps(dbDataReader);
					result = _bridgeDataReaderFactory.Create(dbDataReader, columnMap, metadataWorkspace, nextResultColumnMaps);
				}
			}
			catch
			{
				dbDataReader.Dispose();
				throw;
			}
		}
		return result;
	}

	internal virtual async Task<DbDataReader> ExecuteAsync(EntityCommand entityCommand, CommandBehavior behavior, CancellationToken cancellationToken)
	{
		if (CommandBehavior.SequentialAccess != (behavior & CommandBehavior.SequentialAccess))
		{
			throw new InvalidOperationException(Strings.ADP_MustUseSequentialAccess);
		}
		cancellationToken.ThrowIfCancellationRequested();
		DbDataReader storeDataReader = await ExecuteStoreCommandsAsync(entityCommand, behavior & ~CommandBehavior.SequentialAccess, cancellationToken).WithCurrentCulture();
		DbDataReader result = null;
		if (storeDataReader != null)
		{
			try
			{
				ColumnMap columnMap = CreateColumnMap(storeDataReader, 0);
				if (columnMap == null)
				{
					await CommandHelper.ConsumeReaderAsync(storeDataReader, cancellationToken).WithCurrentCulture();
					result = storeDataReader;
				}
				else
				{
					MetadataWorkspace metadataWorkspace = entityCommand.Connection.GetMetadataWorkspace();
					IEnumerable<ColumnMap> nextResultColumnMaps = GetNextResultColumnMaps(storeDataReader);
					result = _bridgeDataReaderFactory.Create(storeDataReader, columnMap, metadataWorkspace, nextResultColumnMaps);
				}
			}
			catch
			{
				storeDataReader.Dispose();
				throw;
			}
		}
		return result;
	}

	private IEnumerable<ColumnMap> GetNextResultColumnMaps(DbDataReader storeDataReader)
	{
		int i = 1;
		while (i < _columnMapGenerators.Length)
		{
			yield return CreateColumnMap(storeDataReader, i);
			int num = i + 1;
			i = num;
		}
	}

	internal virtual DbDataReader ExecuteStoreCommands(EntityCommand entityCommand, CommandBehavior behavior)
	{
		DbCommand dbCommand = PrepareEntityCommandBeforeExecution(entityCommand);
		DbDataReader dbDataReader = null;
		try
		{
			return dbCommand.ExecuteReader(behavior);
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_CommandDefinitionExecutionFailed, ex);
			}
			throw;
		}
	}

	internal virtual async Task<DbDataReader> ExecuteStoreCommandsAsync(EntityCommand entityCommand, CommandBehavior behavior, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		DbCommand dbCommand = PrepareEntityCommandBeforeExecution(entityCommand);
		try
		{
			return await dbCommand.ExecuteReaderAsync(behavior, cancellationToken).WithCurrentCulture();
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new EntityCommandExecutionException(Strings.EntityClient_CommandDefinitionExecutionFailed, ex);
			}
			throw;
		}
	}

	private DbCommand PrepareEntityCommandBeforeExecution(EntityCommand entityCommand)
	{
		if (1 != _mappedCommandDefinitions.Count)
		{
			throw new NotSupportedException("MARS");
		}
		EntityTransaction entityTransaction = entityCommand.ValidateAndGetEntityTransaction();
		InterceptableDbCommand interceptableDbCommand = new InterceptableDbCommand(_mappedCommandDefinitions[0].CreateCommand(), entityCommand.InterceptionContext);
		CommandHelper.SetStoreProviderCommandState(entityCommand, entityTransaction, interceptableDbCommand);
		bool flag = false;
		if (interceptableDbCommand.Parameters != null)
		{
			foreach (DbParameter parameter in interceptableDbCommand.Parameters)
			{
				int num = entityCommand.Parameters.IndexOf(parameter.ParameterName);
				if (-1 != num)
				{
					SyncParameterProperties(entityCommand.Parameters[num], parameter, _storeProviderServices);
					if (parameter.Direction != ParameterDirection.Input)
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			entityCommand.SetStoreProviderCommand(interceptableDbCommand);
		}
		return interceptableDbCommand;
	}

	private static void SyncParameterProperties(EntityParameter entityParameter, DbParameter storeParameter, DbProviderServices storeProviderServices)
	{
		TypeUsage primitiveTypeUsageForScalar = TypeHelpers.GetPrimitiveTypeUsageForScalar(entityParameter.GetTypeUsage());
		storeProviderServices.SetParameterValue(storeParameter, primitiveTypeUsageForScalar, entityParameter.Value);
		if (entityParameter.IsDirectionSpecified)
		{
			storeParameter.Direction = entityParameter.Direction;
		}
		if (entityParameter.IsIsNullableSpecified)
		{
			storeParameter.IsNullable = entityParameter.IsNullable;
		}
		if (entityParameter.IsSizeSpecified)
		{
			storeParameter.Size = entityParameter.Size;
		}
		if (entityParameter.IsPrecisionSpecified)
		{
			((IDbDataParameter)storeParameter).Precision = entityParameter.Precision;
		}
		if (entityParameter.IsScaleSpecified)
		{
			((IDbDataParameter)storeParameter).Scale = entityParameter.Scale;
		}
	}

	internal virtual string ToTraceString()
	{
		if (_mappedCommandDefinitions != null)
		{
			if (_mappedCommandDefinitions.Count == 1)
			{
				return _mappedCommandDefinitions[0].CreateCommand().CommandText;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (DbCommandDefinition mappedCommandDefinition in _mappedCommandDefinitions)
			{
				DbCommand dbCommand = mappedCommandDefinition.CreateCommand();
				stringBuilder.Append(dbCommand.CommandText);
			}
			return stringBuilder.ToString();
		}
		return string.Empty;
	}
}
