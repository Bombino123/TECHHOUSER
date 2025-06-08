using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Internal;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

public static class DbExpressionBuilder
{
	private static readonly TypeUsage _booleanType = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean);

	private static readonly AliasGenerator _bindingAliases = new AliasGenerator("Var_", 0);

	private static readonly DbNullExpression _binaryNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Binary).Null();

	private static readonly DbNullExpression _boolNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean).Null();

	private static readonly DbNullExpression _byteNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Byte).Null();

	private static readonly DbNullExpression _dateTimeNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.DateTime).Null();

	private static readonly DbNullExpression _dateTimeOffsetNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.DateTimeOffset).Null();

	private static readonly DbNullExpression _decimalNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Decimal).Null();

	private static readonly DbNullExpression _doubleNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Double).Null();

	private static readonly DbNullExpression _geographyNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Geography).Null();

	private static readonly DbNullExpression _geometryNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Geometry).Null();

	private static readonly DbNullExpression _guidNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Guid).Null();

	private static readonly DbNullExpression _int16Null = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int16).Null();

	private static readonly DbNullExpression _int32Null = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int32).Null();

	private static readonly DbNullExpression _int64Null = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int64).Null();

	private static readonly DbNullExpression _sbyteNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.SByte).Null();

	private static readonly DbNullExpression _singleNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Single).Null();

	private static readonly DbNullExpression _stringNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.String).Null();

	private static readonly DbNullExpression _timeNull = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Time).Null();

	private static readonly DbConstantExpression _boolTrue = Constant(true);

	private static readonly DbConstantExpression _boolFalse = Constant(false);

	public static DbConstantExpression True => _boolTrue;

	public static DbConstantExpression False => _boolFalse;

	internal static AliasGenerator AliasGenerator => _bindingAliases;

	public static KeyValuePair<string, DbExpression> As(this DbExpression value, string alias)
	{
		return new KeyValuePair<string, DbExpression>(alias, value);
	}

	public static KeyValuePair<string, DbAggregate> As(this DbAggregate value, string alias)
	{
		return new KeyValuePair<string, DbAggregate>(alias, value);
	}

	public static DbExpressionBinding Bind(this DbExpression input)
	{
		Check.NotNull(input, "input");
		return input.BindAs(_bindingAliases.Next());
	}

	public static DbExpressionBinding BindAs(this DbExpression input, string varName)
	{
		Check.NotNull(input, "input");
		Check.NotNull(varName, "varName");
		Check.NotEmpty(varName, "varName");
		TypeUsage elementType = null;
		if (!TypeHelpers.TryGetCollectionElementType(input.ResultType, out elementType))
		{
			throw new ArgumentException(Strings.Cqt_Binding_CollectionRequired, "input");
		}
		DbVariableReferenceExpression varRef = new DbVariableReferenceExpression(elementType, varName);
		return new DbExpressionBinding(input, varRef);
	}

	public static DbGroupExpressionBinding GroupBind(this DbExpression input)
	{
		Check.NotNull(input, "input");
		string text = _bindingAliases.Next();
		return input.GroupBindAs(text, string.Format(CultureInfo.InvariantCulture, "Group{0}", new object[1] { text }));
	}

	public static DbGroupExpressionBinding GroupBindAs(this DbExpression input, string varName, string groupVarName)
	{
		Check.NotNull(input, "input");
		Check.NotNull(varName, "varName");
		Check.NotEmpty(varName, "varName");
		Check.NotNull(groupVarName, "groupVarName");
		Check.NotEmpty(groupVarName, "groupVarName");
		TypeUsage elementType = null;
		if (!TypeHelpers.TryGetCollectionElementType(input.ResultType, out elementType))
		{
			throw new ArgumentException(Strings.Cqt_GroupBinding_CollectionRequired, "input");
		}
		DbVariableReferenceExpression inputRef = new DbVariableReferenceExpression(elementType, varName);
		DbVariableReferenceExpression groupRef = new DbVariableReferenceExpression(elementType, groupVarName);
		return new DbGroupExpressionBinding(input, inputRef, groupRef);
	}

	public static DbFunctionAggregate Aggregate(this EdmFunction function, DbExpression argument)
	{
		Check.NotNull(function, "function");
		Check.NotNull(argument, "argument");
		return CreateFunctionAggregate(function, argument, isDistinct: false);
	}

	public static DbFunctionAggregate AggregateDistinct(this EdmFunction function, DbExpression argument)
	{
		Check.NotNull(function, "function");
		Check.NotNull(argument, "argument");
		return CreateFunctionAggregate(function, argument, isDistinct: true);
	}

	private static DbFunctionAggregate CreateFunctionAggregate(EdmFunction function, DbExpression argument, bool isDistinct)
	{
		DbExpressionList arguments = ArgumentValidation.ValidateFunctionAggregate(function, new DbExpression[1] { argument });
		return new DbFunctionAggregate(function.ReturnParameter.TypeUsage, arguments, function, isDistinct);
	}

	public static DbFunctionAggregate Aggregate(this EdmFunction function, IEnumerable<DbExpression> arguments)
	{
		Check.NotNull(function, "function");
		Check.NotNull(arguments, "argument");
		if (!arguments.Any())
		{
			throw new ArgumentNullException("arguments");
		}
		return CreateFunctionAggregate(function, arguments, isDistinct: false);
	}

	public static DbFunctionAggregate AggregateDistinct(this EdmFunction function, IEnumerable<DbExpression> arguments)
	{
		Check.NotNull(function, "function");
		Check.NotNull(arguments, "argument");
		if (!arguments.Any())
		{
			throw new ArgumentNullException("arguments");
		}
		return CreateFunctionAggregate(function, arguments, isDistinct: true);
	}

	private static DbFunctionAggregate CreateFunctionAggregate(EdmFunction function, IEnumerable<DbExpression> arguments, bool isDistinct)
	{
		DbExpressionList arguments2 = ArgumentValidation.ValidateFunctionAggregate(function, arguments);
		return new DbFunctionAggregate(function.ReturnParameter.TypeUsage, arguments2, function, isDistinct);
	}

	public static DbGroupAggregate GroupAggregate(DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		DbExpressionList arguments = new DbExpressionList(new DbExpression[1] { argument });
		return new DbGroupAggregate(TypeHelpers.CreateCollectionTypeUsage(argument.ResultType), arguments);
	}

	public static DbLambda Lambda(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
	{
		Check.NotNull(body, "body");
		Check.NotNull(variables, "variables");
		return CreateLambda(body, variables);
	}

	public static DbLambda Lambda(DbExpression body, params DbVariableReferenceExpression[] variables)
	{
		Check.NotNull(body, "body");
		Check.NotNull(variables, "variables");
		return CreateLambda(body, variables);
	}

	private static DbLambda CreateLambda(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
	{
		return new DbLambda(ArgumentValidation.ValidateLambda(variables), body);
	}

	public static DbSortClause ToSortClause(this DbExpression key)
	{
		Check.NotNull(key, "key");
		ArgumentValidation.ValidateSortClause(key);
		return new DbSortClause(key, asc: true, string.Empty);
	}

	public static DbSortClause ToSortClauseDescending(this DbExpression key)
	{
		Check.NotNull(key, "key");
		ArgumentValidation.ValidateSortClause(key);
		return new DbSortClause(key, asc: false, string.Empty);
	}

	public static DbSortClause ToSortClause(this DbExpression key, string collation)
	{
		Check.NotNull(key, "key");
		Check.NotNull(collation, "collation");
		ArgumentValidation.ValidateSortClause(key, collation);
		return new DbSortClause(key, asc: true, collation);
	}

	public static DbSortClause ToSortClauseDescending(this DbExpression key, string collation)
	{
		Check.NotNull(key, "key");
		Check.NotNull(collation, "collation");
		ArgumentValidation.ValidateSortClause(key, collation);
		return new DbSortClause(key, asc: false, collation);
	}

	public static DbQuantifierExpression All(this DbExpressionBinding input, DbExpression predicate)
	{
		Check.NotNull(predicate, "predicate");
		Check.NotNull(input, "input");
		TypeUsage booleanResultType = ArgumentValidation.ValidateQuantifier(predicate);
		return new DbQuantifierExpression(DbExpressionKind.All, booleanResultType, input, predicate);
	}

	public static DbQuantifierExpression Any(this DbExpressionBinding input, DbExpression predicate)
	{
		Check.NotNull(predicate, "predicate");
		Check.NotNull(input, "input");
		TypeUsage booleanResultType = ArgumentValidation.ValidateQuantifier(predicate);
		return new DbQuantifierExpression(DbExpressionKind.Any, booleanResultType, input, predicate);
	}

	public static DbApplyExpression CrossApply(this DbExpressionBinding input, DbExpressionBinding apply)
	{
		Check.NotNull(input, "input");
		Check.NotNull(apply, "apply");
		ValidateApply(input, apply);
		TypeUsage resultRowCollectionTypeUsage = CreateApplyResultType(input, apply);
		return new DbApplyExpression(DbExpressionKind.CrossApply, resultRowCollectionTypeUsage, input, apply);
	}

	public static DbApplyExpression OuterApply(this DbExpressionBinding input, DbExpressionBinding apply)
	{
		Check.NotNull(input, "input");
		Check.NotNull(apply, "apply");
		ValidateApply(input, apply);
		TypeUsage resultRowCollectionTypeUsage = CreateApplyResultType(input, apply);
		return new DbApplyExpression(DbExpressionKind.OuterApply, resultRowCollectionTypeUsage, input, apply);
	}

	private static void ValidateApply(DbExpressionBinding input, DbExpressionBinding apply)
	{
		if (input.VariableName.Equals(apply.VariableName, StringComparison.Ordinal))
		{
			throw new ArgumentException(Strings.Cqt_Apply_DuplicateVariableNames);
		}
	}

	private static TypeUsage CreateApplyResultType(DbExpressionBinding input, DbExpressionBinding apply)
	{
		return ArgumentValidation.CreateCollectionOfRowResultType(new List<KeyValuePair<string, TypeUsage>>
		{
			new KeyValuePair<string, TypeUsage>(input.VariableName, input.VariableType),
			new KeyValuePair<string, TypeUsage>(apply.VariableName, apply.VariableType)
		});
	}

	public static DbCrossJoinExpression CrossJoin(IEnumerable<DbExpressionBinding> inputs)
	{
		Check.NotNull(inputs, "inputs");
		TypeUsage resultType;
		ReadOnlyCollection<DbExpressionBinding> inputs2 = ArgumentValidation.ValidateCrossJoin(inputs, out resultType);
		return new DbCrossJoinExpression(resultType, inputs2);
	}

	public static DbJoinExpression InnerJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		TypeUsage collectionOfRowResultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
		return new DbJoinExpression(DbExpressionKind.InnerJoin, collectionOfRowResultType, left, right, joinCondition);
	}

	public static DbJoinExpression LeftOuterJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		TypeUsage collectionOfRowResultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
		return new DbJoinExpression(DbExpressionKind.LeftOuterJoin, collectionOfRowResultType, left, right, joinCondition);
	}

	public static DbJoinExpression FullOuterJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		TypeUsage collectionOfRowResultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
		return new DbJoinExpression(DbExpressionKind.FullOuterJoin, collectionOfRowResultType, left, right, joinCondition);
	}

	public static DbFilterExpression Filter(this DbExpressionBinding input, DbExpression predicate)
	{
		Check.NotNull(input, "input");
		Check.NotNull(predicate, "predicate");
		return new DbFilterExpression(ArgumentValidation.ValidateFilter(input, predicate), input, predicate);
	}

	public static DbGroupByExpression GroupBy(this DbGroupExpressionBinding input, IEnumerable<KeyValuePair<string, DbExpression>> keys, IEnumerable<KeyValuePair<string, DbAggregate>> aggregates)
	{
		Check.NotNull(input, "input");
		Check.NotNull(keys, "keys");
		Check.NotNull(aggregates, "aggregates");
		DbExpressionList validKeys;
		ReadOnlyCollection<DbAggregate> validAggregates;
		return new DbGroupByExpression(ArgumentValidation.ValidateGroupBy(keys, aggregates, out validKeys, out validAggregates), input, validKeys, validAggregates);
	}

	public static DbProjectExpression Project(this DbExpressionBinding input, DbExpression projection)
	{
		Check.NotNull(projection, "projection");
		Check.NotNull(input, "input");
		return new DbProjectExpression(CreateCollectionResultType(projection.ResultType), input, projection);
	}

	public static DbSkipExpression Skip(this DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder, DbExpression count)
	{
		Check.NotNull(input, "input");
		Check.NotNull(sortOrder, "sortOrder");
		Check.NotNull(count, "count");
		ReadOnlyCollection<DbSortClause> sortOrder2 = ArgumentValidation.ValidateSortArguments(sortOrder);
		if (!TypeSemantics.IsIntegerNumericType(count.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_Skip_IntegerRequired, "count");
		}
		if (count.ExpressionKind != DbExpressionKind.Constant && count.ExpressionKind != DbExpressionKind.ParameterReference)
		{
			throw new ArgumentException(Strings.Cqt_Skip_ConstantOrParameterRefRequired, "count");
		}
		if (IsConstantNegativeInteger(count))
		{
			throw new ArgumentException(Strings.Cqt_Skip_NonNegativeCountRequired, "count");
		}
		return new DbSkipExpression(input.Expression.ResultType, input, sortOrder2, count);
	}

	public static DbSortExpression Sort(this DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder)
	{
		Check.NotNull(input, "input");
		ReadOnlyCollection<DbSortClause> sortOrder2 = ArgumentValidation.ValidateSort(sortOrder);
		return new DbSortExpression(input.Expression.ResultType, input, sortOrder2);
	}

	public static DbNullExpression Null(this TypeUsage nullType)
	{
		Check.NotNull(nullType, "nullType");
		ArgumentValidation.CheckType(nullType, "nullType");
		return new DbNullExpression(nullType);
	}

	public static DbConstantExpression Constant(object value)
	{
		Check.NotNull(value, "value");
		return new DbConstantExpression(ArgumentValidation.ValidateConstant(value), value);
	}

	public static DbConstantExpression Constant(this TypeUsage constantType, object value)
	{
		Check.NotNull(constantType, "constantType");
		Check.NotNull(value, "value");
		ArgumentValidation.ValidateConstant(constantType, value);
		return new DbConstantExpression(constantType, value);
	}

	public static DbParameterReferenceExpression Parameter(this TypeUsage type, string name)
	{
		Check.NotNull(type, "type");
		Check.NotNull(name, "name");
		ArgumentValidation.CheckType(type);
		if (!DbCommandTree.IsValidParameterName(name))
		{
			throw new ArgumentException(Strings.Cqt_CommandTree_InvalidParameterName(name), "name");
		}
		return new DbParameterReferenceExpression(type, name);
	}

	public static DbVariableReferenceExpression Variable(this TypeUsage type, string name)
	{
		Check.NotNull(type, "type");
		Check.NotNull(name, "name");
		Check.NotEmpty(name, "name");
		ArgumentValidation.CheckType(type);
		return new DbVariableReferenceExpression(type, name);
	}

	public static DbScanExpression Scan(this EntitySetBase targetSet)
	{
		Check.NotNull(targetSet, "targetSet");
		ArgumentValidation.CheckEntitySet(targetSet, "targetSet");
		return new DbScanExpression(CreateCollectionResultType(targetSet.ElementType), targetSet);
	}

	public static DbAndExpression And(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
		if (commonTypeUsage == null || !TypeSemantics.IsPrimitiveType(commonTypeUsage, PrimitiveTypeKind.Boolean))
		{
			throw new ArgumentException(Strings.Cqt_And_BooleanArgumentsRequired);
		}
		return new DbAndExpression(commonTypeUsage, left, right);
	}

	public static DbOrExpression Or(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
		if (commonTypeUsage == null || !TypeSemantics.IsPrimitiveType(commonTypeUsage, PrimitiveTypeKind.Boolean))
		{
			throw new ArgumentException(Strings.Cqt_Or_BooleanArgumentsRequired);
		}
		return new DbOrExpression(commonTypeUsage, left, right);
	}

	public static DbInExpression In(this DbExpression expression, IList<DbConstantExpression> list)
	{
		Check.NotNull(expression, "expression");
		Check.NotNull(list, "list");
		List<DbExpression> list2 = new List<DbExpression>(list.Count);
		foreach (DbConstantExpression item in list)
		{
			if (!TypeSemantics.IsEqual(expression.ResultType, item.ResultType))
			{
				throw new ArgumentException(Strings.Cqt_In_SameResultTypeRequired);
			}
			list2.Add(item);
		}
		return CreateInExpression(expression, list2);
	}

	internal static DbInExpression CreateInExpression(DbExpression item, IList<DbExpression> list)
	{
		return new DbInExpression(_booleanType, item, new DbExpressionList(list));
	}

	public static DbNotExpression Not(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		if (!TypeSemantics.IsPrimitiveType(argument.ResultType, PrimitiveTypeKind.Boolean))
		{
			throw new ArgumentException(Strings.Cqt_Not_BooleanArgumentRequired);
		}
		return new DbNotExpression(argument.ResultType, argument);
	}

	private static DbArithmeticExpression CreateArithmetic(DbExpressionKind kind, DbExpression left, DbExpression right)
	{
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
		if (commonTypeUsage == null || !TypeSemantics.IsNumericType(commonTypeUsage))
		{
			throw new ArgumentException(Strings.Cqt_Arithmetic_NumericCommonType);
		}
		DbExpressionList args = new DbExpressionList(new DbExpression[2] { left, right });
		return new DbArithmeticExpression(kind, commonTypeUsage, args);
	}

	public static DbArithmeticExpression Divide(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateArithmetic(DbExpressionKind.Divide, left, right);
	}

	public static DbArithmeticExpression Minus(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateArithmetic(DbExpressionKind.Minus, left, right);
	}

	public static DbArithmeticExpression Modulo(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateArithmetic(DbExpressionKind.Modulo, left, right);
	}

	public static DbArithmeticExpression Multiply(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateArithmetic(DbExpressionKind.Multiply, left, right);
	}

	public static DbArithmeticExpression Plus(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateArithmetic(DbExpressionKind.Plus, left, right);
	}

	public static DbArithmeticExpression UnaryMinus(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		TypeUsage promotableType = argument.ResultType;
		if (!TypeSemantics.IsNumericType(promotableType))
		{
			throw new ArgumentException(Strings.Cqt_Arithmetic_NumericCommonType);
		}
		if (TypeSemantics.IsUnsignedNumericType(argument.ResultType))
		{
			promotableType = null;
			if (!TypeHelpers.TryGetClosestPromotableType(argument.ResultType, out promotableType))
			{
				throw new ArgumentException(Strings.Cqt_Arithmetic_InvalidUnsignedTypeForUnaryMinus(argument.ResultType.EdmType.FullName));
			}
		}
		return new DbArithmeticExpression(DbExpressionKind.UnaryMinus, promotableType, new DbExpressionList(new DbExpression[1] { argument }));
	}

	public static DbArithmeticExpression Negate(this DbExpression argument)
	{
		return argument.UnaryMinus();
	}

	private static DbComparisonExpression CreateComparison(DbExpressionKind kind, DbExpression left, DbExpression right)
	{
		bool flag = true;
		bool flag2 = true;
		if (DbExpressionKind.GreaterThanOrEquals == kind || DbExpressionKind.LessThanOrEquals == kind)
		{
			flag = TypeSemantics.IsEqualComparableTo(left.ResultType, right.ResultType);
			flag2 = TypeSemantics.IsOrderComparableTo(left.ResultType, right.ResultType);
		}
		else if (DbExpressionKind.Equals == kind || DbExpressionKind.NotEquals == kind)
		{
			flag = TypeSemantics.IsEqualComparableTo(left.ResultType, right.ResultType);
		}
		else
		{
			flag2 = TypeSemantics.IsOrderComparableTo(left.ResultType, right.ResultType);
		}
		if (!flag || !flag2)
		{
			throw new ArgumentException(Strings.Cqt_Comparison_ComparableRequired);
		}
		return new DbComparisonExpression(kind, _booleanType, left, right);
	}

	public static DbComparisonExpression Equal(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.Equals, left, right);
	}

	public static DbComparisonExpression NotEqual(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.NotEquals, left, right);
	}

	public static DbComparisonExpression GreaterThan(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.GreaterThan, left, right);
	}

	public static DbComparisonExpression LessThan(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.LessThan, left, right);
	}

	public static DbComparisonExpression GreaterThanOrEqual(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.GreaterThanOrEquals, left, right);
	}

	public static DbComparisonExpression LessThanOrEqual(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return CreateComparison(DbExpressionKind.LessThanOrEquals, left, right);
	}

	public static DbIsNullExpression IsNull(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		ValidateIsNull(argument);
		return new DbIsNullExpression(_booleanType, argument);
	}

	private static void ValidateIsNull(DbExpression argument)
	{
		if (TypeSemantics.IsCollectionType(argument.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_IsNull_CollectionNotAllowed);
		}
		if (!TypeHelpers.IsValidIsNullOpType(argument.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_IsNull_InvalidType);
		}
	}

	public static DbLikeExpression Like(this DbExpression argument, DbExpression pattern)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(pattern, "pattern");
		ValidateLike(argument, pattern);
		DbExpression escape = pattern.ResultType.Null();
		return new DbLikeExpression(_booleanType, argument, pattern, escape);
	}

	public static DbLikeExpression Like(this DbExpression argument, DbExpression pattern, DbExpression escape)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(pattern, "pattern");
		Check.NotNull(escape, "escape");
		ValidateLike(argument, pattern, escape);
		return new DbLikeExpression(_booleanType, argument, pattern, escape);
	}

	private static void ValidateLike(DbExpression argument, DbExpression pattern, DbExpression escape)
	{
		ValidateLike(argument, pattern);
		ArgumentValidation.RequireCompatibleType(escape, PrimitiveTypeKind.String, "escape");
	}

	private static void ValidateLike(DbExpression argument, DbExpression pattern)
	{
		ArgumentValidation.RequireCompatibleType(argument, PrimitiveTypeKind.String, "argument");
		ArgumentValidation.RequireCompatibleType(pattern, PrimitiveTypeKind.String, "pattern");
	}

	public static DbCastExpression CastTo(this DbExpression argument, TypeUsage toType)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(toType, "toType");
		ArgumentValidation.CheckType(toType, "toType");
		if (!TypeSemantics.IsCastAllowed(argument.ResultType, toType))
		{
			throw new ArgumentException(Strings.Cqt_Cast_InvalidCast(argument.ResultType.ToString(), toType.ToString()));
		}
		return new DbCastExpression(toType, argument);
	}

	public static DbTreatExpression TreatAs(this DbExpression argument, TypeUsage treatType)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(treatType, "treatType");
		ArgumentValidation.CheckType(treatType, "treatType");
		ArgumentValidation.RequirePolymorphicType(treatType);
		if (!TypeSemantics.IsValidPolymorphicCast(argument.ResultType, treatType))
		{
			throw new ArgumentException(Strings.Cqt_General_PolymorphicArgRequired(typeof(DbTreatExpression).Name));
		}
		return new DbTreatExpression(treatType, argument);
	}

	public static DbOfTypeExpression OfType(this DbExpression argument, TypeUsage type)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(type, "type");
		ValidateOfType(argument, type);
		TypeUsage collectionResultType = CreateCollectionResultType(type);
		return new DbOfTypeExpression(DbExpressionKind.OfType, collectionResultType, argument, type);
	}

	public static DbOfTypeExpression OfTypeOnly(this DbExpression argument, TypeUsage type)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(type, "type");
		ValidateOfType(argument, type);
		TypeUsage collectionResultType = CreateCollectionResultType(type);
		return new DbOfTypeExpression(DbExpressionKind.OfTypeOnly, collectionResultType, argument, type);
	}

	public static DbIsOfExpression IsOf(this DbExpression argument, TypeUsage type)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(type, "type");
		ValidateIsOf(argument, type);
		return new DbIsOfExpression(DbExpressionKind.IsOf, _booleanType, argument, type);
	}

	public static DbIsOfExpression IsOfOnly(this DbExpression argument, TypeUsage type)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(type, "type");
		ValidateIsOf(argument, type);
		return new DbIsOfExpression(DbExpressionKind.IsOfOnly, _booleanType, argument, type);
	}

	private static void ValidateOfType(DbExpression argument, TypeUsage type)
	{
		ArgumentValidation.CheckType(type, "type");
		ArgumentValidation.RequirePolymorphicType(type);
		ArgumentValidation.RequireCollectionArgument<DbOfTypeExpression>(argument);
		TypeUsage elementType = null;
		if (!TypeHelpers.TryGetCollectionElementType(argument.ResultType, out elementType) || !TypeSemantics.IsValidPolymorphicCast(elementType, type))
		{
			throw new ArgumentException(Strings.Cqt_General_PolymorphicArgRequired(typeof(DbOfTypeExpression).Name));
		}
	}

	private static void ValidateIsOf(DbExpression argument, TypeUsage type)
	{
		ArgumentValidation.CheckType(type, "type");
		ArgumentValidation.RequirePolymorphicType(type);
		if (!TypeSemantics.IsValidPolymorphicCast(argument.ResultType, type))
		{
			throw new ArgumentException(Strings.Cqt_General_PolymorphicArgRequired(typeof(DbIsOfExpression).Name));
		}
	}

	public static DbDerefExpression Deref(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		if (!TypeHelpers.TryGetRefEntityType(argument.ResultType, out var referencedEntityType))
		{
			throw new ArgumentException(Strings.Cqt_DeRef_RefRequired, "argument");
		}
		return new DbDerefExpression(TypeUsage.Create(referencedEntityType), argument);
	}

	public static DbEntityRefExpression GetEntityRef(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		EntityType type = null;
		if (!TypeHelpers.TryGetEdmType<EntityType>(argument.ResultType, out type))
		{
			throw new ArgumentException(Strings.Cqt_GetEntityRef_EntityRequired, "argument");
		}
		return new DbEntityRefExpression(ArgumentValidation.CreateReferenceResultType(type), argument);
	}

	public static DbRefExpression CreateRef(this EntitySet entitySet, IEnumerable<DbExpression> keyValues)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(keyValues, "keyValues");
		return CreateRefExpression(entitySet, keyValues);
	}

	public static DbRefExpression CreateRef(this EntitySet entitySet, params DbExpression[] keyValues)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(keyValues, "keyValues");
		return CreateRefExpression(entitySet, keyValues);
	}

	public static DbRefExpression CreateRef(this EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(entityType, "entityType");
		Check.NotNull(keyValues, "keyValues");
		return CreateRefExpression(entitySet, entityType, keyValues);
	}

	public static DbRefExpression CreateRef(this EntitySet entitySet, EntityType entityType, params DbExpression[] keyValues)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(entityType, "entityType");
		Check.NotNull(keyValues, "keyValues");
		return CreateRefExpression(entitySet, entityType, keyValues);
	}

	private static DbRefExpression CreateRefExpression(EntitySet entitySet, IEnumerable<DbExpression> keyValues)
	{
		DbExpression keyConstructor;
		return new DbRefExpression(ArgumentValidation.ValidateCreateRef(entitySet, entitySet.ElementType, keyValues, out keyConstructor), entitySet, keyConstructor);
	}

	private static DbRefExpression CreateRefExpression(EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(entityType, "entityType");
		DbExpression keyConstructor;
		return new DbRefExpression(ArgumentValidation.ValidateCreateRef(entitySet, entityType, keyValues, out keyConstructor), entitySet, keyConstructor);
	}

	public static DbRefExpression RefFromKey(this EntitySet entitySet, DbExpression keyRow)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(keyRow, "keyRow");
		return new DbRefExpression(ArgumentValidation.ValidateRefFromKey(entitySet, keyRow, entitySet.ElementType), entitySet, keyRow);
	}

	public static DbRefExpression RefFromKey(this EntitySet entitySet, DbExpression keyRow, EntityType entityType)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(keyRow, "keyRow");
		Check.NotNull(entityType, "entityType");
		return new DbRefExpression(ArgumentValidation.ValidateRefFromKey(entitySet, keyRow, entityType), entitySet, keyRow);
	}

	public static DbRefKeyExpression GetRefKey(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		RefType type = null;
		if (!TypeHelpers.TryGetEdmType<RefType>(argument.ResultType, out type))
		{
			throw new ArgumentException(Strings.Cqt_GetRefKey_RefRequired, "argument");
		}
		return new DbRefKeyExpression(TypeUsage.Create(TypeHelpers.CreateKeyRowType(type.ElementType)), argument);
	}

	public static DbRelationshipNavigationExpression Navigate(this DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
	{
		Check.NotNull(navigateFrom, "navigateFrom");
		Check.NotNull(fromEnd, "fromEnd");
		Check.NotNull(toEnd, "toEnd");
		RelationshipType relType;
		return new DbRelationshipNavigationExpression(ArgumentValidation.ValidateNavigate(navigateFrom, fromEnd, toEnd, out relType, allowAllRelationshipsInSameTypeHierarchy: false), relType, fromEnd, toEnd, navigateFrom);
	}

	public static DbRelationshipNavigationExpression Navigate(this RelationshipType type, string fromEndName, string toEndName, DbExpression navigateFrom)
	{
		Check.NotNull(type, "type");
		Check.NotNull(fromEndName, "fromEndName");
		Check.NotNull(toEndName, "toEndName");
		Check.NotNull(navigateFrom, "navigateFrom");
		RelationshipEndMember fromEnd;
		RelationshipEndMember toEnd;
		return new DbRelationshipNavigationExpression(ArgumentValidation.ValidateNavigate(navigateFrom, type, fromEndName, toEndName, out fromEnd, out toEnd), type, fromEnd, toEnd, navigateFrom);
	}

	public static DbDistinctExpression Distinct(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		ArgumentValidation.RequireCollectionArgument<DbDistinctExpression>(argument);
		if (!TypeHelpers.IsValidDistinctOpType(TypeHelpers.GetEdmType<CollectionType>(argument.ResultType).TypeUsage))
		{
			throw new ArgumentException(Strings.Cqt_Distinct_InvalidCollection, "argument");
		}
		return new DbDistinctExpression(argument.ResultType, argument);
	}

	public static DbElementExpression Element(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		return new DbElementExpression(ArgumentValidation.ValidateElement(argument), argument);
	}

	public static DbIsEmptyExpression IsEmpty(this DbExpression argument)
	{
		Check.NotNull(argument, "argument");
		ArgumentValidation.RequireCollectionArgument<DbIsEmptyExpression>(argument);
		return new DbIsEmptyExpression(_booleanType, argument);
	}

	public static DbExceptExpression Except(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		ArgumentValidation.RequireComparableCollectionArguments<DbExceptExpression>(left, right);
		return new DbExceptExpression(left.ResultType, left, right);
	}

	public static DbIntersectExpression Intersect(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return new DbIntersectExpression(ArgumentValidation.RequireComparableCollectionArguments<DbIntersectExpression>(left, right), left, right);
	}

	public static DbUnionAllExpression UnionAll(this DbExpression left, DbExpression right)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		return new DbUnionAllExpression(ArgumentValidation.RequireCollectionArguments<DbUnionAllExpression>(left, right), left, right);
	}

	public static DbLimitExpression Limit(this DbExpression argument, DbExpression count)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(count, "count");
		ArgumentValidation.RequireCollectionArgument<DbLimitExpression>(argument);
		if (!TypeSemantics.IsIntegerNumericType(count.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_Limit_IntegerRequired, "count");
		}
		if (count.ExpressionKind != DbExpressionKind.Constant && count.ExpressionKind != DbExpressionKind.ParameterReference)
		{
			throw new ArgumentException(Strings.Cqt_Limit_ConstantOrParameterRefRequired, "count");
		}
		if (IsConstantNegativeInteger(count))
		{
			throw new ArgumentException(Strings.Cqt_Limit_NonNegativeLimitRequired, "count");
		}
		return new DbLimitExpression(argument.ResultType, argument, count, withTies: false);
	}

	public static DbCaseExpression Case(IEnumerable<DbExpression> whenExpressions, IEnumerable<DbExpression> thenExpressions, DbExpression elseExpression)
	{
		Check.NotNull(whenExpressions, "whenExpressions");
		Check.NotNull(thenExpressions, "thenExpressions");
		Check.NotNull(elseExpression, "elseExpression");
		DbExpressionList validWhens;
		DbExpressionList validThens;
		return new DbCaseExpression(ArgumentValidation.ValidateCase(whenExpressions, thenExpressions, elseExpression, out validWhens, out validThens), validWhens, validThens, elseExpression);
	}

	public static DbFunctionExpression Invoke(this EdmFunction function, IEnumerable<DbExpression> arguments)
	{
		Check.NotNull(function, "function");
		return InvokeFunction(function, arguments);
	}

	public static DbFunctionExpression Invoke(this EdmFunction function, params DbExpression[] arguments)
	{
		Check.NotNull(function, "function");
		return InvokeFunction(function, arguments);
	}

	private static DbFunctionExpression InvokeFunction(EdmFunction function, IEnumerable<DbExpression> arguments)
	{
		DbExpressionList validArgs;
		return new DbFunctionExpression(ArgumentValidation.ValidateFunction(function, arguments, out validArgs), function, validArgs);
	}

	public static DbLambdaExpression Invoke(this DbLambda lambda, IEnumerable<DbExpression> arguments)
	{
		Check.NotNull(lambda, "lambda");
		Check.NotNull(arguments, "arguments");
		return InvokeLambda(lambda, arguments);
	}

	public static DbLambdaExpression Invoke(this DbLambda lambda, params DbExpression[] arguments)
	{
		Check.NotNull(lambda, "lambda");
		Check.NotNull(arguments, "arguments");
		return InvokeLambda(lambda, arguments);
	}

	private static DbLambdaExpression InvokeLambda(DbLambda lambda, IEnumerable<DbExpression> arguments)
	{
		DbExpressionList validArguments;
		return new DbLambdaExpression(ArgumentValidation.ValidateInvoke(lambda, arguments, out validArguments), lambda, validArguments);
	}

	public static DbNewInstanceExpression New(this TypeUsage instanceType, IEnumerable<DbExpression> arguments)
	{
		Check.NotNull(instanceType, "instanceType");
		return NewInstance(instanceType, arguments);
	}

	public static DbNewInstanceExpression New(this TypeUsage instanceType, params DbExpression[] arguments)
	{
		Check.NotNull(instanceType, "instanceType");
		return NewInstance(instanceType, arguments);
	}

	private static DbNewInstanceExpression NewInstance(TypeUsage instanceType, IEnumerable<DbExpression> arguments)
	{
		DbExpressionList validArguments;
		return new DbNewInstanceExpression(ArgumentValidation.ValidateNew(instanceType, arguments, out validArguments), validArguments);
	}

	public static DbNewInstanceExpression NewCollection(IEnumerable<DbExpression> elements)
	{
		return CreateNewCollection(elements);
	}

	public static DbNewInstanceExpression NewCollection(params DbExpression[] elements)
	{
		Check.NotNull(elements, "elements");
		return CreateNewCollection(elements);
	}

	private static DbNewInstanceExpression CreateNewCollection(IEnumerable<DbExpression> elements)
	{
		TypeUsage commonElementType = null;
		DbExpressionList args = ArgumentValidation.CreateExpressionList(elements, "elements", delegate(DbExpression exp, int idx)
		{
			if (commonElementType == null)
			{
				commonElementType = exp.ResultType;
			}
			else
			{
				commonElementType = TypeSemantics.GetCommonType(commonElementType, exp.ResultType);
			}
			if (commonElementType == null)
			{
				throw new ArgumentException(Strings.Cqt_Factory_NewCollectionInvalidCommonType, "collectionElements");
			}
		});
		return new DbNewInstanceExpression(CreateCollectionResultType(commonElementType), args);
	}

	public static DbNewInstanceExpression NewEmptyCollection(this TypeUsage collectionType)
	{
		Check.NotNull(collectionType, "collectionType");
		DbExpressionList validElements;
		return new DbNewInstanceExpression(ArgumentValidation.ValidateNewEmptyCollection(collectionType, out validElements), validElements);
	}

	public static DbNewInstanceExpression NewRow(IEnumerable<KeyValuePair<string, DbExpression>> columnValues)
	{
		Check.NotNull(columnValues, "columnValues");
		DbExpressionList validElements;
		return new DbNewInstanceExpression(ArgumentValidation.ValidateNewRow(columnValues, out validElements), validElements);
	}

	public static DbPropertyExpression Property(this DbExpression instance, EdmProperty propertyMetadata)
	{
		Check.NotNull(instance, "instance");
		Check.NotNull(propertyMetadata, "propertyMetadata");
		return PropertyFromMember(instance, propertyMetadata, "propertyMetadata");
	}

	public static DbPropertyExpression Property(this DbExpression instance, NavigationProperty navigationProperty)
	{
		Check.NotNull(instance, "instance");
		Check.NotNull(navigationProperty, "navigationProperty");
		return PropertyFromMember(instance, navigationProperty, "navigationProperty");
	}

	public static DbPropertyExpression Property(this DbExpression instance, RelationshipEndMember relationshipEnd)
	{
		Check.NotNull(instance, "instance");
		Check.NotNull(relationshipEnd, "relationshipEnd");
		return PropertyFromMember(instance, relationshipEnd, "relationshipEnd");
	}

	public static DbPropertyExpression Property(this DbExpression instance, string propertyName)
	{
		return PropertyByName(instance, propertyName, ignoreCase: false);
	}

	private static DbPropertyExpression PropertyFromMember(DbExpression instance, EdmMember property, string propertyArgumentName)
	{
		ArgumentValidation.CheckMember(property, propertyArgumentName);
		if (instance == null)
		{
			throw new ArgumentException(Strings.Cqt_Property_InstanceRequiredForInstance, "instance");
		}
		TypeUsage requiredResultType = TypeUsage.Create(property.DeclaringType);
		ArgumentValidation.RequireCompatibleType(instance, requiredResultType, "instance");
		return new DbPropertyExpression(Helper.GetModelTypeUsage(property), property, instance);
	}

	private static DbPropertyExpression PropertyByName(DbExpression instance, string propertyName, bool ignoreCase)
	{
		Check.NotNull(instance, "instance");
		Check.NotNull(propertyName, "propertyName");
		EdmMember foundMember;
		return new DbPropertyExpression(ArgumentValidation.ValidateProperty(instance, propertyName, ignoreCase, out foundMember), foundMember, instance);
	}

	public static DbSetClause SetClause(DbExpression property, DbExpression value)
	{
		Check.NotNull(property, "property");
		Check.NotNull(value, "value");
		return new DbSetClause(property, value);
	}

	private static string ExtractAlias(MethodInfo method)
	{
		return ExtractAliases(method)[0];
	}

	internal static string[] ExtractAliases(MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters();
		int num;
		int num2;
		if (method.IsStatic && "System.Runtime.CompilerServices.Closure" == parameters[0].ParameterType.FullName)
		{
			num = 1;
			num2 = parameters.Length - 1;
		}
		else
		{
			num = 0;
			num2 = parameters.Length;
		}
		string[] array = new string[num2];
		bool flag = parameters.Skip(num).Any((ParameterInfo p) => p.Name == null);
		for (int i = num; i < parameters.Length; i++)
		{
			array[i - num] = (flag ? _bindingAliases.Next() : parameters[i].Name);
		}
		return array;
	}

	private static DbExpressionBinding ConvertToBinding<TResult>(DbExpression source, Func<DbExpression, TResult> argument, out TResult argumentResult)
	{
		string varName = ExtractAlias(argument.Method);
		DbExpressionBinding dbExpressionBinding = source.BindAs(varName);
		argumentResult = argument(dbExpressionBinding.Variable);
		return dbExpressionBinding;
	}

	private static DbExpressionBinding[] ConvertToBinding(DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> argument, out DbExpression argumentExp)
	{
		string[] array = ExtractAliases(argument.Method);
		DbExpressionBinding dbExpressionBinding = left.BindAs(array[0]);
		DbExpressionBinding dbExpressionBinding2 = right.BindAs(array[1]);
		argumentExp = argument(dbExpressionBinding.Variable, dbExpressionBinding2.Variable);
		return new DbExpressionBinding[2] { dbExpressionBinding, dbExpressionBinding2 };
	}

	internal static List<KeyValuePair<string, TRequired>> TryGetAnonymousTypeValues<TInstance, TRequired>(object instance)
	{
		IEnumerable<PropertyInfo> instanceProperties = typeof(TInstance).GetInstanceProperties();
		if (typeof(TInstance).BaseType() != typeof(object) || instanceProperties.Any((PropertyInfo p) => !p.IsPublic()))
		{
			return null;
		}
		List<KeyValuePair<string, TRequired>> list = null;
		foreach (PropertyInfo item in instanceProperties.Where((PropertyInfo p) => p.IsPublic()))
		{
			if (item.CanRead && typeof(TRequired).IsAssignableFrom(item.PropertyType))
			{
				if (list == null)
				{
					list = new List<KeyValuePair<string, TRequired>>();
				}
				list.Add(new KeyValuePair<string, TRequired>(item.Name, (TRequired)item.GetValue(instance, null)));
				continue;
			}
			return null;
		}
		return list;
	}

	private static bool TryResolveToConstant(Type type, object value, out DbExpression constantOrNullExpression)
	{
		constantOrNullExpression = null;
		Type clrType = type;
		if (type.IsGenericType() && typeof(Nullable<>).Equals(type.GetGenericTypeDefinition()))
		{
			clrType = type.GetGenericArguments()[0];
		}
		if (ClrProviderManifest.TryGetPrimitiveTypeKind(clrType, out var resolvedPrimitiveTypeKind))
		{
			TypeUsage literalTypeUsage = TypeHelpers.GetLiteralTypeUsage(resolvedPrimitiveTypeKind);
			if (value == null)
			{
				constantOrNullExpression = literalTypeUsage.Null();
			}
			else
			{
				constantOrNullExpression = literalTypeUsage.Constant(value);
			}
		}
		return constantOrNullExpression != null;
	}

	private static DbExpression ResolveToExpression<TArgument>(TArgument argument)
	{
		object obj = argument;
		if (TryResolveToConstant(typeof(TArgument), obj, out var constantOrNullExpression))
		{
			return constantOrNullExpression;
		}
		if (obj == null)
		{
			return null;
		}
		if (typeof(DbExpression).IsAssignableFrom(typeof(TArgument)))
		{
			return (DbExpression)obj;
		}
		if (typeof(Row).Equals(typeof(TArgument)))
		{
			return ((Row)obj).ToExpression();
		}
		List<KeyValuePair<string, DbExpression>> list = TryGetAnonymousTypeValues<TArgument, DbExpression>(obj);
		if (list != null)
		{
			return NewRow(list);
		}
		throw new NotSupportedException(Strings.Cqt_Factory_MethodResultTypeNotSupported(typeof(TArgument).FullName));
	}

	private static DbApplyExpression CreateApply(DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply, Func<DbExpressionBinding, DbExpressionBinding, DbApplyExpression> resultBuilder)
	{
		KeyValuePair<string, DbExpression> argumentResult;
		DbExpressionBinding arg = ConvertToBinding(source, apply, out argumentResult);
		DbExpressionBinding arg2 = argumentResult.Value.BindAs(argumentResult.Key);
		return resultBuilder(arg, arg2);
	}

	public static DbQuantifierExpression All(this DbExpression source, Func<DbExpression, DbExpression> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		DbExpression argumentResult;
		return ConvertToBinding(source, predicate, out argumentResult).All(argumentResult);
	}

	public static DbExpression Any(this DbExpression source)
	{
		return source.Exists();
	}

	public static DbExpression Exists(this DbExpression argument)
	{
		return argument.IsEmpty().Not();
	}

	public static DbQuantifierExpression Any(this DbExpression source, Func<DbExpression, DbExpression> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		DbExpression argumentResult;
		return ConvertToBinding(source, predicate, out argumentResult).Any(argumentResult);
	}

	public static DbApplyExpression CrossApply(this DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply)
	{
		Check.NotNull(source, "source");
		Check.NotNull(apply, "apply");
		return CreateApply(source, apply, CrossApply);
	}

	public static DbApplyExpression OuterApply(this DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply)
	{
		Check.NotNull(source, "source");
		Check.NotNull(apply, "apply");
		return CreateApply(source, apply, OuterApply);
	}

	public static DbJoinExpression FullOuterJoin(this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		DbExpression argumentExp;
		DbExpressionBinding[] array = ConvertToBinding(left, right, joinCondition, out argumentExp);
		return array[0].FullOuterJoin(array[1], argumentExp);
	}

	public static DbJoinExpression InnerJoin(this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		DbExpression argumentExp;
		DbExpressionBinding[] array = ConvertToBinding(left, right, joinCondition, out argumentExp);
		return array[0].InnerJoin(array[1], argumentExp);
	}

	public static DbJoinExpression LeftOuterJoin(this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
	{
		Check.NotNull(left, "left");
		Check.NotNull(right, "right");
		Check.NotNull(joinCondition, "joinCondition");
		DbExpression argumentExp;
		DbExpressionBinding[] array = ConvertToBinding(left, right, joinCondition, out argumentExp);
		return array[0].LeftOuterJoin(array[1], argumentExp);
	}

	public static DbJoinExpression Join(this DbExpression outer, DbExpression inner, Func<DbExpression, DbExpression> outerKey, Func<DbExpression, DbExpression> innerKey)
	{
		Check.NotNull(outer, "outer");
		Check.NotNull(inner, "inner");
		Check.NotNull(outerKey, "outerKey");
		Check.NotNull(innerKey, "innerKey");
		DbExpression argumentResult;
		DbExpressionBinding left = ConvertToBinding(outer, outerKey, out argumentResult);
		DbExpression argumentResult2;
		DbExpressionBinding right = ConvertToBinding(inner, innerKey, out argumentResult2);
		DbExpression joinCondition = argumentResult.Equal(argumentResult2);
		return left.InnerJoin(right, joinCondition);
	}

	public static DbProjectExpression Join<TSelector>(this DbExpression outer, DbExpression inner, Func<DbExpression, DbExpression> outerKey, Func<DbExpression, DbExpression> innerKey, Func<DbExpression, DbExpression, TSelector> selector)
	{
		Check.NotNull(selector, "selector");
		DbJoinExpression dbJoinExpression = outer.Join(inner, outerKey, innerKey);
		DbExpressionBinding dbExpressionBinding = dbJoinExpression.Bind();
		DbExpression arg = dbExpressionBinding.Variable.Property(dbJoinExpression.Left.VariableName);
		DbExpression arg2 = dbExpressionBinding.Variable.Property(dbJoinExpression.Right.VariableName);
		DbExpression projection = ResolveToExpression(selector(arg, arg2));
		return dbExpressionBinding.Project(projection);
	}

	public static DbSortExpression OrderBy(this DbExpression source, Func<DbExpression, DbExpression> sortKey)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		DbExpression argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, sortKey, out argumentResult);
		DbSortClause dbSortClause = argumentResult.ToSortClause();
		return input.Sort(new DbSortClause[1] { dbSortClause });
	}

	public static DbSortExpression OrderBy(this DbExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		DbExpression argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, sortKey, out argumentResult);
		DbSortClause dbSortClause = argumentResult.ToSortClause(collation);
		return input.Sort(new DbSortClause[1] { dbSortClause });
	}

	public static DbSortExpression OrderByDescending(this DbExpression source, Func<DbExpression, DbExpression> sortKey)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		DbExpression argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, sortKey, out argumentResult);
		DbSortClause dbSortClause = argumentResult.ToSortClauseDescending();
		return input.Sort(new DbSortClause[1] { dbSortClause });
	}

	public static DbSortExpression OrderByDescending(this DbExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		DbExpression argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, sortKey, out argumentResult);
		DbSortClause dbSortClause = argumentResult.ToSortClauseDescending(collation);
		return input.Sort(new DbSortClause[1] { dbSortClause });
	}

	public static DbProjectExpression Select<TProjection>(this DbExpression source, Func<DbExpression, TProjection> projection)
	{
		Check.NotNull(source, "source");
		Check.NotNull(projection, "projection");
		TProjection argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, projection, out argumentResult);
		DbExpression projection2 = ResolveToExpression(argumentResult);
		return input.Project(projection2);
	}

	public static DbProjectExpression SelectMany(this DbExpression source, Func<DbExpression, DbExpression> apply)
	{
		Check.NotNull(source, "source");
		Check.NotNull(apply, "apply");
		DbExpression argumentResult;
		DbExpressionBinding input = ConvertToBinding(source, apply, out argumentResult);
		DbExpressionBinding dbExpressionBinding = argumentResult.Bind();
		DbExpressionBinding dbExpressionBinding2 = input.CrossApply(dbExpressionBinding).Bind();
		return dbExpressionBinding2.Project(dbExpressionBinding2.Variable.Property(dbExpressionBinding.VariableName));
	}

	public static DbProjectExpression SelectMany<TSelector>(this DbExpression source, Func<DbExpression, DbExpression> apply, Func<DbExpression, DbExpression, TSelector> selector)
	{
		Check.NotNull(source, "source");
		Check.NotNull(apply, "apply");
		Check.NotNull(selector, "selector");
		DbExpression argumentResult;
		DbExpressionBinding dbExpressionBinding = ConvertToBinding(source, apply, out argumentResult);
		DbExpressionBinding dbExpressionBinding2 = argumentResult.Bind();
		DbExpressionBinding dbExpressionBinding3 = dbExpressionBinding.CrossApply(dbExpressionBinding2).Bind();
		DbExpression arg = dbExpressionBinding3.Variable.Property(dbExpressionBinding.VariableName);
		DbExpression arg2 = dbExpressionBinding3.Variable.Property(dbExpressionBinding2.VariableName);
		DbExpression projection = ResolveToExpression(selector(arg, arg2));
		return dbExpressionBinding3.Project(projection);
	}

	public static DbSkipExpression Skip(this DbSortExpression argument, DbExpression count)
	{
		Check.NotNull(argument, "argument");
		return argument.Input.Skip(argument.SortOrder, count);
	}

	public static DbLimitExpression Take(this DbExpression argument, DbExpression count)
	{
		Check.NotNull(argument, "argument");
		Check.NotNull(count, "count");
		return argument.Limit(count);
	}

	private static DbSortExpression CreateThenBy(DbSortExpression source, Func<DbExpression, DbExpression> sortKey, bool ascending, string collation, bool useCollation)
	{
		DbExpression key = sortKey(source.Input.Variable);
		DbSortClause item = ((!useCollation) ? (ascending ? key.ToSortClause() : key.ToSortClauseDescending()) : (ascending ? key.ToSortClause(collation) : key.ToSortClauseDescending(collation)));
		List<DbSortClause> list = new List<DbSortClause>(source.SortOrder.Count + 1);
		list.AddRange(source.SortOrder);
		list.Add(item);
		return source.Input.Sort(list);
	}

	public static DbSortExpression ThenBy(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		return CreateThenBy(source, sortKey, ascending: true, null, useCollation: false);
	}

	public static DbSortExpression ThenBy(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		return CreateThenBy(source, sortKey, ascending: true, collation, useCollation: true);
	}

	public static DbSortExpression ThenByDescending(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		return CreateThenBy(source, sortKey, ascending: false, null, useCollation: false);
	}

	public static DbSortExpression ThenByDescending(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
	{
		Check.NotNull(source, "source");
		Check.NotNull(sortKey, "sortKey");
		return CreateThenBy(source, sortKey, ascending: false, collation, useCollation: true);
	}

	public static DbFilterExpression Where(this DbExpression source, Func<DbExpression, DbExpression> predicate)
	{
		Check.NotNull(source, "source");
		Check.NotNull(predicate, "predicate");
		DbExpression argumentResult;
		return ConvertToBinding(source, predicate, out argumentResult).Filter(argumentResult);
	}

	public static DbExpression Union(this DbExpression left, DbExpression right)
	{
		return left.UnionAll(right).Distinct();
	}

	internal static DbNullExpression CreatePrimitiveNullExpression(PrimitiveTypeKind primitiveType)
	{
		switch (primitiveType)
		{
		case PrimitiveTypeKind.Binary:
			return _binaryNull;
		case PrimitiveTypeKind.Boolean:
			return _boolNull;
		case PrimitiveTypeKind.Byte:
			return _byteNull;
		case PrimitiveTypeKind.DateTime:
			return _dateTimeNull;
		case PrimitiveTypeKind.DateTimeOffset:
			return _dateTimeOffsetNull;
		case PrimitiveTypeKind.Decimal:
			return _decimalNull;
		case PrimitiveTypeKind.Double:
			return _doubleNull;
		case PrimitiveTypeKind.Geography:
			return _geographyNull;
		case PrimitiveTypeKind.Geometry:
			return _geometryNull;
		case PrimitiveTypeKind.Guid:
			return _guidNull;
		case PrimitiveTypeKind.Int16:
			return _int16Null;
		case PrimitiveTypeKind.Int32:
			return _int32Null;
		case PrimitiveTypeKind.Int64:
			return _int64Null;
		case PrimitiveTypeKind.SByte:
			return _sbyteNull;
		case PrimitiveTypeKind.Single:
			return _singleNull;
		case PrimitiveTypeKind.String:
			return _stringNull;
		case PrimitiveTypeKind.Time:
			return _timeNull;
		default:
		{
			string name = typeof(PrimitiveTypeKind).Name;
			int num = (int)primitiveType;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name, num.ToString(CultureInfo.InvariantCulture)));
		}
		}
	}

	internal static DbApplyExpression CreateApplyExpressionByKind(DbExpressionKind applyKind, DbExpressionBinding input, DbExpressionBinding apply)
	{
		switch (applyKind)
		{
		case DbExpressionKind.CrossApply:
			return input.CrossApply(apply);
		case DbExpressionKind.OuterApply:
			return input.OuterApply(apply);
		default:
		{
			string name = typeof(DbExpressionKind).Name;
			int num = (int)applyKind;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name, num.ToString(CultureInfo.InvariantCulture)));
		}
		}
	}

	internal static DbExpression CreateJoinExpressionByKind(DbExpressionKind joinKind, DbExpression joinCondition, DbExpressionBinding input1, DbExpressionBinding input2)
	{
		if (DbExpressionKind.CrossJoin == joinKind)
		{
			return CrossJoin(new DbExpressionBinding[2] { input1, input2 });
		}
		switch (joinKind)
		{
		case DbExpressionKind.InnerJoin:
			return input1.InnerJoin(input2, joinCondition);
		case DbExpressionKind.LeftOuterJoin:
			return input1.LeftOuterJoin(input2, joinCondition);
		case DbExpressionKind.FullOuterJoin:
			return input1.FullOuterJoin(input2, joinCondition);
		default:
		{
			string name = typeof(DbExpressionKind).Name;
			int num = (int)joinKind;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name, num.ToString(CultureInfo.InvariantCulture)));
		}
		}
	}

	internal static DbElementExpression CreateElementExpressionUnwrapSingleProperty(DbExpression argument)
	{
		IList<EdmProperty> properties = TypeHelpers.GetProperties(ArgumentValidation.ValidateElement(argument));
		if (properties == null || properties.Count != 1)
		{
			throw new ArgumentException(Strings.Cqt_Element_InvalidArgumentForUnwrapSingleProperty, "argument");
		}
		return new DbElementExpression(properties[0].TypeUsage, argument, unwrapSingleProperty: true);
	}

	internal static DbRelatedEntityRef CreateRelatedEntityRef(RelationshipEndMember sourceEnd, RelationshipEndMember targetEnd, DbExpression targetEntity)
	{
		return new DbRelatedEntityRef(sourceEnd, targetEnd, targetEntity);
	}

	internal static DbNewInstanceExpression CreateNewEntityWithRelationshipsExpression(EntityType entityType, IList<DbExpression> attributeValues, IList<DbRelatedEntityRef> relationships)
	{
		DbExpressionList validArguments;
		ReadOnlyCollection<DbRelatedEntityRef> validRelatedRefs;
		return new DbNewInstanceExpression(ArgumentValidation.ValidateNewEntityWithRelationships(entityType, attributeValues, relationships, out validArguments, out validRelatedRefs), validArguments, validRelatedRefs);
	}

	internal static DbRelationshipNavigationExpression NavigateAllowingAllRelationshipsInSameTypeHierarchy(this DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
	{
		RelationshipType relType;
		return new DbRelationshipNavigationExpression(ArgumentValidation.ValidateNavigate(navigateFrom, fromEnd, toEnd, out relType, allowAllRelationshipsInSameTypeHierarchy: true), relType, fromEnd, toEnd, navigateFrom);
	}

	internal static DbPropertyExpression CreatePropertyExpressionFromMember(DbExpression instance, EdmMember member)
	{
		return PropertyFromMember(instance, member, "member");
	}

	private static TypeUsage CreateCollectionResultType(EdmType type)
	{
		return TypeUsage.Create(TypeHelpers.CreateCollectionType(TypeUsage.Create(type)));
	}

	private static TypeUsage CreateCollectionResultType(TypeUsage elementType)
	{
		return TypeUsage.Create(TypeHelpers.CreateCollectionType(elementType));
	}

	private static bool IsConstantNegativeInteger(DbExpression expression)
	{
		if (expression.ExpressionKind == DbExpressionKind.Constant && TypeSemantics.IsIntegerNumericType(expression.ResultType))
		{
			return Convert.ToInt64(((DbConstantExpression)expression).Value, CultureInfo.InvariantCulture) < 0;
		}
		return false;
	}
}
