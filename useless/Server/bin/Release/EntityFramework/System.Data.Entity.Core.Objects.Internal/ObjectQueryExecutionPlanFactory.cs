using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ObjectQueryExecutionPlanFactory
{
	private readonly Translator _translator;

	public ObjectQueryExecutionPlanFactory(Translator translator = null)
	{
		_translator = translator ?? new Translator();
	}

	public virtual ObjectQueryExecutionPlan Prepare(ObjectContext context, DbQueryCommandTree tree, Type elementType, MergeOption mergeOption, bool streaming, Span span, IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters, AliasGenerator aliasGenerator)
	{
		TypeUsage resultType = tree.Query.ResultType;
		if (ObjectSpanRewriter.TryRewrite(tree, span, mergeOption, aliasGenerator, out var newQuery, out var spanInfo))
		{
			tree = DbQueryCommandTree.FromValidExpression(tree.MetadataWorkspace, tree.DataSpace, newQuery, tree.UseDatabaseNullSemantics, tree.DisableFilterOverProjectionSimplificationForCustomFunctions);
		}
		else
		{
			spanInfo = null;
		}
		EntityCommandDefinition entityCommandDefinition = CreateCommandDefinition(context, tree);
		ShaperFactory resultShaperFactory = Translator.TranslateColumnMap(_translator, elementType, entityCommandDefinition.CreateColumnMap(null), context.MetadataWorkspace, spanInfo, mergeOption, streaming, valueLayer: false);
		EntitySet entitySet = null;
		if (resultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType && entityCommandDefinition.EntitySets != null)
		{
			foreach (EntitySet entitySet2 in entityCommandDefinition.EntitySets)
			{
				if (entitySet2 != null && entitySet2.ElementType.IsAssignableFrom(((CollectionType)resultType.EdmType).TypeUsage.EdmType))
				{
					if (entitySet != null)
					{
						entitySet = null;
						break;
					}
					entitySet = entitySet2;
				}
			}
		}
		return new ObjectQueryExecutionPlan(entityCommandDefinition, resultShaperFactory, resultType, mergeOption, streaming, entitySet, compiledQueryParameters);
	}

	private static EntityCommandDefinition CreateCommandDefinition(ObjectContext context, DbQueryCommandTree tree)
	{
		DbProviderServices providerServices = DbProviderServices.GetProviderServices(context.Connection ?? throw new InvalidOperationException(Strings.ObjectQuery_InvalidConnection));
		DbCommandDefinition dbCommandDefinition;
		try
		{
			dbCommandDefinition = providerServices.CreateCommandDefinition(tree, context.InterceptionContext);
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
		if (dbCommandDefinition == null)
		{
			throw new NotSupportedException(Strings.ADP_ProviderDoesNotSupportCommandTrees);
		}
		return (EntityCommandDefinition)dbCommandDefinition;
	}
}
