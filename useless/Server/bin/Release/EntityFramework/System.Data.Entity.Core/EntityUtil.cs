using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core;

internal static class EntityUtil
{
	internal enum InternalErrorCode
	{
		WrongNumberOfKeys = 1000,
		UnknownColumnMapKind = 1001,
		NestOverNest = 1002,
		ColumnCountMismatch = 1003,
		AssertionFailed = 1004,
		UnknownVar = 1005,
		WrongVarType = 1006,
		ExtentWithoutEntity = 1007,
		UnnestWithoutInput = 1008,
		UnnestMultipleCollections = 1009,
		CodeGen_NoSuchProperty = 1011,
		JoinOverSingleStreamNest = 1012,
		InvalidInternalTree = 1013,
		NameValuePairNext = 1014,
		InvalidParserState1 = 1015,
		InvalidParserState2 = 1016,
		SqlGenParametersNotPermitted = 1017,
		EntityKeyMissingKeyValue = 1018,
		UpdatePipelineResultRequestInvalid = 1019,
		InvalidStateEntry = 1020,
		InvalidPrimitiveTypeKind = 1021,
		UnknownLinqNodeType = 1023,
		CollectionWithNoColumns = 1024,
		UnexpectedLinqLambdaExpressionFormat = 1025,
		CommandTreeOnStoredProcedureEntityCommand = 1026,
		BoolExprAssert = 1027,
		FailedToGeneratePromotionRank = 1029
	}

	internal const int AssemblyQualifiedNameIndex = 3;

	internal const int InvariantNameIndex = 2;

	internal const string Parameter = "Parameter";

	internal const CompareOptions StringCompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

	internal static Dictionary<string, string> COMPILER_VERSION = new Dictionary<string, string> { { "CompilerVersion", "V3.5" } };

	internal static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
	{
		if (first == null || second == null)
		{
			yield break;
		}
		using IEnumerator<T1> firstEnumerator = first.GetEnumerator();
		using IEnumerator<T2> secondEnumerator = second.GetEnumerator();
		while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
		{
			yield return new KeyValuePair<T1, T2>(firstEnumerator.Current, secondEnumerator.Current);
		}
	}

	internal static bool IsAnICollection(Type type)
	{
		if (!typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
		{
			return type.GetInterface(typeof(ICollection<>).FullName) != null;
		}
		return true;
	}

	internal static Type GetCollectionElementType(Type propertyType)
	{
		Type type = propertyType.TryGetElementType(typeof(ICollection<>));
		if (type == null)
		{
			throw new InvalidOperationException(Strings.PocoEntityWrapper_UnexpectedTypeForNavigationProperty(propertyType.FullName, typeof(ICollection<>)));
		}
		return type;
	}

	internal static Type DetermineCollectionType(Type requestedType)
	{
		Type collectionElementType = GetCollectionElementType(requestedType);
		if (requestedType.IsArray)
		{
			throw new InvalidOperationException(Strings.ObjectQuery_UnableToMaterializeArray(requestedType, typeof(List<>).MakeGenericType(collectionElementType)));
		}
		if (!requestedType.IsAbstract() && requestedType.GetPublicConstructor() != null)
		{
			return requestedType;
		}
		Type type = typeof(HashSet<>).MakeGenericType(collectionElementType);
		if (requestedType.IsAssignableFrom(type))
		{
			return type;
		}
		Type type2 = typeof(List<>).MakeGenericType(collectionElementType);
		if (requestedType.IsAssignableFrom(type2))
		{
			return type2;
		}
		return null;
	}

	internal static Type GetEntityIdentityType(Type entityType)
	{
		if (!EntityProxyFactory.IsProxyType(entityType))
		{
			return entityType;
		}
		return entityType.BaseType();
	}

	internal static string QuoteIdentifier(string identifier)
	{
		return "[" + identifier.Replace("]", "]]") + "]";
	}

	internal static MetadataException InvalidSchemaEncountered(string errors)
	{
		return new MetadataException(string.Format(CultureInfo.CurrentCulture, EntityRes.GetString("InvalidSchemaEncountered"), new object[1] { errors }));
	}

	internal static Exception InternalError(InternalErrorCode internalError, int location, object additionalInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0}, {1}", (int)internalError, location);
		if (additionalInfo != null)
		{
			stringBuilder.AppendFormat(", {0}", additionalInfo);
		}
		return new InvalidOperationException(Strings.ADP_InternalProviderError(stringBuilder.ToString()));
	}

	internal static void CheckValidStateForChangeEntityState(EntityState state)
	{
		switch (state)
		{
		case EntityState.Detached:
		case EntityState.Unchanged:
		case EntityState.Added:
		case EntityState.Deleted:
		case EntityState.Modified:
			return;
		}
		throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "state");
	}

	internal static void CheckValidStateForChangeRelationshipState(EntityState state, string paramName)
	{
		if ((uint)(state - 1) > 1u && state != EntityState.Added && state != EntityState.Deleted)
		{
			throw new ArgumentException(Strings.ObjectContext_InvalidRelationshipState, paramName);
		}
	}

	internal static void ThrowPropertyIsNotNullable(string propertyName)
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			throw new ConstraintException(Strings.Materializer_PropertyIsNotNullable);
		}
		throw new PropertyConstraintException(Strings.Materializer_PropertyIsNotNullableWithName(propertyName), propertyName);
	}

	internal static void ThrowSetInvalidValue(object value, Type destinationType, string className, string propertyName)
	{
		if (value == null)
		{
			throw new ConstraintException(Strings.Materializer_SetInvalidValue((Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name, className, propertyName, "null"));
		}
		throw new InvalidOperationException(Strings.Materializer_SetInvalidValue((Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name, className, propertyName, value.GetType().Name));
	}

	internal static InvalidOperationException ValueInvalidCast(Type valueType, Type destinationType)
	{
		if (destinationType.IsValueType() && destinationType.IsGenericType() && typeof(Nullable<>) == destinationType.GetGenericTypeDefinition())
		{
			return new InvalidOperationException(Strings.Materializer_InvalidCastNullable(valueType, destinationType.GetGenericArguments()[0]));
		}
		return new InvalidOperationException(Strings.Materializer_InvalidCastReference(valueType, destinationType));
	}

	internal static void CheckArgumentMergeOption(MergeOption mergeOption)
	{
		if ((uint)mergeOption > 3u)
		{
			string name = typeof(MergeOption).Name;
			string name2 = typeof(MergeOption).Name;
			int num = (int)mergeOption;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name2, num.ToString(CultureInfo.InvariantCulture)));
		}
	}

	internal static void CheckArgumentRefreshMode(RefreshMode refreshMode)
	{
		if (refreshMode != RefreshMode.ClientWins && refreshMode != RefreshMode.StoreWins)
		{
			string name = typeof(RefreshMode).Name;
			string name2 = typeof(RefreshMode).Name;
			int num = (int)refreshMode;
			throw new ArgumentOutOfRangeException(name, Strings.ADP_InvalidEnumerationValue(name2, num.ToString(CultureInfo.InvariantCulture)));
		}
	}

	internal static InvalidOperationException ExecuteFunctionCalledWithNonReaderFunction(EdmFunction functionImport)
	{
		string message = ((functionImport.ReturnParameter != null) ? Strings.ObjectContext_ExecuteFunctionCalledWithScalarFunction(functionImport.ReturnParameter.TypeUsage.EdmType.FullName, functionImport.Name) : Strings.ObjectContext_ExecuteFunctionCalledWithNonQueryFunction(functionImport.Name));
		return new InvalidOperationException(message);
	}

	internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet)
	{
		ValidateEntitySetInKey(key, entitySet, null);
	}

	internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet, string argument)
	{
		string entityContainerName = key.EntityContainerName;
		string entitySetName = key.EntitySetName;
		string name = entitySet.EntityContainer.Name;
		string name2 = entitySet.Name;
		if (!StringComparer.Ordinal.Equals(entityContainerName, name) || !StringComparer.Ordinal.Equals(entitySetName, name2))
		{
			if (string.IsNullOrEmpty(argument))
			{
				throw new InvalidOperationException(Strings.ObjectContext_InvalidEntitySetInKey(entityContainerName, entitySetName, name, name2));
			}
			throw new InvalidOperationException(Strings.ObjectContext_InvalidEntitySetInKeyFromName(entityContainerName, entitySetName, name, name2, argument));
		}
	}

	internal static void ValidateNecessaryModificationFunctionMapping(ModificationFunctionMapping mapping, string currentState, IEntityStateEntry stateEntry, string type, string typeName)
	{
		if (mapping == null)
		{
			throw new UpdateException(Strings.Update_MissingFunctionMapping(currentState, type, typeName), null, new List<IEntityStateEntry> { stateEntry }.Cast<ObjectStateEntry>().Distinct());
		}
	}

	internal static UpdateException Update(string message, Exception innerException, params IEntityStateEntry[] stateEntries)
	{
		return new UpdateException(message, innerException, stateEntries.Cast<ObjectStateEntry>().Distinct());
	}

	internal static UpdateException UpdateRelationshipCardinalityConstraintViolation(string relationshipSetName, int minimumCount, int? maximumCount, string entitySetName, int actualCount, string otherEndPluralName, IEntityStateEntry stateEntry)
	{
		string text = ConvertCardinalityToString(minimumCount);
		string text2 = ConvertCardinalityToString(maximumCount);
		string p = ConvertCardinalityToString(actualCount);
		if (minimumCount == 1 && text == text2)
		{
			return Update(Strings.Update_RelationshipCardinalityConstraintViolationSingleValue(entitySetName, relationshipSetName, p, otherEndPluralName, text), null, stateEntry);
		}
		return Update(Strings.Update_RelationshipCardinalityConstraintViolation(entitySetName, relationshipSetName, p, otherEndPluralName, text, text2), null, stateEntry);
	}

	private static string ConvertCardinalityToString(int? cardinality)
	{
		if (cardinality.HasValue)
		{
			return cardinality.Value.ToString(CultureInfo.CurrentCulture);
		}
		return "*";
	}

	internal static T CheckArgumentOutOfRange<T>(T[] values, int index, string parameterName)
	{
		if ((uint)values.Length <= (uint)index)
		{
			throw new ArgumentOutOfRangeException(parameterName);
		}
		return values[index];
	}

	internal static IEnumerable<T> CheckArgumentContainsNull<T>(ref IEnumerable<T> enumerableArgument, string argumentName) where T : class
	{
		GetCheapestSafeEnumerableAsCollection(ref enumerableArgument);
		foreach (T item in enumerableArgument)
		{
			if (item == null)
			{
				throw new ArgumentException(Strings.CheckArgumentContainsNullFailed(argumentName));
			}
		}
		return enumerableArgument;
	}

	internal static IEnumerable<T> CheckArgumentEmpty<T>(ref IEnumerable<T> enumerableArgument, Func<string, string> errorMessage, string argumentName)
	{
		GetCheapestSafeCountOfEnumerable(ref enumerableArgument, out var count);
		if (count <= 0)
		{
			throw new ArgumentException(errorMessage(argumentName));
		}
		return enumerableArgument;
	}

	private static void GetCheapestSafeCountOfEnumerable<T>(ref IEnumerable<T> enumerable, out int count)
	{
		ICollection<T> cheapestSafeEnumerableAsCollection = GetCheapestSafeEnumerableAsCollection(ref enumerable);
		count = cheapestSafeEnumerableAsCollection.Count;
	}

	private static ICollection<T> GetCheapestSafeEnumerableAsCollection<T>(ref IEnumerable<T> enumerable)
	{
		if (enumerable is ICollection<T> result)
		{
			return result;
		}
		enumerable = new List<T>(enumerable);
		return enumerable as ICollection<T>;
	}

	internal static bool IsNull(object value)
	{
		if (value == null || DBNull.Value == value)
		{
			return true;
		}
		if (value is INullable nullable)
		{
			return nullable.IsNull;
		}
		return false;
	}

	internal static int SrcCompare(string strA, string strB)
	{
		if (!(strA == strB))
		{
			return 1;
		}
		return 0;
	}

	internal static int DstCompare(string strA, string strB)
	{
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
	}
}
