namespace System.Data.Entity.Core.Mapping;

internal enum MappingErrorCode
{
	Value = 2000,
	InvalidContent = 2001,
	InvalidEntityContainer = 2002,
	InvalidEntitySet = 2003,
	InvalidEntityType = 2004,
	InvalidAssociationSet = 2005,
	InvalidAssociationType = 2006,
	InvalidTable = 2007,
	InvalidComplexType = 2008,
	InvalidEdmMember = 2009,
	InvalidStorageMember = 2010,
	TableMappingFragmentExpected = 2011,
	SetMappingExpected = 2012,
	DuplicateSetMapping = 2014,
	DuplicateTypeMapping = 2015,
	ConditionError = 2016,
	RootMappingElementMissing = 2018,
	IncompatibleMemberMapping = 2019,
	InvalidEnumValue = 2023,
	XmlSchemaParsingError = 2024,
	XmlSchemaValidationError = 2025,
	AmbiguousModificationFunctionMappingForAssociationSet = 2026,
	MissingSetClosureInModificationFunctionMapping = 2027,
	MissingModificationFunctionMappingForEntityType = 2028,
	InvalidTableNameAttributeWithModificationFunctionMapping = 2029,
	InvalidModificationFunctionMappingForMultipleTypes = 2030,
	AmbiguousResultBindingInModificationFunctionMapping = 2031,
	InvalidAssociationSetRoleInModificationFunctionMapping = 2032,
	InvalidAssociationSetCardinalityInModificationFunctionMapping = 2033,
	RedundantEntityTypeMappingInModificationFunctionMapping = 2034,
	MissingVersionInModificationFunctionMapping = 2035,
	InvalidVersionInModificationFunctionMapping = 2036,
	InvalidParameterInModificationFunctionMapping = 2037,
	ParameterBoundTwiceInModificationFunctionMapping = 2038,
	CSpaceMemberMappedToMultipleSSpaceMemberWithDifferentTypes = 2039,
	NoEquivalentStorePrimitiveTypeFound = 2040,
	NoEquivalentStorePrimitiveTypeWithFacetsFound = 2041,
	InvalidModificationFunctionMappingPropertyParameterTypeMismatch = 2042,
	InvalidModificationFunctionMappingMultipleEndsOfAssociationMapped = 2043,
	InvalidModificationFunctionMappingUnknownFunction = 2044,
	InvalidModificationFunctionMappingAmbiguousFunction = 2045,
	InvalidModificationFunctionMappingNotValidFunction = 2046,
	InvalidModificationFunctionMappingNotValidFunctionParameter = 2047,
	InvalidModificationFunctionMappingAssociationSetNotMappedForOperation = 2048,
	InvalidModificationFunctionMappingAssociationEndMappingInvalidForEntityType = 2049,
	MappingFunctionImportStoreFunctionDoesNotExist = 2050,
	MappingFunctionImportStoreFunctionAmbiguous = 2051,
	MappingFunctionImportFunctionImportDoesNotExist = 2052,
	MappingFunctionImportFunctionImportMappedMultipleTimes = 2053,
	MappingFunctionImportTargetFunctionMustBeNonComposable = 2054,
	MappingFunctionImportTargetParameterHasNoCorrespondingImportParameter = 2055,
	MappingFunctionImportImportParameterHasNoCorrespondingTargetParameter = 2056,
	MappingFunctionImportIncompatibleParameterMode = 2057,
	MappingFunctionImportIncompatibleParameterType = 2058,
	MappingFunctionImportRowsAffectedParameterDoesNotExist = 2059,
	MappingFunctionImportRowsAffectedParameterHasWrongType = 2060,
	MappingFunctionImportRowsAffectedParameterHasWrongMode = 2061,
	EmptyContainerMapping = 2062,
	EmptySetMapping = 2063,
	TableNameAttributeWithQueryView = 2064,
	EmptyQueryView = 2065,
	PropertyMapsWithQueryView = 2066,
	MissingSetClosureInQueryViews = 2067,
	InvalidQueryView = 2068,
	InvalidQueryViewResultType = 2069,
	ItemWithSameNameExistsBothInCSpaceAndSSpace = 2070,
	MappingUnsupportedExpressionKindQueryView = 2071,
	MappingUnsupportedScanTargetQueryView = 2072,
	MappingUnsupportedPropertyKindQueryView = 2073,
	MappingUnsupportedInitializationQueryView = 2074,
	MappingFunctionImportEntityTypeMappingForFunctionNotReturningEntitySet = 2075,
	MappingFunctionImportAmbiguousTypeConditions = 2076,
	MappingOfAbstractType = 2078,
	StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping = 2079,
	TypeNameForFirstQueryView = 2080,
	NoTypeNameForTypeSpecificQueryView = 2081,
	QueryViewExistsForEntitySetAndType = 2082,
	TypeNameContainsMultipleTypesForQueryView = 2083,
	IsTypeOfQueryViewForBaseType = 2084,
	InvalidTypeInScalarProperty = 2085,
	AlreadyMappedStorageEntityContainer = 2086,
	UnsupportedQueryViewInEntityContainerMapping = 2087,
	MappingAllQueryViewAtCompileTime = 2088,
	MappingNoViewsCanBeGenerated = 2089,
	MappingStoreProviderReturnsNullEdmType = 2090,
	DuplicateMemberMapping = 2092,
	MappingFunctionImportUnexpectedEntityTypeMapping = 2093,
	MappingFunctionImportUnexpectedComplexTypeMapping = 2094,
	DistinctFragmentInReadWriteContainer = 2096,
	EntitySetMismatchOnAssociationSetEnd = 2097,
	InvalidModificationFunctionMappingAssociationEndForeignKey = 2098,
	CannotLoadDifferentVersionOfSchemaInTheSameItemCollection = 2100,
	MappingDifferentMappingEdmStoreVersion = 2101,
	MappingDifferentEdmStoreVersion = 2102,
	UnmappedFunctionImport = 2103,
	MappingFunctionImportReturnTypePropertyNotMapped = 2104,
	InvalidType = 2106,
	MappingFunctionImportTVFExpected = 2108,
	MappingFunctionImportScalarMappingTypeMismatch = 2109,
	MappingFunctionImportScalarMappingToMulticolumnTVF = 2110,
	MappingFunctionImportTargetFunctionMustBeComposable = 2111,
	UnsupportedFunctionCallInQueryView = 2112,
	FunctionResultMappingCountMismatch = 2113,
	MappingFunctionImportCannotInferTargetFunctionKeys = 2114
}
