using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Common.EntitySql;

internal static class FunctionOverloadResolver
{
	internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata, IList<TypeUsage> argTypes, bool isGroupAggregateFunction, out bool isAmbiguous)
	{
		return ResolveFunctionOverloads(functionsMetadata, argTypes, (EdmFunction edmFunction) => edmFunction.Parameters, (FunctionParameter functionParameter) => functionParameter.TypeUsage, (FunctionParameter functionParameter) => functionParameter.Mode, (TypeUsage argType) => TypeSemantics.FlattenType(argType), (TypeUsage paramType, TypeUsage argType) => TypeSemantics.FlattenType(paramType), (TypeUsage fromType, TypeUsage toType) => TypeSemantics.IsPromotableTo(fromType, toType), (TypeUsage fromType, TypeUsage toType) => TypeSemantics.IsStructurallyEqual(fromType, toType), isGroupAggregateFunction, out isAmbiguous);
	}

	internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata, IList<TypeUsage> argTypes, Func<TypeUsage, IEnumerable<TypeUsage>> flattenArgumentType, Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType, Func<TypeUsage, TypeUsage, bool> isPromotableTo, Func<TypeUsage, TypeUsage, bool> isStructurallyEqual, bool isGroupAggregateFunction, out bool isAmbiguous)
	{
		return ResolveFunctionOverloads(functionsMetadata, argTypes, (EdmFunction edmFunction) => edmFunction.Parameters, (FunctionParameter functionParameter) => functionParameter.TypeUsage, (FunctionParameter functionParameter) => functionParameter.Mode, flattenArgumentType, flattenParameterType, isPromotableTo, isStructurallyEqual, isGroupAggregateFunction, out isAmbiguous);
	}

	internal static TFunctionMetadata ResolveFunctionOverloads<TFunctionMetadata, TFunctionParameterMetadata>(IList<TFunctionMetadata> functionsMetadata, IList<TypeUsage> argTypes, Func<TFunctionMetadata, IList<TFunctionParameterMetadata>> getSignatureParams, Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage, Func<TFunctionParameterMetadata, ParameterMode> getParameterMode, Func<TypeUsage, IEnumerable<TypeUsage>> flattenArgumentType, Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType, Func<TypeUsage, TypeUsage, bool> isPromotableTo, Func<TypeUsage, TypeUsage, bool> isStructurallyEqual, bool isGroupAggregateFunction, out bool isAmbiguous) where TFunctionMetadata : class
	{
		List<TypeUsage> list = new List<TypeUsage>(argTypes.Count);
		foreach (TypeUsage argType in argTypes)
		{
			list.AddRange(flattenArgumentType(argType));
		}
		TFunctionMetadata val = null;
		isAmbiguous = false;
		List<int[]> list2 = new List<int[]>(functionsMetadata.Count);
		int[] bestCandidateRank = null;
		int i = 0;
		int num = int.MinValue;
		for (; i < functionsMetadata.Count; i++)
		{
			if (TryRankFunctionParameters(argTypes, list, getSignatureParams(functionsMetadata[i]), getParameterTypeUsage, getParameterMode, flattenParameterType, isPromotableTo, isStructurallyEqual, isGroupAggregateFunction, out var totalRank, out var parameterRanks))
			{
				if (totalRank == num)
				{
					isAmbiguous = true;
				}
				else if (totalRank > num)
				{
					isAmbiguous = false;
					num = totalRank;
					val = functionsMetadata[i];
					bestCandidateRank = parameterRanks;
				}
				list2.Add(parameterRanks);
			}
		}
		if (val != null && !isAmbiguous && list.Count > 1 && list2.Count > 1)
		{
			isAmbiguous = list2.Any(delegate(int[] rank)
			{
				if (bestCandidateRank != rank)
				{
					for (int j = 0; j < rank.Length; j++)
					{
						if (bestCandidateRank[j] < rank[j])
						{
							return true;
						}
					}
				}
				return false;
			});
		}
		if (!isAmbiguous)
		{
			return val;
		}
		return null;
	}

	private static bool TryRankFunctionParameters<TFunctionParameterMetadata>(IList<TypeUsage> argumentList, IList<TypeUsage> flatArgumentList, IList<TFunctionParameterMetadata> overloadParamList, Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage, Func<TFunctionParameterMetadata, ParameterMode> getParameterMode, Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType, Func<TypeUsage, TypeUsage, bool> isPromotableTo, Func<TypeUsage, TypeUsage, bool> isStructurallyEqual, bool isGroupAggregateFunction, out int totalRank, out int[] parameterRanks)
	{
		totalRank = 0;
		parameterRanks = null;
		if (argumentList.Count != overloadParamList.Count)
		{
			return false;
		}
		List<TypeUsage> list = new List<TypeUsage>(flatArgumentList.Count);
		for (int i = 0; i < overloadParamList.Count; i++)
		{
			TypeUsage typeUsage = argumentList[i];
			TypeUsage typeUsage2 = getParameterTypeUsage(overloadParamList[i]);
			ParameterMode parameterMode = getParameterMode(overloadParamList[i]);
			if (parameterMode != 0 && parameterMode != ParameterMode.InOut)
			{
				return false;
			}
			if (isGroupAggregateFunction)
			{
				if (!TypeSemantics.IsCollectionType(typeUsage2))
				{
					throw new EntitySqlException(Strings.InvalidArgumentTypeForAggregateFunction);
				}
				typeUsage2 = TypeHelpers.GetElementTypeUsage(typeUsage2);
			}
			if (!isPromotableTo(typeUsage, typeUsage2))
			{
				return false;
			}
			list.AddRange(flattenParameterType(typeUsage2, typeUsage));
		}
		parameterRanks = new int[list.Count];
		for (int j = 0; j < parameterRanks.Length; j++)
		{
			int promotionRank = GetPromotionRank(flatArgumentList[j], list[j], isPromotableTo, isStructurallyEqual);
			totalRank += promotionRank;
			parameterRanks[j] = promotionRank;
		}
		return true;
	}

	private static int GetPromotionRank(TypeUsage fromType, TypeUsage toType, Func<TypeUsage, TypeUsage, bool> isPromotableTo, Func<TypeUsage, TypeUsage, bool> isStructurallyEqual)
	{
		if (isStructurallyEqual(fromType, toType))
		{
			return 0;
		}
		PrimitiveType primitiveType = fromType.EdmType as PrimitiveType;
		PrimitiveType primitiveType2 = toType.EdmType as PrimitiveType;
		if (primitiveType != null && primitiveType2 != null)
		{
			if (Helper.AreSameSpatialUnionType(primitiveType, primitiveType2))
			{
				return 0;
			}
			int num = ((IList<PrimitiveType>)EdmProviderManifest.Instance.GetPromotionTypes(primitiveType)).IndexOf(primitiveType2);
			if (num < 0)
			{
				throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 1, null);
			}
			return -num;
		}
		EntityTypeBase entityTypeBase = fromType.EdmType as EntityTypeBase;
		EntityTypeBase entityTypeBase2 = toType.EdmType as EntityTypeBase;
		if (entityTypeBase != null && entityTypeBase2 != null)
		{
			int num2 = 0;
			EdmType edmType = entityTypeBase;
			while (edmType != entityTypeBase2 && edmType != null)
			{
				edmType = edmType.BaseType;
				num2++;
			}
			if (edmType == null)
			{
				throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 2, null);
			}
			return -num2;
		}
		throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 3, null);
	}
}
