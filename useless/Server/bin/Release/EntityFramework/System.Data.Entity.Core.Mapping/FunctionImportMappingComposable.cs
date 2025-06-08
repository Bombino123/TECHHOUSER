using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Core.Query.PlanCompiler;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportMappingComposable : FunctionImportMapping
{
	private sealed class FunctionViewOpCopier : OpCopier
	{
		private readonly Dictionary<string, Node> m_viewArguments;

		private FunctionViewOpCopier(Command cmd, Dictionary<string, Node> viewArguments)
			: base(cmd)
		{
			m_viewArguments = viewArguments;
		}

		internal static Node Copy(Command cmd, Node viewNode, Dictionary<string, Node> viewArguments)
		{
			return new FunctionViewOpCopier(cmd, viewArguments).CopyNode(viewNode);
		}

		public override Node Visit(VarRefOp op, Node n)
		{
			if (op.Var.VarType == VarType.Parameter && m_viewArguments.TryGetValue(((ParameterVar)op.Var).ParameterName, out var value))
			{
				return OpCopier.Copy(m_destCmd, value);
			}
			return base.Visit(op, n);
		}
	}

	private readonly FunctionImportResultMapping _resultMapping;

	private readonly EntityContainerMapping _containerMapping;

	private readonly DbParameterReferenceExpression[] m_commandParameters;

	private readonly List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> m_structuralTypeMappings;

	private readonly EdmProperty[] m_targetFunctionKeys;

	private Node m_internalTreeNode;

	public FunctionImportResultMapping ResultMapping => _resultMapping;

	internal ReadOnlyCollection<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> StructuralTypeMappings
	{
		get
		{
			if (m_structuralTypeMappings != null)
			{
				return new ReadOnlyCollection<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>(m_structuralTypeMappings);
			}
			return null;
		}
	}

	internal EdmProperty[] TvfKeys => m_targetFunctionKeys;

	public FunctionImportMappingComposable(EdmFunction functionImport, EdmFunction targetFunction, FunctionImportResultMapping resultMapping, EntityContainerMapping containerMapping)
		: base(Check.NotNull(functionImport, "functionImport"), Check.NotNull(targetFunction, "targetFunction"))
	{
		Check.NotNull(resultMapping, "resultMapping");
		Check.NotNull(containerMapping, "containerMapping");
		if (!functionImport.IsComposableAttribute)
		{
			throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("functionImport"));
		}
		if (!targetFunction.IsComposableAttribute)
		{
			throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("targetFunction"));
		}
		if (!MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, 0, out var returnType))
		{
			throw new ArgumentException(Strings.InvalidReturnTypeForComposableFunction);
		}
		EdmFunction edmFunction = ((containerMapping.StorageMappingItemCollection != null) ? containerMapping.StorageMappingItemCollection.StoreItemCollection.ConvertToCTypeFunction(targetFunction) : StoreItemCollection.ConvertFunctionSignatureToCType(targetFunction));
		RowType tvfReturnType = TypeHelpers.GetTvfReturnType(edmFunction);
		RowType tvfReturnType2 = TypeHelpers.GetTvfReturnType(targetFunction);
		if (tvfReturnType == null)
		{
			throw new ArgumentException(Strings.Mapping_FunctionImport_ResultMapping_InvalidSType(functionImport.Identity), "functionImport");
		}
		List<EdmSchemaError> list = new List<EdmSchemaError>();
		FunctionImportMappingComposableHelper functionImportMappingComposableHelper = new FunctionImportMappingComposableHelper(containerMapping, string.Empty, list);
		FunctionImportMappingComposable mapping;
		if (Helper.IsStructuralType(returnType))
		{
			functionImportMappingComposableHelper.TryCreateFunctionImportMappingComposableWithStructuralResult(functionImport, edmFunction, resultMapping.SourceList, tvfReturnType, tvfReturnType2, LineInfo.Empty, out mapping);
		}
		else
		{
			functionImportMappingComposableHelper.TryCreateFunctionImportMappingComposableWithScalarResult(functionImport, edmFunction, targetFunction, returnType, tvfReturnType, LineInfo.Empty, out mapping);
		}
		if (mapping == null)
		{
			throw new InvalidOperationException((list.Count > 0) ? list[0].Message : string.Empty);
		}
		_containerMapping = mapping._containerMapping;
		m_commandParameters = mapping.m_commandParameters;
		m_structuralTypeMappings = mapping.m_structuralTypeMappings;
		m_targetFunctionKeys = mapping.m_targetFunctionKeys;
		_resultMapping = resultMapping;
	}

	internal FunctionImportMappingComposable(EdmFunction functionImport, EdmFunction targetFunction, List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> structuralTypeMappings)
		: base(functionImport, targetFunction)
	{
		if (!functionImport.IsComposableAttribute)
		{
			throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("functionImport"));
		}
		if (!targetFunction.IsComposableAttribute)
		{
			throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("targetFunction"));
		}
		if (!MetadataHelper.TryGetFunctionImportReturnType<EdmType>(functionImport, 0, out var returnType))
		{
			throw new ArgumentException(Strings.InvalidReturnTypeForComposableFunction);
		}
		if (!TypeSemantics.IsScalarType(returnType) && (structuralTypeMappings == null || structuralTypeMappings.Count == 0))
		{
			throw new ArgumentException(Strings.StructuralTypeMappingsMustNotBeNullForFunctionImportsReturningNonScalarValues);
		}
		m_structuralTypeMappings = structuralTypeMappings;
	}

	internal FunctionImportMappingComposable(EdmFunction functionImport, EdmFunction targetFunction, List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> structuralTypeMappings, EdmProperty[] targetFunctionKeys, EntityContainerMapping containerMapping)
		: base(functionImport, targetFunction)
	{
		_containerMapping = containerMapping;
		m_commandParameters = functionImport.Parameters.Select((FunctionParameter p) => TypeHelpers.GetPrimitiveTypeUsageForScalar(p.TypeUsage).Parameter(p.Name)).ToArray();
		m_structuralTypeMappings = structuralTypeMappings;
		m_targetFunctionKeys = targetFunctionKeys;
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_resultMapping);
		base.SetReadOnly();
	}

	internal Node GetInternalTree(Command targetIqtCommand, IList<Node> targetIqtArguments)
	{
		if (m_internalTreeNode == null)
		{
			DiscriminatorMap discriminatorMap;
			Command command = ITreeGenerator.Generate(GenerateFunctionView(out discriminatorMap), discriminatorMap);
			Node root = command.Root;
			PlanCompiler.Assert(root.Op.OpType == OpType.PhysicalProject, "Expected a physical projectOp at the root of the tree - found " + root.Op.OpType);
			PhysicalProjectOp obj = (PhysicalProjectOp)root.Op;
			Node child = root.Child0;
			command.DisableVarVecEnumCaching();
			Node node = child;
			Var computedVar = obj.Outputs[0];
			if (!Command.EqualTypes(obj.ColumnMap.Type, base.FunctionImport.ReturnParameter.TypeUsage))
			{
				TypeUsage typeUsage = ((CollectionType)base.FunctionImport.ReturnParameter.TypeUsage.EdmType).TypeUsage;
				Node arg = command.CreateNode(command.CreateVarRefOp(computedVar));
				Node definingExpr = command.CreateNode(command.CreateSoftCastOp(typeUsage), arg);
				Node arg2 = command.CreateVarDefListNode(definingExpr, out computedVar);
				ProjectOp op = command.CreateProjectOp(computedVar);
				node = command.CreateNode(op, node, arg2);
			}
			m_internalTreeNode = command.BuildCollect(node, computedVar);
		}
		Dictionary<string, Node> dictionary = new Dictionary<string, Node>(m_commandParameters.Length);
		for (int i = 0; i < m_commandParameters.Length; i++)
		{
			DbParameterReferenceExpression dbParameterReferenceExpression = m_commandParameters[i];
			Node node2 = targetIqtArguments[i];
			if (TypeSemantics.IsEnumerationType(node2.Op.Type))
			{
				node2 = targetIqtCommand.CreateNode(targetIqtCommand.CreateSoftCastOp(TypeHelpers.CreateEnumUnderlyingTypeUsage(node2.Op.Type)), node2);
			}
			dictionary.Add(dbParameterReferenceExpression.ParameterName, node2);
		}
		return FunctionViewOpCopier.Copy(targetIqtCommand, m_internalTreeNode, dictionary);
	}

	internal DbQueryCommandTree GenerateFunctionView(out DiscriminatorMap discriminatorMap)
	{
		discriminatorMap = null;
		DbExpression storeFunctionInvoke = base.TargetFunction.Invoke(GetParametersForTargetFunctionCall());
		return DbQueryCommandTree.FromValidExpression(query: (m_structuralTypeMappings == null) ? GenerateScalarResultMappingView(storeFunctionInvoke) : GenerateStructuralTypeResultMappingView(storeFunctionInvoke, out discriminatorMap), metadata: _containerMapping.StorageMappingItemCollection.Workspace, dataSpace: DataSpace.SSpace, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false);
	}

	private IEnumerable<DbExpression> GetParametersForTargetFunctionCall()
	{
		foreach (FunctionParameter targetParameter in base.TargetFunction.Parameters)
		{
			FunctionParameter value = base.FunctionImport.Parameters.Single((FunctionParameter p) => p.Name == targetParameter.Name);
			yield return m_commandParameters[base.FunctionImport.Parameters.IndexOf(value)];
		}
	}

	private DbExpression GenerateStructuralTypeResultMappingView(DbExpression storeFunctionInvoke, out DiscriminatorMap discriminatorMap)
	{
		discriminatorMap = null;
		DbExpression dbExpression = storeFunctionInvoke;
		if (m_structuralTypeMappings.Count == 1)
		{
			Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>> tuple = m_structuralTypeMappings[0];
			StructuralType item = tuple.Item1;
			List<ConditionPropertyMapping> conditions = tuple.Item2;
			List<PropertyMapping> item2 = tuple.Item3;
			if (conditions.Count > 0)
			{
				dbExpression = dbExpression.Where((DbExpression row) => GenerateStructuralTypeConditionsPredicate(conditions, row));
			}
			DbExpressionBinding dbExpressionBinding = dbExpression.BindAs("row");
			DbExpression projection = GenerateStructuralTypeMappingView(item, item2, dbExpressionBinding.Variable);
			dbExpression = dbExpressionBinding.Project(projection);
		}
		else
		{
			DbExpressionBinding binding = dbExpression.BindAs("row");
			List<DbExpression> list = m_structuralTypeMappings.Select((Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>> m) => GenerateStructuralTypeConditionsPredicate(m.Item2, binding.Variable)).ToList();
			dbExpression = binding.Filter(Helpers.BuildBalancedTreeInPlace(list.ToArray(), (DbExpression prev, DbExpression next) => prev.Or(next)));
			binding = dbExpression.BindAs("row");
			List<DbExpression> list2 = new List<DbExpression>(m_structuralTypeMappings.Count);
			foreach (Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>> structuralTypeMapping in m_structuralTypeMappings)
			{
				StructuralType item3 = structuralTypeMapping.Item1;
				List<PropertyMapping> item4 = structuralTypeMapping.Item3;
				list2.Add(GenerateStructuralTypeMappingView(item3, item4, binding.Variable));
			}
			DbExpression projection2 = DbExpressionBuilder.Case(list.Take(m_structuralTypeMappings.Count - 1), list2.Take(m_structuralTypeMappings.Count - 1), list2[m_structuralTypeMappings.Count - 1]);
			dbExpression = binding.Project(projection2);
			DiscriminatorMap.TryCreateDiscriminatorMap(base.FunctionImport.EntitySet, dbExpression, out discriminatorMap);
		}
		return dbExpression;
	}

	private static DbExpression GenerateStructuralTypeMappingView(StructuralType structuralType, List<PropertyMapping> propertyMappings, DbExpression row)
	{
		List<DbExpression> list = new List<DbExpression>(TypeHelpers.GetAllStructuralMembers(structuralType).Count);
		for (int i = 0; i < propertyMappings.Count; i++)
		{
			PropertyMapping mapping = propertyMappings[i];
			list.Add(GeneratePropertyMappingView(mapping, row));
		}
		return TypeUsage.Create(structuralType).New(list);
	}

	private static DbExpression GenerateStructuralTypeConditionsPredicate(List<ConditionPropertyMapping> conditions, DbExpression row)
	{
		return Helpers.BuildBalancedTreeInPlace(conditions.Select((ConditionPropertyMapping c) => GeneratePredicate(c, row)).ToArray(), (DbExpression prev, DbExpression next) => prev.And(next));
	}

	private static DbExpression GeneratePredicate(ConditionPropertyMapping condition, DbExpression row)
	{
		DbExpression dbExpression = GenerateColumnRef(row, condition.Column);
		if (condition.IsNull.HasValue)
		{
			if (!condition.IsNull.Value)
			{
				return dbExpression.IsNull().Not();
			}
			return dbExpression.IsNull();
		}
		return dbExpression.Equal(dbExpression.ResultType.Constant(condition.Value));
	}

	private static DbExpression GeneratePropertyMappingView(PropertyMapping mapping, DbExpression row)
	{
		ScalarPropertyMapping scalarPropertyMapping = (ScalarPropertyMapping)mapping;
		return GenerateScalarPropertyMappingView(scalarPropertyMapping.Property, scalarPropertyMapping.Column, row);
	}

	private static DbExpression GenerateScalarPropertyMappingView(EdmProperty edmProperty, EdmProperty columnProperty, DbExpression row)
	{
		DbExpression dbExpression = GenerateColumnRef(row, columnProperty);
		if (!TypeSemantics.IsEqual(dbExpression.ResultType, edmProperty.TypeUsage))
		{
			dbExpression = dbExpression.CastTo(edmProperty.TypeUsage);
		}
		return dbExpression;
	}

	private static DbExpression GenerateColumnRef(DbExpression row, EdmProperty column)
	{
		_ = (RowType)row.ResultType.EdmType;
		return row.Property(column.Name);
	}

	private DbExpression GenerateScalarResultMappingView(DbExpression storeFunctionInvoke)
	{
		MetadataHelper.TryGetFunctionImportReturnCollectionType(base.FunctionImport, 0, out var functionImportReturnType);
		RowType rowType = (RowType)((CollectionType)storeFunctionInvoke.ResultType.EdmType).TypeUsage.EdmType;
		EdmProperty column = rowType.Properties[0];
		Func<DbExpression, DbExpression> scalarView = delegate(DbExpression row)
		{
			DbPropertyExpression dbPropertyExpression = row.Property(column);
			return TypeSemantics.IsEqual(functionImportReturnType.TypeUsage, column.TypeUsage) ? ((DbExpression)dbPropertyExpression) : ((DbExpression)dbPropertyExpression.CastTo(functionImportReturnType.TypeUsage));
		};
		return storeFunctionInvoke.Select((DbExpression row) => scalarView(row));
	}
}
