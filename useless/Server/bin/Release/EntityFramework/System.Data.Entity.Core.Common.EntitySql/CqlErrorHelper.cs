using System.Collections.Generic;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Common.EntitySql;

internal static class CqlErrorHelper
{
	internal static void ReportFunctionOverloadError(MethodExpr functionExpr, EdmFunction functionType, List<TypeUsage> argTypes)
	{
		string value = "";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(functionType.Name).Append("(");
		for (int i = 0; i < argTypes.Count; i++)
		{
			stringBuilder.Append(value);
			stringBuilder.Append((argTypes[i] != null) ? argTypes[i].EdmType.FullName : "NULL");
			value = ", ";
		}
		stringBuilder.Append(")");
		throw EntitySqlException.Create(errorDescription: ((!TypeSemantics.IsAggregateFunction(functionType)) ? (TypeHelpers.IsCanonicalFunction(functionType) ? new Func<object, object, object, string>(Strings.NoCanonicalFunctionOverloadMatch) : new Func<object, object, object, string>(Strings.NoFunctionOverloadMatch)) : (TypeHelpers.IsCanonicalFunction(functionType) ? new Func<object, object, object, string>(Strings.NoCanonicalAggrFunctionOverloadMatch) : new Func<object, object, object, string>(Strings.NoAggrFunctionOverloadMatch)))(functionType.NamespaceName, functionType.Name, stringBuilder.ToString()), commandText: functionExpr.ErrCtx.CommandText, errorPosition: functionExpr.ErrCtx.InputPosition, errorContextInfo: Strings.CtxFunction(functionType.Name), loadErrorContextInfoFromResource: false, innerException: null);
	}

	internal static void ReportAliasAlreadyUsedError(string aliasName, ErrorContext errCtx, string contextMessage)
	{
		throw EntitySqlException.Create(errCtx, string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[2]
		{
			Strings.AliasNameAlreadyUsed(aliasName),
			contextMessage
		}), null);
	}

	internal static void ReportIncompatibleCommonType(ErrorContext errCtx, TypeUsage leftType, TypeUsage rightType)
	{
		ReportIncompatibleCommonType(errCtx, leftType, rightType, leftType, rightType);
		throw EntitySqlException.Create(errCtx, Strings.ArgumentTypesAreIncompatible(leftType.Identity, rightType.Identity), null);
	}

	private static void ReportIncompatibleCommonType(ErrorContext errCtx, TypeUsage rootLeftType, TypeUsage rootRightType, TypeUsage leftType, TypeUsage rightType)
	{
		TypeUsage commonType = null;
		bool flag = rootLeftType == leftType;
		string empty = string.Empty;
		if (leftType.EdmType.BuiltInTypeKind != rightType.EdmType.BuiltInTypeKind)
		{
			throw EntitySqlException.Create(errCtx, Strings.TypeKindMismatch(GetReadableTypeKind(leftType), GetReadableTypeName(leftType), GetReadableTypeKind(rightType), GetReadableTypeName(rightType)), null);
		}
		switch (leftType.EdmType.BuiltInTypeKind)
		{
		case BuiltInTypeKind.RowType:
		{
			RowType rowType = (RowType)leftType.EdmType;
			RowType rowType2 = (RowType)rightType.EdmType;
			if (rowType.Members.Count != rowType2.Members.Count)
			{
				empty = ((!flag) ? Strings.InvalidRowType(GetReadableTypeName(rowType), GetReadableTypeName(rootLeftType), GetReadableTypeName(rowType2), GetReadableTypeName(rootRightType)) : Strings.InvalidRootRowType(GetReadableTypeName(rowType), GetReadableTypeName(rowType2)));
				throw EntitySqlException.Create(errCtx, empty, null);
			}
			for (int j = 0; j < rowType.Members.Count; j++)
			{
				ReportIncompatibleCommonType(errCtx, rootLeftType, rootRightType, rowType.Members[j].TypeUsage, rowType2.Members[j].TypeUsage);
			}
			break;
		}
		case BuiltInTypeKind.CollectionType:
		case BuiltInTypeKind.RefType:
			ReportIncompatibleCommonType(errCtx, rootLeftType, rootRightType, TypeHelpers.GetElementTypeUsage(leftType), TypeHelpers.GetElementTypeUsage(rightType));
			break;
		case BuiltInTypeKind.EntityType:
			if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
			{
				empty = ((!flag) ? Strings.InvalidEntityTypeArgument(GetReadableTypeName(leftType), GetReadableTypeName(rootLeftType), GetReadableTypeName(rightType), GetReadableTypeName(rootRightType)) : Strings.InvalidEntityRootTypeArgument(GetReadableTypeName(leftType), GetReadableTypeName(rightType)));
				throw EntitySqlException.Create(errCtx, empty, null);
			}
			break;
		case BuiltInTypeKind.ComplexType:
		{
			ComplexType complexType = (ComplexType)leftType.EdmType;
			ComplexType complexType2 = (ComplexType)rightType.EdmType;
			if (complexType.Members.Count != complexType2.Members.Count)
			{
				empty = ((!flag) ? Strings.InvalidComplexType(GetReadableTypeName(complexType), GetReadableTypeName(rootLeftType), GetReadableTypeName(complexType2), GetReadableTypeName(rootRightType)) : Strings.InvalidRootComplexType(GetReadableTypeName(complexType), GetReadableTypeName(complexType2)));
				throw EntitySqlException.Create(errCtx, empty, null);
			}
			for (int i = 0; i < complexType.Members.Count; i++)
			{
				ReportIncompatibleCommonType(errCtx, rootLeftType, rootRightType, complexType.Members[i].TypeUsage, complexType2.Members[i].TypeUsage);
			}
			break;
		}
		default:
			if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
			{
				empty = ((!flag) ? Strings.InvalidPlaceholderTypeArgument(GetReadableTypeKind(leftType), GetReadableTypeName(leftType), GetReadableTypeName(rootLeftType), GetReadableTypeKind(rightType), GetReadableTypeName(rightType), GetReadableTypeName(rootRightType)) : Strings.InvalidPlaceholderRootTypeArgument(GetReadableTypeKind(leftType), GetReadableTypeName(leftType), GetReadableTypeKind(rightType), GetReadableTypeName(rightType)));
				throw EntitySqlException.Create(errCtx, empty, null);
			}
			break;
		}
	}

	private static string GetReadableTypeName(TypeUsage type)
	{
		return GetReadableTypeName(type.EdmType);
	}

	private static string GetReadableTypeName(EdmType type)
	{
		if (type.BuiltInTypeKind == BuiltInTypeKind.RowType || type.BuiltInTypeKind == BuiltInTypeKind.CollectionType || type.BuiltInTypeKind == BuiltInTypeKind.RefType)
		{
			return type.Name;
		}
		return type.FullName;
	}

	private static string GetReadableTypeKind(TypeUsage type)
	{
		return GetReadableTypeKind(type.EdmType);
	}

	private static string GetReadableTypeKind(EdmType type)
	{
		string empty = string.Empty;
		return type.BuiltInTypeKind switch
		{
			BuiltInTypeKind.RowType => Strings.LocalizedRow, 
			BuiltInTypeKind.CollectionType => Strings.LocalizedCollection, 
			BuiltInTypeKind.RefType => Strings.LocalizedReference, 
			BuiltInTypeKind.EntityType => Strings.LocalizedEntity, 
			BuiltInTypeKind.ComplexType => Strings.LocalizedComplex, 
			BuiltInTypeKind.PrimitiveType => Strings.LocalizedPrimitive, 
			_ => type.BuiltInTypeKind.ToString(), 
		} + " " + Strings.LocalizedType;
	}
}
