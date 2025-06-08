using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class SemanticResolver
{
	private readonly ParserOptions _parserOptions;

	private readonly Dictionary<string, DbParameterReferenceExpression> _parameters;

	private readonly Dictionary<string, DbVariableReferenceExpression> _variables;

	private readonly TypeResolver _typeResolver;

	private readonly ScopeManager _scopeManager;

	private readonly List<ScopeRegion> _scopeRegions = new List<ScopeRegion>();

	private bool _ignoreEntityContainerNameResolution;

	private GroupAggregateInfo _currentGroupAggregateInfo;

	private uint _namegenCounter;

	internal Dictionary<string, DbParameterReferenceExpression> Parameters => _parameters;

	internal Dictionary<string, DbVariableReferenceExpression> Variables => _variables;

	internal TypeResolver TypeResolver => _typeResolver;

	internal ParserOptions ParserOptions => _parserOptions;

	internal StringComparer NameComparer => _parserOptions.NameComparer;

	internal IEnumerable<ScopeRegion> ScopeRegions => _scopeRegions;

	internal ScopeRegion CurrentScopeRegion => _scopeRegions[_scopeRegions.Count - 1];

	internal Scope CurrentScope => _scopeManager.CurrentScope;

	internal int CurrentScopeIndex => _scopeManager.CurrentScopeIndex;

	internal GroupAggregateInfo CurrentGroupAggregateInfo => _currentGroupAggregateInfo;

	internal static SemanticResolver Create(Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters, IEnumerable<DbVariableReferenceExpression> variables)
	{
		return new SemanticResolver(parserOptions, ProcessParameters(parameters, parserOptions), ProcessVariables(variables, parserOptions), new TypeResolver(perspective, parserOptions));
	}

	internal SemanticResolver CloneForInlineFunctionConversion()
	{
		return new SemanticResolver(_parserOptions, _parameters, _variables, _typeResolver);
	}

	private SemanticResolver(ParserOptions parserOptions, Dictionary<string, DbParameterReferenceExpression> parameters, Dictionary<string, DbVariableReferenceExpression> variables, TypeResolver typeResolver)
	{
		_parserOptions = parserOptions;
		_parameters = parameters;
		_variables = variables;
		_typeResolver = typeResolver;
		_scopeManager = new ScopeManager(NameComparer);
		EnterScopeRegion();
		foreach (DbVariableReferenceExpression value in _variables.Values)
		{
			CurrentScope.Add(value.VariableName, new FreeVariableScopeEntry(value));
		}
	}

	private static Dictionary<string, DbParameterReferenceExpression> ProcessParameters(IEnumerable<DbParameterReferenceExpression> paramDefs, ParserOptions parserOptions)
	{
		Dictionary<string, DbParameterReferenceExpression> dictionary = new Dictionary<string, DbParameterReferenceExpression>(parserOptions.NameComparer);
		if (paramDefs != null)
		{
			foreach (DbParameterReferenceExpression paramDef in paramDefs)
			{
				if (dictionary.ContainsKey(paramDef.ParameterName))
				{
					throw new EntitySqlException(Strings.MultipleDefinitionsOfParameter(paramDef.ParameterName));
				}
				dictionary.Add(paramDef.ParameterName, paramDef);
			}
		}
		return dictionary;
	}

	private static Dictionary<string, DbVariableReferenceExpression> ProcessVariables(IEnumerable<DbVariableReferenceExpression> varDefs, ParserOptions parserOptions)
	{
		Dictionary<string, DbVariableReferenceExpression> dictionary = new Dictionary<string, DbVariableReferenceExpression>(parserOptions.NameComparer);
		if (varDefs != null)
		{
			foreach (DbVariableReferenceExpression varDef in varDefs)
			{
				if (dictionary.ContainsKey(varDef.VariableName))
				{
					throw new EntitySqlException(Strings.MultipleDefinitionsOfVariable(varDef.VariableName));
				}
				dictionary.Add(varDef.VariableName, varDef);
			}
		}
		return dictionary;
	}

	private DbExpression GetExpressionFromScopeEntry(ScopeEntry scopeEntry, int scopeIndex, string varName, ErrorContext errCtx)
	{
		DbExpression result = scopeEntry.GetExpression(varName, errCtx);
		if (_currentGroupAggregateInfo != null)
		{
			ScopeRegion definingScopeRegion = GetDefiningScopeRegion(scopeIndex);
			if (definingScopeRegion.ScopeRegionIndex <= _currentGroupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex)
			{
				_currentGroupAggregateInfo.UpdateScopeIndex(scopeIndex, this);
				if (scopeEntry is IGroupExpressionExtendedInfo groupExpressionExtendedInfo)
				{
					GroupAggregateInfo groupAggregateInfo = _currentGroupAggregateInfo;
					while (groupAggregateInfo != null && groupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex >= definingScopeRegion.ScopeRegionIndex && groupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex != definingScopeRegion.ScopeRegionIndex)
					{
						groupAggregateInfo = groupAggregateInfo.ContainingAggregate;
					}
					if (groupAggregateInfo == null || groupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex < definingScopeRegion.ScopeRegionIndex)
					{
						groupAggregateInfo = _currentGroupAggregateInfo;
					}
					switch (groupAggregateInfo.AggregateKind)
					{
					case GroupAggregateKind.Function:
						if (groupExpressionExtendedInfo.GroupVarBasedExpression != null)
						{
							result = groupExpressionExtendedInfo.GroupVarBasedExpression;
						}
						break;
					case GroupAggregateKind.Partition:
						if (groupExpressionExtendedInfo.GroupAggBasedExpression != null)
						{
							result = groupExpressionExtendedInfo.GroupAggBasedExpression;
						}
						break;
					}
				}
			}
		}
		return result;
	}

	internal IDisposable EnterIgnoreEntityContainerNameResolution()
	{
		_ignoreEntityContainerNameResolution = true;
		return new Disposer(delegate
		{
			_ignoreEntityContainerNameResolution = false;
		});
	}

	internal ExpressionResolution ResolveSimpleName(string name, bool leftHandSideOfMemberAccess, ErrorContext errCtx)
	{
		if (TryScopeLookup(name, out var scopeEntry, out var scopeIndex))
		{
			if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar && ((SourceScopeEntry)scopeEntry).IsJoinClauseLeftExpr)
			{
				string invalidJoinLeftCorrelation = Strings.InvalidJoinLeftCorrelation;
				throw EntitySqlException.Create(errCtx, invalidJoinLeftCorrelation, null);
			}
			SetScopeRegionCorrelationFlag(scopeIndex);
			return new ValueExpression(GetExpressionFromScopeEntry(scopeEntry, scopeIndex, name, errCtx));
		}
		EntityContainer defaultContainer = TypeResolver.Perspective.GetDefaultContainer();
		if (defaultContainer != null && TryResolveEntityContainerMemberAccess(defaultContainer, name, out var resolution))
		{
			return resolution;
		}
		if (!_ignoreEntityContainerNameResolution && TypeResolver.Perspective.TryGetEntityContainer(name, _parserOptions.NameComparisonCaseInsensitive, out var entityContainer))
		{
			return new EntityContainerExpression(entityContainer);
		}
		return TypeResolver.ResolveUnqualifiedName(name, leftHandSideOfMemberAccess, errCtx);
	}

	internal MetadataMember ResolveSimpleFunctionName(string name, ErrorContext errCtx)
	{
		MetadataMember metadataMember = TypeResolver.ResolveUnqualifiedName(name, partOfQualifiedName: false, errCtx);
		if (metadataMember.MetadataMemberClass == MetadataMemberClass.Namespace)
		{
			EntityContainer defaultContainer = TypeResolver.Perspective.GetDefaultContainer();
			if (defaultContainer != null && TryResolveEntityContainerMemberAccess(defaultContainer, name, out var resolution) && resolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
			{
				metadataMember = (MetadataMember)resolution;
			}
		}
		return metadataMember;
	}

	private bool TryScopeLookup(string key, out ScopeEntry scopeEntry, out int scopeIndex)
	{
		scopeEntry = null;
		scopeIndex = -1;
		for (int num = CurrentScopeIndex; num >= 0; num--)
		{
			if (_scopeManager.GetScopeByIndex(num).TryLookup(key, out scopeEntry))
			{
				scopeIndex = num;
				return true;
			}
		}
		return false;
	}

	internal MetadataMember ResolveMetadataMemberName(string[] name, ErrorContext errCtx)
	{
		return TypeResolver.ResolveMetadataMemberName(name, errCtx);
	}

	internal ValueExpression ResolvePropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx)
	{
		if (TryResolveAsPropertyAccess(valueExpr, name, out var propertyExpr))
		{
			return new ValueExpression(propertyExpr);
		}
		if (TryResolveAsRefPropertyAccess(valueExpr, name, errCtx, out propertyExpr))
		{
			return new ValueExpression(propertyExpr);
		}
		if (TypeSemantics.IsCollectionType(valueExpr.ResultType))
		{
			string errorMessage = Strings.NotAMemberOfCollection(name, valueExpr.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		string errorMessage2 = Strings.NotAMemberOfType(name, valueExpr.ResultType.EdmType.FullName);
		throw EntitySqlException.Create(errCtx, errorMessage2, null);
	}

	private bool TryResolveAsPropertyAccess(DbExpression valueExpr, string name, out DbExpression propertyExpr)
	{
		propertyExpr = null;
		if (Helper.IsStructuralType(valueExpr.ResultType.EdmType) && TypeResolver.Perspective.TryGetMember((StructuralType)valueExpr.ResultType.EdmType, name, _parserOptions.NameComparisonCaseInsensitive, out var outMember))
		{
			propertyExpr = DbExpressionBuilder.CreatePropertyExpressionFromMember(valueExpr, outMember);
			return true;
		}
		return false;
	}

	private bool TryResolveAsRefPropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx, out DbExpression propertyExpr)
	{
		propertyExpr = null;
		if (TypeSemantics.IsReferenceType(valueExpr.ResultType))
		{
			DbExpression dbExpression = valueExpr.Deref();
			TypeUsage resultType = dbExpression.ResultType;
			if (TryResolveAsPropertyAccess(dbExpression, name, out propertyExpr))
			{
				return true;
			}
			string errorMessage = Strings.InvalidDeRefProperty(name, resultType.EdmType.FullName, valueExpr.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return false;
	}

	internal ExpressionResolution ResolveEntityContainerMemberAccess(EntityContainer entityContainer, string name, ErrorContext errCtx)
	{
		if (TryResolveEntityContainerMemberAccess(entityContainer, name, out var resolution))
		{
			return resolution;
		}
		string errorMessage = Strings.MemberDoesNotBelongToEntityContainer(name, entityContainer.Name);
		throw EntitySqlException.Create(errCtx, errorMessage, null);
	}

	private bool TryResolveEntityContainerMemberAccess(EntityContainer entityContainer, string name, out ExpressionResolution resolution)
	{
		if (TypeResolver.Perspective.TryGetExtent(entityContainer, name, _parserOptions.NameComparisonCaseInsensitive, out var outSet))
		{
			resolution = new ValueExpression(outSet.Scan());
			return true;
		}
		if (TypeResolver.Perspective.TryGetFunctionImport(entityContainer, name, _parserOptions.NameComparisonCaseInsensitive, out var functionImport))
		{
			resolution = new MetadataFunctionGroup(functionImport.FullName, new EdmFunction[1] { functionImport });
			return true;
		}
		resolution = null;
		return false;
	}

	internal MetadataMember ResolveMetadataMemberAccess(MetadataMember metadataMember, string name, ErrorContext errCtx)
	{
		return TypeResolver.ResolveMetadataMemberAccess(metadataMember, name, errCtx);
	}

	internal bool TryResolveInternalAggregateName(string name, ErrorContext errCtx, out DbExpression dbExpression)
	{
		if (TryScopeLookup(name, out var scopeEntry, out var scopeIndex))
		{
			SetScopeRegionCorrelationFlag(scopeIndex);
			dbExpression = scopeEntry.GetExpression(name, errCtx);
			return true;
		}
		dbExpression = null;
		return false;
	}

	internal bool TryResolveDotExprAsGroupKeyAlternativeName(DotExpr dotExpr, out ValueExpression groupKeyResolution)
	{
		groupKeyResolution = null;
		if (IsInAnyGroupScope() && dotExpr.IsMultipartIdentifier(out var names) && TryScopeLookup(TypeResolver.GetFullName(names), out var scopeEntry, out var scopeIndex) && scopeEntry is IGetAlternativeName { AlternativeName: not null } getAlternativeName && names.SequenceEqual<string>(getAlternativeName.AlternativeName, NameComparer))
		{
			SetScopeRegionCorrelationFlag(scopeIndex);
			groupKeyResolution = new ValueExpression(GetExpressionFromScopeEntry(scopeEntry, scopeIndex, TypeResolver.GetFullName(names), dotExpr.ErrCtx));
			return true;
		}
		return false;
	}

	internal string GenerateInternalName(string hint)
	{
		return "_##" + hint + _namegenCounter++.ToString(CultureInfo.InvariantCulture);
	}

	private string CreateNewAlias(DbExpression expr)
	{
		if (expr is DbScanExpression dbScanExpression)
		{
			return dbScanExpression.Target.Name;
		}
		if (expr is DbPropertyExpression dbPropertyExpression)
		{
			return dbPropertyExpression.Property.Name;
		}
		if (expr is DbVariableReferenceExpression dbVariableReferenceExpression)
		{
			return dbVariableReferenceExpression.VariableName;
		}
		return GenerateInternalName(string.Empty);
	}

	internal string InferAliasName(AliasedExpr aliasedExpr, DbExpression convertedExpression)
	{
		if (aliasedExpr.Alias != null)
		{
			return aliasedExpr.Alias.Name;
		}
		if (aliasedExpr.Expr is Identifier identifier)
		{
			return identifier.Name;
		}
		if (aliasedExpr.Expr is DotExpr dotExpr && dotExpr.IsMultipartIdentifier(out var names))
		{
			return names[^1];
		}
		return CreateNewAlias(convertedExpression);
	}

	internal IDisposable EnterScopeRegion()
	{
		_scopeManager.EnterScope();
		ScopeRegion item = new ScopeRegion(_scopeManager, CurrentScopeIndex, _scopeRegions.Count);
		_scopeRegions.Add(item);
		return new Disposer(delegate
		{
			CurrentScopeRegion.GroupAggregateInfos.Each(delegate(GroupAggregateInfo groupAggregateInfo)
			{
				groupAggregateInfo.DetachFromAstNode();
			});
			CurrentScopeRegion.RollbackAllScopes();
			_scopeRegions.Remove(CurrentScopeRegion);
		});
	}

	internal void RollbackToScope(int scopeIndex)
	{
		_scopeManager.RollbackToScope(scopeIndex);
	}

	internal void EnterScope()
	{
		_scopeManager.EnterScope();
	}

	internal void LeaveScope()
	{
		_scopeManager.LeaveScope();
	}

	internal bool IsInAnyGroupScope()
	{
		for (int i = 0; i < _scopeRegions.Count; i++)
		{
			if (_scopeRegions[i].IsAggregating)
			{
				return true;
			}
		}
		return false;
	}

	internal ScopeRegion GetDefiningScopeRegion(int scopeIndex)
	{
		for (int num = _scopeRegions.Count - 1; num >= 0; num--)
		{
			if (_scopeRegions[num].ContainsScope(scopeIndex))
			{
				return _scopeRegions[num];
			}
		}
		return null;
	}

	private void SetScopeRegionCorrelationFlag(int scopeIndex)
	{
		GetDefiningScopeRegion(scopeIndex).WasResolutionCorrelated = true;
	}

	internal IDisposable EnterFunctionAggregate(MethodExpr methodExpr, ErrorContext errCtx, out FunctionAggregateInfo aggregateInfo)
	{
		aggregateInfo = new FunctionAggregateInfo(methodExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
		return EnterGroupAggregate(aggregateInfo);
	}

	internal IDisposable EnterGroupPartition(GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, out GroupPartitionInfo aggregateInfo)
	{
		aggregateInfo = new GroupPartitionInfo(groupPartitionExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
		return EnterGroupAggregate(aggregateInfo);
	}

	internal IDisposable EnterGroupKeyDefinition(GroupAggregateKind aggregateKind, ErrorContext errCtx, out GroupKeyAggregateInfo aggregateInfo)
	{
		aggregateInfo = new GroupKeyAggregateInfo(aggregateKind, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
		return EnterGroupAggregate(aggregateInfo);
	}

	private IDisposable EnterGroupAggregate(GroupAggregateInfo aggregateInfo)
	{
		_currentGroupAggregateInfo = aggregateInfo;
		return new Disposer(delegate
		{
			_currentGroupAggregateInfo = aggregateInfo.ContainingAggregate;
			aggregateInfo.ValidateAndComputeEvaluatingScopeRegion(this);
		});
	}

	internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata, IList<TypeUsage> argTypes, bool isGroupAggregateFunction, out bool isAmbiguous)
	{
		return FunctionOverloadResolver.ResolveFunctionOverloads(functionsMetadata, argTypes, UntypedNullAwareFlattenArgumentType, UntypedNullAwareFlattenParameterType, UntypedNullAwareIsPromotableTo, UntypedNullAwareIsStructurallyEqual, isGroupAggregateFunction, out isAmbiguous);
	}

	internal static TFunctionMetadata ResolveFunctionOverloads<TFunctionMetadata, TFunctionParameterMetadata>(IList<TFunctionMetadata> functionsMetadata, IList<TypeUsage> argTypes, Func<TFunctionMetadata, IList<TFunctionParameterMetadata>> getSignatureParams, Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage, Func<TFunctionParameterMetadata, ParameterMode> getParameterMode, bool isGroupAggregateFunction, out bool isAmbiguous) where TFunctionMetadata : class
	{
		return FunctionOverloadResolver.ResolveFunctionOverloads(functionsMetadata, argTypes, getSignatureParams, getParameterTypeUsage, getParameterMode, UntypedNullAwareFlattenArgumentType, UntypedNullAwareFlattenParameterType, UntypedNullAwareIsPromotableTo, UntypedNullAwareIsStructurallyEqual, isGroupAggregateFunction, out isAmbiguous);
	}

	private static IEnumerable<TypeUsage> UntypedNullAwareFlattenArgumentType(TypeUsage argType)
	{
		if (argType == null)
		{
			return new TypeUsage[1];
		}
		return TypeSemantics.FlattenType(argType);
	}

	private static IEnumerable<TypeUsage> UntypedNullAwareFlattenParameterType(TypeUsage paramType, TypeUsage argType)
	{
		if (argType == null)
		{
			return new TypeUsage[1] { paramType };
		}
		return TypeSemantics.FlattenType(paramType);
	}

	private static bool UntypedNullAwareIsPromotableTo(TypeUsage fromType, TypeUsage toType)
	{
		if (fromType == null)
		{
			return !Helper.IsCollectionType(toType.EdmType);
		}
		return TypeSemantics.IsPromotableTo(fromType, toType);
	}

	private static bool UntypedNullAwareIsStructurallyEqual(TypeUsage fromType, TypeUsage toType)
	{
		if (fromType == null)
		{
			return UntypedNullAwareIsPromotableTo(fromType, toType);
		}
		return TypeSemantics.IsStructurallyEqual(fromType, toType);
	}
}
