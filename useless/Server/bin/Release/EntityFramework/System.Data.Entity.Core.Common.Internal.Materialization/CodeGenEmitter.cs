using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal static class CodeGenEmitter
{
	internal static readonly MethodInfo CodeGenEmitter_BinaryEquals = typeof(CodeGenEmitter).GetOnlyDeclaredMethod("BinaryEquals");

	internal static readonly MethodInfo CodeGenEmitter_CheckedConvert = typeof(CodeGenEmitter).GetOnlyDeclaredMethod("CheckedConvert");

	internal static readonly MethodInfo CodeGenEmitter_Compile = typeof(CodeGenEmitter).GetDeclaredMethod("Compile", typeof(Expression));

	internal static readonly MethodInfo DbDataReader_GetValue = typeof(DbDataReader).GetOnlyDeclaredMethod("GetValue");

	internal static readonly MethodInfo DbDataReader_GetString = typeof(DbDataReader).GetOnlyDeclaredMethod("GetString");

	internal static readonly MethodInfo DbDataReader_GetInt16 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt16");

	internal static readonly MethodInfo DbDataReader_GetInt32 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt32");

	internal static readonly MethodInfo DbDataReader_GetInt64 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt64");

	internal static readonly MethodInfo DbDataReader_GetBoolean = typeof(DbDataReader).GetOnlyDeclaredMethod("GetBoolean");

	internal static readonly MethodInfo DbDataReader_GetDecimal = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDecimal");

	internal static readonly MethodInfo DbDataReader_GetFloat = typeof(DbDataReader).GetOnlyDeclaredMethod("GetFloat");

	internal static readonly MethodInfo DbDataReader_GetDouble = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDouble");

	internal static readonly MethodInfo DbDataReader_GetDateTime = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDateTime");

	internal static readonly MethodInfo DbDataReader_GetGuid = typeof(DbDataReader).GetOnlyDeclaredMethod("GetGuid");

	internal static readonly MethodInfo DbDataReader_GetByte = typeof(DbDataReader).GetOnlyDeclaredMethod("GetByte");

	internal static readonly MethodInfo DbDataReader_IsDBNull = typeof(DbDataReader).GetOnlyDeclaredMethod("IsDBNull");

	internal static readonly ConstructorInfo EntityKey_ctor_SingleKey = typeof(EntityKey).GetDeclaredConstructor(typeof(EntitySetBase), typeof(object));

	internal static readonly ConstructorInfo EntityKey_ctor_CompositeKey = typeof(EntityKey).GetDeclaredConstructor(typeof(EntitySetBase), typeof(object[]));

	internal static readonly MethodInfo EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetEntityWithChangeTrackerStrategyFunc");

	internal static readonly MethodInfo EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetEntityWithKeyStrategyStrategyFunc");

	internal static readonly MethodInfo EntityProxyTypeInfo_SetEntityWrapper = typeof(EntityProxyTypeInfo).GetOnlyDeclaredMethod("SetEntityWrapper");

	internal static readonly MethodInfo EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetNullPropertyAccessorStrategyFunc");

	internal static readonly MethodInfo EntityWrapperFactory_GetPocoEntityKeyStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetPocoEntityKeyStrategyFunc");

	internal static readonly MethodInfo EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetPocoPropertyAccessorStrategyFunc");

	internal static readonly MethodInfo EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("GetSnapshotChangeTrackingStrategyFunc");

	internal static readonly PropertyInfo EntityWrapperFactory_NullWrapper = typeof(NullEntityWrapper).GetDeclaredProperty("NullWrapper");

	internal static readonly PropertyInfo IEntityWrapper_Entity = typeof(IEntityWrapper).GetDeclaredProperty("Entity");

	internal static readonly MethodInfo IEqualityComparerOfString_Equals = typeof(IEqualityComparer<string>).GetDeclaredMethod("Equals", typeof(string), typeof(string));

	internal static readonly ConstructorInfo MaterializedDataRecord_ctor = typeof(MaterializedDataRecord).GetDeclaredConstructor(typeof(MetadataWorkspace), typeof(TypeUsage), typeof(object[]));

	internal static readonly MethodInfo RecordState_GatherData = typeof(RecordState).GetOnlyDeclaredMethod("GatherData");

	internal static readonly MethodInfo RecordState_SetNullRecord = typeof(RecordState).GetOnlyDeclaredMethod("SetNullRecord");

	internal static readonly MethodInfo Shaper_Discriminate = typeof(Shaper).GetOnlyDeclaredMethod("Discriminate");

	internal static readonly MethodInfo Shaper_GetPropertyValueWithErrorHandling = typeof(Shaper).GetOnlyDeclaredMethod("GetPropertyValueWithErrorHandling");

	internal static readonly MethodInfo Shaper_GetColumnValueWithErrorHandling = typeof(Shaper).GetOnlyDeclaredMethod("GetColumnValueWithErrorHandling");

	internal static readonly MethodInfo Shaper_GetHierarchyIdColumnValue = typeof(Shaper).GetOnlyDeclaredMethod("GetHierarchyIdColumnValue");

	internal static readonly MethodInfo Shaper_GetGeographyColumnValue = typeof(Shaper).GetOnlyDeclaredMethod("GetGeographyColumnValue");

	internal static readonly MethodInfo Shaper_GetGeometryColumnValue = typeof(Shaper).GetOnlyDeclaredMethod("GetGeometryColumnValue");

	internal static readonly MethodInfo Shaper_GetSpatialColumnValueWithErrorHandling = typeof(Shaper).GetOnlyDeclaredMethod("GetSpatialColumnValueWithErrorHandling");

	internal static readonly MethodInfo Shaper_GetSpatialPropertyValueWithErrorHandling = typeof(Shaper).GetOnlyDeclaredMethod("GetSpatialPropertyValueWithErrorHandling");

	internal static readonly MethodInfo Shaper_HandleEntity = typeof(Shaper).GetOnlyDeclaredMethod("HandleEntity");

	internal static readonly MethodInfo Shaper_HandleEntityAppendOnly = typeof(Shaper).GetOnlyDeclaredMethod("HandleEntityAppendOnly");

	internal static readonly MethodInfo Shaper_HandleEntityNoTracking = typeof(Shaper).GetOnlyDeclaredMethod("HandleEntityNoTracking");

	internal static readonly MethodInfo Shaper_HandleFullSpanCollection = typeof(Shaper).GetOnlyDeclaredMethod("HandleFullSpanCollection");

	internal static readonly MethodInfo Shaper_HandleFullSpanElement = typeof(Shaper).GetOnlyDeclaredMethod("HandleFullSpanElement");

	internal static readonly MethodInfo Shaper_HandleIEntityWithKey = typeof(Shaper).GetOnlyDeclaredMethod("HandleIEntityWithKey");

	internal static readonly MethodInfo Shaper_HandleRelationshipSpan = typeof(Shaper).GetOnlyDeclaredMethod("HandleRelationshipSpan");

	internal static readonly MethodInfo Shaper_SetColumnValue = typeof(Shaper).GetOnlyDeclaredMethod("SetColumnValue");

	internal static readonly MethodInfo Shaper_SetEntityRecordInfo = typeof(Shaper).GetOnlyDeclaredMethod("SetEntityRecordInfo");

	internal static readonly MethodInfo Shaper_SetState = typeof(Shaper).GetOnlyDeclaredMethod("SetState");

	internal static readonly MethodInfo Shaper_SetStatePassthrough = typeof(Shaper).GetOnlyDeclaredMethod("SetStatePassthrough");

	internal static readonly Expression DBNull_Value = Expression.Constant(DBNull.Value, typeof(object));

	internal static readonly ParameterExpression Shaper_Parameter = Expression.Parameter(typeof(Shaper), "shaper");

	internal static readonly Expression Shaper_Reader = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Reader"));

	internal static readonly Expression Shaper_Workspace = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Workspace"));

	internal static readonly Expression Shaper_State = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("State"));

	internal static readonly Expression Shaper_Context = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Context"));

	internal static readonly Expression Shaper_Context_Options = Expression.Property(Shaper_Context, typeof(ObjectContext).GetDeclaredProperty("ContextOptions"));

	internal static readonly Expression Shaper_ProxyCreationEnabled = Expression.Property(Shaper_Context_Options, typeof(ObjectContextOptions).GetDeclaredProperty("ProxyCreationEnabled"));

	internal static bool BinaryEquals(byte[] left, byte[] right)
	{
		if (left == null)
		{
			return right == null;
		}
		if (right == null)
		{
			return false;
		}
		if (left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < left.Length; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static Func<Shaper, TResult> Compile<TResult>(Expression body)
	{
		return BuildShaperLambda<TResult>(body).Compile();
	}

	internal static Expression<Func<Shaper, TResult>> BuildShaperLambda<TResult>(Expression body)
	{
		if (body != null)
		{
			return Expression.Lambda<Func<Shaper, TResult>>(body, new ParameterExpression[1] { Shaper_Parameter });
		}
		return null;
	}

	internal static object Compile(Type resultType, Expression body)
	{
		return CodeGenEmitter_Compile.MakeGenericMethod(resultType).Invoke(null, new object[1] { body });
	}

	internal static Expression Emit_AndAlso(IEnumerable<Expression> operands)
	{
		Expression expression = null;
		foreach (Expression operand in operands)
		{
			expression = ((expression != null) ? Expression.AndAlso(expression, operand) : operand);
		}
		return expression;
	}

	internal static Expression Emit_BitwiseOr(IEnumerable<Expression> operands)
	{
		Expression expression = null;
		foreach (Expression operand in operands)
		{
			expression = ((expression != null) ? Expression.Or(expression, operand) : operand);
		}
		return expression;
	}

	internal static Expression Emit_NullConstant(Type type)
	{
		if (type.IsNullable())
		{
			return Expression.Constant(null, type);
		}
		return Emit_EnsureType(Expression.Constant(null, typeof(object)), type);
	}

	internal static Expression Emit_WrappedNullConstant()
	{
		return Expression.Property(null, EntityWrapperFactory_NullWrapper);
	}

	internal static Expression Emit_EnsureType(Expression input, Type type)
	{
		Expression result = input;
		if (input.Type != type && !typeof(IEntityWrapper).IsAssignableFrom(input.Type))
		{
			result = ((!type.IsAssignableFrom(input.Type)) ? ((Expression)Expression.Call(CodeGenEmitter_CheckedConvert.MakeGenericMethod(input.Type, type), input)) : ((Expression)Expression.Convert(input, type)));
		}
		return result;
	}

	internal static Expression Emit_EnsureTypeAndWrap(Expression input, Expression keyReader, Expression entitySetReader, Type requestedType, Type identityType, Type actualType, MergeOption mergeOption, bool isProxy)
	{
		Expression input2 = Emit_EnsureType(input, requestedType);
		if (!requestedType.IsClass())
		{
			input2 = Emit_EnsureType(input, typeof(object));
		}
		input2 = Emit_EnsureType(input2, actualType);
		return CreateEntityWrapper(input2, keyReader, entitySetReader, actualType, identityType, mergeOption, isProxy);
	}

	internal static Expression CreateEntityWrapper(Expression input, Expression keyReader, Expression entitySetReader, Type actualType, Type identityType, MergeOption mergeOption, bool isProxy)
	{
		bool flag = actualType.OverridesEqualsOrGetHashCode();
		bool flag2 = typeof(IEntityWithKey).IsAssignableFrom(actualType);
		bool flag3 = typeof(IEntityWithRelationships).IsAssignableFrom(actualType);
		bool flag4 = typeof(IEntityWithChangeTracker).IsAssignableFrom(actualType);
		Expression expression;
		if (flag3 && flag4 && flag2 && !isProxy)
		{
			expression = Expression.New(typeof(LightweightEntityWrapper<>).MakeGenericType(actualType).GetDeclaredConstructor(actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type), typeof(bool)), input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)), Expression.Constant(identityType, typeof(Type)), Expression.Constant(flag, typeof(bool)));
		}
		else
		{
			Expression expression2 = ((!flag3 || isProxy) ? Expression.Call(EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc) : Expression.Call(EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc));
			Expression expression3 = (flag2 ? Expression.Call(EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc) : Expression.Call(EntityWrapperFactory_GetPocoEntityKeyStrategyFunc));
			Expression expression4 = (flag4 ? Expression.Call(EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc) : Expression.Call(EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc));
			expression = Expression.New((flag3 ? typeof(EntityWrapperWithRelationships<>).MakeGenericType(actualType) : typeof(EntityWrapperWithoutRelationships<>).MakeGenericType(actualType)).GetDeclaredConstructor(actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type), typeof(Func<object, IPropertyAccessorStrategy>), typeof(Func<object, IChangeTrackingStrategy>), typeof(Func<object, IEntityKeyStrategy>), typeof(bool)), input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)), Expression.Constant(identityType, typeof(Type)), expression2, expression4, expression3, Expression.Constant(flag, typeof(bool)));
		}
		return Expression.Convert(expression, typeof(IEntityWrapper));
	}

	internal static Expression Emit_UnwrapAndEnsureType(Expression input, Type type)
	{
		return Emit_EnsureType(Expression.Property(input, IEntityWrapper_Entity), type);
	}

	internal static TTarget CheckedConvert<TSource, TTarget>(TSource value)
	{
		try
		{
			return (TTarget)(object)value;
		}
		catch (InvalidCastException)
		{
			Type type = value.GetType();
			if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(CompensatingCollection<>))
			{
				type = typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments());
			}
			throw EntityUtil.ValueInvalidCast(type, typeof(TTarget));
		}
		catch (NullReferenceException)
		{
			throw new InvalidOperationException(Strings.Materializer_NullReferenceCast(typeof(TTarget).Name));
		}
	}

	internal static Expression Emit_Equal(Expression left, Expression right)
	{
		if (typeof(byte[]) == left.Type)
		{
			return Expression.Call(CodeGenEmitter_BinaryEquals, left, right);
		}
		return Expression.Equal(left, right);
	}

	internal static Expression Emit_EntityKey_HasValue(SimpleColumnMap[] keyColumns)
	{
		return Expression.Not(Emit_Reader_IsDBNull(keyColumns[0]));
	}

	internal static Expression Emit_Reader_GetValue(int ordinal, Type type)
	{
		return Emit_EnsureType(Expression.Call(Shaper_Reader, DbDataReader_GetValue, Expression.Constant(ordinal)), type);
	}

	internal static Expression Emit_Reader_IsDBNull(int ordinal)
	{
		return Expression.Call(Shaper_Reader, DbDataReader_IsDBNull, Expression.Constant(ordinal));
	}

	internal static Expression Emit_Reader_IsDBNull(ColumnMap columnMap)
	{
		return Emit_Reader_IsDBNull(((ScalarColumnMap)columnMap).ColumnPos);
	}

	internal static Expression Emit_Conditional_NotDBNull(Expression result, int ordinal, Type columnType)
	{
		result = Expression.Condition(Emit_Reader_IsDBNull(ordinal), Expression.Constant(TypeSystem.GetDefaultValue(columnType), columnType), result);
		return result;
	}

	internal static MethodInfo GetReaderMethod(Type type, out bool isNullable)
	{
		isNullable = false;
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if (null != underlyingType)
		{
			isNullable = true;
			type = underlyingType;
		}
		MethodInfo result;
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.String:
			result = DbDataReader_GetString;
			isNullable = true;
			break;
		case TypeCode.Int16:
			result = DbDataReader_GetInt16;
			break;
		case TypeCode.Int32:
			result = DbDataReader_GetInt32;
			break;
		case TypeCode.Int64:
			result = DbDataReader_GetInt64;
			break;
		case TypeCode.Boolean:
			result = DbDataReader_GetBoolean;
			break;
		case TypeCode.Decimal:
			result = DbDataReader_GetDecimal;
			break;
		case TypeCode.Double:
			result = DbDataReader_GetDouble;
			break;
		case TypeCode.Single:
			result = DbDataReader_GetFloat;
			break;
		case TypeCode.DateTime:
			result = DbDataReader_GetDateTime;
			break;
		case TypeCode.Byte:
			result = DbDataReader_GetByte;
			break;
		default:
			if (typeof(Guid) == type)
			{
				result = DbDataReader_GetGuid;
				break;
			}
			if (typeof(TimeSpan) == type || typeof(DateTimeOffset) == type)
			{
				result = DbDataReader_GetValue;
				break;
			}
			if (typeof(object) == type)
			{
				result = DbDataReader_GetValue;
				break;
			}
			result = DbDataReader_GetValue;
			isNullable = true;
			break;
		}
		return result;
	}

	internal static Expression Emit_Shaper_GetPropertyValueWithErrorHandling(Type propertyType, int ordinal, string propertyName, string typeName, TypeUsage columnType)
	{
		if (Helper.IsSpatialType(columnType, out var spatialType))
		{
			return Expression.Call(Shaper_Parameter, Shaper_GetSpatialPropertyValueWithErrorHandling.MakeGenericMethod(propertyType), Expression.Constant(ordinal), Expression.Constant(propertyName), Expression.Constant(typeName), Expression.Constant(spatialType, typeof(PrimitiveTypeKind)));
		}
		return Expression.Call(Shaper_Parameter, Shaper_GetPropertyValueWithErrorHandling.MakeGenericMethod(propertyType), Expression.Constant(ordinal), Expression.Constant(propertyName), Expression.Constant(typeName));
	}

	internal static Expression Emit_Shaper_GetColumnValueWithErrorHandling(Type resultType, int ordinal, TypeUsage columnType)
	{
		if (Helper.IsSpatialType(columnType, out var spatialType))
		{
			spatialType = (Helper.IsGeographicType((PrimitiveType)columnType.EdmType) ? PrimitiveTypeKind.Geography : PrimitiveTypeKind.Geometry);
			return Expression.Call(Shaper_Parameter, Shaper_GetSpatialColumnValueWithErrorHandling.MakeGenericMethod(resultType), Expression.Constant(ordinal), Expression.Constant(spatialType, typeof(PrimitiveTypeKind)));
		}
		return Expression.Call(Shaper_Parameter, Shaper_GetColumnValueWithErrorHandling.MakeGenericMethod(resultType), Expression.Constant(ordinal));
	}

	internal static Expression Emit_Shaper_GetHierarchyIdColumnValue(int ordinal)
	{
		return Expression.Call(Shaper_Parameter, Shaper_GetHierarchyIdColumnValue, Expression.Constant(ordinal));
	}

	internal static Expression Emit_Shaper_GetGeographyColumnValue(int ordinal)
	{
		return Expression.Call(Shaper_Parameter, Shaper_GetGeographyColumnValue, Expression.Constant(ordinal));
	}

	internal static Expression Emit_Shaper_GetGeometryColumnValue(int ordinal)
	{
		return Expression.Call(Shaper_Parameter, Shaper_GetGeometryColumnValue, Expression.Constant(ordinal));
	}

	internal static Expression Emit_Shaper_GetState(int stateSlotNumber, Type type)
	{
		return Emit_EnsureType(Expression.ArrayIndex(Shaper_State, Expression.Constant(stateSlotNumber)), type);
	}

	internal static Expression Emit_Shaper_SetState(int stateSlotNumber, Expression value)
	{
		return Expression.Call(Shaper_Parameter, Shaper_SetState.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
	}

	internal static Expression Emit_Shaper_SetStatePassthrough(int stateSlotNumber, Expression value)
	{
		return Expression.Call(Shaper_Parameter, Shaper_SetStatePassthrough.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
	}
}
