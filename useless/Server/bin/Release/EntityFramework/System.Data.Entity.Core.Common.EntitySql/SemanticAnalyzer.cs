using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class SemanticAnalyzer
{
	private delegate ParseResult StatementConverter(Statement astExpr, SemanticResolver sr);

	private sealed class InlineFunctionInfoImpl : InlineFunctionInfo
	{
		private DbLambda _convertedDefinition;

		private bool _convertingDefinition;

		internal InlineFunctionInfoImpl(System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
			: base(functionDef, parameters)
		{
		}

		internal override DbLambda GetLambda(SemanticResolver sr)
		{
			if (_convertedDefinition == null)
			{
				if (_convertingDefinition)
				{
					ErrorContext errCtx = FunctionDefAst.ErrCtx;
					string errorMessage = Strings.Cqt_UDF_FunctionDefinitionWithCircularReference(FunctionDefAst.Name);
					throw EntitySqlException.Create(errCtx, errorMessage, null);
				}
				SemanticResolver sr2 = sr.CloneForInlineFunctionConversion();
				_convertingDefinition = true;
				_convertedDefinition = ConvertInlineFunctionDefinition(this, sr2);
				_convertingDefinition = false;
			}
			return _convertedDefinition;
		}
	}

	private sealed class GroupKeyInfo
	{
		internal readonly string Name;

		private string[] _alternativeName;

		internal readonly DbVariableReferenceExpression VarRef;

		internal readonly DbExpression VarBasedKeyExpr;

		internal readonly DbExpression GroupVarBasedKeyExpr;

		internal readonly DbExpression GroupAggBasedKeyExpr;

		internal string[] AlternativeName
		{
			get
			{
				return _alternativeName;
			}
			set
			{
				_alternativeName = value;
			}
		}

		internal GroupKeyInfo(string name, DbExpression varBasedKeyExpr, DbExpression groupVarBasedKeyExpr, DbExpression groupAggBasedKeyExpr)
		{
			Name = name;
			VarRef = varBasedKeyExpr.ResultType.Variable(name);
			VarBasedKeyExpr = varBasedKeyExpr;
			GroupVarBasedKeyExpr = groupVarBasedKeyExpr;
			GroupAggBasedKeyExpr = groupAggBasedKeyExpr;
		}
	}

	private delegate ExpressionResolution AstExprConverter(Node astExpr, SemanticResolver sr);

	private delegate DbExpression BuiltInExprConverter(BuiltInExpr astBltInExpr, SemanticResolver sr);

	private readonly SemanticResolver _sr;

	private static readonly DbExpressionKind[] _joinMap = new DbExpressionKind[4]
	{
		DbExpressionKind.CrossJoin,
		DbExpressionKind.InnerJoin,
		DbExpressionKind.LeftOuterJoin,
		DbExpressionKind.FullOuterJoin
	};

	private static readonly DbExpressionKind[] _applyMap = new DbExpressionKind[2]
	{
		DbExpressionKind.CrossApply,
		DbExpressionKind.OuterApply
	};

	private static readonly Dictionary<Type, AstExprConverter> _astExprConverters = CreateAstExprConverters();

	private static readonly Dictionary<BuiltInKind, BuiltInExprConverter> _builtInExprConverter = CreateBuiltInExprConverter();

	internal SemanticAnalyzer(SemanticResolver sr)
	{
		_sr = sr;
	}

	internal ParseResult AnalyzeCommand(Node astExpr)
	{
		Command command = ValidateQueryCommandAst(astExpr);
		ConvertAndRegisterNamespaceImports(command.NamespaceImportList, command.ErrCtx, _sr);
		return ConvertStatement(command.Statement, _sr);
	}

	internal DbLambda AnalyzeQueryCommand(Node astExpr)
	{
		Command command = ValidateQueryCommandAst(astExpr);
		ConvertAndRegisterNamespaceImports(command.NamespaceImportList, command.ErrCtx, _sr);
		List<FunctionDefinition> functionDefs;
		return DbExpressionBuilder.Lambda(ConvertQueryStatementToDbExpression(command.Statement, _sr, out functionDefs), _sr.Variables.Values);
	}

	private static Command ValidateQueryCommandAst(Node astExpr)
	{
		if (!(astExpr is Command command))
		{
			throw new ArgumentException(Strings.UnknownAstCommandExpression);
		}
		if (!(command.Statement is QueryStatement))
		{
			throw new ArgumentException(Strings.UnknownAstExpressionType);
		}
		return command;
	}

	private static void ConvertAndRegisterNamespaceImports(NodeList<NamespaceImport> nsImportList, ErrorContext cmdErrCtx, SemanticResolver sr)
	{
		List<Tuple<string, MetadataNamespace, ErrorContext>> list = new List<Tuple<string, MetadataNamespace, ErrorContext>>();
		List<Tuple<MetadataNamespace, ErrorContext>> list2 = new List<Tuple<MetadataNamespace, ErrorContext>>();
		if (nsImportList != null)
		{
			foreach (NamespaceImport item in (IEnumerable<NamespaceImport>)nsImportList)
			{
				string[] names = null;
				if (item.NamespaceName is Identifier identifier)
				{
					names = new string[1] { identifier.Name };
				}
				if (item.NamespaceName is DotExpr dotExpr)
				{
					dotExpr.IsMultipartIdentifier(out names);
				}
				if (names == null)
				{
					ErrorContext errCtx = item.NamespaceName.ErrCtx;
					string invalidMetadataMemberName = Strings.InvalidMetadataMemberName;
					throw EntitySqlException.Create(errCtx, invalidMetadataMemberName, null);
				}
				string text = ((item.Alias != null) ? item.Alias.Name : null);
				MetadataMember metadataMember = sr.ResolveMetadataMemberName(names, item.NamespaceName.ErrCtx);
				if (metadataMember.MetadataMemberClass == MetadataMemberClass.Namespace)
				{
					MetadataNamespace metadataNamespace = (MetadataNamespace)metadataMember;
					if (text != null)
					{
						list.Add(Tuple.Create(text, metadataNamespace, item.ErrCtx));
					}
					else
					{
						list2.Add(Tuple.Create(metadataNamespace, item.ErrCtx));
					}
					continue;
				}
				ErrorContext errCtx2 = item.NamespaceName.ErrCtx;
				string errorMessage = Strings.InvalidMetadataMemberClassResolution(metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataNamespace.NamespaceClassName);
				throw EntitySqlException.Create(errCtx2, errorMessage, null);
			}
		}
		sr.TypeResolver.AddNamespaceImport(new MetadataNamespace("Edm"), (nsImportList != null) ? nsImportList.ErrCtx : cmdErrCtx);
		foreach (Tuple<string, MetadataNamespace, ErrorContext> item2 in list)
		{
			sr.TypeResolver.AddAliasedNamespaceImport(item2.Item1, item2.Item2, item2.Item3);
		}
		foreach (Tuple<MetadataNamespace, ErrorContext> item3 in list2)
		{
			sr.TypeResolver.AddNamespaceImport(item3.Item1, item3.Item2);
		}
	}

	private static ParseResult ConvertStatement(Statement astStatement, SemanticResolver sr)
	{
		if (astStatement is QueryStatement)
		{
			StatementConverter statementConverter = ConvertQueryStatementToDbCommandTree;
			return statementConverter(astStatement, sr);
		}
		throw new ArgumentException(Strings.UnknownAstExpressionType);
	}

	private static ParseResult ConvertQueryStatementToDbCommandTree(Statement astStatement, SemanticResolver sr)
	{
		List<FunctionDefinition> functionDefs;
		DbExpression query = ConvertQueryStatementToDbExpression(astStatement, sr, out functionDefs);
		return new ParseResult(DbQueryCommandTree.FromValidExpression(sr.TypeResolver.Perspective.MetadataWorkspace, sr.TypeResolver.Perspective.TargetDataspace, query, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false), functionDefs);
	}

	private static DbExpression ConvertQueryStatementToDbExpression(Statement astStatement, SemanticResolver sr, out List<FunctionDefinition> functionDefs)
	{
		if (!(astStatement is QueryStatement queryStatement))
		{
			throw new ArgumentException(Strings.UnknownAstExpressionType);
		}
		functionDefs = ConvertInlineFunctionDefinitions(queryStatement.FunctionDefList, sr);
		DbExpression dbExpression = ConvertValueExpressionAllowUntypedNulls(queryStatement.Expr, sr);
		if (dbExpression == null)
		{
			ErrorContext errCtx = queryStatement.Expr.ErrCtx;
			string resultingExpressionTypeCannotBeNull = Strings.ResultingExpressionTypeCannotBeNull;
			throw EntitySqlException.Create(errCtx, resultingExpressionTypeCannotBeNull, null);
		}
		if (dbExpression is DbScanExpression)
		{
			DbExpressionBinding dbExpressionBinding = dbExpression.BindAs(sr.GenerateInternalName("extent"));
			dbExpression = dbExpressionBinding.Project(dbExpressionBinding.Variable);
		}
		if (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.NormalMode)
		{
			ValidateQueryResultType(dbExpression.ResultType, queryStatement.Expr.ErrCtx);
		}
		return dbExpression;
	}

	private static void ValidateQueryResultType(TypeUsage resultType, ErrorContext errCtx)
	{
		if (Helper.IsCollectionType(resultType.EdmType))
		{
			ValidateQueryResultType(((CollectionType)resultType.EdmType).TypeUsage, errCtx);
			return;
		}
		if (Helper.IsRowType(resultType.EdmType))
		{
			foreach (EdmProperty property in ((RowType)resultType.EdmType).Properties)
			{
				ValidateQueryResultType(property.TypeUsage, errCtx);
			}
			return;
		}
		if (!Helper.IsAssociationType(resultType.EdmType))
		{
			return;
		}
		string errorMessage = Strings.InvalidQueryResultType(resultType.EdmType.FullName);
		throw EntitySqlException.Create(errCtx, errorMessage, null);
	}

	private static List<FunctionDefinition> ConvertInlineFunctionDefinitions(NodeList<System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition> functionDefList, SemanticResolver sr)
	{
		List<FunctionDefinition> list = new List<FunctionDefinition>();
		if (functionDefList != null)
		{
			List<InlineFunctionInfo> list2 = new List<InlineFunctionInfo>();
			foreach (System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition item in (IEnumerable<System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition>)functionDefList)
			{
				string name = item.Name;
				List<DbVariableReferenceExpression> parameters = ConvertInlineFunctionParameterDefs(item.Parameters, sr);
				InlineFunctionInfo inlineFunctionInfo = new InlineFunctionInfoImpl(item, parameters);
				list2.Add(inlineFunctionInfo);
				sr.TypeResolver.DeclareInlineFunction(name, inlineFunctionInfo);
			}
			foreach (InlineFunctionInfo item2 in list2)
			{
				list.Add(new FunctionDefinition(item2.FunctionDefAst.Name, item2.GetLambda(sr), item2.FunctionDefAst.StartPosition, item2.FunctionDefAst.EndPosition));
			}
		}
		return list;
	}

	private static List<DbVariableReferenceExpression> ConvertInlineFunctionParameterDefs(NodeList<PropDefinition> parameterDefs, SemanticResolver sr)
	{
		List<DbVariableReferenceExpression> list = new List<DbVariableReferenceExpression>();
		if (parameterDefs != null)
		{
			foreach (PropDefinition item2 in (IEnumerable<PropDefinition>)parameterDefs)
			{
				string name = item2.Name.Name;
				if (list.Exists((DbVariableReferenceExpression arg) => sr.NameComparer.Compare(arg.VariableName, name) == 0))
				{
					ErrorContext errCtx = item2.ErrCtx;
					string errorMessage = Strings.MultipleDefinitionsOfParameter(name);
					throw EntitySqlException.Create(errCtx, errorMessage, null);
				}
				DbVariableReferenceExpression item = new DbVariableReferenceExpression(ConvertTypeDefinition(item2.Type, sr), name);
				list.Add(item);
			}
		}
		return list;
	}

	private static DbLambda ConvertInlineFunctionDefinition(InlineFunctionInfo functionInfo, SemanticResolver sr)
	{
		sr.EnterScope();
		functionInfo.Parameters.Each((DbVariableReferenceExpression p) => sr.CurrentScope.Add(p.VariableName, new FreeVariableScopeEntry(p)));
		DbExpression body = ConvertValueExpression(functionInfo.FunctionDefAst.Body, sr);
		sr.LeaveScope();
		return DbExpressionBuilder.Lambda(body, functionInfo.Parameters);
	}

	private static ExpressionResolution Convert(Node astExpr, SemanticResolver sr)
	{
		return (_astExprConverters[astExpr.GetType()] ?? throw new EntitySqlException(Strings.UnknownAstExpressionType))(astExpr, sr);
	}

	private static DbExpression ConvertValueExpression(Node astExpr, SemanticResolver sr)
	{
		DbExpression dbExpression = ConvertValueExpressionAllowUntypedNulls(astExpr, sr);
		if (dbExpression == null)
		{
			ErrorContext errCtx = astExpr.ErrCtx;
			string expressionCannotBeNull = Strings.ExpressionCannotBeNull;
			throw EntitySqlException.Create(errCtx, expressionCannotBeNull, null);
		}
		return dbExpression;
	}

	private static DbExpression ConvertValueExpressionAllowUntypedNulls(Node astExpr, SemanticResolver sr)
	{
		ExpressionResolution expressionResolution = Convert(astExpr, sr);
		if (expressionResolution.ExpressionClass == ExpressionResolutionClass.Value)
		{
			return ((ValueExpression)expressionResolution).Value;
		}
		if (expressionResolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
		{
			MetadataMember metadataMember = (MetadataMember)expressionResolution;
			if (metadataMember.MetadataMemberClass == MetadataMemberClass.EnumMember)
			{
				MetadataEnumMember metadataEnumMember = (MetadataEnumMember)metadataMember;
				return metadataEnumMember.EnumType.Constant(metadataEnumMember.EnumMember.Value);
			}
		}
		string errorMessage = Strings.InvalidExpressionResolutionClass(expressionResolution.ExpressionClassName, ValueExpression.ValueClassName);
		if (astExpr is Identifier identifier)
		{
			errorMessage = Strings.CouldNotResolveIdentifier(identifier.Name);
		}
		if (astExpr is DotExpr dotExpr && dotExpr.IsMultipartIdentifier(out var names))
		{
			errorMessage = Strings.CouldNotResolveIdentifier(TypeResolver.GetFullName(names));
		}
		throw EntitySqlException.Create(astExpr.ErrCtx, errorMessage, null);
	}

	private static Pair<DbExpression, DbExpression> ConvertValueExpressionsWithUntypedNulls(Node leftAst, Node rightAst, ErrorContext errCtx, Func<string> formatMessage, SemanticResolver sr)
	{
		DbExpression dbExpression = ((leftAst != null) ? ConvertValueExpressionAllowUntypedNulls(leftAst, sr) : null);
		DbExpression dbExpression2 = ((rightAst != null) ? ConvertValueExpressionAllowUntypedNulls(rightAst, sr) : null);
		if (dbExpression == null)
		{
			if (dbExpression2 == null)
			{
				string errorMessage = formatMessage();
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			dbExpression = dbExpression2.ResultType.Null();
		}
		else if (dbExpression2 == null)
		{
			dbExpression2 = dbExpression.ResultType.Null();
		}
		return new Pair<DbExpression, DbExpression>(dbExpression, dbExpression2);
	}

	private static ExpressionResolution ConvertLiteral(Node expr, SemanticResolver sr)
	{
		Literal literal = (Literal)expr;
		if (literal.IsNullLiteral)
		{
			return new ValueExpression(null);
		}
		return new ValueExpression(GetLiteralTypeUsage(literal).Constant(literal.Value));
	}

	private static TypeUsage GetLiteralTypeUsage(Literal literal)
	{
		PrimitiveType primitiveType = null;
		if (!ClrProviderManifest.Instance.TryGetPrimitiveType(literal.Type, out primitiveType))
		{
			ErrorContext errCtx = literal.ErrCtx;
			string errorMessage = Strings.LiteralTypeNotFoundInMetadata(literal.OriginalValue);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return TypeHelpers.GetLiteralTypeUsage(primitiveType.PrimitiveTypeKind, literal.IsUnicodeString);
	}

	private static ExpressionResolution ConvertIdentifier(Node expr, SemanticResolver sr)
	{
		return ConvertIdentifier((Identifier)expr, leftHandSideOfMemberAccess: false, sr);
	}

	private static ExpressionResolution ConvertIdentifier(Identifier identifier, bool leftHandSideOfMemberAccess, SemanticResolver sr)
	{
		return sr.ResolveSimpleName(identifier.Name, leftHandSideOfMemberAccess, identifier.ErrCtx);
	}

	private static ExpressionResolution ConvertDotExpr(Node expr, SemanticResolver sr)
	{
		DotExpr dotExpr = (DotExpr)expr;
		if (sr.TryResolveDotExprAsGroupKeyAlternativeName(dotExpr, out var groupKeyResolution))
		{
			return groupKeyResolution;
		}
		ExpressionResolution expressionResolution = ((!(dotExpr.Left is Identifier identifier)) ? Convert(dotExpr.Left, sr) : ConvertIdentifier(identifier, leftHandSideOfMemberAccess: true, sr));
		switch (expressionResolution.ExpressionClass)
		{
		case ExpressionResolutionClass.Value:
			return sr.ResolvePropertyAccess(((ValueExpression)expressionResolution).Value, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);
		case ExpressionResolutionClass.EntityContainer:
			return sr.ResolveEntityContainerMemberAccess(((EntityContainerExpression)expressionResolution).EntityContainer, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);
		case ExpressionResolutionClass.MetadataMember:
			return sr.ResolveMetadataMemberAccess((MetadataMember)expressionResolution, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);
		default:
		{
			ErrorContext errCtx = dotExpr.Left.ErrCtx;
			string errorMessage = Strings.UnknownExpressionResolutionClass(expressionResolution.ExpressionClass);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		}
	}

	private static ExpressionResolution ConvertParenExpr(Node astExpr, SemanticResolver sr)
	{
		return new ValueExpression(ConvertValueExpressionAllowUntypedNulls(((ParenExpr)astExpr).Expr, sr));
	}

	private static ExpressionResolution ConvertGroupPartitionExpr(Node astExpr, SemanticResolver sr)
	{
		GroupPartitionExpr groupPartitionExpr = (GroupPartitionExpr)astExpr;
		DbExpression converted = null;
		if (!TryConvertAsResolvedGroupAggregate(groupPartitionExpr, sr, out converted))
		{
			if (!sr.IsInAnyGroupScope())
			{
				ErrorContext errCtx = astExpr.ErrCtx;
				string groupPartitionOutOfContext = Strings.GroupPartitionOutOfContext;
				throw EntitySqlException.Create(errCtx, groupPartitionOutOfContext, null);
			}
			GroupPartitionInfo aggregateInfo;
			DbExpression dbExpression;
			using (sr.EnterGroupPartition(groupPartitionExpr, groupPartitionExpr.ErrCtx, out aggregateInfo))
			{
				dbExpression = ConvertValueExpressionAllowUntypedNulls(groupPartitionExpr.ArgExpr, sr);
			}
			if (dbExpression == null)
			{
				ErrorContext errCtx2 = groupPartitionExpr.ArgExpr.ErrCtx;
				string resultingExpressionTypeCannotBeNull = Strings.ResultingExpressionTypeCannotBeNull;
				throw EntitySqlException.Create(errCtx2, resultingExpressionTypeCannotBeNull, null);
			}
			DbExpression dbExpression2 = aggregateInfo.EvaluatingScopeRegion.GroupAggregateBinding.Project(dbExpression);
			if (groupPartitionExpr.DistinctKind == DistinctKind.Distinct)
			{
				ValidateDistinctProjection(dbExpression2.ResultType, groupPartitionExpr.ArgExpr.ErrCtx, null);
				dbExpression2 = dbExpression2.Distinct();
			}
			aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupPartition"), dbExpression2);
			aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);
			converted = aggregateInfo.AggregateStubExpression;
		}
		return new ValueExpression(converted);
	}

	private static ExpressionResolution ConvertMethodExpr(Node expr, SemanticResolver sr)
	{
		return ConvertMethodExpr((MethodExpr)expr, includeInlineFunctions: true, sr);
	}

	private static ExpressionResolution ConvertMethodExpr(MethodExpr methodExpr, bool includeInlineFunctions, SemanticResolver sr)
	{
		ExpressionResolution expressionResolution;
		using (sr.TypeResolver.EnterFunctionNameResolution(includeInlineFunctions))
		{
			if (methodExpr.Expr is Identifier identifier)
			{
				expressionResolution = sr.ResolveSimpleFunctionName(identifier.Name, identifier.ErrCtx);
			}
			else
			{
				DotExpr leftExpr = methodExpr.Expr as DotExpr;
				using (ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(leftExpr, sr))
				{
					using (ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(leftExpr, sr))
					{
						expressionResolution = Convert(methodExpr.Expr, sr);
					}
				}
			}
		}
		if (expressionResolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
		{
			MetadataMember metadataMember = (MetadataMember)expressionResolution;
			if (metadataMember.MetadataMemberClass == MetadataMemberClass.InlineFunctionGroup)
			{
				methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
				methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
				if (TryConvertInlineFunctionCall((InlineFunctionGroup)metadataMember, methodExpr, sr, out var inlineFunctionCall))
				{
					return inlineFunctionCall;
				}
				return ConvertMethodExpr(methodExpr, includeInlineFunctions: false, sr);
			}
			switch (metadataMember.MetadataMemberClass)
			{
			case MetadataMemberClass.Type:
				methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxTypeCtor(metadataMember.Name);
				methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
				return ConvertTypeConstructorCall((MetadataType)metadataMember, methodExpr, sr);
			case MetadataMemberClass.FunctionGroup:
				methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
				methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
				return ConvertModelFunctionCall((MetadataFunctionGroup)metadataMember, methodExpr, sr);
			default:
			{
				ErrorContext errCtx = methodExpr.Expr.ErrCtx;
				string errorMessage = Strings.CannotResolveNameToTypeOrFunction(metadataMember.Name);
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			}
		}
		ErrorContext errCtx2 = methodExpr.ErrCtx;
		string methodInvocationNotSupported = Strings.MethodInvocationNotSupported;
		throw EntitySqlException.Create(errCtx2, methodInvocationNotSupported, null);
	}

	private static IDisposable ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(DotExpr leftExpr, SemanticResolver sr)
	{
		if (leftExpr == null || !(leftExpr.Left is Identifier))
		{
			return null;
		}
		return sr.EnterIgnoreEntityContainerNameResolution();
	}

	private static IDisposable ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(DotExpr leftExpr, SemanticResolver sr)
	{
		if (leftExpr != null && leftExpr.Left is Identifier && (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode || sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.UserViewGenerationMode) && (sr.TypeResolver.Perspective.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection).MappingVersion < 2.0)
		{
			return sr.TypeResolver.EnterBackwardCompatibilityResolution();
		}
		return null;
	}

	private static bool TryConvertInlineFunctionCall(InlineFunctionGroup inlineFunctionGroup, MethodExpr methodExpr, SemanticResolver sr, out ValueExpression inlineFunctionCall)
	{
		inlineFunctionCall = null;
		if (methodExpr.DistinctKind != 0)
		{
			return false;
		}
		List<TypeUsage> argTypes;
		List<DbExpression> list = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);
		bool isAmbiguous = false;
		InlineFunctionInfo inlineFunctionInfo = SemanticResolver.ResolveFunctionOverloads(inlineFunctionGroup.FunctionMetadata, argTypes, (InlineFunctionInfo lambdaOverload) => lambdaOverload.Parameters, (DbVariableReferenceExpression varRef) => varRef.ResultType, (DbVariableReferenceExpression varRef) => ParameterMode.In, isGroupAggregateFunction: false, out isAmbiguous);
		if (isAmbiguous)
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string ambiguousFunctionArguments = Strings.AmbiguousFunctionArguments;
			throw EntitySqlException.Create(errCtx, ambiguousFunctionArguments, null);
		}
		if (inlineFunctionInfo == null)
		{
			return false;
		}
		ConvertUntypedNullsInArguments(list, inlineFunctionInfo.Parameters, (DbVariableReferenceExpression formal) => formal.ResultType);
		inlineFunctionCall = new ValueExpression(inlineFunctionInfo.GetLambda(sr).Invoke(list));
		return true;
	}

	private static ValueExpression ConvertTypeConstructorCall(MetadataType metadataType, MethodExpr methodExpr, SemanticResolver sr)
	{
		if (!TypeSemantics.IsComplexType(metadataType.TypeUsage) && !TypeSemantics.IsEntityType(metadataType.TypeUsage) && !TypeSemantics.IsRelationshipType(metadataType.TypeUsage))
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string errorMessage = Strings.InvalidCtorUseOnType(metadataType.TypeUsage.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (metadataType.TypeUsage.EdmType.Abstract)
		{
			ErrorContext errCtx2 = methodExpr.ErrCtx;
			string errorMessage2 = Strings.CannotInstantiateAbstractType(metadataType.TypeUsage.EdmType.FullName);
			throw EntitySqlException.Create(errCtx2, errorMessage2, null);
		}
		if (methodExpr.DistinctKind != 0)
		{
			ErrorContext errCtx3 = methodExpr.ErrCtx;
			string invalidDistinctArgumentInCtor = Strings.InvalidDistinctArgumentInCtor;
			throw EntitySqlException.Create(errCtx3, invalidDistinctArgumentInCtor, null);
		}
		List<DbRelatedEntityRef> list = null;
		if (methodExpr.HasRelationships)
		{
			if (sr.ParserOptions.ParserCompilationMode != ParserOptions.CompilationMode.RestrictedViewGenerationMode && sr.ParserOptions.ParserCompilationMode != ParserOptions.CompilationMode.UserViewGenerationMode)
			{
				ErrorContext errCtx4 = methodExpr.Relationships.ErrCtx;
				string invalidModeForWithRelationshipClause = Strings.InvalidModeForWithRelationshipClause;
				throw EntitySqlException.Create(errCtx4, invalidModeForWithRelationshipClause, null);
			}
			if (!(metadataType.TypeUsage.EdmType is EntityType driverEntityType))
			{
				ErrorContext errCtx5 = methodExpr.Relationships.ErrCtx;
				string invalidTypeForWithRelationshipClause = Strings.InvalidTypeForWithRelationshipClause;
				throw EntitySqlException.Create(errCtx5, invalidTypeForWithRelationshipClause, null);
			}
			HashSet<string> hashSet = new HashSet<string>();
			list = new List<DbRelatedEntityRef>(methodExpr.Relationships.Count);
			for (int i = 0; i < methodExpr.Relationships.Count; i++)
			{
				RelshipNavigationExpr relshipNavigationExpr = methodExpr.Relationships[i];
				DbRelatedEntityRef dbRelatedEntityRef = ConvertRelatedEntityRef(relshipNavigationExpr, driverEntityType, sr);
				string text = string.Join(":", dbRelatedEntityRef.TargetEnd.DeclaringType.Identity, dbRelatedEntityRef.TargetEnd.Identity);
				if (hashSet.Contains(text))
				{
					ErrorContext errCtx6 = relshipNavigationExpr.ErrCtx;
					string errorMessage3 = Strings.RelationshipTargetMustBeUnique(text);
					throw EntitySqlException.Create(errCtx6, errorMessage3, null);
				}
				hashSet.Add(text);
				list.Add(dbRelatedEntityRef);
			}
		}
		List<TypeUsage> argTypes;
		return new ValueExpression(CreateConstructorCallExpression(methodExpr, metadataType.TypeUsage, ConvertFunctionArguments(methodExpr.Args, sr, out argTypes), list, sr));
	}

	private static ValueExpression ConvertModelFunctionCall(MetadataFunctionGroup metadataFunctionGroup, MethodExpr methodExpr, SemanticResolver sr)
	{
		if (metadataFunctionGroup.FunctionMetadata.Any((EdmFunction f) => !f.IsComposableAttribute))
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string errorMessage = Strings.CannotCallNoncomposableFunction(metadataFunctionGroup.Name);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (TypeSemantics.IsAggregateFunction(metadataFunctionGroup.FunctionMetadata[0]) && sr.IsInAnyGroupScope())
		{
			return new ValueExpression(ConvertAggregateFunctionInGroupScope(methodExpr, metadataFunctionGroup, sr));
		}
		return new ValueExpression(CreateModelFunctionCallExpression(methodExpr, metadataFunctionGroup, sr));
	}

	private static DbExpression ConvertAggregateFunctionInGroupScope(MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, SemanticResolver sr)
	{
		DbExpression converted = null;
		if (TryConvertAsResolvedGroupAggregate(methodExpr, sr, out converted))
		{
			return converted;
		}
		ScopeRegion innermostReferencedScopeRegion = ((sr.CurrentGroupAggregateInfo != null) ? sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion : null);
		if (TryConvertAsCollectionFunction(methodExpr, metadataFunctionGroup, sr, out var argTypes, out converted))
		{
			return converted;
		}
		if (sr.CurrentGroupAggregateInfo != null)
		{
			sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion = innermostReferencedScopeRegion;
		}
		if (TryConvertAsFunctionAggregate(methodExpr, metadataFunctionGroup, argTypes, sr, out converted))
		{
			return converted;
		}
		ErrorContext errCtx = methodExpr.ErrCtx;
		string errorMessage = Strings.FailedToResolveAggregateFunction(metadataFunctionGroup.Name);
		throw EntitySqlException.Create(errCtx, errorMessage, null);
	}

	private static bool TryConvertAsResolvedGroupAggregate(GroupAggregateExpr groupAggregateExpr, SemanticResolver sr, out DbExpression converted)
	{
		converted = null;
		if (groupAggregateExpr.AggregateInfo == null)
		{
			return false;
		}
		groupAggregateExpr.AggregateInfo.SetContainingAggregate(sr.CurrentGroupAggregateInfo);
		if (!sr.TryResolveInternalAggregateName(groupAggregateExpr.AggregateInfo.AggregateName, groupAggregateExpr.AggregateInfo.ErrCtx, out converted))
		{
			converted = groupAggregateExpr.AggregateInfo.AggregateStubExpression;
		}
		return true;
	}

	private static bool TryConvertAsCollectionFunction(MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, SemanticResolver sr, out List<TypeUsage> argTypes, out DbExpression converted)
	{
		List<DbExpression> list = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);
		bool isAmbiguous = false;
		EdmFunction edmFunction = SemanticResolver.ResolveFunctionOverloads(metadataFunctionGroup.FunctionMetadata, argTypes, isGroupAggregateFunction: false, out isAmbiguous);
		if (isAmbiguous)
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string ambiguousFunctionArguments = Strings.AmbiguousFunctionArguments;
			throw EntitySqlException.Create(errCtx, ambiguousFunctionArguments, null);
		}
		if (edmFunction != null)
		{
			ConvertUntypedNullsInArguments(list, edmFunction.Parameters, (FunctionParameter parameter) => parameter.TypeUsage);
			converted = edmFunction.Invoke(list);
			return true;
		}
		converted = null;
		return false;
	}

	private static bool TryConvertAsFunctionAggregate(MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, List<TypeUsage> argTypes, SemanticResolver sr, out DbExpression converted)
	{
		converted = null;
		bool isAmbiguous = false;
		EdmFunction edmFunction = SemanticResolver.ResolveFunctionOverloads(metadataFunctionGroup.FunctionMetadata, argTypes, isGroupAggregateFunction: true, out isAmbiguous);
		if (isAmbiguous)
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string ambiguousFunctionArguments = Strings.AmbiguousFunctionArguments;
			throw EntitySqlException.Create(errCtx, ambiguousFunctionArguments, null);
		}
		if (edmFunction == null)
		{
			CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
		}
		FunctionAggregateInfo aggregateInfo;
		List<DbExpression> list;
		using (sr.EnterFunctionAggregate(methodExpr, methodExpr.ErrCtx, out aggregateInfo))
		{
			list = ConvertFunctionArguments(methodExpr.Args, sr, out var _);
		}
		ConvertUntypedNullsInArguments(list, edmFunction.Parameters, (FunctionParameter parameter) => TypeHelpers.GetElementTypeUsage(parameter.TypeUsage));
		DbFunctionAggregate aggregateDefinition = ((methodExpr.DistinctKind != DistinctKind.Distinct) ? edmFunction.Aggregate(list) : edmFunction.AggregateDistinct(list));
		aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupAgg" + edmFunction.Name), aggregateDefinition);
		aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);
		converted = aggregateInfo.AggregateStubExpression;
		return true;
	}

	private static DbExpression CreateConstructorCallExpression(MethodExpr methodExpr, TypeUsage type, List<DbExpression> args, List<DbRelatedEntityRef> relshipExprList, SemanticResolver sr)
	{
		DbExpression dbExpression = null;
		int num = 0;
		int count = args.Count;
		StructuralType structuralType = (StructuralType)type.EdmType;
		foreach (EdmMember allStructuralMember in TypeHelpers.GetAllStructuralMembers(structuralType))
		{
			TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(allStructuralMember);
			if (count <= num)
			{
				ErrorContext errCtx = methodExpr.ErrCtx;
				string errorMessage = Strings.NumberOfTypeCtorIsLessThenFormalSpec(allStructuralMember.Name);
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			if (args[num] == null)
			{
				if (allStructuralMember is EdmProperty { Nullable: false })
				{
					ErrorContext errCtx2 = methodExpr.Args[num].ErrCtx;
					string errorMessage2 = Strings.InvalidNullLiteralForNonNullableMember(allStructuralMember.Name, structuralType.FullName);
					throw EntitySqlException.Create(errCtx2, errorMessage2, null);
				}
				args[num] = modelTypeUsage.Null();
			}
			bool flag = TypeSemantics.IsPromotableTo(args[num].ResultType, modelTypeUsage);
			if (ParserOptions.CompilationMode.RestrictedViewGenerationMode == sr.ParserOptions.ParserCompilationMode || ParserOptions.CompilationMode.UserViewGenerationMode == sr.ParserOptions.ParserCompilationMode)
			{
				if (!flag && !TypeSemantics.IsPromotableTo(modelTypeUsage, args[num].ResultType))
				{
					ErrorContext errCtx3 = methodExpr.Args[num].ErrCtx;
					string errorMessage3 = Strings.InvalidCtorArgumentType(args[num].ResultType.EdmType.FullName, allStructuralMember.Name, modelTypeUsage.EdmType.FullName);
					throw EntitySqlException.Create(errCtx3, errorMessage3, null);
				}
				if (Helper.IsPrimitiveType(modelTypeUsage.EdmType) && !TypeSemantics.IsSubTypeOf(args[num].ResultType, modelTypeUsage))
				{
					args[num] = args[num].CastTo(modelTypeUsage);
				}
			}
			else if (!flag)
			{
				ErrorContext errCtx4 = methodExpr.Args[num].ErrCtx;
				string errorMessage4 = Strings.InvalidCtorArgumentType(args[num].ResultType.EdmType.FullName, allStructuralMember.Name, modelTypeUsage.EdmType.FullName);
				throw EntitySqlException.Create(errCtx4, errorMessage4, null);
			}
			num++;
		}
		if (num != count)
		{
			ErrorContext errCtx5 = methodExpr.ErrCtx;
			string errorMessage5 = Strings.NumberOfTypeCtorIsMoreThenFormalSpec(structuralType.FullName);
			throw EntitySqlException.Create(errCtx5, errorMessage5, null);
		}
		if (relshipExprList != null && relshipExprList.Count > 0)
		{
			return DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression((EntityType)type.EdmType, args, relshipExprList);
		}
		return TypeHelpers.GetReadOnlyType(type).New(args);
	}

	private static DbFunctionExpression CreateModelFunctionCallExpression(MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, SemanticResolver sr)
	{
		bool isAmbiguous = false;
		if (methodExpr.DistinctKind != 0)
		{
			ErrorContext errCtx = methodExpr.ErrCtx;
			string invalidDistinctArgumentInNonAggFunction = Strings.InvalidDistinctArgumentInNonAggFunction;
			throw EntitySqlException.Create(errCtx, invalidDistinctArgumentInNonAggFunction, null);
		}
		List<TypeUsage> argTypes;
		List<DbExpression> list = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);
		EdmFunction edmFunction = SemanticResolver.ResolveFunctionOverloads(metadataFunctionGroup.FunctionMetadata, argTypes, isGroupAggregateFunction: false, out isAmbiguous);
		if (isAmbiguous)
		{
			ErrorContext errCtx2 = methodExpr.ErrCtx;
			string ambiguousFunctionArguments = Strings.AmbiguousFunctionArguments;
			throw EntitySqlException.Create(errCtx2, ambiguousFunctionArguments, null);
		}
		if (edmFunction == null)
		{
			CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
		}
		ConvertUntypedNullsInArguments(list, edmFunction.Parameters, (FunctionParameter parameter) => parameter.TypeUsage);
		return edmFunction.Invoke(list);
	}

	private static List<DbExpression> ConvertFunctionArguments(NodeList<Node> astExprList, SemanticResolver sr, out List<TypeUsage> argTypes)
	{
		List<DbExpression> list = new List<DbExpression>();
		if (astExprList != null)
		{
			for (int i = 0; i < astExprList.Count; i++)
			{
				list.Add(ConvertValueExpressionAllowUntypedNulls(astExprList[i], sr));
			}
		}
		argTypes = list.Select((DbExpression a) => a?.ResultType).ToList();
		return list;
	}

	private static void ConvertUntypedNullsInArguments<TParameterMetadata>(List<DbExpression> args, IList<TParameterMetadata> parametersMetadata, Func<TParameterMetadata, TypeUsage> getParameterTypeUsage)
	{
		for (int i = 0; i < args.Count; i++)
		{
			if (args[i] == null)
			{
				args[i] = getParameterTypeUsage(parametersMetadata[i]).Null();
			}
		}
	}

	private static ExpressionResolution ConvertParameter(Node expr, SemanticResolver sr)
	{
		QueryParameter queryParameter = (QueryParameter)expr;
		if (sr.Parameters == null || !sr.Parameters.TryGetValue(queryParameter.Name, out var value))
		{
			ErrorContext errCtx = queryParameter.ErrCtx;
			string errorMessage = Strings.ParameterWasNotDefined(queryParameter.Name);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return new ValueExpression(value);
	}

	private static DbRelatedEntityRef ConvertRelatedEntityRef(RelshipNavigationExpr relshipExpr, EntityType driverEntityType, SemanticResolver sr)
	{
		EdmType edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
		if (!(edmType is RelationshipType relationshipType))
		{
			ErrorContext errCtx = relshipExpr.TypeName.ErrCtx;
			string errorMessage = Strings.RelationshipTypeExpected(edmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		DbExpression dbExpression = ConvertValueExpression(relshipExpr.RefExpr, sr);
		RefType refType = dbExpression.ResultType.EdmType as RefType;
		if (refType == null)
		{
			ErrorContext errCtx2 = relshipExpr.RefExpr.ErrCtx;
			string relatedEndExprTypeMustBeReference = Strings.RelatedEndExprTypeMustBeReference;
			throw EntitySqlException.Create(errCtx2, relatedEndExprTypeMustBeReference, null);
		}
		RelationshipEndMember toEnd;
		if (relshipExpr.ToEndIdentifier != null)
		{
			toEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault((EdmMember m) => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
			if (toEnd == null)
			{
				ErrorContext errCtx3 = relshipExpr.ToEndIdentifier.ErrCtx;
				string errorMessage2 = Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName);
				throw EntitySqlException.Create(errCtx3, errorMessage2, null);
			}
			if (toEnd.RelationshipMultiplicity != RelationshipMultiplicity.One && toEnd.RelationshipMultiplicity != 0)
			{
				ErrorContext errCtx4 = relshipExpr.ToEndIdentifier.ErrCtx;
				string errorMessage3 = Strings.InvalidWithRelationshipTargetEndMultiplicity(toEnd.Name, toEnd.RelationshipMultiplicity.ToString());
				throw EntitySqlException.Create(errCtx4, errorMessage3, null);
			}
			if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, toEnd.TypeUsage.EdmType))
			{
				ErrorContext errCtx5 = relshipExpr.RefExpr.ErrCtx;
				string errorMessage4 = Strings.RelatedEndExprTypeMustBePromotoableToToEnd(refType.FullName, toEnd.TypeUsage.EdmType.FullName);
				throw EntitySqlException.Create(errCtx5, errorMessage4, null);
			}
		}
		else
		{
			RelationshipEndMember[] array = (from m in relationshipType.Members
				select (RelationshipEndMember)m into e
				where TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, e.TypeUsage.EdmType) && (e.RelationshipMultiplicity == RelationshipMultiplicity.One || e.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
				select e).ToArray();
			switch (array.Length)
			{
			case 1:
				break;
			case 0:
			{
				ErrorContext errCtx7 = relshipExpr.ErrCtx;
				string errorMessage5 = Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName);
				throw EntitySqlException.Create(errCtx7, errorMessage5, null);
			}
			default:
			{
				ErrorContext errCtx6 = relshipExpr.ErrCtx;
				string relationshipToEndIsAmbiguos = Strings.RelationshipToEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx6, relationshipToEndIsAmbiguos, null);
			}
			}
			toEnd = array[0];
		}
		RelationshipEndMember relationshipEndMember;
		if (relshipExpr.FromEndIdentifier != null)
		{
			relationshipEndMember = (RelationshipEndMember)relationshipType.Members.FirstOrDefault((EdmMember m) => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
			if (relationshipEndMember == null)
			{
				ErrorContext errCtx8 = relshipExpr.FromEndIdentifier.ErrCtx;
				string errorMessage6 = Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName);
				throw EntitySqlException.Create(errCtx8, errorMessage6, null);
			}
			if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(driverEntityType.GetReferenceType(), relationshipEndMember.TypeUsage.EdmType))
			{
				ErrorContext errCtx9 = relshipExpr.FromEndIdentifier.ErrCtx;
				string errorMessage7 = Strings.SourceTypeMustBePromotoableToFromEndRelationType(driverEntityType.FullName, relationshipEndMember.TypeUsage.EdmType.FullName);
				throw EntitySqlException.Create(errCtx9, errorMessage7, null);
			}
			if (relationshipEndMember.EdmEquals(toEnd))
			{
				ErrorContext errCtx10 = relshipExpr.ErrCtx;
				string relationshipFromEndIsAmbiguos = Strings.RelationshipFromEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx10, relationshipFromEndIsAmbiguos, null);
			}
		}
		else
		{
			RelationshipEndMember[] array2 = (from m in relationshipType.Members
				select (RelationshipEndMember)m into e
				where TypeSemantics.IsStructurallyEqualOrPromotableTo(driverEntityType.GetReferenceType(), e.TypeUsage.EdmType) && !e.EdmEquals(toEnd)
				select e).ToArray();
			switch (array2.Length)
			{
			case 1:
				break;
			case 0:
			{
				ErrorContext errCtx12 = relshipExpr.ErrCtx;
				string errorMessage8 = Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName);
				throw EntitySqlException.Create(errCtx12, errorMessage8, null);
			}
			default:
			{
				ErrorContext errCtx11 = relshipExpr.ErrCtx;
				string relationshipFromEndIsAmbiguos2 = Strings.RelationshipFromEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx11, relationshipFromEndIsAmbiguos2, null);
			}
			}
			relationshipEndMember = array2[0];
		}
		return DbExpressionBuilder.CreateRelatedEntityRef(relationshipEndMember, toEnd, dbExpression);
	}

	private static ExpressionResolution ConvertRelshipNavigationExpr(Node astExpr, SemanticResolver sr)
	{
		RelshipNavigationExpr relshipExpr = (RelshipNavigationExpr)astExpr;
		EdmType edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
		if (!(edmType is RelationshipType relationshipType))
		{
			ErrorContext errCtx = relshipExpr.TypeName.ErrCtx;
			string errorMessage = Strings.RelationshipTypeExpected(edmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		DbExpression dbExpression = ConvertValueExpression(relshipExpr.RefExpr, sr);
		RefType sourceRefType = dbExpression.ResultType.EdmType as RefType;
		if (sourceRefType == null)
		{
			if (!(dbExpression.ResultType.EdmType is EntityType))
			{
				ErrorContext errCtx2 = relshipExpr.RefExpr.ErrCtx;
				string relatedEndExprTypeMustBeReference = Strings.RelatedEndExprTypeMustBeReference;
				throw EntitySqlException.Create(errCtx2, relatedEndExprTypeMustBeReference, null);
			}
			dbExpression = dbExpression.GetEntityRef();
			sourceRefType = (RefType)dbExpression.ResultType.EdmType;
		}
		RelationshipEndMember toEnd;
		if (relshipExpr.ToEndIdentifier != null)
		{
			toEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault((EdmMember m) => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
			if (toEnd == null)
			{
				ErrorContext errCtx3 = relshipExpr.ToEndIdentifier.ErrCtx;
				string errorMessage2 = Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName);
				throw EntitySqlException.Create(errCtx3, errorMessage2, null);
			}
		}
		else
		{
			toEnd = null;
		}
		RelationshipEndMember fromEnd;
		if (relshipExpr.FromEndIdentifier != null)
		{
			fromEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault((EdmMember m) => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
			if (fromEnd == null)
			{
				ErrorContext errCtx4 = relshipExpr.FromEndIdentifier.ErrCtx;
				string errorMessage3 = Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName);
				throw EntitySqlException.Create(errCtx4, errorMessage3, null);
			}
			if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(sourceRefType, fromEnd.TypeUsage.EdmType))
			{
				ErrorContext errCtx5 = relshipExpr.FromEndIdentifier.ErrCtx;
				string errorMessage4 = Strings.SourceTypeMustBePromotoableToFromEndRelationType(sourceRefType.FullName, fromEnd.TypeUsage.EdmType.FullName);
				throw EntitySqlException.Create(errCtx5, errorMessage4, null);
			}
			if (toEnd != null && fromEnd.EdmEquals(toEnd))
			{
				ErrorContext errCtx6 = relshipExpr.ErrCtx;
				string relationshipFromEndIsAmbiguos = Strings.RelationshipFromEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx6, relationshipFromEndIsAmbiguos, null);
			}
		}
		else
		{
			RelationshipEndMember[] array = (from m in relationshipType.Members
				select (RelationshipEndMember)m into e
				where TypeSemantics.IsStructurallyEqualOrPromotableTo(sourceRefType, e.TypeUsage.EdmType) && (toEnd == null || !e.EdmEquals(toEnd))
				select e).ToArray();
			switch (array.Length)
			{
			case 1:
				break;
			case 0:
			{
				ErrorContext errCtx8 = relshipExpr.ErrCtx;
				string errorMessage5 = Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName);
				throw EntitySqlException.Create(errCtx8, errorMessage5, null);
			}
			default:
			{
				ErrorContext errCtx7 = relshipExpr.ErrCtx;
				string relationshipFromEndIsAmbiguos2 = Strings.RelationshipFromEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx7, relationshipFromEndIsAmbiguos2, null);
			}
			}
			fromEnd = array[0];
		}
		if (toEnd == null)
		{
			RelationshipEndMember[] array2 = (from m in relationshipType.Members
				select (RelationshipEndMember)m into e
				where !e.EdmEquals(fromEnd)
				select e).ToArray();
			switch (array2.Length)
			{
			case 1:
				break;
			case 0:
			{
				ErrorContext errCtx10 = relshipExpr.ErrCtx;
				string errorMessage6 = Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName);
				throw EntitySqlException.Create(errCtx10, errorMessage6, null);
			}
			default:
			{
				ErrorContext errCtx9 = relshipExpr.ErrCtx;
				string relationshipToEndIsAmbiguos = Strings.RelationshipToEndIsAmbiguos;
				throw EntitySqlException.Create(errCtx9, relationshipToEndIsAmbiguos, null);
			}
			}
			toEnd = array2[0];
		}
		return new ValueExpression(dbExpression.Navigate(fromEnd, toEnd));
	}

	private static ExpressionResolution ConvertRefExpr(Node astExpr, SemanticResolver sr)
	{
		RefExpr refExpr = (RefExpr)astExpr;
		DbExpression dbExpression = ConvertValueExpression(refExpr.ArgExpr, sr);
		if (!TypeSemantics.IsEntityType(dbExpression.ResultType))
		{
			ErrorContext errCtx = refExpr.ArgExpr.ErrCtx;
			string errorMessage = Strings.RefArgIsNotOfEntityType(dbExpression.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		dbExpression = dbExpression.GetEntityRef();
		return new ValueExpression(dbExpression);
	}

	private static ExpressionResolution ConvertDeRefExpr(Node astExpr, SemanticResolver sr)
	{
		DerefExpr derefExpr = (DerefExpr)astExpr;
		DbExpression dbExpression = null;
		dbExpression = ConvertValueExpression(derefExpr.ArgExpr, sr);
		if (!TypeSemantics.IsReferenceType(dbExpression.ResultType))
		{
			ErrorContext errCtx = derefExpr.ArgExpr.ErrCtx;
			string errorMessage = Strings.DeRefArgIsNotOfRefType(dbExpression.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		dbExpression = dbExpression.Deref();
		return new ValueExpression(dbExpression);
	}

	private static ExpressionResolution ConvertCreateRefExpr(Node astExpr, SemanticResolver sr)
	{
		CreateRefExpr createRefExpr = (CreateRefExpr)astExpr;
		DbExpression dbExpression = null;
		DbScanExpression obj = ConvertValueExpression(createRefExpr.EntitySet, sr) as DbScanExpression;
		if (obj == null)
		{
			ErrorContext errCtx = createRefExpr.EntitySet.ErrCtx;
			string exprIsNotValidEntitySetForCreateRef = Strings.ExprIsNotValidEntitySetForCreateRef;
			throw EntitySqlException.Create(errCtx, exprIsNotValidEntitySetForCreateRef, null);
		}
		if (!(obj.Target is EntitySet entitySet))
		{
			ErrorContext errCtx2 = createRefExpr.EntitySet.ErrCtx;
			string exprIsNotValidEntitySetForCreateRef2 = Strings.ExprIsNotValidEntitySetForCreateRef;
			throw EntitySqlException.Create(errCtx2, exprIsNotValidEntitySetForCreateRef2, null);
		}
		DbExpression dbExpression2 = ConvertValueExpression(createRefExpr.Keys, sr);
		if (!(dbExpression2.ResultType.EdmType is RowType rowType))
		{
			ErrorContext errCtx3 = createRefExpr.Keys.ErrCtx;
			string invalidCreateRefKeyType = Strings.InvalidCreateRefKeyType;
			throw EntitySqlException.Create(errCtx3, invalidCreateRefKeyType, null);
		}
		RowType rowType2 = TypeHelpers.CreateKeyRowType(entitySet.ElementType);
		if (rowType2.Members.Count != rowType.Members.Count)
		{
			ErrorContext errCtx4 = createRefExpr.Keys.ErrCtx;
			string imcompatibleCreateRefKeyType = Strings.ImcompatibleCreateRefKeyType;
			throw EntitySqlException.Create(errCtx4, imcompatibleCreateRefKeyType, null);
		}
		if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(dbExpression2.ResultType, TypeUsage.Create(rowType2)))
		{
			ErrorContext errCtx5 = createRefExpr.Keys.ErrCtx;
			string imcompatibleCreateRefKeyElementType = Strings.ImcompatibleCreateRefKeyElementType;
			throw EntitySqlException.Create(errCtx5, imcompatibleCreateRefKeyElementType, null);
		}
		if (createRefExpr.TypeIdentifier != null)
		{
			TypeUsage typeUsage = ConvertTypeName(createRefExpr.TypeIdentifier, sr);
			if (!TypeSemantics.IsEntityType(typeUsage))
			{
				ErrorContext errCtx6 = createRefExpr.TypeIdentifier.ErrCtx;
				string errorMessage = Strings.CreateRefTypeIdentifierMustSpecifyAnEntityType(typeUsage.EdmType.FullName, typeUsage.EdmType.BuiltInTypeKind.ToString());
				throw EntitySqlException.Create(errCtx6, errorMessage, null);
			}
			if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, typeUsage.EdmType))
			{
				ErrorContext errCtx7 = createRefExpr.TypeIdentifier.ErrCtx;
				string errorMessage2 = Strings.CreateRefTypeIdentifierMustBeASubOrSuperType(entitySet.ElementType.FullName, typeUsage.EdmType.FullName);
				throw EntitySqlException.Create(errCtx7, errorMessage2, null);
			}
			dbExpression = entitySet.RefFromKey(dbExpression2, (EntityType)typeUsage.EdmType);
		}
		else
		{
			dbExpression = entitySet.RefFromKey(dbExpression2);
		}
		return new ValueExpression(dbExpression);
	}

	private static ExpressionResolution ConvertKeyExpr(Node astExpr, SemanticResolver sr)
	{
		KeyExpr keyExpr = (KeyExpr)astExpr;
		DbExpression dbExpression = ConvertValueExpression(keyExpr.ArgExpr, sr);
		if (TypeSemantics.IsEntityType(dbExpression.ResultType))
		{
			dbExpression = dbExpression.GetEntityRef();
		}
		else if (!TypeSemantics.IsReferenceType(dbExpression.ResultType))
		{
			ErrorContext errCtx = keyExpr.ArgExpr.ErrCtx;
			string errorMessage = Strings.InvalidKeyArgument(dbExpression.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		dbExpression = dbExpression.GetRefKey();
		return new ValueExpression(dbExpression);
	}

	private static ExpressionResolution ConvertBuiltIn(Node astExpr, SemanticResolver sr)
	{
		BuiltInExpr builtInExpr = (BuiltInExpr)astExpr;
		return new ValueExpression((_builtInExprConverter[builtInExpr.Kind] ?? throw new EntitySqlException(Strings.UnknownBuiltInAstExpressionType))(builtInExpr, sr));
	}

	private static Pair<DbExpression, DbExpression> ConvertArithmeticArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		Pair<DbExpression, DbExpression> pair = ConvertValueExpressionsWithUntypedNulls(astBuiltInExpr.Arg1, astBuiltInExpr.Arg2, astBuiltInExpr.ErrCtx, () => Strings.InvalidNullArithmetic, sr);
		if (!TypeSemantics.IsNumericType(pair.Left.ResultType))
		{
			ErrorContext errCtx = astBuiltInExpr.Arg1.ErrCtx;
			string expressionMustBeNumericType = Strings.ExpressionMustBeNumericType;
			throw EntitySqlException.Create(errCtx, expressionMustBeNumericType, null);
		}
		if (pair.Right != null)
		{
			if (!TypeSemantics.IsNumericType(pair.Right.ResultType))
			{
				ErrorContext errCtx2 = astBuiltInExpr.Arg2.ErrCtx;
				string expressionMustBeNumericType2 = Strings.ExpressionMustBeNumericType;
				throw EntitySqlException.Create(errCtx2, expressionMustBeNumericType2, null);
			}
			if (TypeHelpers.GetCommonTypeUsage(pair.Left.ResultType, pair.Right.ResultType) == null)
			{
				ErrorContext errCtx3 = astBuiltInExpr.ErrCtx;
				string errorMessage = Strings.ArgumentTypesAreIncompatible(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
				throw EntitySqlException.Create(errCtx3, errorMessage, null);
			}
		}
		return pair;
	}

	private static Pair<DbExpression, DbExpression> ConvertPlusOperands(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		Pair<DbExpression, DbExpression> pair = ConvertValueExpressionsWithUntypedNulls(astBuiltInExpr.Arg1, astBuiltInExpr.Arg2, astBuiltInExpr.ErrCtx, () => Strings.InvalidNullArithmetic, sr);
		if (!TypeSemantics.IsNumericType(pair.Left.ResultType) && !TypeSemantics.IsPrimitiveType(pair.Left.ResultType, PrimitiveTypeKind.String))
		{
			ErrorContext errCtx = astBuiltInExpr.Arg1.ErrCtx;
			string plusLeftExpressionInvalidType = Strings.PlusLeftExpressionInvalidType;
			throw EntitySqlException.Create(errCtx, plusLeftExpressionInvalidType, null);
		}
		if (!TypeSemantics.IsNumericType(pair.Right.ResultType) && !TypeSemantics.IsPrimitiveType(pair.Right.ResultType, PrimitiveTypeKind.String))
		{
			ErrorContext errCtx2 = astBuiltInExpr.Arg2.ErrCtx;
			string plusRightExpressionInvalidType = Strings.PlusRightExpressionInvalidType;
			throw EntitySqlException.Create(errCtx2, plusRightExpressionInvalidType, null);
		}
		if (TypeHelpers.GetCommonTypeUsage(pair.Left.ResultType, pair.Right.ResultType) == null)
		{
			ErrorContext errCtx3 = astBuiltInExpr.ErrCtx;
			string errorMessage = Strings.ArgumentTypesAreIncompatible(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx3, errorMessage, null);
		}
		return pair;
	}

	private static Pair<DbExpression, DbExpression> ConvertLogicalArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		DbExpression dbExpression = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
		if (dbExpression == null)
		{
			dbExpression = TypeResolver.BooleanType.Null();
		}
		DbExpression dbExpression2 = null;
		if (astBuiltInExpr.Arg2 != null)
		{
			dbExpression2 = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg2, sr);
			if (dbExpression2 == null)
			{
				dbExpression2 = TypeResolver.BooleanType.Null();
			}
		}
		if (!IsBooleanType(dbExpression.ResultType))
		{
			ErrorContext errCtx = astBuiltInExpr.Arg1.ErrCtx;
			string expressionTypeMustBeBoolean = Strings.ExpressionTypeMustBeBoolean;
			throw EntitySqlException.Create(errCtx, expressionTypeMustBeBoolean, null);
		}
		if (dbExpression2 != null && !IsBooleanType(dbExpression2.ResultType))
		{
			ErrorContext errCtx2 = astBuiltInExpr.Arg2.ErrCtx;
			string expressionTypeMustBeBoolean2 = Strings.ExpressionTypeMustBeBoolean;
			throw EntitySqlException.Create(errCtx2, expressionTypeMustBeBoolean2, null);
		}
		return new Pair<DbExpression, DbExpression>(dbExpression, dbExpression2);
	}

	private static Pair<DbExpression, DbExpression> ConvertEqualCompArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		Pair<DbExpression, DbExpression> pair = ConvertValueExpressionsWithUntypedNulls(astBuiltInExpr.Arg1, astBuiltInExpr.Arg2, astBuiltInExpr.ErrCtx, () => Strings.InvalidNullComparison, sr);
		if (!TypeSemantics.IsEqualComparableTo(pair.Left.ResultType, pair.Right.ResultType))
		{
			ErrorContext errCtx = astBuiltInExpr.ErrCtx;
			string errorMessage = Strings.ArgumentTypesAreIncompatible(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return pair;
	}

	private static Pair<DbExpression, DbExpression> ConvertOrderCompArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		Pair<DbExpression, DbExpression> pair = ConvertValueExpressionsWithUntypedNulls(astBuiltInExpr.Arg1, astBuiltInExpr.Arg2, astBuiltInExpr.ErrCtx, () => Strings.InvalidNullComparison, sr);
		if (!TypeSemantics.IsOrderComparableTo(pair.Left.ResultType, pair.Right.ResultType))
		{
			ErrorContext errCtx = astBuiltInExpr.ErrCtx;
			string errorMessage = Strings.ArgumentTypesAreIncompatible(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		return pair;
	}

	private static Pair<DbExpression, DbExpression> ConvertSetArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		DbExpression dbExpression = ConvertValueExpression(astBuiltInExpr.Arg1, sr);
		DbExpression dbExpression2 = null;
		if (astBuiltInExpr.Arg2 != null)
		{
			if (!TypeSemantics.IsCollectionType(dbExpression.ResultType))
			{
				ErrorContext errCtx = astBuiltInExpr.Arg1.ErrCtx;
				string leftSetExpressionArgsMustBeCollection = Strings.LeftSetExpressionArgsMustBeCollection;
				throw EntitySqlException.Create(errCtx, leftSetExpressionArgsMustBeCollection, null);
			}
			dbExpression2 = ConvertValueExpression(astBuiltInExpr.Arg2, sr);
			if (!TypeSemantics.IsCollectionType(dbExpression2.ResultType))
			{
				ErrorContext errCtx2 = astBuiltInExpr.Arg2.ErrCtx;
				string rightSetExpressionArgsMustBeCollection = Strings.RightSetExpressionArgsMustBeCollection;
				throw EntitySqlException.Create(errCtx2, rightSetExpressionArgsMustBeCollection, null);
			}
			TypeUsage elementTypeUsage = TypeHelpers.GetElementTypeUsage(dbExpression.ResultType);
			TypeUsage elementTypeUsage2 = TypeHelpers.GetElementTypeUsage(dbExpression2.ResultType);
			if (!TypeSemantics.TryGetCommonType(elementTypeUsage, elementTypeUsage2, out var _))
			{
				CqlErrorHelper.ReportIncompatibleCommonType(astBuiltInExpr.ErrCtx, elementTypeUsage, elementTypeUsage2);
			}
			if (astBuiltInExpr.Kind != BuiltInKind.UnionAll)
			{
				if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(dbExpression.ResultType)))
				{
					ErrorContext errCtx3 = astBuiltInExpr.Arg1.ErrCtx;
					string errorMessage = Strings.PlaceholderSetArgTypeIsNotEqualComparable(Strings.LocalizedLeft, astBuiltInExpr.Kind.ToString().ToUpperInvariant(), TypeHelpers.GetElementTypeUsage(dbExpression.ResultType).EdmType.FullName);
					throw EntitySqlException.Create(errCtx3, errorMessage, null);
				}
				if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(dbExpression2.ResultType)))
				{
					ErrorContext errCtx4 = astBuiltInExpr.Arg2.ErrCtx;
					string errorMessage2 = Strings.PlaceholderSetArgTypeIsNotEqualComparable(Strings.LocalizedRight, astBuiltInExpr.Kind.ToString().ToUpperInvariant(), TypeHelpers.GetElementTypeUsage(dbExpression2.ResultType).EdmType.FullName);
					throw EntitySqlException.Create(errCtx4, errorMessage2, null);
				}
			}
			else
			{
				if (Helper.IsAssociationType(elementTypeUsage.EdmType))
				{
					ErrorContext errCtx5 = astBuiltInExpr.Arg1.ErrCtx;
					string errorMessage3 = Strings.InvalidAssociationTypeForUnion(elementTypeUsage.EdmType.FullName);
					throw EntitySqlException.Create(errCtx5, errorMessage3, null);
				}
				if (Helper.IsAssociationType(elementTypeUsage2.EdmType))
				{
					ErrorContext errCtx6 = astBuiltInExpr.Arg2.ErrCtx;
					string errorMessage4 = Strings.InvalidAssociationTypeForUnion(elementTypeUsage2.EdmType.FullName);
					throw EntitySqlException.Create(errCtx6, errorMessage4, null);
				}
			}
		}
		else
		{
			if (!TypeSemantics.IsCollectionType(dbExpression.ResultType))
			{
				ErrorContext errCtx7 = astBuiltInExpr.Arg1.ErrCtx;
				string errorMessage5 = Strings.InvalidUnarySetOpArgument(astBuiltInExpr.Name);
				throw EntitySqlException.Create(errCtx7, errorMessage5, null);
			}
			if (astBuiltInExpr.Kind == BuiltInKind.Distinct && !TypeHelpers.IsValidDistinctOpType(TypeHelpers.GetElementTypeUsage(dbExpression.ResultType)))
			{
				ErrorContext errCtx8 = astBuiltInExpr.Arg1.ErrCtx;
				string expressionTypeMustBeEqualComparable = Strings.ExpressionTypeMustBeEqualComparable;
				throw EntitySqlException.Create(errCtx8, expressionTypeMustBeEqualComparable, null);
			}
		}
		return new Pair<DbExpression, DbExpression>(dbExpression, dbExpression2);
	}

	private static Pair<DbExpression, DbExpression> ConvertInExprArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
	{
		DbExpression dbExpression = ConvertValueExpression(astBuiltInExpr.Arg2, sr);
		if (!TypeSemantics.IsCollectionType(dbExpression.ResultType))
		{
			ErrorContext errCtx = astBuiltInExpr.Arg2.ErrCtx;
			string rightSetExpressionArgsMustBeCollection = Strings.RightSetExpressionArgsMustBeCollection;
			throw EntitySqlException.Create(errCtx, rightSetExpressionArgsMustBeCollection, null);
		}
		DbExpression dbExpression2 = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
		if (dbExpression2 == null)
		{
			TypeUsage elementTypeUsage = TypeHelpers.GetElementTypeUsage(dbExpression.ResultType);
			ValidateTypeForNullExpression(elementTypeUsage, astBuiltInExpr.Arg1.ErrCtx);
			dbExpression2 = elementTypeUsage.Null();
		}
		if (TypeSemantics.IsCollectionType(dbExpression2.ResultType))
		{
			ErrorContext errCtx2 = astBuiltInExpr.Arg1.ErrCtx;
			string expressionTypeMustNotBeCollection = Strings.ExpressionTypeMustNotBeCollection;
			throw EntitySqlException.Create(errCtx2, expressionTypeMustNotBeCollection, null);
		}
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(dbExpression2.ResultType, TypeHelpers.GetElementTypeUsage(dbExpression.ResultType));
		if (commonTypeUsage == null || !TypeHelpers.IsValidInOpType(commonTypeUsage))
		{
			ErrorContext errCtx3 = astBuiltInExpr.ErrCtx;
			string errorMessage = Strings.InvalidInExprArgs(dbExpression2.ResultType.EdmType.FullName, dbExpression.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx3, errorMessage, null);
		}
		return new Pair<DbExpression, DbExpression>(dbExpression2, dbExpression);
	}

	private static void ValidateTypeForNullExpression(TypeUsage type, ErrorContext errCtx)
	{
		if (TypeSemantics.IsCollectionType(type))
		{
			string nullLiteralCannotBePromotedToCollectionOfNulls = Strings.NullLiteralCannotBePromotedToCollectionOfNulls;
			throw EntitySqlException.Create(errCtx, nullLiteralCannotBePromotedToCollectionOfNulls, null);
		}
	}

	private static TypeUsage ConvertTypeName(Node typeName, SemanticResolver sr)
	{
		string[] names = null;
		NodeList<Node> nodeList = null;
		if (typeName is MethodExpr methodExpr)
		{
			typeName = methodExpr.Expr;
			typeName.ErrCtx.ErrorContextInfo = methodExpr.ErrCtx.ErrorContextInfo;
			typeName.ErrCtx.UseContextInfoAsResourceIdentifier = methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier;
			nodeList = methodExpr.Args;
		}
		if (typeName is Identifier identifier)
		{
			names = new string[1] { identifier.Name };
		}
		if (typeName is DotExpr dotExpr)
		{
			dotExpr.IsMultipartIdentifier(out names);
		}
		if (names == null)
		{
			ErrorContext errCtx = typeName.ErrCtx;
			string invalidMetadataMemberName = Strings.InvalidMetadataMemberName;
			throw EntitySqlException.Create(errCtx, invalidMetadataMemberName, null);
		}
		MetadataMember metadataMember = sr.ResolveMetadataMemberName(names, typeName.ErrCtx);
		switch (metadataMember.MetadataMemberClass)
		{
		case MetadataMemberClass.Type:
		{
			TypeUsage typeUsage = ((MetadataType)metadataMember).TypeUsage;
			if (nodeList != null)
			{
				typeUsage = ConvertTypeSpecArgs(typeUsage, nodeList, typeName.ErrCtx);
			}
			return typeUsage;
		}
		case MetadataMemberClass.Namespace:
		{
			ErrorContext errCtx3 = typeName.ErrCtx;
			string errorMessage2 = Strings.TypeNameNotFound(metadataMember.Name);
			throw EntitySqlException.Create(errCtx3, errorMessage2, null);
		}
		default:
		{
			ErrorContext errCtx2 = typeName.ErrCtx;
			string errorMessage = Strings.InvalidMetadataMemberClassResolution(metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataType.TypeClassName);
			throw EntitySqlException.Create(errCtx2, errorMessage, null);
		}
		}
	}

	private static TypeUsage ConvertTypeSpecArgs(TypeUsage parameterizedType, NodeList<Node> typeSpecArgs, ErrorContext errCtx)
	{
		foreach (Node item in (IEnumerable<Node>)typeSpecArgs)
		{
			if (!(item is Literal))
			{
				ErrorContext errCtx2 = item.ErrCtx;
				string typeArgumentMustBeLiteral = Strings.TypeArgumentMustBeLiteral;
				throw EntitySqlException.Create(errCtx2, typeArgumentMustBeLiteral, null);
			}
		}
		PrimitiveType primitiveType = parameterizedType.EdmType as PrimitiveType;
		if (primitiveType == null || primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
		{
			string errorMessage = Strings.TypeDoesNotSupportSpec(primitiveType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (typeSpecArgs.Count > 2)
		{
			string errorMessage2 = Strings.TypeArgumentCountMismatch(primitiveType.FullName, 2);
			throw EntitySqlException.Create(errCtx, errorMessage2, null);
		}
		ConvertTypeFacetValue(primitiveType, (Literal)typeSpecArgs[0], "Precision", out var byteValue);
		byte byteValue2 = 0;
		if (typeSpecArgs.Count == 2)
		{
			ConvertTypeFacetValue(primitiveType, (Literal)typeSpecArgs[1], "Scale", out byteValue2);
		}
		if (byteValue < byteValue2)
		{
			ErrorContext errCtx3 = typeSpecArgs[0].ErrCtx;
			string errorMessage3 = Strings.PrecisionMustBeGreaterThanScale(byteValue, byteValue2);
			throw EntitySqlException.Create(errCtx3, errorMessage3, null);
		}
		return TypeUsage.CreateDecimalTypeUsage(primitiveType, byteValue, byteValue2);
	}

	private static void ConvertTypeFacetValue(PrimitiveType type, Literal value, string facetName, out byte byteValue)
	{
		FacetDescription facet = Helper.GetFacet(type.ProviderManifest.GetFacetDescriptions(type), facetName);
		if (facet == null)
		{
			ErrorContext errCtx = value.ErrCtx;
			string errorMessage = Strings.TypeDoesNotSupportFacet(type.FullName, facetName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (value.IsNumber && byte.TryParse(value.OriginalValue, out byteValue))
		{
			if (facet.MaxValue.HasValue && byteValue > facet.MaxValue.Value)
			{
				ErrorContext errCtx2 = value.ErrCtx;
				string errorMessage2 = Strings.TypeArgumentExceedsMax(facetName);
				throw EntitySqlException.Create(errCtx2, errorMessage2, null);
			}
			if (facet.MinValue.HasValue && byteValue < facet.MinValue.Value)
			{
				ErrorContext errCtx3 = value.ErrCtx;
				string errorMessage3 = Strings.TypeArgumentBelowMin(facetName);
				throw EntitySqlException.Create(errCtx3, errorMessage3, null);
			}
			return;
		}
		ErrorContext errCtx4 = value.ErrCtx;
		string typeArgumentIsNotValid = Strings.TypeArgumentIsNotValid;
		throw EntitySqlException.Create(errCtx4, typeArgumentIsNotValid, null);
	}

	private static TypeUsage ConvertTypeDefinition(Node typeDefinitionExpr, SemanticResolver sr)
	{
		TypeUsage typeUsage = null;
		CollectionTypeDefinition collectionTypeDefinition = typeDefinitionExpr as CollectionTypeDefinition;
		RefTypeDefinition refTypeDefinition = typeDefinitionExpr as RefTypeDefinition;
		RowTypeDefinition rowTypeDefinition = typeDefinitionExpr as RowTypeDefinition;
		if (collectionTypeDefinition != null)
		{
			return TypeHelpers.CreateCollectionTypeUsage(ConvertTypeDefinition(collectionTypeDefinition.ElementTypeDef, sr));
		}
		if (refTypeDefinition != null)
		{
			TypeUsage typeUsage2 = ConvertTypeName(refTypeDefinition.RefTypeIdentifier, sr);
			if (!TypeSemantics.IsEntityType(typeUsage2))
			{
				ErrorContext errCtx = refTypeDefinition.RefTypeIdentifier.ErrCtx;
				string errorMessage = Strings.RefTypeIdentifierMustSpecifyAnEntityType(typeUsage2.EdmType.FullName, typeUsage2.EdmType.BuiltInTypeKind.ToString());
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			return TypeHelpers.CreateReferenceTypeUsage((EntityType)typeUsage2.EdmType);
		}
		if (rowTypeDefinition != null)
		{
			return TypeHelpers.CreateRowTypeUsage(rowTypeDefinition.Properties.Select((PropDefinition p) => new KeyValuePair<string, TypeUsage>(p.Name.Name, ConvertTypeDefinition(p.Type, sr))));
		}
		return ConvertTypeName(typeDefinitionExpr, sr);
	}

	private static ExpressionResolution ConvertRowConstructor(Node expr, SemanticResolver sr)
	{
		RowConstructorExpr rowConstructorExpr = (RowConstructorExpr)expr;
		Dictionary<string, TypeUsage> dictionary = new Dictionary<string, TypeUsage>(sr.NameComparer);
		List<DbExpression> list = new List<DbExpression>(rowConstructorExpr.AliasedExprList.Count);
		for (int i = 0; i < rowConstructorExpr.AliasedExprList.Count; i++)
		{
			AliasedExpr aliasedExpr = rowConstructorExpr.AliasedExprList[i];
			DbExpression dbExpression = ConvertValueExpressionAllowUntypedNulls(aliasedExpr.Expr, sr);
			if (dbExpression == null)
			{
				ErrorContext errCtx = aliasedExpr.Expr.ErrCtx;
				string rowCtorElementCannotBeNull = Strings.RowCtorElementCannotBeNull;
				throw EntitySqlException.Create(errCtx, rowCtorElementCannotBeNull, null);
			}
			string text = sr.InferAliasName(aliasedExpr, dbExpression);
			if (dictionary.ContainsKey(text))
			{
				if (aliasedExpr.Alias != null)
				{
					CqlErrorHelper.ReportAliasAlreadyUsedError(text, aliasedExpr.Alias.ErrCtx, Strings.InRowCtor);
				}
				else
				{
					text = sr.GenerateInternalName("autoRowCol");
				}
			}
			dictionary.Add(text, dbExpression.ResultType);
			list.Add(dbExpression);
		}
		return new ValueExpression(TypeHelpers.CreateRowTypeUsage(dictionary).New(list));
	}

	private static ExpressionResolution ConvertMultisetConstructor(Node expr, SemanticResolver sr)
	{
		MultisetConstructorExpr multisetConstructorExpr = (MultisetConstructorExpr)expr;
		if (multisetConstructorExpr.ExprList == null)
		{
			ErrorContext errCtx = expr.ErrCtx;
			string cannotCreateEmptyMultiset = Strings.CannotCreateEmptyMultiset;
			throw EntitySqlException.Create(errCtx, cannotCreateEmptyMultiset, null);
		}
		DbExpression[] array = multisetConstructorExpr.ExprList.Select((Node e) => ConvertValueExpressionAllowUntypedNulls(e, sr)).ToArray();
		TypeUsage[] array2 = (from e in array
			where e != null
			select e.ResultType).ToArray();
		if (array2.Length == 0)
		{
			ErrorContext errCtx2 = expr.ErrCtx;
			string cannotCreateMultisetofNulls = Strings.CannotCreateMultisetofNulls;
			throw EntitySqlException.Create(errCtx2, cannotCreateMultisetofNulls, null);
		}
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(array2);
		if (commonTypeUsage == null)
		{
			ErrorContext errCtx3 = expr.ErrCtx;
			string multisetElemsAreNotTypeCompatible = Strings.MultisetElemsAreNotTypeCompatible;
			throw EntitySqlException.Create(errCtx3, multisetElemsAreNotTypeCompatible, null);
		}
		commonTypeUsage = TypeHelpers.GetReadOnlyType(commonTypeUsage);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				ValidateTypeForNullExpression(commonTypeUsage, multisetConstructorExpr.ExprList[i].ErrCtx);
				array[i] = commonTypeUsage.Null();
			}
		}
		return new ValueExpression(TypeHelpers.CreateCollectionTypeUsage(commonTypeUsage).New(array));
	}

	private static ExpressionResolution ConvertCaseExpr(Node expr, SemanticResolver sr)
	{
		CaseExpr caseExpr = (CaseExpr)expr;
		List<DbExpression> list = new List<DbExpression>(caseExpr.WhenThenExprList.Count);
		List<DbExpression> list2 = new List<DbExpression>(caseExpr.WhenThenExprList.Count);
		for (int i = 0; i < caseExpr.WhenThenExprList.Count; i++)
		{
			WhenThenExpr whenThenExpr = caseExpr.WhenThenExprList[i];
			DbExpression dbExpression = ConvertValueExpression(whenThenExpr.WhenExpr, sr);
			if (!IsBooleanType(dbExpression.ResultType))
			{
				ErrorContext errCtx = whenThenExpr.WhenExpr.ErrCtx;
				string expressionTypeMustBeBoolean = Strings.ExpressionTypeMustBeBoolean;
				throw EntitySqlException.Create(errCtx, expressionTypeMustBeBoolean, null);
			}
			list.Add(dbExpression);
			DbExpression item = ConvertValueExpressionAllowUntypedNulls(whenThenExpr.ThenExpr, sr);
			list2.Add(item);
		}
		DbExpression dbExpression2 = ((caseExpr.ElseExpr != null) ? ConvertValueExpressionAllowUntypedNulls(caseExpr.ElseExpr, sr) : null);
		List<TypeUsage> list3 = (from e in list2
			where e != null
			select e.ResultType).ToList();
		if (dbExpression2 != null)
		{
			list3.Add(dbExpression2.ResultType);
		}
		if (list3.Count == 0)
		{
			ErrorContext errCtx2 = caseExpr.ElseExpr.ErrCtx;
			string invalidCaseWhenThenNullType = Strings.InvalidCaseWhenThenNullType;
			throw EntitySqlException.Create(errCtx2, invalidCaseWhenThenNullType, null);
		}
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(list3);
		if (commonTypeUsage == null)
		{
			ErrorContext errCtx3 = caseExpr.WhenThenExprList[0].ThenExpr.ErrCtx;
			string invalidCaseResultTypes = Strings.InvalidCaseResultTypes;
			throw EntitySqlException.Create(errCtx3, invalidCaseResultTypes, null);
		}
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j] == null)
			{
				ValidateTypeForNullExpression(commonTypeUsage, caseExpr.WhenThenExprList[j].ThenExpr.ErrCtx);
				list2[j] = commonTypeUsage.Null();
			}
		}
		if (dbExpression2 == null)
		{
			if (caseExpr.ElseExpr == null && TypeSemantics.IsCollectionType(commonTypeUsage))
			{
				dbExpression2 = commonTypeUsage.NewEmptyCollection();
			}
			else
			{
				ValidateTypeForNullExpression(commonTypeUsage, (caseExpr.ElseExpr ?? caseExpr).ErrCtx);
				dbExpression2 = commonTypeUsage.Null();
			}
		}
		return new ValueExpression(DbExpressionBuilder.Case(list, list2, dbExpression2));
	}

	private static ExpressionResolution ConvertQueryExpr(Node expr, SemanticResolver sr)
	{
		QueryExpr queryExpr = (QueryExpr)expr;
		DbExpression value = null;
		bool flag = ParserOptions.CompilationMode.RestrictedViewGenerationMode == sr.ParserOptions.ParserCompilationMode;
		if (queryExpr.HavingClause != null && queryExpr.GroupByClause == null)
		{
			ErrorContext errCtx = queryExpr.ErrCtx;
			string havingRequiresGroupClause = Strings.HavingRequiresGroupClause;
			throw EntitySqlException.Create(errCtx, havingRequiresGroupClause, null);
		}
		if (queryExpr.SelectClause.TopExpr != null)
		{
			if (queryExpr.OrderByClause != null && queryExpr.OrderByClause.LimitSubClause != null)
			{
				ErrorContext errCtx2 = queryExpr.SelectClause.TopExpr.ErrCtx;
				string topAndLimitCannotCoexist = Strings.TopAndLimitCannotCoexist;
				throw EntitySqlException.Create(errCtx2, topAndLimitCannotCoexist, null);
			}
			if (queryExpr.OrderByClause != null && queryExpr.OrderByClause.SkipSubClause != null)
			{
				ErrorContext errCtx3 = queryExpr.SelectClause.TopExpr.ErrCtx;
				string topAndSkipCannotCoexist = Strings.TopAndSkipCannotCoexist;
				throw EntitySqlException.Create(errCtx3, topAndSkipCannotCoexist, null);
			}
		}
		using (sr.EnterScopeRegion())
		{
			DbExpressionBinding source = ProcessFromClause(queryExpr.FromClause, sr);
			source = ProcessWhereClause(source, queryExpr.WhereClause, sr);
			bool queryProjectionProcessed = false;
			if (!flag)
			{
				source = ProcessGroupByClause(source, queryExpr, sr);
				source = ProcessHavingClause(source, queryExpr.HavingClause, sr);
				source = ProcessOrderByClause(source, queryExpr, out queryProjectionProcessed, sr);
			}
			value = ProcessSelectClause(source, queryExpr, queryProjectionProcessed, sr);
		}
		return new ValueExpression(value);
	}

	private static DbExpression ProcessSelectClause(DbExpressionBinding source, QueryExpr queryExpr, bool queryProjectionProcessed, SemanticResolver sr)
	{
		SelectClause selectClause = queryExpr.SelectClause;
		DbExpression dbExpression;
		if (queryProjectionProcessed)
		{
			dbExpression = source.Expression;
		}
		else
		{
			List<KeyValuePair<string, DbExpression>> projectionItems = ConvertSelectClauseItems(queryExpr, sr);
			dbExpression = CreateProjectExpression(source, selectClause, projectionItems);
		}
		if (selectClause.TopExpr != null || (queryExpr.OrderByClause != null && queryExpr.OrderByClause.LimitSubClause != null))
		{
			Node node;
			string exprName;
			if (selectClause.TopExpr != null)
			{
				node = selectClause.TopExpr;
				exprName = "TOP";
			}
			else
			{
				node = queryExpr.OrderByClause.LimitSubClause;
				exprName = "LIMIT";
			}
			DbExpression dbExpression2 = ConvertValueExpression(node, sr);
			ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(dbExpression2, node.ErrCtx, exprName);
			dbExpression = dbExpression.Limit(dbExpression2);
		}
		return dbExpression;
	}

	private static List<KeyValuePair<string, DbExpression>> ConvertSelectClauseItems(QueryExpr queryExpr, SemanticResolver sr)
	{
		SelectClause selectClause = queryExpr.SelectClause;
		if (selectClause.SelectKind == SelectKind.Value)
		{
			if (selectClause.Items.Count != 1)
			{
				ErrorContext errCtx = selectClause.ErrCtx;
				string invalidSelectValueList = Strings.InvalidSelectValueList;
				throw EntitySqlException.Create(errCtx, invalidSelectValueList, null);
			}
			if (selectClause.Items[0].Alias != null && queryExpr.OrderByClause == null)
			{
				ErrorContext errCtx2 = selectClause.Items[0].ErrCtx;
				string invalidSelectValueAliasedExpression = Strings.InvalidSelectValueAliasedExpression;
				throw EntitySqlException.Create(errCtx2, invalidSelectValueAliasedExpression, null);
			}
		}
		HashSet<string> hashSet = new HashSet<string>(sr.NameComparer);
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>(selectClause.Items.Count);
		for (int i = 0; i < selectClause.Items.Count; i++)
		{
			AliasedExpr aliasedExpr = selectClause.Items[i];
			DbExpression dbExpression = ConvertValueExpression(aliasedExpr.Expr, sr);
			string text = sr.InferAliasName(aliasedExpr, dbExpression);
			if (hashSet.Contains(text))
			{
				if (aliasedExpr.Alias != null)
				{
					CqlErrorHelper.ReportAliasAlreadyUsedError(text, aliasedExpr.Alias.ErrCtx, Strings.InSelectProjectionList);
				}
				else
				{
					text = sr.GenerateInternalName("autoProject");
				}
			}
			hashSet.Add(text);
			list.Add(new KeyValuePair<string, DbExpression>(text, dbExpression));
		}
		return list;
	}

	private static DbExpression CreateProjectExpression(DbExpressionBinding source, SelectClause selectClause, List<KeyValuePair<string, DbExpression>> projectionItems)
	{
		DbExpression dbExpression = ((selectClause.SelectKind != 0) ? source.Project(DbExpressionBuilder.NewRow(projectionItems)) : source.Project(projectionItems[0].Value));
		if (selectClause.DistinctKind == DistinctKind.Distinct)
		{
			ValidateDistinctProjection(dbExpression.ResultType, selectClause);
			dbExpression = dbExpression.Distinct();
		}
		return dbExpression;
	}

	private static void ValidateDistinctProjection(TypeUsage projectExpressionResultType, SelectClause selectClause)
	{
		ValidateDistinctProjection(projectExpressionResultType, selectClause.Items[0].Expr.ErrCtx, (selectClause.SelectKind == SelectKind.Row) ? new List<ErrorContext>(selectClause.Items.Select((AliasedExpr item) => item.Expr.ErrCtx)) : null);
	}

	private static void ValidateDistinctProjection(TypeUsage projectExpressionResultType, ErrorContext defaultErrCtx, List<ErrorContext> projectionItemErrCtxs)
	{
		TypeUsage elementTypeUsage = TypeHelpers.GetElementTypeUsage(projectExpressionResultType);
		if (TypeHelpers.IsValidDistinctOpType(elementTypeUsage))
		{
			return;
		}
		ErrorContext errCtx = defaultErrCtx;
		if (projectionItemErrCtxs != null && TypeSemantics.IsRowType(elementTypeUsage))
		{
			RowType rowType = elementTypeUsage.EdmType as RowType;
			for (int i = 0; i < rowType.Members.Count; i++)
			{
				if (!TypeHelpers.IsValidDistinctOpType(rowType.Members[i].TypeUsage))
				{
					errCtx = projectionItemErrCtxs[i];
					break;
				}
			}
		}
		string selectDistinctMustBeEqualComparable = Strings.SelectDistinctMustBeEqualComparable;
		throw EntitySqlException.Create(errCtx, selectDistinctMustBeEqualComparable, null);
	}

	private static void ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(DbExpression expr, ErrorContext errCtx, string exprName)
	{
		if (expr.ExpressionKind != DbExpressionKind.Constant && expr.ExpressionKind != DbExpressionKind.ParameterReference)
		{
			string errorMessage = Strings.PlaceholderExpressionMustBeConstant(exprName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (!TypeSemantics.IsPromotableTo(expr.ResultType, TypeResolver.Int64Type))
		{
			string errorMessage2 = Strings.PlaceholderExpressionMustBeCompatibleWithEdm64(exprName, expr.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage2, null);
		}
		if (expr is DbConstantExpression dbConstantExpression && System.Convert.ToInt64(dbConstantExpression.Value, CultureInfo.InvariantCulture) < 0)
		{
			string errorMessage3 = Strings.PlaceholderExpressionMustBeGreaterThanOrEqualToZero(exprName);
			throw EntitySqlException.Create(errCtx, errorMessage3, null);
		}
	}

	private static DbExpressionBinding ProcessFromClause(FromClause fromClause, SemanticResolver sr)
	{
		DbExpressionBinding fromBinding = null;
		List<SourceScopeEntry> list = new List<SourceScopeEntry>();
		for (int i = 0; i < fromClause.FromClauseItems.Count; i++)
		{
			List<SourceScopeEntry> scopeEntries;
			DbExpressionBinding dbExpressionBinding = ProcessFromClauseItem(fromClause.FromClauseItems[i], sr, out scopeEntries);
			list.AddRange(scopeEntries);
			if (fromBinding == null)
			{
				fromBinding = dbExpressionBinding;
				continue;
			}
			fromBinding = fromBinding.CrossApply(dbExpressionBinding).BindAs(sr.GenerateInternalName("lcapply"));
			list.Each((SourceScopeEntry scopeEntry) => scopeEntry.AddParentVar(fromBinding.Variable));
		}
		return fromBinding;
	}

	private static DbExpressionBinding ProcessFromClauseItem(FromClauseItem fromClauseItem, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
	{
		DbExpressionBinding dbExpressionBinding = null;
		return fromClauseItem.FromClauseItemKind switch
		{
			FromClauseItemKind.AliasedFromClause => ProcessAliasedFromClauseItem((AliasedExpr)fromClauseItem.FromExpr, sr, out scopeEntries), 
			FromClauseItemKind.JoinFromClause => ProcessJoinClauseItem((JoinClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries), 
			_ => ProcessApplyClauseItem((ApplyClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries), 
		};
	}

	private static DbExpressionBinding ProcessAliasedFromClauseItem(AliasedExpr aliasedExpr, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
	{
		DbExpressionBinding dbExpressionBinding = null;
		DbExpression dbExpression = ConvertValueExpression(aliasedExpr.Expr, sr);
		if (!TypeSemantics.IsCollectionType(dbExpression.ResultType))
		{
			ErrorContext errCtx = aliasedExpr.Expr.ErrCtx;
			string expressionMustBeCollection = Strings.ExpressionMustBeCollection;
			throw EntitySqlException.Create(errCtx, expressionMustBeCollection, null);
		}
		string text = sr.InferAliasName(aliasedExpr, dbExpression);
		if (sr.CurrentScope.Contains(text))
		{
			if (aliasedExpr.Alias != null)
			{
				CqlErrorHelper.ReportAliasAlreadyUsedError(text, aliasedExpr.Alias.ErrCtx, Strings.InFromClause);
			}
			else
			{
				text = sr.GenerateInternalName("autoFrom");
			}
		}
		dbExpressionBinding = dbExpression.BindAs(text);
		SourceScopeEntry sourceScopeEntry = new SourceScopeEntry(dbExpressionBinding.Variable);
		sr.CurrentScope.Add(dbExpressionBinding.Variable.VariableName, sourceScopeEntry);
		scopeEntries = new List<SourceScopeEntry>();
		scopeEntries.Add(sourceScopeEntry);
		return dbExpressionBinding;
	}

	private static DbExpressionBinding ProcessJoinClauseItem(JoinClauseItem joinClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
	{
		DbExpressionBinding joinBinding = null;
		if (joinClause.OnExpr == null)
		{
			if (JoinKind.Inner == joinClause.JoinKind)
			{
				ErrorContext errCtx = joinClause.ErrCtx;
				string innerJoinMustHaveOnPredicate = Strings.InnerJoinMustHaveOnPredicate;
				throw EntitySqlException.Create(errCtx, innerJoinMustHaveOnPredicate, null);
			}
		}
		else if (joinClause.JoinKind == JoinKind.Cross)
		{
			ErrorContext errCtx2 = joinClause.OnExpr.ErrCtx;
			string invalidPredicateForCrossJoin = Strings.InvalidPredicateForCrossJoin;
			throw EntitySqlException.Create(errCtx2, invalidPredicateForCrossJoin, null);
		}
		List<SourceScopeEntry> scopeEntries2;
		DbExpressionBinding dbExpressionBinding = ProcessFromClauseItem(joinClause.LeftExpr, sr, out scopeEntries2);
		scopeEntries2.Each((SourceScopeEntry scopeEntry) => scopeEntry.IsJoinClauseLeftExpr = true);
		List<SourceScopeEntry> scopeEntries3;
		DbExpressionBinding dbExpressionBinding2 = ProcessFromClauseItem(joinClause.RightExpr, sr, out scopeEntries3);
		scopeEntries2.Each((SourceScopeEntry scopeEntry) => scopeEntry.IsJoinClauseLeftExpr = false);
		if (joinClause.JoinKind == JoinKind.RightOuter)
		{
			joinClause.JoinKind = JoinKind.LeftOuter;
			DbExpressionBinding dbExpressionBinding3 = dbExpressionBinding;
			dbExpressionBinding = dbExpressionBinding2;
			dbExpressionBinding2 = dbExpressionBinding3;
		}
		DbExpressionKind dbExpressionKind = MapJoinKind(joinClause.JoinKind);
		DbExpression joinCondition = null;
		if (joinClause.OnExpr == null)
		{
			if (DbExpressionKind.CrossJoin != dbExpressionKind)
			{
				joinCondition = DbExpressionBuilder.True;
			}
		}
		else
		{
			joinCondition = ConvertValueExpression(joinClause.OnExpr, sr);
		}
		joinBinding = DbExpressionBuilder.CreateJoinExpressionByKind(dbExpressionKind, joinCondition, dbExpressionBinding, dbExpressionBinding2).BindAs(sr.GenerateInternalName("join"));
		scopeEntries = scopeEntries2;
		scopeEntries.AddRange(scopeEntries3);
		scopeEntries.Each((SourceScopeEntry scopeEntry) => scopeEntry.AddParentVar(joinBinding.Variable));
		return joinBinding;
	}

	private static DbExpressionKind MapJoinKind(JoinKind joinKind)
	{
		return _joinMap[(int)joinKind];
	}

	private static DbExpressionBinding ProcessApplyClauseItem(ApplyClauseItem applyClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
	{
		DbExpressionBinding applyBinding = null;
		List<SourceScopeEntry> scopeEntries2;
		DbExpressionBinding input = ProcessFromClauseItem(applyClause.LeftExpr, sr, out scopeEntries2);
		List<SourceScopeEntry> scopeEntries3;
		DbExpressionBinding apply = ProcessFromClauseItem(applyClause.RightExpr, sr, out scopeEntries3);
		applyBinding = DbExpressionBuilder.CreateApplyExpressionByKind(MapApplyKind(applyClause.ApplyKind), input, apply).BindAs(sr.GenerateInternalName("apply"));
		scopeEntries = scopeEntries2;
		scopeEntries.AddRange(scopeEntries3);
		scopeEntries.Each((SourceScopeEntry scopeEntry) => scopeEntry.AddParentVar(applyBinding.Variable));
		return applyBinding;
	}

	private static DbExpressionKind MapApplyKind(ApplyKind applyKind)
	{
		return _applyMap[(int)applyKind];
	}

	private static DbExpressionBinding ProcessWhereClause(DbExpressionBinding source, Node whereClause, SemanticResolver sr)
	{
		if (whereClause == null)
		{
			return source;
		}
		return ProcessWhereHavingClausePredicate(source, whereClause, whereClause.ErrCtx, "where", sr);
	}

	private static DbExpressionBinding ProcessHavingClause(DbExpressionBinding source, HavingClause havingClause, SemanticResolver sr)
	{
		if (havingClause == null)
		{
			return source;
		}
		return ProcessWhereHavingClausePredicate(source, havingClause.HavingPredicate, havingClause.ErrCtx, "having", sr);
	}

	private static DbExpressionBinding ProcessWhereHavingClausePredicate(DbExpressionBinding source, Node predicate, ErrorContext errCtx, string bindingNameTemplate, SemanticResolver sr)
	{
		DbExpressionBinding whereBinding = null;
		DbExpression dbExpression = ConvertValueExpression(predicate, sr);
		if (!IsBooleanType(dbExpression.ResultType))
		{
			string expressionTypeMustBeBoolean = Strings.ExpressionTypeMustBeBoolean;
			throw EntitySqlException.Create(errCtx, expressionTypeMustBeBoolean, null);
		}
		whereBinding = source.Filter(dbExpression).BindAs(sr.GenerateInternalName(bindingNameTemplate));
		sr.CurrentScopeRegion.ApplyToScopeEntries(delegate(ScopeEntry scopeEntry)
		{
			if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar)
			{
				((SourceScopeEntry)scopeEntry).ReplaceParentVar(whereBinding.Variable);
			}
		});
		return whereBinding;
	}

	private static DbExpressionBinding ProcessGroupByClause(DbExpressionBinding source, QueryExpr queryExpr, SemanticResolver sr)
	{
		GroupByClause groupByClause = queryExpr.GroupByClause;
		int num = groupByClause?.GroupItems.Count ?? 0;
		bool flag = num == 0;
		if (flag && !queryExpr.HasMethodCall)
		{
			return source;
		}
		DbGroupExpressionBinding groupInputBinding = source.Expression.GroupBindAs(sr.GenerateInternalName("geb"), sr.GenerateInternalName("group"));
		DbGroupAggregate groupAggregate = groupInputBinding.GroupAggregate;
		DbVariableReferenceExpression dbVariableReferenceExpression = groupAggregate.ResultType.Variable(sr.GenerateInternalName("groupAggregate"));
		DbExpressionBinding groupAggregateBinding = dbVariableReferenceExpression.BindAs(sr.GenerateInternalName("groupPartitionItem"));
		sr.CurrentScopeRegion.EnterGroupOperation(groupAggregateBinding);
		sr.CurrentScopeRegion.ApplyToScopeEntries(delegate(ScopeEntry scopeEntry)
		{
			((SourceScopeEntry)scopeEntry).AdjustToGroupVar(groupInputBinding.Variable, groupInputBinding.GroupVariable, groupAggregateBinding.Variable);
		});
		HashSet<string> hashSet = new HashSet<string>(sr.NameComparer);
		List<GroupKeyInfo> list = new List<GroupKeyInfo>(num);
		if (!flag)
		{
			for (int i = 0; i < num; i++)
			{
				AliasedExpr aliasedExpr = groupByClause.GroupItems[i];
				sr.CurrentScopeRegion.WasResolutionCorrelated = false;
				GroupKeyAggregateInfo aggregateInfo;
				DbExpression dbExpression;
				using (sr.EnterGroupKeyDefinition(GroupAggregateKind.GroupKey, aliasedExpr.ErrCtx, out aggregateInfo))
				{
					dbExpression = ConvertValueExpression(aliasedExpr.Expr, sr);
				}
				if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
				{
					ErrorContext errCtx = aliasedExpr.Expr.ErrCtx;
					string errorMessage = Strings.KeyMustBeCorrelated("GROUP BY");
					throw EntitySqlException.Create(errCtx, errorMessage, null);
				}
				if (!TypeHelpers.IsValidGroupKeyType(dbExpression.ResultType))
				{
					ErrorContext errCtx2 = aliasedExpr.Expr.ErrCtx;
					string groupingKeysMustBeEqualComparable = Strings.GroupingKeysMustBeEqualComparable;
					throw EntitySqlException.Create(errCtx2, groupingKeysMustBeEqualComparable, null);
				}
				GroupKeyAggregateInfo aggregateInfo2;
				DbExpression groupVarBasedKeyExpr;
				using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Function, aliasedExpr.ErrCtx, out aggregateInfo2))
				{
					groupVarBasedKeyExpr = ConvertValueExpression(aliasedExpr.Expr, sr);
				}
				GroupKeyAggregateInfo aggregateInfo3;
				DbExpression groupAggBasedKeyExpr;
				using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Partition, aliasedExpr.ErrCtx, out aggregateInfo3))
				{
					groupAggBasedKeyExpr = ConvertValueExpression(aliasedExpr.Expr, sr);
				}
				string text = sr.InferAliasName(aliasedExpr, dbExpression);
				if (hashSet.Contains(text))
				{
					if (aliasedExpr.Alias != null)
					{
						CqlErrorHelper.ReportAliasAlreadyUsedError(text, aliasedExpr.Alias.ErrCtx, Strings.InGroupClause);
					}
					else
					{
						text = sr.GenerateInternalName("autoGroup");
					}
				}
				hashSet.Add(text);
				GroupKeyInfo groupKeyInfo = new GroupKeyInfo(text, dbExpression, groupVarBasedKeyExpr, groupAggBasedKeyExpr);
				list.Add(groupKeyInfo);
				if (aliasedExpr.Alias == null && aliasedExpr.Expr is DotExpr dotExpr && dotExpr.IsMultipartIdentifier(out var names))
				{
					groupKeyInfo.AlternativeName = names;
					string fullName = TypeResolver.GetFullName(names);
					if (hashSet.Contains(fullName))
					{
						CqlErrorHelper.ReportAliasAlreadyUsedError(fullName, dotExpr.ErrCtx, Strings.InGroupClause);
					}
					hashSet.Add(fullName);
				}
			}
		}
		int currentScopeIndex = sr.CurrentScopeIndex;
		sr.EnterScope();
		foreach (GroupKeyInfo item in list)
		{
			sr.CurrentScope.Add(item.Name, new GroupKeyDefinitionScopeEntry(item.VarBasedKeyExpr, item.GroupVarBasedKeyExpr, item.GroupAggBasedKeyExpr, null));
			if (item.AlternativeName != null)
			{
				string fullName2 = TypeResolver.GetFullName(item.AlternativeName);
				sr.CurrentScope.Add(fullName2, new GroupKeyDefinitionScopeEntry(item.VarBasedKeyExpr, item.GroupVarBasedKeyExpr, item.GroupAggBasedKeyExpr, item.AlternativeName));
			}
		}
		if (queryExpr.HavingClause != null && queryExpr.HavingClause.HasMethodCall)
		{
			ConvertValueExpression(queryExpr.HavingClause.HavingPredicate, sr);
		}
		Dictionary<string, DbExpression> dictionary = null;
		if (queryExpr.OrderByClause != null || queryExpr.SelectClause.HasMethodCall)
		{
			dictionary = new Dictionary<string, DbExpression>(queryExpr.SelectClause.Items.Count, sr.NameComparer);
			for (int j = 0; j < queryExpr.SelectClause.Items.Count; j++)
			{
				AliasedExpr aliasedExpr2 = queryExpr.SelectClause.Items[j];
				DbExpression dbExpression2 = ConvertValueExpression(aliasedExpr2.Expr, sr);
				dbExpression2 = ((dbExpression2.ExpressionKind == DbExpressionKind.Null) ? dbExpression2 : dbExpression2.ResultType.Null());
				string text2 = sr.InferAliasName(aliasedExpr2, dbExpression2);
				if (dictionary.ContainsKey(text2))
				{
					if (aliasedExpr2.Alias != null)
					{
						CqlErrorHelper.ReportAliasAlreadyUsedError(text2, aliasedExpr2.Alias.ErrCtx, Strings.InSelectProjectionList);
					}
					else
					{
						text2 = sr.GenerateInternalName("autoProject");
					}
				}
				dictionary.Add(text2, dbExpression2);
			}
		}
		if (queryExpr.OrderByClause != null && queryExpr.OrderByClause.HasMethodCall)
		{
			sr.EnterScope();
			foreach (KeyValuePair<string, DbExpression> item2 in dictionary)
			{
				sr.CurrentScope.Add(item2.Key, new ProjectionItemDefinitionScopeEntry(item2.Value));
			}
			for (int k = 0; k < queryExpr.OrderByClause.OrderByClauseItem.Count; k++)
			{
				OrderByClauseItem orderByClauseItem = queryExpr.OrderByClause.OrderByClauseItem[k];
				sr.CurrentScopeRegion.WasResolutionCorrelated = false;
				ConvertValueExpression(orderByClauseItem.OrderExpr, sr);
				if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
				{
					ErrorContext errCtx3 = orderByClauseItem.ErrCtx;
					string errorMessage2 = Strings.KeyMustBeCorrelated("ORDER BY");
					throw EntitySqlException.Create(errCtx3, errorMessage2, null);
				}
			}
			sr.LeaveScope();
		}
		if (flag && sr.CurrentScopeRegion.GroupAggregateInfos.Count == 0)
		{
			sr.RollbackToScope(currentScopeIndex);
			sr.CurrentScopeRegion.ApplyToScopeEntries(delegate(ScopeEntry scopeEntry)
			{
				((SourceScopeEntry)scopeEntry).RollbackAdjustmentToGroupVar(source.Variable);
			});
			sr.CurrentScopeRegion.RollbackGroupOperation();
			return source;
		}
		List<KeyValuePair<string, DbAggregate>> list2 = new List<KeyValuePair<string, DbAggregate>>(sr.CurrentScopeRegion.GroupAggregateInfos.Count);
		bool flag2 = false;
		foreach (GroupAggregateInfo groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos)
		{
			switch (groupAggregateInfo.AggregateKind)
			{
			case GroupAggregateKind.Function:
				list2.Add(new KeyValuePair<string, DbAggregate>(groupAggregateInfo.AggregateName, ((FunctionAggregateInfo)groupAggregateInfo).AggregateDefinition));
				break;
			case GroupAggregateKind.Partition:
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			list2.Add(new KeyValuePair<string, DbAggregate>(dbVariableReferenceExpression.VariableName, groupAggregate));
		}
		DbGroupByExpression input = groupInputBinding.GroupBy(list.Select((GroupKeyInfo keyInfo) => new KeyValuePair<string, DbExpression>(keyInfo.Name, keyInfo.VarBasedKeyExpr)), list2);
		DbExpressionBinding groupBinding = input.BindAs(sr.GenerateInternalName("group"));
		if (flag2)
		{
			List<KeyValuePair<string, DbExpression>> list3 = ProcessGroupPartitionDefinitions(sr.CurrentScopeRegion.GroupAggregateInfos, dbVariableReferenceExpression, groupBinding);
			if (list3 != null)
			{
				list3.AddRange(list.Select((GroupKeyInfo keyInfo) => new KeyValuePair<string, DbExpression>(keyInfo.Name, groupBinding.Variable.Property(keyInfo.Name))));
				list3.AddRange(from groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos
					where groupAggregateInfo.AggregateKind == GroupAggregateKind.Function
					select new KeyValuePair<string, DbExpression>(groupAggregateInfo.AggregateName, groupBinding.Variable.Property(groupAggregateInfo.AggregateName)));
				DbExpression projection = DbExpressionBuilder.NewRow(list3);
				groupBinding = groupBinding.Project(projection).BindAs(sr.GenerateInternalName("groupPartitionDefs"));
			}
		}
		sr.RollbackToScope(currentScopeIndex);
		sr.CurrentScopeRegion.ApplyToScopeEntries((ScopeEntry scopeEntry) => new InvalidGroupInputRefScopeEntry());
		sr.EnterScope();
		foreach (GroupKeyInfo item3 in list)
		{
			sr.CurrentScope.Add(item3.VarRef.VariableName, new SourceScopeEntry(item3.VarRef).AddParentVar(groupBinding.Variable));
			if (item3.AlternativeName != null)
			{
				string fullName3 = TypeResolver.GetFullName(item3.AlternativeName);
				sr.CurrentScope.Add(fullName3, new SourceScopeEntry(item3.VarRef, item3.AlternativeName).AddParentVar(groupBinding.Variable));
			}
		}
		foreach (GroupAggregateInfo groupAggregateInfo2 in sr.CurrentScopeRegion.GroupAggregateInfos)
		{
			DbVariableReferenceExpression dbVariableReferenceExpression2 = groupAggregateInfo2.AggregateStubExpression.ResultType.Variable(groupAggregateInfo2.AggregateName);
			if (!sr.CurrentScope.Contains(dbVariableReferenceExpression2.VariableName))
			{
				sr.CurrentScope.Add(dbVariableReferenceExpression2.VariableName, new SourceScopeEntry(dbVariableReferenceExpression2).AddParentVar(groupBinding.Variable));
				sr.CurrentScopeRegion.RegisterGroupAggregateName(dbVariableReferenceExpression2.VariableName);
			}
			groupAggregateInfo2.AggregateStubExpression = null;
		}
		return groupBinding;
	}

	private static List<KeyValuePair<string, DbExpression>> ProcessGroupPartitionDefinitions(List<GroupAggregateInfo> groupAggregateInfos, DbVariableReferenceExpression groupAggregateVarRef, DbExpressionBinding groupBinding)
	{
		ReadOnlyCollection<DbVariableReferenceExpression> variables = new ReadOnlyCollection<DbVariableReferenceExpression>(new DbVariableReferenceExpression[1] { groupAggregateVarRef });
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
		bool flag = false;
		foreach (GroupAggregateInfo groupAggregateInfo in groupAggregateInfos)
		{
			if (groupAggregateInfo.AggregateKind == GroupAggregateKind.Partition)
			{
				GroupPartitionInfo groupPartitionInfo = (GroupPartitionInfo)groupAggregateInfo;
				DbExpression aggregateDefinition = groupPartitionInfo.AggregateDefinition;
				if (IsTrivialInputProjection(groupAggregateVarRef, aggregateDefinition))
				{
					groupAggregateInfo.AggregateName = groupAggregateVarRef.VariableName;
					flag = true;
				}
				else
				{
					DbLambda lambda = new DbLambda(variables, groupPartitionInfo.AggregateDefinition);
					list.Add(new KeyValuePair<string, DbExpression>(groupAggregateInfo.AggregateName, lambda.Invoke(groupBinding.Variable.Property(groupAggregateVarRef.VariableName))));
				}
			}
		}
		if (flag)
		{
			if (list.Count > 0)
			{
				list.Add(new KeyValuePair<string, DbExpression>(groupAggregateVarRef.VariableName, groupBinding.Variable.Property(groupAggregateVarRef.VariableName)));
			}
			else
			{
				list = null;
			}
		}
		return list;
	}

	private static bool IsTrivialInputProjection(DbVariableReferenceExpression lambdaVariable, DbExpression lambdaBody)
	{
		if (lambdaBody.ExpressionKind != DbExpressionKind.Project)
		{
			return false;
		}
		DbProjectExpression dbProjectExpression = (DbProjectExpression)lambdaBody;
		if (dbProjectExpression.Input.Expression != lambdaVariable)
		{
			return false;
		}
		if (dbProjectExpression.Projection.ExpressionKind == DbExpressionKind.VariableReference)
		{
			return (DbVariableReferenceExpression)dbProjectExpression.Projection == dbProjectExpression.Input.Variable;
		}
		if (dbProjectExpression.Projection.ExpressionKind == DbExpressionKind.NewInstance && TypeSemantics.IsRowType(dbProjectExpression.Projection.ResultType))
		{
			if (!TypeSemantics.IsEqual(dbProjectExpression.Projection.ResultType, dbProjectExpression.Input.Variable.ResultType))
			{
				return false;
			}
			IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(dbProjectExpression.Input.Variable.ResultType);
			DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)dbProjectExpression.Projection;
			for (int i = 0; i < dbNewInstanceExpression.Arguments.Count; i++)
			{
				if (dbNewInstanceExpression.Arguments[i].ExpressionKind != DbExpressionKind.Property)
				{
					return false;
				}
				DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)dbNewInstanceExpression.Arguments[i];
				if (dbPropertyExpression.Instance != dbProjectExpression.Input.Variable || dbPropertyExpression.Property != allStructuralMembers[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private static DbExpressionBinding ProcessOrderByClause(DbExpressionBinding source, QueryExpr queryExpr, out bool queryProjectionProcessed, SemanticResolver sr)
	{
		queryProjectionProcessed = false;
		if (queryExpr.OrderByClause == null)
		{
			return source;
		}
		DbExpressionBinding sortBinding = null;
		OrderByClause orderByClause = queryExpr.OrderByClause;
		SelectClause selectClause = queryExpr.SelectClause;
		DbExpression dbExpression = null;
		if (orderByClause.SkipSubClause != null)
		{
			dbExpression = ConvertValueExpression(orderByClause.SkipSubClause, sr);
			ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(dbExpression, orderByClause.SkipSubClause.ErrCtx, "SKIP");
		}
		List<KeyValuePair<string, DbExpression>> list = ConvertSelectClauseItems(queryExpr, sr);
		if (selectClause.DistinctKind == DistinctKind.Distinct)
		{
			sr.CurrentScopeRegion.RollbackAllScopes();
		}
		int currentScopeIndex = sr.CurrentScopeIndex;
		sr.EnterScope();
		list.Each((KeyValuePair<string, DbExpression> projectionItem) => sr.CurrentScope.Add(projectionItem.Key, new ProjectionItemDefinitionScopeEntry(projectionItem.Value)));
		if (selectClause.DistinctKind == DistinctKind.Distinct)
		{
			source = CreateProjectExpression(source, selectClause, list).BindAs(sr.GenerateInternalName("distinct"));
			if (selectClause.SelectKind == SelectKind.Value)
			{
				sr.CurrentScope.Replace(list[0].Key, new SourceScopeEntry(source.Variable));
			}
			else
			{
				foreach (KeyValuePair<string, DbExpression> item in list)
				{
					DbVariableReferenceExpression dbVariableReferenceExpression = item.Value.ResultType.Variable(item.Key);
					sr.CurrentScope.Replace(dbVariableReferenceExpression.VariableName, new SourceScopeEntry(dbVariableReferenceExpression).AddParentVar(source.Variable));
				}
			}
			queryProjectionProcessed = true;
		}
		List<DbSortClause> list2 = new List<DbSortClause>(orderByClause.OrderByClauseItem.Count);
		for (int i = 0; i < orderByClause.OrderByClauseItem.Count; i++)
		{
			OrderByClauseItem orderByClauseItem = orderByClause.OrderByClauseItem[i];
			sr.CurrentScopeRegion.WasResolutionCorrelated = false;
			DbExpression dbExpression2 = ConvertValueExpression(orderByClauseItem.OrderExpr, sr);
			if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
			{
				ErrorContext errCtx = orderByClauseItem.ErrCtx;
				string errorMessage = Strings.KeyMustBeCorrelated("ORDER BY");
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
			if (!TypeHelpers.IsValidSortOpKeyType(dbExpression2.ResultType))
			{
				ErrorContext errCtx2 = orderByClauseItem.OrderExpr.ErrCtx;
				string orderByKeyIsNotOrderComparable = Strings.OrderByKeyIsNotOrderComparable;
				throw EntitySqlException.Create(errCtx2, orderByKeyIsNotOrderComparable, null);
			}
			bool flag = orderByClauseItem.OrderKind == OrderKind.None || orderByClauseItem.OrderKind == OrderKind.Asc;
			string text = null;
			if (orderByClauseItem.Collation != null)
			{
				if (!IsStringType(dbExpression2.ResultType))
				{
					ErrorContext errCtx3 = orderByClauseItem.OrderExpr.ErrCtx;
					string errorMessage2 = Strings.InvalidKeyTypeForCollation(dbExpression2.ResultType.EdmType.FullName);
					throw EntitySqlException.Create(errCtx3, errorMessage2, null);
				}
				text = orderByClauseItem.Collation.Name;
			}
			if (string.IsNullOrEmpty(text))
			{
				list2.Add(flag ? dbExpression2.ToSortClause() : dbExpression2.ToSortClauseDescending());
			}
			else
			{
				list2.Add(flag ? dbExpression2.ToSortClause(text) : dbExpression2.ToSortClauseDescending(text));
			}
		}
		sr.RollbackToScope(currentScopeIndex);
		DbExpression dbExpression3 = null;
		dbExpression3 = ((dbExpression == null) ? ((DbExpression)source.Sort(list2)) : ((DbExpression)source.Skip(list2, dbExpression)));
		sortBinding = dbExpression3.BindAs(sr.GenerateInternalName("sort"));
		if (!queryProjectionProcessed)
		{
			sr.CurrentScopeRegion.ApplyToScopeEntries(delegate(ScopeEntry scopeEntry)
			{
				if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar)
				{
					((SourceScopeEntry)scopeEntry).ReplaceParentVar(sortBinding.Variable);
				}
			});
		}
		return sortBinding;
	}

	private static DbExpression ConvertSimpleInExpression(DbExpression left, DbExpression right)
	{
		DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)right;
		if (dbNewInstanceExpression.Arguments.Count == 0)
		{
			return DbExpressionBuilder.False;
		}
		return Helpers.BuildBalancedTreeInPlace(new List<DbExpression>(dbNewInstanceExpression.Arguments.Select((DbExpression arg) => left.Equal(arg))), (DbExpression prev, DbExpression next) => prev.Or(next));
	}

	private static bool IsStringType(TypeUsage type)
	{
		return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String);
	}

	private static bool IsBooleanType(TypeUsage type)
	{
		return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean);
	}

	private static bool IsSubOrSuperType(TypeUsage type1, TypeUsage type2)
	{
		if (!TypeSemantics.IsStructurallyEqual(type1, type2) && !type1.IsSubtypeOf(type2))
		{
			return type2.IsSubtypeOf(type1);
		}
		return true;
	}

	private static Dictionary<Type, AstExprConverter> CreateAstExprConverters()
	{
		return new Dictionary<Type, AstExprConverter>(17)
		{
			{
				typeof(Literal),
				ConvertLiteral
			},
			{
				typeof(QueryParameter),
				ConvertParameter
			},
			{
				typeof(Identifier),
				ConvertIdentifier
			},
			{
				typeof(DotExpr),
				ConvertDotExpr
			},
			{
				typeof(BuiltInExpr),
				ConvertBuiltIn
			},
			{
				typeof(QueryExpr),
				ConvertQueryExpr
			},
			{
				typeof(ParenExpr),
				ConvertParenExpr
			},
			{
				typeof(RowConstructorExpr),
				ConvertRowConstructor
			},
			{
				typeof(MultisetConstructorExpr),
				ConvertMultisetConstructor
			},
			{
				typeof(CaseExpr),
				ConvertCaseExpr
			},
			{
				typeof(RelshipNavigationExpr),
				ConvertRelshipNavigationExpr
			},
			{
				typeof(RefExpr),
				ConvertRefExpr
			},
			{
				typeof(DerefExpr),
				ConvertDeRefExpr
			},
			{
				typeof(MethodExpr),
				ConvertMethodExpr
			},
			{
				typeof(CreateRefExpr),
				ConvertCreateRefExpr
			},
			{
				typeof(KeyExpr),
				ConvertKeyExpr
			},
			{
				typeof(GroupPartitionExpr),
				ConvertGroupPartitionExpr
			}
		};
	}

	private static Dictionary<BuiltInKind, BuiltInExprConverter> CreateBuiltInExprConverter()
	{
		return new Dictionary<BuiltInKind, BuiltInExprConverter>(4)
		{
			{
				BuiltInKind.Plus,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair20 = ConvertPlusOperands(bltInExpr, sr);
					if (TypeSemantics.IsNumericType(pair20.Left.ResultType))
					{
						return pair20.Left.Plus(pair20.Right);
					}
					if (!sr.TypeResolver.TryGetFunctionFromMetadata("Edm", "Concat", out var functionGroup))
					{
						ErrorContext errCtx32 = bltInExpr.ErrCtx;
						string concatBuiltinNotSupported = Strings.ConcatBuiltinNotSupported;
						throw EntitySqlException.Create(errCtx32, concatBuiltinNotSupported, null);
					}
					List<TypeUsage> argTypes = new List<TypeUsage>(2)
					{
						pair20.Left.ResultType,
						pair20.Right.ResultType
					};
					bool isAmbiguous = false;
					EdmFunction edmFunction = SemanticResolver.ResolveFunctionOverloads(functionGroup.FunctionMetadata, argTypes, isGroupAggregateFunction: false, out isAmbiguous);
					if (edmFunction == null || isAmbiguous)
					{
						ErrorContext errCtx33 = bltInExpr.ErrCtx;
						string concatBuiltinNotSupported2 = Strings.ConcatBuiltinNotSupported;
						throw EntitySqlException.Create(errCtx33, concatBuiltinNotSupported2, null);
					}
					return edmFunction.Invoke(pair20.Left, pair20.Right);
				}
			},
			{
				BuiltInKind.Minus,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair19 = ConvertArithmeticArgs(bltInExpr, sr);
					return pair19.Left.Minus(pair19.Right);
				}
			},
			{
				BuiltInKind.Multiply,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair18 = ConvertArithmeticArgs(bltInExpr, sr);
					return pair18.Left.Multiply(pair18.Right);
				}
			},
			{
				BuiltInKind.Divide,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair17 = ConvertArithmeticArgs(bltInExpr, sr);
					return pair17.Left.Divide(pair17.Right);
				}
			},
			{
				BuiltInKind.Modulus,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair16 = ConvertArithmeticArgs(bltInExpr, sr);
					return pair16.Left.Modulo(pair16.Right);
				}
			},
			{
				BuiltInKind.UnaryMinus,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression left3 = ConvertArithmeticArgs(bltInExpr, sr).Left;
					if (TypeSemantics.IsUnsignedNumericType(left3.ResultType))
					{
						TypeUsage promotableType = null;
						if (!TypeHelpers.TryGetClosestPromotableType(left3.ResultType, out promotableType))
						{
							throw new EntitySqlException(Strings.InvalidUnsignedTypeForUnaryMinusOperation(left3.ResultType.EdmType.FullName));
						}
					}
					return left3.UnaryMinus();
				}
			},
			{
				BuiltInKind.UnaryPlus,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertArithmeticArgs(bltInExpr, sr).Left
			},
			{
				BuiltInKind.And,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair15 = ConvertLogicalArgs(bltInExpr, sr);
					return pair15.Left.And(pair15.Right);
				}
			},
			{
				BuiltInKind.Or,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair14 = ConvertLogicalArgs(bltInExpr, sr);
					return pair14.Left.Or(pair14.Right);
				}
			},
			{
				BuiltInKind.Not,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertLogicalArgs(bltInExpr, sr).Left.Not()
			},
			{
				BuiltInKind.Equal,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair13 = ConvertEqualCompArgs(bltInExpr, sr);
					return pair13.Left.Equal(pair13.Right);
				}
			},
			{
				BuiltInKind.NotEqual,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair12 = ConvertEqualCompArgs(bltInExpr, sr);
					return pair12.Left.Equal(pair12.Right).Not();
				}
			},
			{
				BuiltInKind.GreaterEqual,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair11 = ConvertOrderCompArgs(bltInExpr, sr);
					return pair11.Left.GreaterThanOrEqual(pair11.Right);
				}
			},
			{
				BuiltInKind.GreaterThan,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair10 = ConvertOrderCompArgs(bltInExpr, sr);
					return pair10.Left.GreaterThan(pair10.Right);
				}
			},
			{
				BuiltInKind.LessEqual,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair9 = ConvertOrderCompArgs(bltInExpr, sr);
					return pair9.Left.LessThanOrEqual(pair9.Right);
				}
			},
			{
				BuiltInKind.LessThan,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair8 = ConvertOrderCompArgs(bltInExpr, sr);
					return pair8.Left.LessThan(pair8.Right);
				}
			},
			{
				BuiltInKind.Union,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair7 = ConvertSetArgs(bltInExpr, sr);
					return pair7.Left.UnionAll(pair7.Right).Distinct();
				}
			},
			{
				BuiltInKind.UnionAll,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair6 = ConvertSetArgs(bltInExpr, sr);
					return pair6.Left.UnionAll(pair6.Right);
				}
			},
			{
				BuiltInKind.Intersect,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair5 = ConvertSetArgs(bltInExpr, sr);
					return pair5.Left.Intersect(pair5.Right);
				}
			},
			{
				BuiltInKind.Overlaps,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair4 = ConvertSetArgs(bltInExpr, sr);
					return pair4.Left.Intersect(pair4.Right).IsEmpty().Not();
				}
			},
			{
				BuiltInKind.AnyElement,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertSetArgs(bltInExpr, sr).Left.Element()
			},
			{
				BuiltInKind.Element,
				delegate
				{
					throw new NotSupportedException(Strings.ElementOperatorIsNotSupported);
				}
			},
			{
				BuiltInKind.Except,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair3 = ConvertSetArgs(bltInExpr, sr);
					return pair3.Left.Except(pair3.Right);
				}
			},
			{
				BuiltInKind.Exists,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertSetArgs(bltInExpr, sr).Left.IsEmpty().Not()
			},
			{
				BuiltInKind.Flatten,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression13 = ConvertValueExpression(bltInExpr.Arg1, sr);
					if (!TypeSemantics.IsCollectionType(dbExpression13.ResultType))
					{
						ErrorContext errCtx30 = bltInExpr.Arg1.ErrCtx;
						string invalidFlattenArgument = Strings.InvalidFlattenArgument;
						throw EntitySqlException.Create(errCtx30, invalidFlattenArgument, null);
					}
					if (!TypeSemantics.IsCollectionType(TypeHelpers.GetElementTypeUsage(dbExpression13.ResultType)))
					{
						ErrorContext errCtx31 = bltInExpr.Arg1.ErrCtx;
						string invalidFlattenArgument2 = Strings.InvalidFlattenArgument;
						throw EntitySqlException.Create(errCtx31, invalidFlattenArgument2, null);
					}
					DbExpressionBinding dbExpressionBinding3 = dbExpression13.BindAs(sr.GenerateInternalName("l_flatten"));
					DbExpressionBinding dbExpressionBinding4 = dbExpressionBinding3.Variable.BindAs(sr.GenerateInternalName("r_flatten"));
					DbExpressionBinding dbExpressionBinding5 = dbExpressionBinding3.CrossApply(dbExpressionBinding4).BindAs(sr.GenerateInternalName("flatten"));
					return dbExpressionBinding5.Project(dbExpressionBinding5.Variable.Property(dbExpressionBinding4.VariableName));
				}
			},
			{
				BuiltInKind.In,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair2 = ConvertInExprArgs(bltInExpr, sr);
					if (pair2.Right.ExpressionKind == DbExpressionKind.NewInstance)
					{
						return ConvertSimpleInExpression(pair2.Left, pair2.Right);
					}
					DbExpressionBinding dbExpressionBinding2 = pair2.Right.BindAs(sr.GenerateInternalName("in-filter"));
					DbExpression left2 = pair2.Left;
					DbExpression variable2 = dbExpressionBinding2.Variable;
					DbExpression right2 = dbExpressionBinding2.Filter(left2.Equal(variable2)).IsEmpty().Not();
					return DbExpressionBuilder.Case(new List<DbExpression>(1) { left2.IsNull() }, new List<DbExpression>(1) { TypeResolver.BooleanType.Null() }, DbExpressionBuilder.False).Or(right2);
				}
			},
			{
				BuiltInKind.NotIn,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					Pair<DbExpression, DbExpression> pair = ConvertInExprArgs(bltInExpr, sr);
					if (pair.Right.ExpressionKind == DbExpressionKind.NewInstance)
					{
						return ConvertSimpleInExpression(pair.Left, pair.Right).Not();
					}
					DbExpressionBinding dbExpressionBinding = pair.Right.BindAs(sr.GenerateInternalName("in-filter"));
					DbExpression left = pair.Left;
					DbExpression variable = dbExpressionBinding.Variable;
					DbExpression right = dbExpressionBinding.Filter(left.Equal(variable)).IsEmpty();
					return DbExpressionBuilder.Case(new List<DbExpression>(1) { left.IsNull() }, new List<DbExpression>(1) { TypeResolver.BooleanType.Null() }, DbExpressionBuilder.True).And(right);
				}
			},
			{
				BuiltInKind.Distinct,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertSetArgs(bltInExpr, sr).Left.Distinct()
			},
			{
				BuiltInKind.IsNull,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression12 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
					if (dbExpression12 != null && !TypeHelpers.IsValidIsNullOpType(dbExpression12.ResultType))
					{
						ErrorContext errCtx29 = bltInExpr.Arg1.ErrCtx;
						string isNullInvalidType2 = Strings.IsNullInvalidType;
						throw EntitySqlException.Create(errCtx29, isNullInvalidType2, null);
					}
					return (dbExpression12 == null) ? ((DbExpression)DbExpressionBuilder.True) : ((DbExpression)dbExpression12.IsNull());
				}
			},
			{
				BuiltInKind.IsNotNull,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression11 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
					if (dbExpression11 != null && !TypeHelpers.IsValidIsNullOpType(dbExpression11.ResultType))
					{
						ErrorContext errCtx28 = bltInExpr.Arg1.ErrCtx;
						string isNullInvalidType = Strings.IsNullInvalidType;
						throw EntitySqlException.Create(errCtx28, isNullInvalidType, null);
					}
					return (dbExpression11 == null) ? ((DbExpression)DbExpressionBuilder.False) : ((DbExpression)dbExpression11.IsNull().Not());
				}
			},
			{
				BuiltInKind.IsOf,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression9 = ConvertValueExpression(bltInExpr.Arg1, sr);
					TypeUsage typeUsage4 = ConvertTypeName(bltInExpr.Arg2, sr);
					bool num2 = (bool)((Literal)bltInExpr.Arg3).Value;
					bool flag3 = (bool)((Literal)bltInExpr.Arg4).Value;
					bool flag4 = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;
					if (!flag4 && !TypeSemantics.IsEntityType(dbExpression9.ResultType))
					{
						ErrorContext errCtx21 = bltInExpr.Arg1.ErrCtx;
						string errorMessage13 = Strings.ExpressionTypeMustBeEntityType(Strings.CtxIsOf, dbExpression9.ResultType.EdmType.BuiltInTypeKind.ToString(), dbExpression9.ResultType.EdmType.FullName);
						throw EntitySqlException.Create(errCtx21, errorMessage13, null);
					}
					if (flag4 && !TypeSemantics.IsNominalType(dbExpression9.ResultType))
					{
						ErrorContext errCtx22 = bltInExpr.Arg1.ErrCtx;
						string errorMessage14 = Strings.ExpressionTypeMustBeNominalType(Strings.CtxIsOf, dbExpression9.ResultType.EdmType.BuiltInTypeKind.ToString(), dbExpression9.ResultType.EdmType.FullName);
						throw EntitySqlException.Create(errCtx22, errorMessage14, null);
					}
					if (!flag4 && !TypeSemantics.IsEntityType(typeUsage4))
					{
						ErrorContext errCtx23 = bltInExpr.Arg2.ErrCtx;
						string errorMessage15 = Strings.TypeMustBeEntityType(Strings.CtxIsOf, typeUsage4.EdmType.BuiltInTypeKind.ToString(), typeUsage4.EdmType.FullName);
						throw EntitySqlException.Create(errCtx23, errorMessage15, null);
					}
					if (flag4 && !TypeSemantics.IsNominalType(typeUsage4))
					{
						ErrorContext errCtx24 = bltInExpr.Arg2.ErrCtx;
						string errorMessage16 = Strings.TypeMustBeNominalType(Strings.CtxIsOf, typeUsage4.EdmType.BuiltInTypeKind.ToString(), typeUsage4.EdmType.FullName);
						throw EntitySqlException.Create(errCtx24, errorMessage16, null);
					}
					if (!TypeSemantics.IsPolymorphicType(dbExpression9.ResultType))
					{
						ErrorContext errCtx25 = bltInExpr.Arg1.ErrCtx;
						string typeMustBeInheritableType3 = Strings.TypeMustBeInheritableType;
						throw EntitySqlException.Create(errCtx25, typeMustBeInheritableType3, null);
					}
					if (!TypeSemantics.IsPolymorphicType(typeUsage4))
					{
						ErrorContext errCtx26 = bltInExpr.Arg2.ErrCtx;
						string typeMustBeInheritableType4 = Strings.TypeMustBeInheritableType;
						throw EntitySqlException.Create(errCtx26, typeMustBeInheritableType4, null);
					}
					if (!IsSubOrSuperType(dbExpression9.ResultType, typeUsage4))
					{
						ErrorContext errCtx27 = bltInExpr.ErrCtx;
						string errorMessage17 = Strings.NotASuperOrSubType(dbExpression9.ResultType.EdmType.FullName, typeUsage4.EdmType.FullName);
						throw EntitySqlException.Create(errCtx27, errorMessage17, null);
					}
					typeUsage4 = TypeHelpers.GetReadOnlyType(typeUsage4);
					DbExpression dbExpression10 = null;
					dbExpression10 = ((!num2) ? dbExpression9.IsOf(typeUsage4) : dbExpression9.IsOfOnly(typeUsage4));
					if (flag3)
					{
						dbExpression10 = dbExpression10.Not();
					}
					return dbExpression10;
				}
			},
			{
				BuiltInKind.Treat,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression8 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
					TypeUsage typeUsage3 = ConvertTypeName(bltInExpr.Arg2, sr);
					bool flag2 = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;
					if (!flag2 && !TypeSemantics.IsEntityType(typeUsage3))
					{
						ErrorContext errCtx14 = bltInExpr.Arg2.ErrCtx;
						string errorMessage8 = Strings.TypeMustBeEntityType(Strings.CtxTreat, typeUsage3.EdmType.BuiltInTypeKind.ToString(), typeUsage3.EdmType.FullName);
						throw EntitySqlException.Create(errCtx14, errorMessage8, null);
					}
					if (flag2 && !TypeSemantics.IsNominalType(typeUsage3))
					{
						ErrorContext errCtx15 = bltInExpr.Arg2.ErrCtx;
						string errorMessage9 = Strings.TypeMustBeNominalType(Strings.CtxTreat, typeUsage3.EdmType.BuiltInTypeKind.ToString(), typeUsage3.EdmType.FullName);
						throw EntitySqlException.Create(errCtx15, errorMessage9, null);
					}
					if (dbExpression8 == null)
					{
						dbExpression8 = typeUsage3.Null();
					}
					else
					{
						if (!flag2 && !TypeSemantics.IsEntityType(dbExpression8.ResultType))
						{
							ErrorContext errCtx16 = bltInExpr.Arg1.ErrCtx;
							string errorMessage10 = Strings.ExpressionTypeMustBeEntityType(Strings.CtxTreat, dbExpression8.ResultType.EdmType.BuiltInTypeKind.ToString(), dbExpression8.ResultType.EdmType.FullName);
							throw EntitySqlException.Create(errCtx16, errorMessage10, null);
						}
						if (flag2 && !TypeSemantics.IsNominalType(dbExpression8.ResultType))
						{
							ErrorContext errCtx17 = bltInExpr.Arg1.ErrCtx;
							string errorMessage11 = Strings.ExpressionTypeMustBeNominalType(Strings.CtxTreat, dbExpression8.ResultType.EdmType.BuiltInTypeKind.ToString(), dbExpression8.ResultType.EdmType.FullName);
							throw EntitySqlException.Create(errCtx17, errorMessage11, null);
						}
					}
					if (!TypeSemantics.IsPolymorphicType(dbExpression8.ResultType))
					{
						ErrorContext errCtx18 = bltInExpr.Arg1.ErrCtx;
						string typeMustBeInheritableType = Strings.TypeMustBeInheritableType;
						throw EntitySqlException.Create(errCtx18, typeMustBeInheritableType, null);
					}
					if (!TypeSemantics.IsPolymorphicType(typeUsage3))
					{
						ErrorContext errCtx19 = bltInExpr.Arg2.ErrCtx;
						string typeMustBeInheritableType2 = Strings.TypeMustBeInheritableType;
						throw EntitySqlException.Create(errCtx19, typeMustBeInheritableType2, null);
					}
					if (!IsSubOrSuperType(dbExpression8.ResultType, typeUsage3))
					{
						ErrorContext errCtx20 = bltInExpr.Arg1.ErrCtx;
						string errorMessage12 = Strings.NotASuperOrSubType(dbExpression8.ResultType.EdmType.FullName, typeUsage3.EdmType.FullName);
						throw EntitySqlException.Create(errCtx20, errorMessage12, null);
					}
					return dbExpression8.TreatAs(TypeHelpers.GetReadOnlyType(typeUsage3));
				}
			},
			{
				BuiltInKind.Cast,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression7 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
					TypeUsage typeUsage2 = ConvertTypeName(bltInExpr.Arg2, sr);
					if (!TypeSemantics.IsScalarType(typeUsage2))
					{
						ErrorContext errCtx11 = bltInExpr.Arg2.ErrCtx;
						string invalidCastType = Strings.InvalidCastType;
						throw EntitySqlException.Create(errCtx11, invalidCastType, null);
					}
					if (dbExpression7 == null)
					{
						return typeUsage2.Null();
					}
					if (!TypeSemantics.IsScalarType(dbExpression7.ResultType))
					{
						ErrorContext errCtx12 = bltInExpr.Arg1.ErrCtx;
						string invalidCastExpressionType = Strings.InvalidCastExpressionType;
						throw EntitySqlException.Create(errCtx12, invalidCastExpressionType, null);
					}
					if (!TypeSemantics.IsCastAllowed(dbExpression7.ResultType, typeUsage2))
					{
						ErrorContext errCtx13 = bltInExpr.Arg1.ErrCtx;
						string errorMessage7 = Strings.InvalidCast(dbExpression7.ResultType.EdmType.FullName, typeUsage2.EdmType.FullName);
						throw EntitySqlException.Create(errCtx13, errorMessage7, null);
					}
					return dbExpression7.CastTo(TypeHelpers.GetReadOnlyType(typeUsage2));
				}
			},
			{
				BuiltInKind.OfType,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression5 = ConvertValueExpression(bltInExpr.Arg1, sr);
					TypeUsage typeUsage = ConvertTypeName(bltInExpr.Arg2, sr);
					bool num = (bool)((Literal)bltInExpr.Arg3).Value;
					bool flag = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;
					if (!TypeSemantics.IsCollectionType(dbExpression5.ResultType))
					{
						ErrorContext errCtx4 = bltInExpr.Arg1.ErrCtx;
						string expressionMustBeCollection = Strings.ExpressionMustBeCollection;
						throw EntitySqlException.Create(errCtx4, expressionMustBeCollection, null);
					}
					TypeUsage elementTypeUsage = TypeHelpers.GetElementTypeUsage(dbExpression5.ResultType);
					if (!flag && !TypeSemantics.IsEntityType(elementTypeUsage))
					{
						ErrorContext errCtx5 = bltInExpr.Arg1.ErrCtx;
						string errorMessage = Strings.OfTypeExpressionElementTypeMustBeEntityType(elementTypeUsage.EdmType.BuiltInTypeKind.ToString(), elementTypeUsage);
						throw EntitySqlException.Create(errCtx5, errorMessage, null);
					}
					if (flag && !TypeSemantics.IsNominalType(elementTypeUsage))
					{
						ErrorContext errCtx6 = bltInExpr.Arg1.ErrCtx;
						string errorMessage2 = Strings.OfTypeExpressionElementTypeMustBeNominalType(elementTypeUsage.EdmType.BuiltInTypeKind.ToString(), elementTypeUsage);
						throw EntitySqlException.Create(errCtx6, errorMessage2, null);
					}
					if (!flag && !TypeSemantics.IsEntityType(typeUsage))
					{
						ErrorContext errCtx7 = bltInExpr.Arg2.ErrCtx;
						string errorMessage3 = Strings.TypeMustBeEntityType(Strings.CtxOfType, typeUsage.EdmType.BuiltInTypeKind.ToString(), typeUsage.EdmType.FullName);
						throw EntitySqlException.Create(errCtx7, errorMessage3, null);
					}
					if (flag && !TypeSemantics.IsNominalType(typeUsage))
					{
						ErrorContext errCtx8 = bltInExpr.Arg2.ErrCtx;
						string errorMessage4 = Strings.TypeMustBeNominalType(Strings.CtxOfType, typeUsage.EdmType.BuiltInTypeKind.ToString(), typeUsage.EdmType.FullName);
						throw EntitySqlException.Create(errCtx8, errorMessage4, null);
					}
					if (num && typeUsage.EdmType.Abstract)
					{
						ErrorContext errCtx9 = bltInExpr.Arg2.ErrCtx;
						string errorMessage5 = Strings.OfTypeOnlyTypeArgumentCannotBeAbstract(typeUsage.EdmType.FullName);
						throw EntitySqlException.Create(errCtx9, errorMessage5, null);
					}
					if (!IsSubOrSuperType(elementTypeUsage, typeUsage))
					{
						ErrorContext errCtx10 = bltInExpr.Arg1.ErrCtx;
						string errorMessage6 = Strings.NotASuperOrSubType(elementTypeUsage.EdmType.FullName, typeUsage.EdmType.FullName);
						throw EntitySqlException.Create(errCtx10, errorMessage6, null);
					}
					DbExpression dbExpression6 = null;
					return num ? dbExpression5.OfTypeOnly(TypeHelpers.GetReadOnlyType(typeUsage)) : dbExpression5.OfType(TypeHelpers.GetReadOnlyType(typeUsage));
				}
			},
			{
				BuiltInKind.Like,
				delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
				{
					DbExpression dbExpression = null;
					DbExpression dbExpression2 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
					if (dbExpression2 == null)
					{
						dbExpression2 = TypeResolver.StringType.Null();
					}
					else if (!IsStringType(dbExpression2.ResultType))
					{
						ErrorContext errCtx = bltInExpr.Arg1.ErrCtx;
						string likeArgMustBeStringType = Strings.LikeArgMustBeStringType;
						throw EntitySqlException.Create(errCtx, likeArgMustBeStringType, null);
					}
					DbExpression dbExpression3 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg2, sr);
					if (dbExpression3 == null)
					{
						dbExpression3 = TypeResolver.StringType.Null();
					}
					else if (!IsStringType(dbExpression3.ResultType))
					{
						ErrorContext errCtx2 = bltInExpr.Arg2.ErrCtx;
						string likeArgMustBeStringType2 = Strings.LikeArgMustBeStringType;
						throw EntitySqlException.Create(errCtx2, likeArgMustBeStringType2, null);
					}
					if (3 == bltInExpr.ArgCount)
					{
						DbExpression dbExpression4 = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg3, sr);
						if (dbExpression4 == null)
						{
							dbExpression4 = TypeResolver.StringType.Null();
						}
						else if (!IsStringType(dbExpression4.ResultType))
						{
							ErrorContext errCtx3 = bltInExpr.Arg3.ErrCtx;
							string likeArgMustBeStringType3 = Strings.LikeArgMustBeStringType;
							throw EntitySqlException.Create(errCtx3, likeArgMustBeStringType3, null);
						}
						return dbExpression2.Like(dbExpression3, dbExpression4);
					}
					return dbExpression2.Like(dbExpression3);
				}
			},
			{
				BuiltInKind.Between,
				ConvertBetweenExpr
			},
			{
				BuiltInKind.NotBetween,
				(BuiltInExpr bltInExpr, SemanticResolver sr) => ConvertBetweenExpr(bltInExpr, sr).Not()
			}
		};
	}

	private static DbExpression ConvertBetweenExpr(BuiltInExpr bltInExpr, SemanticResolver sr)
	{
		Pair<DbExpression, DbExpression> pair = ConvertValueExpressionsWithUntypedNulls(bltInExpr.Arg2, bltInExpr.Arg3, bltInExpr.Arg1.ErrCtx, () => Strings.BetweenLimitsCannotBeUntypedNulls, sr);
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(pair.Left.ResultType, pair.Right.ResultType);
		if (commonTypeUsage == null)
		{
			ErrorContext errCtx = bltInExpr.Arg1.ErrCtx;
			string errorMessage = Strings.BetweenLimitsTypesAreNotCompatible(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		if (!TypeSemantics.IsOrderComparableTo(pair.Left.ResultType, pair.Right.ResultType))
		{
			ErrorContext errCtx2 = bltInExpr.Arg1.ErrCtx;
			string errorMessage2 = Strings.BetweenLimitsTypesAreNotOrderComparable(pair.Left.ResultType.EdmType.FullName, pair.Right.ResultType.EdmType.FullName);
			throw EntitySqlException.Create(errCtx2, errorMessage2, null);
		}
		DbExpression dbExpression = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
		if (dbExpression == null)
		{
			dbExpression = commonTypeUsage.Null();
		}
		if (!TypeSemantics.IsOrderComparableTo(dbExpression.ResultType, commonTypeUsage))
		{
			ErrorContext errCtx3 = bltInExpr.Arg1.ErrCtx;
			string errorMessage3 = Strings.BetweenValueIsNotOrderComparable(dbExpression.ResultType.EdmType.FullName, commonTypeUsage.EdmType.FullName);
			throw EntitySqlException.Create(errCtx3, errorMessage3, null);
		}
		return dbExpression.GreaterThanOrEqual(pair.Left).And(dbExpression.LessThanOrEqual(pair.Right));
	}
}
