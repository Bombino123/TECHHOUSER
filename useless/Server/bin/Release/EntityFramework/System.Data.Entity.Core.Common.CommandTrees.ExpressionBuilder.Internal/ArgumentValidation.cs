using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Internal;

internal static class ArgumentValidation
{
	internal static ReadOnlyCollection<TElement> NewReadOnlyCollection<TElement>(IList<TElement> list)
	{
		return new ReadOnlyCollection<TElement>(list);
	}

	internal static void RequirePolymorphicType(TypeUsage type)
	{
		if (!TypeSemantics.IsPolymorphicType(type))
		{
			throw new ArgumentException(Strings.Cqt_General_PolymorphicTypeRequired(type.ToString()), "type");
		}
	}

	internal static void RequireCompatibleType(DbExpression expression, TypeUsage requiredResultType, string argumentName)
	{
		RequireCompatibleType(expression, requiredResultType, argumentName, -1);
	}

	private static void RequireCompatibleType(DbExpression expression, TypeUsage requiredResultType, string argumentName, int argumentIndex)
	{
		if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(expression.ResultType, requiredResultType))
		{
			if (argumentIndex != -1)
			{
				argumentName = StringUtil.FormatIndex(argumentName, argumentIndex);
			}
			throw new ArgumentException(Strings.Cqt_ExpressionLink_TypeMismatch(expression.ResultType.ToString(), requiredResultType.ToString()), argumentName);
		}
	}

	internal static void RequireCompatibleType(DbExpression expression, PrimitiveTypeKind requiredResultType, string argumentName)
	{
		RequireCompatibleType(expression, requiredResultType, argumentName, -1);
	}

	private static void RequireCompatibleType(DbExpression expression, PrimitiveTypeKind requiredResultType, string argumentName, int index)
	{
		PrimitiveTypeKind typeKind;
		bool flag = TypeHelpers.TryGetPrimitiveTypeKind(expression.ResultType, out typeKind);
		if (!flag || typeKind != requiredResultType)
		{
			if (index != -1)
			{
				argumentName = StringUtil.FormatIndex(argumentName, index);
			}
			throw new ArgumentException(Strings.Cqt_ExpressionLink_TypeMismatch(flag ? Enum.GetName(typeof(PrimitiveTypeKind), typeKind) : expression.ResultType.ToString(), Enum.GetName(typeof(PrimitiveTypeKind), requiredResultType)), argumentName);
		}
	}

	private static void RequireCompatibleType(DbExpression from, RelationshipEndMember end, bool allowAllRelationshipsInSameTypeHierarchy)
	{
		TypeUsage typeUsage = end.TypeUsage;
		if (!TypeSemantics.IsReferenceType(typeUsage))
		{
			typeUsage = TypeHelpers.CreateReferenceTypeUsage(TypeHelpers.GetEdmType<EntityType>(typeUsage));
		}
		if (allowAllRelationshipsInSameTypeHierarchy)
		{
			if (TypeHelpers.GetCommonTypeUsage(typeUsage, from.ResultType) == null)
			{
				throw new ArgumentException(Strings.Cqt_RelNav_WrongSourceType(typeUsage.ToString()), "from");
			}
		}
		else if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(from.ResultType.EdmType, typeUsage.EdmType))
		{
			throw new ArgumentException(Strings.Cqt_RelNav_WrongSourceType(typeUsage.ToString()), "from");
		}
	}

	internal static void RequireCollectionArgument<TExpressionType>(DbExpression argument)
	{
		if (!TypeSemantics.IsCollectionType(argument.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_Unary_CollectionRequired(typeof(TExpressionType).Name), "argument");
		}
	}

	internal static TypeUsage RequireCollectionArguments<TExpressionType>(DbExpression left, DbExpression right)
	{
		if (!TypeSemantics.IsCollectionType(left.ResultType) || !TypeSemantics.IsCollectionType(right.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_Binary_CollectionsRequired(typeof(TExpressionType).Name));
		}
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
		if (commonTypeUsage == null)
		{
			throw new ArgumentException(Strings.Cqt_Binary_CollectionsRequired(typeof(TExpressionType).Name));
		}
		return commonTypeUsage;
	}

	internal static TypeUsage RequireComparableCollectionArguments<TExpressionType>(DbExpression left, DbExpression right)
	{
		TypeUsage result = RequireCollectionArguments<TExpressionType>(left, right);
		if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(left.ResultType)))
		{
			throw new ArgumentException(Strings.Cqt_InvalidTypeForSetOperation(TypeHelpers.GetElementTypeUsage(left.ResultType).Identity, typeof(TExpressionType).Name), "left");
		}
		if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(right.ResultType)))
		{
			throw new ArgumentException(Strings.Cqt_InvalidTypeForSetOperation(TypeHelpers.GetElementTypeUsage(right.ResultType).Identity, typeof(TExpressionType).Name), "right");
		}
		return result;
	}

	private static EnumerableValidator<TElementIn, TElementOut, TResult> CreateValidator<TElementIn, TElementOut, TResult>(IEnumerable<TElementIn> argument, string argumentName, Func<TElementIn, int, TElementOut> convertElement, Func<List<TElementOut>, TResult> createResult)
	{
		return new EnumerableValidator<TElementIn, TElementOut, TResult>(argument, argumentName)
		{
			ConvertElement = convertElement,
			CreateResult = createResult
		};
	}

	internal static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, Action<DbExpression, int> validationCallback)
	{
		return CreateExpressionList(arguments, argumentName, allowEmpty: false, validationCallback);
	}

	private static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, bool allowEmpty, Action<DbExpression, int> validationCallback)
	{
		EnumerableValidator<DbExpression, DbExpression, DbExpressionList> enumerableValidator = CreateValidator(arguments, argumentName, delegate(DbExpression exp, int idx)
		{
			if (validationCallback != null)
			{
				validationCallback(exp, idx);
			}
			return exp;
		}, (List<DbExpression> expList) => new DbExpressionList(expList));
		enumerableValidator.AllowEmpty = allowEmpty;
		return enumerableValidator.Validate();
	}

	private static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, int expectedElementCount, Action<DbExpression, int> validationCallback)
	{
		EnumerableValidator<DbExpression, DbExpression, DbExpressionList> enumerableValidator = CreateValidator(arguments, argumentName, delegate(DbExpression exp, int idx)
		{
			if (validationCallback != null)
			{
				validationCallback(exp, idx);
			}
			return exp;
		}, (List<DbExpression> expList) => new DbExpressionList(expList));
		enumerableValidator.ExpectedElementCount = expectedElementCount;
		enumerableValidator.AllowEmpty = false;
		return enumerableValidator.Validate();
	}

	private static FunctionParameter[] GetExpectedParameters(EdmFunction function)
	{
		return function.Parameters.Where((FunctionParameter p) => p.Mode == ParameterMode.In || p.Mode == ParameterMode.InOut).ToArray();
	}

	internal static DbExpressionList ValidateFunctionAggregate(EdmFunction function, IEnumerable<DbExpression> args)
	{
		CheckFunction(function);
		if (!TypeSemantics.IsAggregateFunction(function) || function.ReturnParameter == null)
		{
			throw new ArgumentException(Strings.Cqt_Aggregate_InvalidFunction, "function");
		}
		FunctionParameter[] expectedParams = GetExpectedParameters(function);
		return CreateExpressionList(args, "argument", expectedParams.Length, delegate(DbExpression exp, int idx)
		{
			TypeUsage typeUsage = expectedParams[idx].TypeUsage;
			TypeUsage elementType = null;
			if (TypeHelpers.TryGetCollectionElementType(typeUsage, out elementType))
			{
				typeUsage = elementType;
			}
			RequireCompatibleType(exp, typeUsage, "argument");
		});
	}

	internal static void ValidateSortClause(DbExpression key)
	{
		if (!TypeHelpers.IsValidSortOpKeyType(key.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_Sort_OrderComparable, "key");
		}
	}

	internal static void ValidateSortClause(DbExpression key, string collation)
	{
		ValidateSortClause(key);
		Check.NotEmpty(collation, "collation");
		if (!TypeSemantics.IsPrimitiveType(key.ResultType, PrimitiveTypeKind.String))
		{
			throw new ArgumentException(Strings.Cqt_Sort_NonStringCollationInvalid, "collation");
		}
	}

	internal static ReadOnlyCollection<DbVariableReferenceExpression> ValidateLambda(IEnumerable<DbVariableReferenceExpression> variables)
	{
		EnumerableValidator<DbVariableReferenceExpression, DbVariableReferenceExpression, ReadOnlyCollection<DbVariableReferenceExpression>> enumerableValidator = CreateValidator(variables, "variables", delegate(DbVariableReferenceExpression varExp, int idx)
		{
			if (varExp == null)
			{
				throw new ArgumentNullException(StringUtil.FormatIndex("variables", idx));
			}
			return varExp;
		}, (List<DbVariableReferenceExpression> varList) => new ReadOnlyCollection<DbVariableReferenceExpression>(varList));
		enumerableValidator.AllowEmpty = true;
		enumerableValidator.GetName = (DbVariableReferenceExpression varDef, int idx) => varDef.VariableName;
		return enumerableValidator.Validate();
	}

	internal static TypeUsage ValidateQuantifier(DbExpression predicate)
	{
		RequireCompatibleType(predicate, PrimitiveTypeKind.Boolean, "predicate");
		return predicate.ResultType;
	}

	internal static TypeUsage ValidateApply(DbExpressionBinding input, DbExpressionBinding apply)
	{
		if (input.VariableName.Equals(apply.VariableName, StringComparison.Ordinal))
		{
			throw new ArgumentException(Strings.Cqt_Apply_DuplicateVariableNames);
		}
		return CreateCollectionOfRowResultType(new List<KeyValuePair<string, TypeUsage>>
		{
			new KeyValuePair<string, TypeUsage>(input.VariableName, input.VariableType),
			new KeyValuePair<string, TypeUsage>(apply.VariableName, apply.VariableType)
		});
	}

	internal static ReadOnlyCollection<DbExpressionBinding> ValidateCrossJoin(IEnumerable<DbExpressionBinding> inputs, out TypeUsage resultType)
	{
		List<DbExpressionBinding> list = new List<DbExpressionBinding>();
		List<KeyValuePair<string, TypeUsage>> list2 = new List<KeyValuePair<string, TypeUsage>>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		IEnumerator<DbExpressionBinding> enumerator = inputs.GetEnumerator();
		int num = 0;
		while (enumerator.MoveNext())
		{
			DbExpressionBinding current = enumerator.Current;
			string paramName = StringUtil.FormatIndex("inputs", num);
			if (current == null)
			{
				throw new ArgumentNullException(paramName);
			}
			int value = -1;
			if (dictionary.TryGetValue(current.VariableName, out value))
			{
				throw new ArgumentException(Strings.Cqt_CrossJoin_DuplicateVariableNames(value, num, current.VariableName));
			}
			list.Add(current);
			dictionary.Add(current.VariableName, num);
			list2.Add(new KeyValuePair<string, TypeUsage>(current.VariableName, current.VariableType));
			num++;
		}
		if (list.Count < 2)
		{
			throw new ArgumentException(Strings.Cqt_CrossJoin_AtLeastTwoInputs, "inputs");
		}
		resultType = CreateCollectionOfRowResultType(list2);
		return new ReadOnlyCollection<DbExpressionBinding>(list);
	}

	internal static TypeUsage ValidateJoin(DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
	{
		if (left.VariableName.Equals(right.VariableName, StringComparison.Ordinal))
		{
			throw new ArgumentException(Strings.Cqt_Join_DuplicateVariableNames);
		}
		RequireCompatibleType(joinCondition, PrimitiveTypeKind.Boolean, "joinCondition");
		return CreateCollectionOfRowResultType(new List<KeyValuePair<string, TypeUsage>>(2)
		{
			new KeyValuePair<string, TypeUsage>(left.VariableName, left.VariableType),
			new KeyValuePair<string, TypeUsage>(right.VariableName, right.VariableType)
		});
	}

	internal static TypeUsage ValidateFilter(DbExpressionBinding input, DbExpression predicate)
	{
		RequireCompatibleType(predicate, PrimitiveTypeKind.Boolean, "predicate");
		return input.Expression.ResultType;
	}

	internal static TypeUsage ValidateGroupBy(IEnumerable<KeyValuePair<string, DbExpression>> keys, IEnumerable<KeyValuePair<string, DbAggregate>> aggregates, out DbExpressionList validKeys, out ReadOnlyCollection<DbAggregate> validAggregates)
	{
		List<KeyValuePair<string, TypeUsage>> columns = new List<KeyValuePair<string, TypeUsage>>();
		HashSet<string> keyNames = new HashSet<string>();
		EnumerableValidator<KeyValuePair<string, DbExpression>, DbExpression, DbExpressionList> enumerableValidator = CreateValidator(keys, "keys", delegate(KeyValuePair<string, DbExpression> keyInfo, int index)
		{
			CheckNamed(keyInfo, "keys", index);
			if (!TypeHelpers.IsValidGroupKeyType(keyInfo.Value.ResultType))
			{
				throw new ArgumentException(Strings.Cqt_GroupBy_KeyNotEqualityComparable(keyInfo.Key));
			}
			keyNames.Add(keyInfo.Key);
			columns.Add(new KeyValuePair<string, TypeUsage>(keyInfo.Key, keyInfo.Value.ResultType));
			return keyInfo.Value;
		}, (List<DbExpression> expList) => new DbExpressionList(expList));
		enumerableValidator.AllowEmpty = true;
		enumerableValidator.GetName = (KeyValuePair<string, DbExpression> keyInfo, int idx) => keyInfo.Key;
		validKeys = enumerableValidator.Validate();
		bool hasGroupAggregate = false;
		EnumerableValidator<KeyValuePair<string, DbAggregate>, DbAggregate, ReadOnlyCollection<DbAggregate>> enumerableValidator2 = CreateValidator(aggregates, "aggregates", delegate(KeyValuePair<string, DbAggregate> aggInfo, int idx)
		{
			CheckNamed(aggInfo, "aggregates", idx);
			if (keyNames.Contains(aggInfo.Key))
			{
				throw new ArgumentException(Strings.Cqt_GroupBy_AggregateColumnExistsAsGroupColumn(aggInfo.Key));
			}
			if (aggInfo.Value is DbGroupAggregate)
			{
				if (hasGroupAggregate)
				{
					throw new ArgumentException(Strings.Cqt_GroupBy_MoreThanOneGroupAggregate);
				}
				hasGroupAggregate = true;
			}
			columns.Add(new KeyValuePair<string, TypeUsage>(aggInfo.Key, aggInfo.Value.ResultType));
			return aggInfo.Value;
		}, (List<DbAggregate> aggList) => NewReadOnlyCollection(aggList));
		enumerableValidator2.AllowEmpty = true;
		enumerableValidator2.GetName = (KeyValuePair<string, DbAggregate> aggInfo, int idx) => aggInfo.Key;
		validAggregates = enumerableValidator2.Validate();
		if (validKeys.Count == 0 && validAggregates.Count == 0)
		{
			throw new ArgumentException(Strings.Cqt_GroupBy_AtLeastOneKeyOrAggregate);
		}
		return CreateCollectionOfRowResultType(columns);
	}

	internal static ReadOnlyCollection<DbSortClause> ValidateSortArguments(IEnumerable<DbSortClause> sortOrder)
	{
		EnumerableValidator<DbSortClause, DbSortClause, ReadOnlyCollection<DbSortClause>> enumerableValidator = CreateValidator(sortOrder, "sortOrder", (DbSortClause key, int idx) => key, (List<DbSortClause> keyList) => NewReadOnlyCollection(keyList));
		enumerableValidator.AllowEmpty = false;
		return enumerableValidator.Validate();
	}

	internal static ReadOnlyCollection<DbSortClause> ValidateSort(IEnumerable<DbSortClause> sortOrder)
	{
		return ValidateSortArguments(sortOrder);
	}

	internal static TypeUsage ValidateConstant(Type type)
	{
		if (!TryGetPrimitiveTypeKind(type, out var primitiveTypeKind))
		{
			throw new ArgumentException(Strings.Cqt_Constant_InvalidType, "type");
		}
		return TypeHelpers.GetLiteralTypeUsage(primitiveTypeKind);
	}

	internal static TypeUsage ValidateConstant(object value)
	{
		return ValidateConstant(value.GetType());
	}

	internal static void ValidateConstant(TypeUsage constantType, object value)
	{
		CheckType(constantType, "constantType");
		if (TypeHelpers.TryGetEdmType<EnumType>(constantType, out var type))
		{
			Type clrEquivalentType = type.UnderlyingType.ClrEquivalentType;
			if (clrEquivalentType != value.GetType() && (!value.GetType().IsEnum() || !ClrEdmEnumTypesMatch(type, value.GetType())))
			{
				throw new ArgumentException(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType(value.GetType().Name, type.Name, clrEquivalentType.Name), "value");
			}
			return;
		}
		if (!TypeHelpers.TryGetEdmType<PrimitiveType>(constantType, out var type2))
		{
			throw new ArgumentException(Strings.Cqt_Constant_InvalidConstantType(constantType.ToString()), "constantType");
		}
		if ((!TryGetPrimitiveTypeKind(value.GetType(), out var primitiveTypeKind) || type2.PrimitiveTypeKind != primitiveTypeKind) && (!Helper.IsGeographicType(type2) || primitiveTypeKind != PrimitiveTypeKind.Geography) && (!Helper.IsGeometricType(type2) || primitiveTypeKind != PrimitiveTypeKind.Geometry))
		{
			throw new ArgumentException(Strings.Cqt_Constant_InvalidValueForType(constantType.ToString()), "value");
		}
	}

	internal static TypeUsage ValidateCreateRef(EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues, out DbExpression keyConstructor)
	{
		CheckEntitySet(entitySet, "entitySet");
		CheckType(entityType, "entityType");
		if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, entityType))
		{
			throw new ArgumentException(Strings.Cqt_Ref_PolymorphicArgRequired);
		}
		IList<EdmMember> keyMembers = entityType.KeyMembers;
		EnumerableValidator<DbExpression, KeyValuePair<string, DbExpression>, List<KeyValuePair<string, DbExpression>>> enumerableValidator = CreateValidator(keyValues, "keyValues", delegate(DbExpression valueExp, int idx)
		{
			RequireCompatibleType(valueExp, keyMembers[idx].TypeUsage, "keyValues", idx);
			return new KeyValuePair<string, DbExpression>(keyMembers[idx].Name, valueExp);
		}, (List<KeyValuePair<string, DbExpression>> columnList) => columnList);
		enumerableValidator.ExpectedElementCount = keyMembers.Count;
		List<KeyValuePair<string, DbExpression>> columnValues = enumerableValidator.Validate();
		keyConstructor = DbExpressionBuilder.NewRow(columnValues);
		return CreateReferenceResultType(entityType);
	}

	internal static TypeUsage ValidateRefFromKey(EntitySet entitySet, DbExpression keyValues, EntityType entityType)
	{
		CheckEntitySet(entitySet, "entitySet");
		CheckType(entityType);
		if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, entityType))
		{
			throw new ArgumentException(Strings.Cqt_Ref_PolymorphicArgRequired);
		}
		TypeUsage requiredResultType = CreateResultType(TypeHelpers.CreateKeyRowType(entitySet.ElementType));
		RequireCompatibleType(keyValues, requiredResultType, "keyValues");
		return CreateReferenceResultType(entityType);
	}

	internal static TypeUsage ValidateNavigate(DbExpression navigateFrom, RelationshipType type, string fromEndName, string toEndName, out RelationshipEndMember fromEnd, out RelationshipEndMember toEnd)
	{
		CheckType(type);
		if (!type.RelationshipEndMembers.TryGetValue(fromEndName, ignoreCase: false, out fromEnd))
		{
			throw new ArgumentOutOfRangeException(fromEndName, Strings.Cqt_Factory_NoSuchRelationEnd);
		}
		if (!type.RelationshipEndMembers.TryGetValue(toEndName, ignoreCase: false, out toEnd))
		{
			throw new ArgumentOutOfRangeException(toEndName, Strings.Cqt_Factory_NoSuchRelationEnd);
		}
		RequireCompatibleType(navigateFrom, fromEnd, allowAllRelationshipsInSameTypeHierarchy: false);
		return CreateResultType(toEnd);
	}

	internal static TypeUsage ValidateNavigate(DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd, out RelationshipType relType, bool allowAllRelationshipsInSameTypeHierarchy)
	{
		CheckMember(fromEnd, "fromEnd");
		CheckMember(toEnd, "toEnd");
		relType = fromEnd.DeclaringType as RelationshipType;
		CheckType(relType);
		if (!relType.Equals(toEnd.DeclaringType))
		{
			throw new ArgumentException(Strings.Cqt_Factory_IncompatibleRelationEnds, "toEnd");
		}
		RequireCompatibleType(navigateFrom, fromEnd, allowAllRelationshipsInSameTypeHierarchy);
		return CreateResultType(toEnd);
	}

	internal static TypeUsage ValidateElement(DbExpression argument)
	{
		RequireCollectionArgument<DbElementExpression>(argument);
		return TypeHelpers.GetEdmType<CollectionType>(argument.ResultType).TypeUsage;
	}

	internal static TypeUsage ValidateCase(IEnumerable<DbExpression> whenExpressions, IEnumerable<DbExpression> thenExpressions, DbExpression elseExpression, out DbExpressionList validWhens, out DbExpressionList validThens)
	{
		validWhens = CreateExpressionList(whenExpressions, "whenExpressions", delegate(DbExpression exp, int idx)
		{
			RequireCompatibleType(exp, PrimitiveTypeKind.Boolean, "whenExpressions", idx);
		});
		TypeUsage commonResultType = null;
		validThens = CreateExpressionList(thenExpressions, "thenExpressions", delegate(DbExpression exp, int idx)
		{
			if (commonResultType == null)
			{
				commonResultType = exp.ResultType;
			}
			else
			{
				commonResultType = TypeHelpers.GetCommonTypeUsage(exp.ResultType, commonResultType);
				if (commonResultType == null)
				{
					throw new ArgumentException(Strings.Cqt_Case_InvalidResultType);
				}
			}
		});
		commonResultType = TypeHelpers.GetCommonTypeUsage(elseExpression.ResultType, commonResultType);
		if (commonResultType == null)
		{
			throw new ArgumentException(Strings.Cqt_Case_InvalidResultType);
		}
		if (validWhens.Count != validThens.Count)
		{
			throw new ArgumentException(Strings.Cqt_Case_WhensMustEqualThens);
		}
		return commonResultType;
	}

	internal static TypeUsage ValidateFunction(EdmFunction function, IEnumerable<DbExpression> arguments, out DbExpressionList validArgs)
	{
		CheckFunction(function);
		if (!function.IsComposableAttribute)
		{
			throw new ArgumentException(Strings.Cqt_Function_NonComposableInExpression, "function");
		}
		if (!string.IsNullOrEmpty(function.CommandTextAttribute) && !function.HasUserDefinedBody)
		{
			throw new ArgumentException(Strings.Cqt_Function_CommandTextInExpression, "function");
		}
		if (function.ReturnParameter == null)
		{
			throw new ArgumentException(Strings.Cqt_Function_VoidResultInvalid, "function");
		}
		FunctionParameter[] expectedParams = GetExpectedParameters(function);
		validArgs = CreateExpressionList(arguments, "arguments", expectedParams.Length, delegate(DbExpression exp, int idx)
		{
			RequireCompatibleType(exp, expectedParams[idx].TypeUsage, "arguments", idx);
		});
		return function.ReturnParameter.TypeUsage;
	}

	internal static TypeUsage ValidateInvoke(DbLambda lambda, IEnumerable<DbExpression> arguments, out DbExpressionList validArguments)
	{
		validArguments = null;
		EnumerableValidator<DbExpression, DbExpression, DbExpressionList> enumerableValidator = CreateValidator(arguments, "arguments", delegate(DbExpression exp, int idx)
		{
			RequireCompatibleType(exp, lambda.Variables[idx].ResultType, "arguments", idx);
			return exp;
		}, (List<DbExpression> expList) => new DbExpressionList(expList));
		enumerableValidator.ExpectedElementCount = lambda.Variables.Count;
		validArguments = enumerableValidator.Validate();
		return lambda.Body.ResultType;
	}

	internal static TypeUsage ValidateNewEmptyCollection(TypeUsage collectionType, out DbExpressionList validElements)
	{
		CheckType(collectionType, "collectionType");
		if (!TypeSemantics.IsCollectionType(collectionType))
		{
			throw new ArgumentException(Strings.Cqt_NewInstance_CollectionTypeRequired, "collectionType");
		}
		validElements = new DbExpressionList(new DbExpression[0]);
		return collectionType;
	}

	internal static TypeUsage ValidateNewRow(IEnumerable<KeyValuePair<string, DbExpression>> columnValues, out DbExpressionList validElements)
	{
		List<KeyValuePair<string, TypeUsage>> columnTypes = new List<KeyValuePair<string, TypeUsage>>();
		EnumerableValidator<KeyValuePair<string, DbExpression>, DbExpression, DbExpressionList> enumerableValidator = CreateValidator(columnValues, "columnValues", delegate(KeyValuePair<string, DbExpression> columnValue, int idx)
		{
			CheckNamed(columnValue, "columnValues", idx);
			columnTypes.Add(new KeyValuePair<string, TypeUsage>(columnValue.Key, columnValue.Value.ResultType));
			return columnValue.Value;
		}, (List<DbExpression> expList) => new DbExpressionList(expList));
		enumerableValidator.GetName = (KeyValuePair<string, DbExpression> columnValue, int idx) => columnValue.Key;
		validElements = enumerableValidator.Validate();
		return CreateResultType(TypeHelpers.CreateRowType(columnTypes));
	}

	internal static TypeUsage ValidateNew(TypeUsage instanceType, IEnumerable<DbExpression> arguments, out DbExpressionList validArguments)
	{
		CheckType(instanceType, "instanceType");
		CollectionType type = null;
		if (TypeHelpers.TryGetEdmType<CollectionType>(instanceType, out type) && type != null)
		{
			TypeUsage elementType = type.TypeUsage;
			validArguments = CreateExpressionList(arguments, "arguments", allowEmpty: true, delegate(DbExpression exp, int idx)
			{
				RequireCompatibleType(exp, elementType, "arguments", idx);
			});
		}
		else
		{
			List<TypeUsage> expectedTypes = GetStructuralMemberTypes(instanceType);
			int pos = 0;
			validArguments = CreateExpressionList(arguments, "arguments", expectedTypes.Count, delegate(DbExpression exp, int idx)
			{
				RequireCompatibleType(exp, expectedTypes[pos++], "arguments", idx);
			});
		}
		return instanceType;
	}

	private static List<TypeUsage> GetStructuralMemberTypes(TypeUsage instanceType)
	{
		if (!(instanceType.EdmType is StructuralType structuralType))
		{
			throw new ArgumentException(Strings.Cqt_NewInstance_StructuralTypeRequired, "instanceType");
		}
		if (structuralType.Abstract)
		{
			throw new ArgumentException(Strings.Cqt_NewInstance_CannotInstantiateAbstractType(instanceType.ToString()), "instanceType");
		}
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(structuralType);
		if (allStructuralMembers == null || allStructuralMembers.Count < 1)
		{
			throw new ArgumentException(Strings.Cqt_NewInstance_CannotInstantiateMemberlessType(instanceType.ToString()), "instanceType");
		}
		List<TypeUsage> list = new List<TypeUsage>(allStructuralMembers.Count);
		for (int i = 0; i < allStructuralMembers.Count; i++)
		{
			list.Add(Helper.GetModelTypeUsage(allStructuralMembers[i]));
		}
		return list;
	}

	internal static TypeUsage ValidateNewEntityWithRelationships(EntityType entityType, IEnumerable<DbExpression> attributeValues, IList<DbRelatedEntityRef> relationships, out DbExpressionList validArguments, out ReadOnlyCollection<DbRelatedEntityRef> validRelatedRefs)
	{
		TypeUsage instanceType = CreateResultType(entityType);
		instanceType = ValidateNew(instanceType, attributeValues, out validArguments);
		if (relationships.Count > 0)
		{
			List<DbRelatedEntityRef> list = new List<DbRelatedEntityRef>(relationships.Count);
			for (int i = 0; i < relationships.Count; i++)
			{
				DbRelatedEntityRef dbRelatedEntityRef = relationships[i];
				EntityTypeBase elementType = TypeHelpers.GetEdmType<RefType>(dbRelatedEntityRef.SourceEnd.TypeUsage).ElementType;
				if (!entityType.EdmEquals(elementType) && !entityType.IsSubtypeOf(elementType))
				{
					throw new ArgumentException(Strings.Cqt_NewInstance_IncompatibleRelatedEntity_SourceTypeNotValid, StringUtil.FormatIndex("relationships", i));
				}
				list.Add(dbRelatedEntityRef);
			}
			validRelatedRefs = new ReadOnlyCollection<DbRelatedEntityRef>(list);
		}
		else
		{
			validRelatedRefs = new ReadOnlyCollection<DbRelatedEntityRef>(new DbRelatedEntityRef[0]);
		}
		return instanceType;
	}

	internal static TypeUsage ValidateProperty(DbExpression instance, string propertyName, bool ignoreCase, out EdmMember foundMember)
	{
		if (TypeHelpers.TryGetEdmType<StructuralType>(instance.ResultType, out var type) && type.Members.TryGetValue(propertyName, ignoreCase, out foundMember) && foundMember != null && (Helper.IsRelationshipEndMember(foundMember) || Helper.IsEdmProperty(foundMember) || Helper.IsNavigationProperty(foundMember)))
		{
			return Helper.GetModelTypeUsage(foundMember);
		}
		throw new ArgumentOutOfRangeException("propertyName", Strings.NoSuchProperty(propertyName, instance.ResultType.ToString()));
	}

	private static void CheckNamed<T>(KeyValuePair<string, T> element, string argumentName, int index)
	{
		if (string.IsNullOrEmpty(element.Key))
		{
			if (index != -1)
			{
				argumentName = StringUtil.FormatIndex(argumentName, index);
			}
			throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}.Key", new object[1] { argumentName }));
		}
		if (element.Value == null)
		{
			if (index != -1)
			{
				argumentName = StringUtil.FormatIndex(argumentName, index);
			}
			throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}.Value", new object[1] { argumentName }));
		}
	}

	private static void CheckReadOnly(GlobalItem item, string varName)
	{
		if (!item.IsReadOnly)
		{
			throw new ArgumentException(Strings.Cqt_General_MetadataNotReadOnly, varName);
		}
	}

	private static void CheckReadOnly(TypeUsage item, string varName)
	{
		if (!item.IsReadOnly)
		{
			throw new ArgumentException(Strings.Cqt_General_MetadataNotReadOnly, varName);
		}
	}

	private static void CheckReadOnly(EntitySetBase item, string varName)
	{
		if (!item.IsReadOnly)
		{
			throw new ArgumentException(Strings.Cqt_General_MetadataNotReadOnly, varName);
		}
	}

	private static void CheckType(EdmType type)
	{
		CheckType(type, "type");
	}

	private static void CheckType(EdmType type, string argumentName)
	{
		CheckReadOnly(type, argumentName);
	}

	internal static void CheckType(TypeUsage type)
	{
		CheckType(type, "type");
	}

	internal static void CheckType(TypeUsage type, string varName)
	{
		CheckReadOnly(type, varName);
		if (!CheckDataSpace(type))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_TypeUsageIncorrectSpace, "type");
		}
	}

	internal static void CheckMember(EdmMember memberMeta, string varName)
	{
		CheckReadOnly(memberMeta.DeclaringType, varName);
		if (!CheckDataSpace(memberMeta.TypeUsage) || !CheckDataSpace(memberMeta.DeclaringType))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EdmMemberIncorrectSpace, varName);
		}
	}

	private static void CheckParameter(FunctionParameter paramMeta, string varName)
	{
		CheckReadOnly(paramMeta.DeclaringFunction, varName);
		if (!CheckDataSpace(paramMeta.TypeUsage))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_FunctionParameterIncorrectSpace, varName);
		}
	}

	private static void CheckFunction(EdmFunction function)
	{
		CheckReadOnly(function, "function");
		if (!CheckDataSpace(function))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_FunctionIncorrectSpace, "function");
		}
		if (function.IsComposableAttribute && function.ReturnParameter == null)
		{
			throw new ArgumentException(Strings.Cqt_Metadata_FunctionReturnParameterNull, "function");
		}
		if (function.ReturnParameter != null && !CheckDataSpace(function.ReturnParameter.TypeUsage))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_FunctionParameterIncorrectSpace, "function.ReturnParameter");
		}
		IList<FunctionParameter> parameters = function.Parameters;
		for (int i = 0; i < parameters.Count; i++)
		{
			CheckParameter(parameters[i], StringUtil.FormatIndex("function.Parameters", i));
		}
	}

	internal static void CheckEntitySet(EntitySetBase entitySet, string varName)
	{
		CheckReadOnly(entitySet, varName);
		if (entitySet.EntityContainer == null)
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EntitySetEntityContainerNull, varName);
		}
		if (!CheckDataSpace(entitySet.EntityContainer))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EntitySetIncorrectSpace, varName);
		}
		if (!CheckDataSpace(entitySet.ElementType))
		{
			throw new ArgumentException(Strings.Cqt_Metadata_EntitySetIncorrectSpace, varName);
		}
	}

	private static bool CheckDataSpace(TypeUsage type)
	{
		return CheckDataSpace(type.EdmType);
	}

	private static bool CheckDataSpace(GlobalItem item)
	{
		if (BuiltInTypeKind.PrimitiveType == item.BuiltInTypeKind || (BuiltInTypeKind.EdmFunction == item.BuiltInTypeKind && DataSpace.CSpace == item.DataSpace))
		{
			return true;
		}
		if (Helper.IsRowType(item))
		{
			foreach (EdmProperty property in ((RowType)item).Properties)
			{
				if (!CheckDataSpace(property.TypeUsage))
				{
					return false;
				}
			}
			return true;
		}
		if (Helper.IsCollectionType(item))
		{
			return CheckDataSpace(((CollectionType)item).TypeUsage);
		}
		if (Helper.IsRefType(item))
		{
			return CheckDataSpace(((RefType)item).ElementType);
		}
		if (item.DataSpace != DataSpace.SSpace)
		{
			return item.DataSpace == DataSpace.CSpace;
		}
		return true;
	}

	internal static TypeUsage CreateCollectionOfRowResultType(List<KeyValuePair<string, TypeUsage>> columns)
	{
		return TypeUsage.Create(TypeHelpers.CreateCollectionType(TypeUsage.Create(TypeHelpers.CreateRowType(columns))));
	}

	private static TypeUsage CreateResultType(EdmType resultType)
	{
		return TypeUsage.Create(resultType);
	}

	private static TypeUsage CreateResultType(RelationshipEndMember end)
	{
		TypeUsage typeUsage = end.TypeUsage;
		if (!TypeSemantics.IsReferenceType(typeUsage))
		{
			typeUsage = TypeHelpers.CreateReferenceTypeUsage(TypeHelpers.GetEdmType<EntityType>(typeUsage));
		}
		if (RelationshipMultiplicity.Many == end.RelationshipMultiplicity)
		{
			typeUsage = TypeHelpers.CreateCollectionTypeUsage(typeUsage);
		}
		return typeUsage;
	}

	internal static TypeUsage CreateReferenceResultType(EntityTypeBase referencedEntityType)
	{
		return TypeUsage.Create(TypeHelpers.CreateReferenceType(referencedEntityType));
	}

	private static bool TryGetPrimitiveTypeKind(Type clrType, out PrimitiveTypeKind primitiveTypeKind)
	{
		return ClrProviderManifest.TryGetPrimitiveTypeKind(clrType, out primitiveTypeKind);
	}

	private static bool ClrEdmEnumTypesMatch(EnumType edmEnumType, Type clrEnumType)
	{
		if (clrEnumType.Name != edmEnumType.Name || clrEnumType.GetEnumNames().Length < edmEnumType.Members.Count)
		{
			return false;
		}
		if (!TryGetPrimitiveTypeKind(clrEnumType.GetEnumUnderlyingType(), out var primitiveTypeKind) || primitiveTypeKind != edmEnumType.UnderlyingType.PrimitiveTypeKind)
		{
			return false;
		}
		foreach (EnumMember member in edmEnumType.Members)
		{
			if (!clrEnumType.GetEnumNames().Contains(member.Name) || !member.Value.Equals(Convert.ChangeType(Enum.Parse(clrEnumType, member.Name), clrEnumType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)))
			{
				return false;
			}
		}
		return true;
	}
}
