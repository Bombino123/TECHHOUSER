using System;

namespace dnlib.DotNet;

[Flags]
public enum SigComparerOptions : uint
{
	DontCompareTypeScope = 1u,
	CompareMethodFieldDeclaringType = 2u,
	ComparePropertyDeclaringType = 4u,
	CompareEventDeclaringType = 8u,
	CompareDeclaringTypes = 0xEu,
	CompareSentinelParams = 0x10u,
	CompareAssemblyPublicKeyToken = 0x20u,
	CompareAssemblyVersion = 0x40u,
	CompareAssemblyLocale = 0x80u,
	TypeRefCanReferenceGlobalType = 0x100u,
	DontCompareReturnType = 0x200u,
	CaseInsensitiveTypeNamespaces = 0x800u,
	CaseInsensitiveTypeNames = 0x1000u,
	CaseInsensitiveTypes = 0x1800u,
	CaseInsensitiveMethodFieldNames = 0x2000u,
	CaseInsensitivePropertyNames = 0x4000u,
	CaseInsensitiveEventNames = 0x8000u,
	CaseInsensitiveAll = 0xF800u,
	PrivateScopeFieldIsComparable = 0x10000u,
	PrivateScopeMethodIsComparable = 0x20000u,
	PrivateScopeIsComparable = 0x30000u,
	RawSignatureCompare = 0x40000u,
	IgnoreModifiers = 0x80000u,
	MscorlibIsNotSpecial = 0x100000u,
	DontProjectWinMDRefs = 0x200000u,
	DontCheckTypeEquivalence = 0x400000u,
	IgnoreMultiDimensionalArrayLowerBoundsAndSizes = 0x800000u,
	ReferenceCompareForMemberDefsInSameModule = 0x1000000u
}
