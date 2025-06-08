using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace System.Data.Entity.Resources;

[GeneratedCode("Resources.tt", "1.0.0.0")]
internal sealed class EntityRes
{
	internal const string AutomaticMigration = "AutomaticMigration";

	internal const string BootstrapMigration = "BootstrapMigration";

	internal const string InitialCreate = "InitialCreate";

	internal const string AutomaticDataLoss = "AutomaticDataLoss";

	internal const string LoggingAutoMigrate = "LoggingAutoMigrate";

	internal const string LoggingRevertAutoMigrate = "LoggingRevertAutoMigrate";

	internal const string LoggingApplyMigration = "LoggingApplyMigration";

	internal const string LoggingRevertMigration = "LoggingRevertMigration";

	internal const string LoggingSeedingDatabase = "LoggingSeedingDatabase";

	internal const string LoggingPendingMigrations = "LoggingPendingMigrations";

	internal const string LoggingPendingMigrationsDown = "LoggingPendingMigrationsDown";

	internal const string LoggingNoExplicitMigrations = "LoggingNoExplicitMigrations";

	internal const string LoggingAlreadyAtTarget = "LoggingAlreadyAtTarget";

	internal const string LoggingTargetDatabase = "LoggingTargetDatabase";

	internal const string LoggingTargetDatabaseFormat = "LoggingTargetDatabaseFormat";

	internal const string LoggingExplicit = "LoggingExplicit";

	internal const string UpgradingHistoryTable = "UpgradingHistoryTable";

	internal const string MetadataOutOfDate = "MetadataOutOfDate";

	internal const string MigrationNotFound = "MigrationNotFound";

	internal const string PartialFkOperation = "PartialFkOperation";

	internal const string AutoNotValidTarget = "AutoNotValidTarget";

	internal const string AutoNotValidForScriptWindows = "AutoNotValidForScriptWindows";

	internal const string ContextNotConstructible = "ContextNotConstructible";

	internal const string AmbiguousMigrationName = "AmbiguousMigrationName";

	internal const string AutomaticDisabledException = "AutomaticDisabledException";

	internal const string DownScriptWindowsNotSupported = "DownScriptWindowsNotSupported";

	internal const string AssemblyMigrator_NoConfigurationWithName = "AssemblyMigrator_NoConfigurationWithName";

	internal const string AssemblyMigrator_MultipleConfigurationsWithName = "AssemblyMigrator_MultipleConfigurationsWithName";

	internal const string AssemblyMigrator_NoConfiguration = "AssemblyMigrator_NoConfiguration";

	internal const string AssemblyMigrator_MultipleConfigurations = "AssemblyMigrator_MultipleConfigurations";

	internal const string MigrationsNamespaceNotUnderRootNamespace = "MigrationsNamespaceNotUnderRootNamespace";

	internal const string UnableToDispatchAddOrUpdate = "UnableToDispatchAddOrUpdate";

	internal const string NoSqlGeneratorForProvider = "NoSqlGeneratorForProvider";

	internal const string ToolingFacade_AssemblyNotFound = "ToolingFacade_AssemblyNotFound";

	internal const string ArgumentIsNullOrWhitespace = "ArgumentIsNullOrWhitespace";

	internal const string EntityTypeConfigurationMismatch = "EntityTypeConfigurationMismatch";

	internal const string ComplexTypeConfigurationMismatch = "ComplexTypeConfigurationMismatch";

	internal const string KeyPropertyNotFound = "KeyPropertyNotFound";

	internal const string ForeignKeyPropertyNotFound = "ForeignKeyPropertyNotFound";

	internal const string PropertyNotFound = "PropertyNotFound";

	internal const string NavigationPropertyNotFound = "NavigationPropertyNotFound";

	internal const string InvalidPropertyExpression = "InvalidPropertyExpression";

	internal const string InvalidComplexPropertyExpression = "InvalidComplexPropertyExpression";

	internal const string InvalidPropertiesExpression = "InvalidPropertiesExpression";

	internal const string InvalidComplexPropertiesExpression = "InvalidComplexPropertiesExpression";

	internal const string DuplicateStructuralTypeConfiguration = "DuplicateStructuralTypeConfiguration";

	internal const string ConflictingPropertyConfiguration = "ConflictingPropertyConfiguration";

	internal const string ConflictingTypeAnnotation = "ConflictingTypeAnnotation";

	internal const string ConflictingColumnConfiguration = "ConflictingColumnConfiguration";

	internal const string ConflictingConfigurationValue = "ConflictingConfigurationValue";

	internal const string ConflictingAnnotationValue = "ConflictingAnnotationValue";

	internal const string ConflictingIndexAttributeProperty = "ConflictingIndexAttributeProperty";

	internal const string ConflictingIndexAttribute = "ConflictingIndexAttribute";

	internal const string ConflictingIndexAttributesOnProperty = "ConflictingIndexAttributesOnProperty";

	internal const string IncompatibleTypes = "IncompatibleTypes";

	internal const string AnnotationSerializeWrongType = "AnnotationSerializeWrongType";

	internal const string AnnotationSerializeBadFormat = "AnnotationSerializeBadFormat";

	internal const string ConflictWhenConsolidating = "ConflictWhenConsolidating";

	internal const string OrderConflictWhenConsolidating = "OrderConflictWhenConsolidating";

	internal const string CodeFirstInvalidComplexType = "CodeFirstInvalidComplexType";

	internal const string InvalidEntityType = "InvalidEntityType";

	internal const string SimpleNameCollision = "SimpleNameCollision";

	internal const string NavigationInverseItself = "NavigationInverseItself";

	internal const string ConflictingConstraint = "ConflictingConstraint";

	internal const string ConflictingInferredColumnType = "ConflictingInferredColumnType";

	internal const string ConflictingMapping = "ConflictingMapping";

	internal const string ConflictingCascadeDeleteOperation = "ConflictingCascadeDeleteOperation";

	internal const string ConflictingMultiplicities = "ConflictingMultiplicities";

	internal const string MaxLengthAttributeConvention_InvalidMaxLength = "MaxLengthAttributeConvention_InvalidMaxLength";

	internal const string StringLengthAttributeConvention_InvalidMaximumLength = "StringLengthAttributeConvention_InvalidMaximumLength";

	internal const string ModelGeneration_UnableToDetermineKeyOrder = "ModelGeneration_UnableToDetermineKeyOrder";

	internal const string ForeignKeyAttributeConvention_EmptyKey = "ForeignKeyAttributeConvention_EmptyKey";

	internal const string ForeignKeyAttributeConvention_InvalidKey = "ForeignKeyAttributeConvention_InvalidKey";

	internal const string ForeignKeyAttributeConvention_InvalidNavigationProperty = "ForeignKeyAttributeConvention_InvalidNavigationProperty";

	internal const string ForeignKeyAttributeConvention_OrderRequired = "ForeignKeyAttributeConvention_OrderRequired";

	internal const string InversePropertyAttributeConvention_PropertyNotFound = "InversePropertyAttributeConvention_PropertyNotFound";

	internal const string InversePropertyAttributeConvention_SelfInverseDetected = "InversePropertyAttributeConvention_SelfInverseDetected";

	internal const string ValidationHeader = "ValidationHeader";

	internal const string ValidationItemFormat = "ValidationItemFormat";

	internal const string KeyRegisteredOnDerivedType = "KeyRegisteredOnDerivedType";

	internal const string InvalidTableMapping = "InvalidTableMapping";

	internal const string InvalidTableMapping_NoTableName = "InvalidTableMapping_NoTableName";

	internal const string InvalidChainedMappingSyntax = "InvalidChainedMappingSyntax";

	internal const string InvalidNotNullCondition = "InvalidNotNullCondition";

	internal const string InvalidDiscriminatorType = "InvalidDiscriminatorType";

	internal const string ConventionNotFound = "ConventionNotFound";

	internal const string InvalidEntitySplittingProperties = "InvalidEntitySplittingProperties";

	internal const string ProviderNameNotFound = "ProviderNameNotFound";

	internal const string ProviderNotFound = "ProviderNotFound";

	internal const string InvalidDatabaseName = "InvalidDatabaseName";

	internal const string EntityMappingConfiguration_DuplicateMapInheritedProperties = "EntityMappingConfiguration_DuplicateMapInheritedProperties";

	internal const string EntityMappingConfiguration_DuplicateMappedProperties = "EntityMappingConfiguration_DuplicateMappedProperties";

	internal const string EntityMappingConfiguration_DuplicateMappedProperty = "EntityMappingConfiguration_DuplicateMappedProperty";

	internal const string EntityMappingConfiguration_CannotMapIgnoredProperty = "EntityMappingConfiguration_CannotMapIgnoredProperty";

	internal const string EntityMappingConfiguration_InvalidTableSharing = "EntityMappingConfiguration_InvalidTableSharing";

	internal const string EntityMappingConfiguration_TPCWithIAsOnNonLeafType = "EntityMappingConfiguration_TPCWithIAsOnNonLeafType";

	internal const string CannotIgnoreMappedBaseProperty = "CannotIgnoreMappedBaseProperty";

	internal const string ModelBuilder_KeyPropertiesMustBePrimitive = "ModelBuilder_KeyPropertiesMustBePrimitive";

	internal const string TableNotFound = "TableNotFound";

	internal const string IncorrectColumnCount = "IncorrectColumnCount";

	internal const string BadKeyNameForAnnotation = "BadKeyNameForAnnotation";

	internal const string BadAnnotationName = "BadAnnotationName";

	internal const string CircularComplexTypeHierarchy = "CircularComplexTypeHierarchy";

	internal const string UnableToDeterminePrincipal = "UnableToDeterminePrincipal";

	internal const string UnmappedAbstractType = "UnmappedAbstractType";

	internal const string UnsupportedHybridInheritanceMapping = "UnsupportedHybridInheritanceMapping";

	internal const string OrphanedConfiguredTableDetected = "OrphanedConfiguredTableDetected";

	internal const string BadTphMappingToSharedColumn = "BadTphMappingToSharedColumn";

	internal const string DuplicateConfiguredColumnOrder = "DuplicateConfiguredColumnOrder";

	internal const string UnsupportedUseOfV3Type = "UnsupportedUseOfV3Type";

	internal const string MultiplePropertiesMatchedAsKeys = "MultiplePropertiesMatchedAsKeys";

	internal const string FailedToGetProviderInformation = "FailedToGetProviderInformation";

	internal const string DbPropertyEntry_CannotGetCurrentValue = "DbPropertyEntry_CannotGetCurrentValue";

	internal const string DbPropertyEntry_CannotSetCurrentValue = "DbPropertyEntry_CannotSetCurrentValue";

	internal const string DbPropertyEntry_NotSupportedForDetached = "DbPropertyEntry_NotSupportedForDetached";

	internal const string DbPropertyEntry_SettingEntityRefNotSupported = "DbPropertyEntry_SettingEntityRefNotSupported";

	internal const string DbPropertyEntry_NotSupportedForPropertiesNotInTheModel = "DbPropertyEntry_NotSupportedForPropertiesNotInTheModel";

	internal const string DbEntityEntry_NotSupportedForDetached = "DbEntityEntry_NotSupportedForDetached";

	internal const string DbSet_BadTypeForAddAttachRemove = "DbSet_BadTypeForAddAttachRemove";

	internal const string DbSet_BadTypeForCreate = "DbSet_BadTypeForCreate";

	internal const string DbEntity_BadTypeForCast = "DbEntity_BadTypeForCast";

	internal const string DbMember_BadTypeForCast = "DbMember_BadTypeForCast";

	internal const string DbEntityEntry_UsedReferenceForCollectionProp = "DbEntityEntry_UsedReferenceForCollectionProp";

	internal const string DbEntityEntry_UsedCollectionForReferenceProp = "DbEntityEntry_UsedCollectionForReferenceProp";

	internal const string DbEntityEntry_NotANavigationProperty = "DbEntityEntry_NotANavigationProperty";

	internal const string DbEntityEntry_NotAScalarProperty = "DbEntityEntry_NotAScalarProperty";

	internal const string DbEntityEntry_NotAComplexProperty = "DbEntityEntry_NotAComplexProperty";

	internal const string DbEntityEntry_NotAProperty = "DbEntityEntry_NotAProperty";

	internal const string DbEntityEntry_DottedPartNotComplex = "DbEntityEntry_DottedPartNotComplex";

	internal const string DbEntityEntry_DottedPathMustBeProperty = "DbEntityEntry_DottedPathMustBeProperty";

	internal const string DbEntityEntry_WrongGenericForNavProp = "DbEntityEntry_WrongGenericForNavProp";

	internal const string DbEntityEntry_WrongGenericForCollectionNavProp = "DbEntityEntry_WrongGenericForCollectionNavProp";

	internal const string DbEntityEntry_WrongGenericForProp = "DbEntityEntry_WrongGenericForProp";

	internal const string DbEntityEntry_BadPropertyExpression = "DbEntityEntry_BadPropertyExpression";

	internal const string DbContext_IndependentAssociationUpdateException = "DbContext_IndependentAssociationUpdateException";

	internal const string DbPropertyValues_CannotGetValuesForState = "DbPropertyValues_CannotGetValuesForState";

	internal const string DbPropertyValues_CannotSetNullValue = "DbPropertyValues_CannotSetNullValue";

	internal const string DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull = "DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull";

	internal const string DbPropertyValues_WrongTypeForAssignment = "DbPropertyValues_WrongTypeForAssignment";

	internal const string DbPropertyValues_PropertyValueNamesAreReadonly = "DbPropertyValues_PropertyValueNamesAreReadonly";

	internal const string DbPropertyValues_PropertyDoesNotExist = "DbPropertyValues_PropertyDoesNotExist";

	internal const string DbPropertyValues_AttemptToSetValuesFromWrongObject = "DbPropertyValues_AttemptToSetValuesFromWrongObject";

	internal const string DbPropertyValues_AttemptToSetValuesFromWrongType = "DbPropertyValues_AttemptToSetValuesFromWrongType";

	internal const string DbPropertyValues_AttemptToSetNonValuesOnComplexProperty = "DbPropertyValues_AttemptToSetNonValuesOnComplexProperty";

	internal const string DbPropertyValues_ComplexObjectCannotBeNull = "DbPropertyValues_ComplexObjectCannotBeNull";

	internal const string DbPropertyValues_NestedPropertyValuesNull = "DbPropertyValues_NestedPropertyValuesNull";

	internal const string DbPropertyValues_CannotSetPropertyOnNullCurrentValue = "DbPropertyValues_CannotSetPropertyOnNullCurrentValue";

	internal const string DbPropertyValues_CannotSetPropertyOnNullOriginalValue = "DbPropertyValues_CannotSetPropertyOnNullOriginalValue";

	internal const string DatabaseInitializationStrategy_ModelMismatch = "DatabaseInitializationStrategy_ModelMismatch";

	internal const string Database_DatabaseAlreadyExists = "Database_DatabaseAlreadyExists";

	internal const string Database_NonCodeFirstCompatibilityCheck = "Database_NonCodeFirstCompatibilityCheck";

	internal const string Database_NoDatabaseMetadata = "Database_NoDatabaseMetadata";

	internal const string Database_BadLegacyInitializerEntry = "Database_BadLegacyInitializerEntry";

	internal const string Database_InitializeFromLegacyConfigFailed = "Database_InitializeFromLegacyConfigFailed";

	internal const string Database_InitializeFromConfigFailed = "Database_InitializeFromConfigFailed";

	internal const string ContextConfiguredMultipleTimes = "ContextConfiguredMultipleTimes";

	internal const string SetConnectionFactoryFromConfigFailed = "SetConnectionFactoryFromConfigFailed";

	internal const string DbContext_ContextUsedInModelCreating = "DbContext_ContextUsedInModelCreating";

	internal const string DbContext_MESTNotSupported = "DbContext_MESTNotSupported";

	internal const string DbContext_Disposed = "DbContext_Disposed";

	internal const string DbContext_ProviderReturnedNullConnection = "DbContext_ProviderReturnedNullConnection";

	internal const string DbContext_ProviderNameMissing = "DbContext_ProviderNameMissing";

	internal const string DbContext_ConnectionFactoryReturnedNullConnection = "DbContext_ConnectionFactoryReturnedNullConnection";

	internal const string DbSet_WrongNumberOfKeyValuesPassed = "DbSet_WrongNumberOfKeyValuesPassed";

	internal const string DbSet_WrongKeyValueType = "DbSet_WrongKeyValueType";

	internal const string DbSet_WrongEntityTypeFound = "DbSet_WrongEntityTypeFound";

	internal const string DbSet_MultipleAddedEntitiesFound = "DbSet_MultipleAddedEntitiesFound";

	internal const string DbSet_DbSetUsedWithComplexType = "DbSet_DbSetUsedWithComplexType";

	internal const string DbSet_PocoAndNonPocoMixedInSameAssembly = "DbSet_PocoAndNonPocoMixedInSameAssembly";

	internal const string DbSet_EntityTypeNotInModel = "DbSet_EntityTypeNotInModel";

	internal const string DbQuery_BindingToDbQueryNotSupported = "DbQuery_BindingToDbQueryNotSupported";

	internal const string DbExtensions_InvalidIncludePathExpression = "DbExtensions_InvalidIncludePathExpression";

	internal const string DbContext_ConnectionStringNotFound = "DbContext_ConnectionStringNotFound";

	internal const string DbContext_ConnectionHasModel = "DbContext_ConnectionHasModel";

	internal const string DbCollectionEntry_CannotSetCollectionProp = "DbCollectionEntry_CannotSetCollectionProp";

	internal const string CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported = "CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported";

	internal const string Mapping_MESTNotSupported = "Mapping_MESTNotSupported";

	internal const string DbModelBuilder_MissingRequiredCtor = "DbModelBuilder_MissingRequiredCtor";

	internal const string DbEntityValidationException_ValidationFailed = "DbEntityValidationException_ValidationFailed";

	internal const string DbUnexpectedValidationException_ValidationAttribute = "DbUnexpectedValidationException_ValidationAttribute";

	internal const string DbUnexpectedValidationException_IValidatableObject = "DbUnexpectedValidationException_IValidatableObject";

	internal const string SqlConnectionFactory_MdfNotSupported = "SqlConnectionFactory_MdfNotSupported";

	internal const string Database_InitializationException = "Database_InitializationException";

	internal const string EdmxWriter_EdmxFromObjectContextNotSupported = "EdmxWriter_EdmxFromObjectContextNotSupported";

	internal const string EdmxWriter_EdmxFromModelFirstNotSupported = "EdmxWriter_EdmxFromModelFirstNotSupported";

	internal const string EdmxWriter_EdmxFromRawCompiledModelNotSupported = "EdmxWriter_EdmxFromRawCompiledModelNotSupported";

	internal const string UnintentionalCodeFirstException_Message = "UnintentionalCodeFirstException_Message";

	internal const string DbContextServices_MissingDefaultCtor = "DbContextServices_MissingDefaultCtor";

	internal const string CannotCallGenericSetWithProxyType = "CannotCallGenericSetWithProxyType";

	internal const string EdmModel_Validator_Semantic_SystemNamespaceEncountered = "EdmModel_Validator_Semantic_SystemNamespaceEncountered";

	internal const string EdmModel_Validator_Semantic_SimilarRelationshipEnd = "EdmModel_Validator_Semantic_SimilarRelationshipEnd";

	internal const string EdmModel_Validator_Semantic_InvalidEntitySetNameReference = "EdmModel_Validator_Semantic_InvalidEntitySetNameReference";

	internal const string EdmModel_Validator_Semantic_ConcurrencyRedefinedOnSubTypeOfEntitySetType = "EdmModel_Validator_Semantic_ConcurrencyRedefinedOnSubTypeOfEntitySetType";

	internal const string EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys = "EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys";

	internal const string EdmModel_Validator_Semantic_DuplicateEndName = "EdmModel_Validator_Semantic_DuplicateEndName";

	internal const string EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey = "EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey";

	internal const string EdmModel_Validator_Semantic_InvalidCollectionKindNotCollection = "EdmModel_Validator_Semantic_InvalidCollectionKindNotCollection";

	internal const string EdmModel_Validator_Semantic_InvalidCollectionKindNotV1_1 = "EdmModel_Validator_Semantic_InvalidCollectionKindNotV1_1";

	internal const string EdmModel_Validator_Semantic_InvalidComplexTypeAbstract = "EdmModel_Validator_Semantic_InvalidComplexTypeAbstract";

	internal const string EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic = "EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic";

	internal const string EdmModel_Validator_Semantic_InvalidKeyNullablePart = "EdmModel_Validator_Semantic_InvalidKeyNullablePart";

	internal const string EdmModel_Validator_Semantic_EntityKeyMustBeScalar = "EdmModel_Validator_Semantic_EntityKeyMustBeScalar";

	internal const string EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass = "EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass";

	internal const string EdmModel_Validator_Semantic_KeyMissingOnEntityType = "EdmModel_Validator_Semantic_KeyMissingOnEntityType";

	internal const string EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole = "EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole";

	internal const string EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame = "EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame";

	internal const string EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation = "EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation";

	internal const string EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified = "EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified";

	internal const string EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate = "EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate";

	internal const string EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint = "EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleUpperBoundMustBeOne = "EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleUpperBoundMustBeOne";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNullableV1 = "EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNullableV1";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV1 = "EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV1";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV2 = "EdmModel_Validator_Semantic_InvalidMultiplicityFromRoleToPropertyNonNullableV2";

	internal const string EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint = "EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeOne = "EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeOne";

	internal const string EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeMany = "EdmModel_Validator_Semantic_InvalidMultiplicityToRoleUpperBoundMustBeMany";

	internal const string EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint = "EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint";

	internal const string EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint = "EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint";

	internal const string EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraint = "EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraint";

	internal const string EdmModel_Validator_Semantic_NullableComplexType = "EdmModel_Validator_Semantic_NullableComplexType";

	internal const string EdmModel_Validator_Semantic_InvalidPropertyType = "EdmModel_Validator_Semantic_InvalidPropertyType";

	internal const string EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName = "EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName";

	internal const string EdmModel_Validator_Semantic_TypeNameAlreadyDefinedDuplicate = "EdmModel_Validator_Semantic_TypeNameAlreadyDefinedDuplicate";

	internal const string EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName = "EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName";

	internal const string EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate = "EdmModel_Validator_Semantic_PropertyNameAlreadyDefinedDuplicate";

	internal const string EdmModel_Validator_Semantic_CycleInTypeHierarchy = "EdmModel_Validator_Semantic_CycleInTypeHierarchy";

	internal const string EdmModel_Validator_Semantic_InvalidPropertyType_V1_1 = "EdmModel_Validator_Semantic_InvalidPropertyType_V1_1";

	internal const string EdmModel_Validator_Semantic_InvalidPropertyType_V3 = "EdmModel_Validator_Semantic_InvalidPropertyType_V3";

	internal const string EdmModel_Validator_Semantic_ComposableFunctionImportsNotSupportedForSchemaVersion = "EdmModel_Validator_Semantic_ComposableFunctionImportsNotSupportedForSchemaVersion";

	internal const string EdmModel_Validator_Syntactic_MissingName = "EdmModel_Validator_Syntactic_MissingName";

	internal const string EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong = "EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong";

	internal const string EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed = "EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationType_AssociationEndMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationType_AssociationEndMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentEndMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentEndMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty = "EdmModel_Validator_Syntactic_EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty";

	internal const string EdmModel_Validator_Syntactic_EdmNavigationProperty_AssociationMustNotBeNull = "EdmModel_Validator_Syntactic_EdmNavigationProperty_AssociationMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmNavigationProperty_ResultEndMustNotBeNull = "EdmModel_Validator_Syntactic_EdmNavigationProperty_ResultEndMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationEnd_EntityTypeMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationEnd_EntityTypeMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmEntitySet_ElementTypeMustNotBeNull = "EdmModel_Validator_Syntactic_EdmEntitySet_ElementTypeMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationSet_ElementTypeMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationSet_ElementTypeMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationSet_SourceSetMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationSet_SourceSetMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmAssociationSet_TargetSetMustNotBeNull = "EdmModel_Validator_Syntactic_EdmAssociationSet_TargetSetMustNotBeNull";

	internal const string EdmModel_Validator_Syntactic_EdmTypeReferenceNotValid = "EdmModel_Validator_Syntactic_EdmTypeReferenceNotValid";

	internal const string MetadataItem_InvalidDataSpace = "MetadataItem_InvalidDataSpace";

	internal const string EdmModel_AddItem_NonMatchingNamespace = "EdmModel_AddItem_NonMatchingNamespace";

	internal const string Serializer_OneNamespaceAndOneContainer = "Serializer_OneNamespaceAndOneContainer";

	internal const string MaxLengthAttribute_ValidationError = "MaxLengthAttribute_ValidationError";

	internal const string MaxLengthAttribute_InvalidMaxLength = "MaxLengthAttribute_InvalidMaxLength";

	internal const string MinLengthAttribute_ValidationError = "MinLengthAttribute_ValidationError";

	internal const string MinLengthAttribute_InvalidMinLength = "MinLengthAttribute_InvalidMinLength";

	internal const string DbConnectionInfo_ConnectionStringNotFound = "DbConnectionInfo_ConnectionStringNotFound";

	internal const string EagerInternalContext_CannotSetConnectionInfo = "EagerInternalContext_CannotSetConnectionInfo";

	internal const string LazyInternalContext_CannotReplaceEfConnectionWithDbConnection = "LazyInternalContext_CannotReplaceEfConnectionWithDbConnection";

	internal const string LazyInternalContext_CannotReplaceDbConnectionWithEfConnection = "LazyInternalContext_CannotReplaceDbConnectionWithEfConnection";

	internal const string EntityKey_EntitySetDoesNotMatch = "EntityKey_EntitySetDoesNotMatch";

	internal const string EntityKey_IncorrectNumberOfKeyValuePairs = "EntityKey_IncorrectNumberOfKeyValuePairs";

	internal const string EntityKey_IncorrectValueType = "EntityKey_IncorrectValueType";

	internal const string EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember = "EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember";

	internal const string EntityKey_MissingKeyValue = "EntityKey_MissingKeyValue";

	internal const string EntityKey_NoNullsAllowedInKeyValuePairs = "EntityKey_NoNullsAllowedInKeyValuePairs";

	internal const string EntityKey_UnexpectedNull = "EntityKey_UnexpectedNull";

	internal const string EntityKey_DoesntMatchKeyOnEntity = "EntityKey_DoesntMatchKeyOnEntity";

	internal const string EntityKey_EntityKeyMustHaveValues = "EntityKey_EntityKeyMustHaveValues";

	internal const string EntityKey_InvalidQualifiedEntitySetName = "EntityKey_InvalidQualifiedEntitySetName";

	internal const string EntityKey_MissingEntitySetName = "EntityKey_MissingEntitySetName";

	internal const string EntityKey_InvalidName = "EntityKey_InvalidName";

	internal const string EntityKey_CannotChangeKey = "EntityKey_CannotChangeKey";

	internal const string EntityTypesDoNotAgree = "EntityTypesDoNotAgree";

	internal const string EntityKey_NullKeyValue = "EntityKey_NullKeyValue";

	internal const string EdmMembersDefiningTypeDoNotAgreeWithMetadataType = "EdmMembersDefiningTypeDoNotAgreeWithMetadataType";

	internal const string CannotCallNoncomposableFunction = "CannotCallNoncomposableFunction";

	internal const string EntityClient_ConnectionStringMissingInfo = "EntityClient_ConnectionStringMissingInfo";

	internal const string EntityClient_ValueNotString = "EntityClient_ValueNotString";

	internal const string EntityClient_KeywordNotSupported = "EntityClient_KeywordNotSupported";

	internal const string EntityClient_NoCommandText = "EntityClient_NoCommandText";

	internal const string EntityClient_ConnectionStringNeededBeforeOperation = "EntityClient_ConnectionStringNeededBeforeOperation";

	internal const string EntityClient_ConnectionNotOpen = "EntityClient_ConnectionNotOpen";

	internal const string EntityClient_DuplicateParameterNames = "EntityClient_DuplicateParameterNames";

	internal const string EntityClient_NoConnectionForCommand = "EntityClient_NoConnectionForCommand";

	internal const string EntityClient_NoConnectionForAdapter = "EntityClient_NoConnectionForAdapter";

	internal const string EntityClient_ClosedConnectionForUpdate = "EntityClient_ClosedConnectionForUpdate";

	internal const string EntityClient_InvalidNamedConnection = "EntityClient_InvalidNamedConnection";

	internal const string EntityClient_NestedNamedConnection = "EntityClient_NestedNamedConnection";

	internal const string EntityClient_InvalidStoreProvider = "EntityClient_InvalidStoreProvider";

	internal const string EntityClient_DataReaderIsStillOpen = "EntityClient_DataReaderIsStillOpen";

	internal const string EntityClient_SettingsCannotBeChangedOnOpenConnection = "EntityClient_SettingsCannotBeChangedOnOpenConnection";

	internal const string EntityClient_ExecutingOnClosedConnection = "EntityClient_ExecutingOnClosedConnection";

	internal const string EntityClient_ConnectionStateClosed = "EntityClient_ConnectionStateClosed";

	internal const string EntityClient_ConnectionStateBroken = "EntityClient_ConnectionStateBroken";

	internal const string EntityClient_CannotCloneStoreProvider = "EntityClient_CannotCloneStoreProvider";

	internal const string EntityClient_UnsupportedCommandType = "EntityClient_UnsupportedCommandType";

	internal const string EntityClient_ErrorInClosingConnection = "EntityClient_ErrorInClosingConnection";

	internal const string EntityClient_ErrorInBeginningTransaction = "EntityClient_ErrorInBeginningTransaction";

	internal const string EntityClient_ExtraParametersWithNamedConnection = "EntityClient_ExtraParametersWithNamedConnection";

	internal const string EntityClient_CommandDefinitionPreparationFailed = "EntityClient_CommandDefinitionPreparationFailed";

	internal const string EntityClient_CommandDefinitionExecutionFailed = "EntityClient_CommandDefinitionExecutionFailed";

	internal const string EntityClient_CommandExecutionFailed = "EntityClient_CommandExecutionFailed";

	internal const string EntityClient_StoreReaderFailed = "EntityClient_StoreReaderFailed";

	internal const string EntityClient_FailedToGetInformation = "EntityClient_FailedToGetInformation";

	internal const string EntityClient_TooFewColumns = "EntityClient_TooFewColumns";

	internal const string EntityClient_InvalidParameterName = "EntityClient_InvalidParameterName";

	internal const string EntityClient_EmptyParameterName = "EntityClient_EmptyParameterName";

	internal const string EntityClient_ReturnedNullOnProviderMethod = "EntityClient_ReturnedNullOnProviderMethod";

	internal const string EntityClient_CannotDeduceDbType = "EntityClient_CannotDeduceDbType";

	internal const string EntityClient_InvalidParameterDirection = "EntityClient_InvalidParameterDirection";

	internal const string EntityClient_UnknownParameterType = "EntityClient_UnknownParameterType";

	internal const string EntityClient_UnsupportedDbType = "EntityClient_UnsupportedDbType";

	internal const string EntityClient_IncompatibleNavigationPropertyResult = "EntityClient_IncompatibleNavigationPropertyResult";

	internal const string EntityClient_TransactionAlreadyStarted = "EntityClient_TransactionAlreadyStarted";

	internal const string EntityClient_InvalidTransactionForCommand = "EntityClient_InvalidTransactionForCommand";

	internal const string EntityClient_NoStoreConnectionForUpdate = "EntityClient_NoStoreConnectionForUpdate";

	internal const string EntityClient_CommandTreeMetadataIncompatible = "EntityClient_CommandTreeMetadataIncompatible";

	internal const string EntityClient_ProviderGeneralError = "EntityClient_ProviderGeneralError";

	internal const string EntityClient_ProviderSpecificError = "EntityClient_ProviderSpecificError";

	internal const string EntityClient_FunctionImportEmptyCommandText = "EntityClient_FunctionImportEmptyCommandText";

	internal const string EntityClient_UnableToFindFunctionImportContainer = "EntityClient_UnableToFindFunctionImportContainer";

	internal const string EntityClient_UnableToFindFunctionImport = "EntityClient_UnableToFindFunctionImport";

	internal const string EntityClient_FunctionImportMustBeNonComposable = "EntityClient_FunctionImportMustBeNonComposable";

	internal const string EntityClient_UnmappedFunctionImport = "EntityClient_UnmappedFunctionImport";

	internal const string EntityClient_InvalidStoredProcedureCommandText = "EntityClient_InvalidStoredProcedureCommandText";

	internal const string EntityClient_ItemCollectionsNotRegisteredInWorkspace = "EntityClient_ItemCollectionsNotRegisteredInWorkspace";

	internal const string EntityClient_DbConnectionHasNoProvider = "EntityClient_DbConnectionHasNoProvider";

	internal const string EntityClient_RequiresNonStoreCommandTree = "EntityClient_RequiresNonStoreCommandTree";

	internal const string EntityClient_CannotReprepareCommandDefinitionBasedCommand = "EntityClient_CannotReprepareCommandDefinitionBasedCommand";

	internal const string EntityClient_EntityParameterEdmTypeNotScalar = "EntityClient_EntityParameterEdmTypeNotScalar";

	internal const string EntityClient_EntityParameterInconsistentEdmType = "EntityClient_EntityParameterInconsistentEdmType";

	internal const string EntityClient_CannotGetCommandText = "EntityClient_CannotGetCommandText";

	internal const string EntityClient_CannotSetCommandText = "EntityClient_CannotSetCommandText";

	internal const string EntityClient_CannotGetCommandTree = "EntityClient_CannotGetCommandTree";

	internal const string EntityClient_CannotSetCommandTree = "EntityClient_CannotSetCommandTree";

	internal const string ELinq_ExpressionMustBeIQueryable = "ELinq_ExpressionMustBeIQueryable";

	internal const string ELinq_UnsupportedExpressionType = "ELinq_UnsupportedExpressionType";

	internal const string ELinq_UnsupportedUseOfContextParameter = "ELinq_UnsupportedUseOfContextParameter";

	internal const string ELinq_UnboundParameterExpression = "ELinq_UnboundParameterExpression";

	internal const string ELinq_UnsupportedConstructor = "ELinq_UnsupportedConstructor";

	internal const string ELinq_UnsupportedInitializers = "ELinq_UnsupportedInitializers";

	internal const string ELinq_UnsupportedBinding = "ELinq_UnsupportedBinding";

	internal const string ELinq_UnsupportedMethod = "ELinq_UnsupportedMethod";

	internal const string ELinq_UnsupportedMethodSuggestedAlternative = "ELinq_UnsupportedMethodSuggestedAlternative";

	internal const string ELinq_ThenByDoesNotFollowOrderBy = "ELinq_ThenByDoesNotFollowOrderBy";

	internal const string ELinq_UnrecognizedMember = "ELinq_UnrecognizedMember";

	internal const string ELinq_UnresolvableFunctionForMethod = "ELinq_UnresolvableFunctionForMethod";

	internal const string ELinq_UnresolvableFunctionForMethodAmbiguousMatch = "ELinq_UnresolvableFunctionForMethodAmbiguousMatch";

	internal const string ELinq_UnresolvableFunctionForMethodNotFound = "ELinq_UnresolvableFunctionForMethodNotFound";

	internal const string ELinq_UnresolvableFunctionForMember = "ELinq_UnresolvableFunctionForMember";

	internal const string ELinq_UnresolvableStoreFunctionForMember = "ELinq_UnresolvableStoreFunctionForMember";

	internal const string ELinq_UnresolvableFunctionForExpression = "ELinq_UnresolvableFunctionForExpression";

	internal const string ELinq_UnresolvableStoreFunctionForExpression = "ELinq_UnresolvableStoreFunctionForExpression";

	internal const string ELinq_UnsupportedType = "ELinq_UnsupportedType";

	internal const string ELinq_UnsupportedNullConstant = "ELinq_UnsupportedNullConstant";

	internal const string ELinq_UnsupportedConstant = "ELinq_UnsupportedConstant";

	internal const string ELinq_UnsupportedCast = "ELinq_UnsupportedCast";

	internal const string ELinq_UnsupportedIsOrAs = "ELinq_UnsupportedIsOrAs";

	internal const string ELinq_UnsupportedQueryableMethod = "ELinq_UnsupportedQueryableMethod";

	internal const string ELinq_InvalidOfTypeResult = "ELinq_InvalidOfTypeResult";

	internal const string ELinq_UnsupportedNominalType = "ELinq_UnsupportedNominalType";

	internal const string ELinq_UnsupportedEnumerableType = "ELinq_UnsupportedEnumerableType";

	internal const string ELinq_UnsupportedHeterogeneousInitializers = "ELinq_UnsupportedHeterogeneousInitializers";

	internal const string ELinq_UnsupportedDifferentContexts = "ELinq_UnsupportedDifferentContexts";

	internal const string ELinq_UnsupportedCastToDecimal = "ELinq_UnsupportedCastToDecimal";

	internal const string ELinq_UnsupportedKeySelector = "ELinq_UnsupportedKeySelector";

	internal const string ELinq_CreateOrderedEnumerableNotSupported = "ELinq_CreateOrderedEnumerableNotSupported";

	internal const string ELinq_UnsupportedPassthrough = "ELinq_UnsupportedPassthrough";

	internal const string ELinq_UnexpectedTypeForNavigationProperty = "ELinq_UnexpectedTypeForNavigationProperty";

	internal const string ELinq_SkipWithoutOrder = "ELinq_SkipWithoutOrder";

	internal const string ELinq_PropertyIndexNotSupported = "ELinq_PropertyIndexNotSupported";

	internal const string ELinq_NotPropertyOrField = "ELinq_NotPropertyOrField";

	internal const string ELinq_UnsupportedStringRemoveCase = "ELinq_UnsupportedStringRemoveCase";

	internal const string ELinq_UnsupportedTrimStartTrimEndCase = "ELinq_UnsupportedTrimStartTrimEndCase";

	internal const string ELinq_UnsupportedVBDatePartNonConstantInterval = "ELinq_UnsupportedVBDatePartNonConstantInterval";

	internal const string ELinq_UnsupportedVBDatePartInvalidInterval = "ELinq_UnsupportedVBDatePartInvalidInterval";

	internal const string ELinq_UnsupportedAsUnicodeAndAsNonUnicode = "ELinq_UnsupportedAsUnicodeAndAsNonUnicode";

	internal const string ELinq_UnsupportedComparison = "ELinq_UnsupportedComparison";

	internal const string ELinq_UnsupportedRefComparison = "ELinq_UnsupportedRefComparison";

	internal const string ELinq_UnsupportedRowComparison = "ELinq_UnsupportedRowComparison";

	internal const string ELinq_UnsupportedRowMemberComparison = "ELinq_UnsupportedRowMemberComparison";

	internal const string ELinq_UnsupportedRowTypeComparison = "ELinq_UnsupportedRowTypeComparison";

	internal const string ELinq_AnonymousType = "ELinq_AnonymousType";

	internal const string ELinq_ClosureType = "ELinq_ClosureType";

	internal const string ELinq_UnhandledExpressionType = "ELinq_UnhandledExpressionType";

	internal const string ELinq_UnhandledBindingType = "ELinq_UnhandledBindingType";

	internal const string ELinq_UnsupportedNestedFirst = "ELinq_UnsupportedNestedFirst";

	internal const string ELinq_UnsupportedNestedSingle = "ELinq_UnsupportedNestedSingle";

	internal const string ELinq_UnsupportedInclude = "ELinq_UnsupportedInclude";

	internal const string ELinq_UnsupportedMergeAs = "ELinq_UnsupportedMergeAs";

	internal const string ELinq_MethodNotDirectlyCallable = "ELinq_MethodNotDirectlyCallable";

	internal const string ELinq_CycleDetected = "ELinq_CycleDetected";

	internal const string ELinq_DbFunctionAttributedFunctionWithWrongReturnType = "ELinq_DbFunctionAttributedFunctionWithWrongReturnType";

	internal const string ELinq_DbFunctionDirectCall = "ELinq_DbFunctionDirectCall";

	internal const string ELinq_HasFlagArgumentAndSourceTypeMismatch = "ELinq_HasFlagArgumentAndSourceTypeMismatch";

	internal const string Elinq_ToStringNotSupportedForType = "Elinq_ToStringNotSupportedForType";

	internal const string Elinq_ToStringNotSupportedForEnumsWithFlags = "Elinq_ToStringNotSupportedForEnumsWithFlags";

	internal const string CompiledELinq_UnsupportedParameterTypes = "CompiledELinq_UnsupportedParameterTypes";

	internal const string CompiledELinq_UnsupportedNamedParameterType = "CompiledELinq_UnsupportedNamedParameterType";

	internal const string CompiledELinq_UnsupportedNamedParameterUseAsType = "CompiledELinq_UnsupportedNamedParameterUseAsType";

	internal const string Update_UnsupportedExpressionKind = "Update_UnsupportedExpressionKind";

	internal const string Update_UnsupportedCastArgument = "Update_UnsupportedCastArgument";

	internal const string Update_UnsupportedExtentType = "Update_UnsupportedExtentType";

	internal const string Update_ConstraintCycle = "Update_ConstraintCycle";

	internal const string Update_UnsupportedJoinType = "Update_UnsupportedJoinType";

	internal const string Update_UnsupportedProjection = "Update_UnsupportedProjection";

	internal const string Update_ConcurrencyError = "Update_ConcurrencyError";

	internal const string Update_MissingEntity = "Update_MissingEntity";

	internal const string Update_RelationshipCardinalityConstraintViolation = "Update_RelationshipCardinalityConstraintViolation";

	internal const string Update_GeneralExecutionException = "Update_GeneralExecutionException";

	internal const string Update_MissingRequiredEntity = "Update_MissingRequiredEntity";

	internal const string Update_RelationshipCardinalityViolation = "Update_RelationshipCardinalityViolation";

	internal const string Update_NotSupportedComputedKeyColumn = "Update_NotSupportedComputedKeyColumn";

	internal const string Update_AmbiguousServerGenIdentifier = "Update_AmbiguousServerGenIdentifier";

	internal const string Update_WorkspaceMismatch = "Update_WorkspaceMismatch";

	internal const string Update_MissingRequiredRelationshipValue = "Update_MissingRequiredRelationshipValue";

	internal const string Update_MissingResultColumn = "Update_MissingResultColumn";

	internal const string Update_NullReturnValueForNonNullableMember = "Update_NullReturnValueForNonNullableMember";

	internal const string Update_ReturnValueHasUnexpectedType = "Update_ReturnValueHasUnexpectedType";

	internal const string Update_UnableToConvertRowsAffectedParameter = "Update_UnableToConvertRowsAffectedParameter";

	internal const string Update_MappingNotFound = "Update_MappingNotFound";

	internal const string Update_ModifyingIdentityColumn = "Update_ModifyingIdentityColumn";

	internal const string Update_GeneratedDependent = "Update_GeneratedDependent";

	internal const string Update_ReferentialConstraintIntegrityViolation = "Update_ReferentialConstraintIntegrityViolation";

	internal const string Update_ErrorLoadingRecord = "Update_ErrorLoadingRecord";

	internal const string Update_NullValue = "Update_NullValue";

	internal const string Update_CircularRelationships = "Update_CircularRelationships";

	internal const string Update_RelationshipCardinalityConstraintViolationSingleValue = "Update_RelationshipCardinalityConstraintViolationSingleValue";

	internal const string Update_MissingFunctionMapping = "Update_MissingFunctionMapping";

	internal const string Update_InvalidChanges = "Update_InvalidChanges";

	internal const string Update_DuplicateKeys = "Update_DuplicateKeys";

	internal const string Update_AmbiguousForeignKey = "Update_AmbiguousForeignKey";

	internal const string Update_InsertingOrUpdatingReferenceToDeletedEntity = "Update_InsertingOrUpdatingReferenceToDeletedEntity";

	internal const string ViewGen_Extent = "ViewGen_Extent";

	internal const string ViewGen_Null = "ViewGen_Null";

	internal const string ViewGen_CommaBlank = "ViewGen_CommaBlank";

	internal const string ViewGen_Entities = "ViewGen_Entities";

	internal const string ViewGen_Tuples = "ViewGen_Tuples";

	internal const string ViewGen_NotNull = "ViewGen_NotNull";

	internal const string ViewGen_NegatedCellConstant = "ViewGen_NegatedCellConstant";

	internal const string ViewGen_Error = "ViewGen_Error";

	internal const string Viewgen_CannotGenerateQueryViewUnderNoValidation = "Viewgen_CannotGenerateQueryViewUnderNoValidation";

	internal const string ViewGen_Missing_Sets_Mapping = "ViewGen_Missing_Sets_Mapping";

	internal const string ViewGen_Missing_Type_Mapping = "ViewGen_Missing_Type_Mapping";

	internal const string ViewGen_Missing_Set_Mapping = "ViewGen_Missing_Set_Mapping";

	internal const string ViewGen_Concurrency_Derived_Class = "ViewGen_Concurrency_Derived_Class";

	internal const string ViewGen_Concurrency_Invalid_Condition = "ViewGen_Concurrency_Invalid_Condition";

	internal const string ViewGen_TableKey_Missing = "ViewGen_TableKey_Missing";

	internal const string ViewGen_EntitySetKey_Missing = "ViewGen_EntitySetKey_Missing";

	internal const string ViewGen_AssociationSetKey_Missing = "ViewGen_AssociationSetKey_Missing";

	internal const string ViewGen_Cannot_Recover_Attributes = "ViewGen_Cannot_Recover_Attributes";

	internal const string ViewGen_Cannot_Recover_Types = "ViewGen_Cannot_Recover_Types";

	internal const string ViewGen_Cannot_Disambiguate_MultiConstant = "ViewGen_Cannot_Disambiguate_MultiConstant";

	internal const string ViewGen_No_Default_Value = "ViewGen_No_Default_Value";

	internal const string ViewGen_No_Default_Value_For_Configuration = "ViewGen_No_Default_Value_For_Configuration";

	internal const string ViewGen_KeyConstraint_Violation = "ViewGen_KeyConstraint_Violation";

	internal const string ViewGen_KeyConstraint_Update_Violation_EntitySet = "ViewGen_KeyConstraint_Update_Violation_EntitySet";

	internal const string ViewGen_KeyConstraint_Update_Violation_AssociationSet = "ViewGen_KeyConstraint_Update_Violation_AssociationSet";

	internal const string ViewGen_AssociationEndShouldBeMappedToKey = "ViewGen_AssociationEndShouldBeMappedToKey";

	internal const string ViewGen_Duplicate_CProperties = "ViewGen_Duplicate_CProperties";

	internal const string ViewGen_Duplicate_CProperties_IsMapped = "ViewGen_Duplicate_CProperties_IsMapped";

	internal const string ViewGen_NotNull_No_Projected_Slot = "ViewGen_NotNull_No_Projected_Slot";

	internal const string ViewGen_InvalidCondition = "ViewGen_InvalidCondition";

	internal const string ViewGen_NonKeyProjectedWithOverlappingPartitions = "ViewGen_NonKeyProjectedWithOverlappingPartitions";

	internal const string ViewGen_CQ_PartitionConstraint = "ViewGen_CQ_PartitionConstraint";

	internal const string ViewGen_CQ_DomainConstraint = "ViewGen_CQ_DomainConstraint";

	internal const string ViewGen_ErrorLog = "ViewGen_ErrorLog";

	internal const string ViewGen_ErrorLog2 = "ViewGen_ErrorLog2";

	internal const string ViewGen_Foreign_Key_Missing_Table_Mapping = "ViewGen_Foreign_Key_Missing_Table_Mapping";

	internal const string ViewGen_Foreign_Key_ParentTable_NotMappedToEnd = "ViewGen_Foreign_Key_ParentTable_NotMappedToEnd";

	internal const string ViewGen_Foreign_Key = "ViewGen_Foreign_Key";

	internal const string ViewGen_Foreign_Key_UpperBound_MustBeOne = "ViewGen_Foreign_Key_UpperBound_MustBeOne";

	internal const string ViewGen_Foreign_Key_LowerBound_MustBeOne = "ViewGen_Foreign_Key_LowerBound_MustBeOne";

	internal const string ViewGen_Foreign_Key_Missing_Relationship_Mapping = "ViewGen_Foreign_Key_Missing_Relationship_Mapping";

	internal const string ViewGen_Foreign_Key_Not_Guaranteed_InCSpace = "ViewGen_Foreign_Key_Not_Guaranteed_InCSpace";

	internal const string ViewGen_Foreign_Key_ColumnOrder_Incorrect = "ViewGen_Foreign_Key_ColumnOrder_Incorrect";

	internal const string ViewGen_AssociationSet_AsUserString = "ViewGen_AssociationSet_AsUserString";

	internal const string ViewGen_AssociationSet_AsUserString_Negated = "ViewGen_AssociationSet_AsUserString_Negated";

	internal const string ViewGen_EntitySet_AsUserString = "ViewGen_EntitySet_AsUserString";

	internal const string ViewGen_EntitySet_AsUserString_Negated = "ViewGen_EntitySet_AsUserString_Negated";

	internal const string ViewGen_EntityInstanceToken = "ViewGen_EntityInstanceToken";

	internal const string Viewgen_ConfigurationErrorMsg = "Viewgen_ConfigurationErrorMsg";

	internal const string ViewGen_HashOnMappingClosure_Not_Matching = "ViewGen_HashOnMappingClosure_Not_Matching";

	internal const string Viewgen_RightSideNotDisjoint = "Viewgen_RightSideNotDisjoint";

	internal const string Viewgen_QV_RewritingNotFound = "Viewgen_QV_RewritingNotFound";

	internal const string Viewgen_NullableMappingForNonNullableColumn = "Viewgen_NullableMappingForNonNullableColumn";

	internal const string Viewgen_ErrorPattern_ConditionMemberIsMapped = "Viewgen_ErrorPattern_ConditionMemberIsMapped";

	internal const string Viewgen_ErrorPattern_DuplicateConditionValue = "Viewgen_ErrorPattern_DuplicateConditionValue";

	internal const string Viewgen_ErrorPattern_TableMappedToMultipleES = "Viewgen_ErrorPattern_TableMappedToMultipleES";

	internal const string Viewgen_ErrorPattern_Partition_Disj_Eq = "Viewgen_ErrorPattern_Partition_Disj_Eq";

	internal const string Viewgen_ErrorPattern_NotNullConditionMappedToNullableMember = "Viewgen_ErrorPattern_NotNullConditionMappedToNullableMember";

	internal const string Viewgen_ErrorPattern_Partition_MultipleTypesMappedToSameTable_WithoutCondition = "Viewgen_ErrorPattern_Partition_MultipleTypesMappedToSameTable_WithoutCondition";

	internal const string Viewgen_ErrorPattern_Partition_Disj_Subs_Ref = "Viewgen_ErrorPattern_Partition_Disj_Subs_Ref";

	internal const string Viewgen_ErrorPattern_Partition_Disj_Subs = "Viewgen_ErrorPattern_Partition_Disj_Subs";

	internal const string Viewgen_ErrorPattern_Partition_Disj_Unk = "Viewgen_ErrorPattern_Partition_Disj_Unk";

	internal const string Viewgen_ErrorPattern_Partition_Eq_Disj = "Viewgen_ErrorPattern_Partition_Eq_Disj";

	internal const string Viewgen_ErrorPattern_Partition_Eq_Subs_Ref = "Viewgen_ErrorPattern_Partition_Eq_Subs_Ref";

	internal const string Viewgen_ErrorPattern_Partition_Eq_Subs = "Viewgen_ErrorPattern_Partition_Eq_Subs";

	internal const string Viewgen_ErrorPattern_Partition_Eq_Unk = "Viewgen_ErrorPattern_Partition_Eq_Unk";

	internal const string Viewgen_ErrorPattern_Partition_Eq_Unk_Association = "Viewgen_ErrorPattern_Partition_Eq_Unk_Association";

	internal const string Viewgen_ErrorPattern_Partition_Sub_Disj = "Viewgen_ErrorPattern_Partition_Sub_Disj";

	internal const string Viewgen_ErrorPattern_Partition_Sub_Eq = "Viewgen_ErrorPattern_Partition_Sub_Eq";

	internal const string Viewgen_ErrorPattern_Partition_Sub_Eq_Ref = "Viewgen_ErrorPattern_Partition_Sub_Eq_Ref";

	internal const string Viewgen_ErrorPattern_Partition_Sub_Unk = "Viewgen_ErrorPattern_Partition_Sub_Unk";

	internal const string Viewgen_NoJoinKeyOrFK = "Viewgen_NoJoinKeyOrFK";

	internal const string Viewgen_MultipleFragmentsBetweenCandSExtentWithDistinct = "Viewgen_MultipleFragmentsBetweenCandSExtentWithDistinct";

	internal const string Validator_EmptyIdentity = "Validator_EmptyIdentity";

	internal const string Validator_CollectionHasNoTypeUsage = "Validator_CollectionHasNoTypeUsage";

	internal const string Validator_NoKeyMembers = "Validator_NoKeyMembers";

	internal const string Validator_FacetTypeIsNull = "Validator_FacetTypeIsNull";

	internal const string Validator_MemberHasNullDeclaringType = "Validator_MemberHasNullDeclaringType";

	internal const string Validator_MemberHasNullTypeUsage = "Validator_MemberHasNullTypeUsage";

	internal const string Validator_ItemAttributeHasNullTypeUsage = "Validator_ItemAttributeHasNullTypeUsage";

	internal const string Validator_RefTypeHasNullEntityType = "Validator_RefTypeHasNullEntityType";

	internal const string Validator_TypeUsageHasNullEdmType = "Validator_TypeUsageHasNullEdmType";

	internal const string Validator_BaseTypeHasMemberOfSameName = "Validator_BaseTypeHasMemberOfSameName";

	internal const string Validator_CollectionTypesCannotHaveBaseType = "Validator_CollectionTypesCannotHaveBaseType";

	internal const string Validator_RefTypesCannotHaveBaseType = "Validator_RefTypesCannotHaveBaseType";

	internal const string Validator_TypeHasNoName = "Validator_TypeHasNoName";

	internal const string Validator_TypeHasNoNamespace = "Validator_TypeHasNoNamespace";

	internal const string Validator_FacetHasNoName = "Validator_FacetHasNoName";

	internal const string Validator_MemberHasNoName = "Validator_MemberHasNoName";

	internal const string Validator_MetadataPropertyHasNoName = "Validator_MetadataPropertyHasNoName";

	internal const string Validator_NullableEntityKeyProperty = "Validator_NullableEntityKeyProperty";

	internal const string Validator_OSpace_InvalidNavPropReturnType = "Validator_OSpace_InvalidNavPropReturnType";

	internal const string Validator_OSpace_ScalarPropertyNotPrimitive = "Validator_OSpace_ScalarPropertyNotPrimitive";

	internal const string Validator_OSpace_ComplexPropertyNotComplex = "Validator_OSpace_ComplexPropertyNotComplex";

	internal const string Validator_OSpace_Convention_MultipleTypesWithSameName = "Validator_OSpace_Convention_MultipleTypesWithSameName";

	internal const string Validator_OSpace_Convention_NonPrimitiveTypeProperty = "Validator_OSpace_Convention_NonPrimitiveTypeProperty";

	internal const string Validator_OSpace_Convention_MissingRequiredProperty = "Validator_OSpace_Convention_MissingRequiredProperty";

	internal const string Validator_OSpace_Convention_BaseTypeIncompatible = "Validator_OSpace_Convention_BaseTypeIncompatible";

	internal const string Validator_OSpace_Convention_MissingOSpaceType = "Validator_OSpace_Convention_MissingOSpaceType";

	internal const string Validator_OSpace_Convention_RelationshipNotLoaded = "Validator_OSpace_Convention_RelationshipNotLoaded";

	internal const string Validator_OSpace_Convention_AttributeAssemblyReferenced = "Validator_OSpace_Convention_AttributeAssemblyReferenced";

	internal const string Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter = "Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter";

	internal const string Validator_OSpace_Convention_AmbiguousClrType = "Validator_OSpace_Convention_AmbiguousClrType";

	internal const string Validator_OSpace_Convention_Struct = "Validator_OSpace_Convention_Struct";

	internal const string Validator_OSpace_Convention_BaseTypeNotLoaded = "Validator_OSpace_Convention_BaseTypeNotLoaded";

	internal const string Validator_OSpace_Convention_SSpaceOSpaceTypeMismatch = "Validator_OSpace_Convention_SSpaceOSpaceTypeMismatch";

	internal const string Validator_OSpace_Convention_NonMatchingUnderlyingTypes = "Validator_OSpace_Convention_NonMatchingUnderlyingTypes";

	internal const string Validator_UnsupportedEnumUnderlyingType = "Validator_UnsupportedEnumUnderlyingType";

	internal const string ExtraInfo = "ExtraInfo";

	internal const string Metadata_General_Error = "Metadata_General_Error";

	internal const string InvalidNumberOfParametersForAggregateFunction = "InvalidNumberOfParametersForAggregateFunction";

	internal const string InvalidParameterTypeForAggregateFunction = "InvalidParameterTypeForAggregateFunction";

	internal const string InvalidSchemaEncountered = "InvalidSchemaEncountered";

	internal const string SystemNamespaceEncountered = "SystemNamespaceEncountered";

	internal const string NoCollectionForSpace = "NoCollectionForSpace";

	internal const string OperationOnReadOnlyCollection = "OperationOnReadOnlyCollection";

	internal const string OperationOnReadOnlyItem = "OperationOnReadOnlyItem";

	internal const string EntitySetInAnotherContainer = "EntitySetInAnotherContainer";

	internal const string InvalidKeyMember = "InvalidKeyMember";

	internal const string InvalidFileExtension = "InvalidFileExtension";

	internal const string NewTypeConflictsWithExistingType = "NewTypeConflictsWithExistingType";

	internal const string NotValidInputPath = "NotValidInputPath";

	internal const string UnableToDetermineApplicationContext = "UnableToDetermineApplicationContext";

	internal const string WildcardEnumeratorReturnedNull = "WildcardEnumeratorReturnedNull";

	internal const string InvalidUseOfWebPath = "InvalidUseOfWebPath";

	internal const string UnableToFindReflectedType = "UnableToFindReflectedType";

	internal const string AssemblyMissingFromAssembliesToConsider = "AssemblyMissingFromAssembliesToConsider";

	internal const string UnableToLoadResource = "UnableToLoadResource";

	internal const string EdmVersionNotSupportedByRuntime = "EdmVersionNotSupportedByRuntime";

	internal const string AtleastOneSSDLNeeded = "AtleastOneSSDLNeeded";

	internal const string InvalidMetadataPath = "InvalidMetadataPath";

	internal const string UnableToResolveAssembly = "UnableToResolveAssembly";

	internal const string DuplicatedFunctionoverloads = "DuplicatedFunctionoverloads";

	internal const string EntitySetNotInCSPace = "EntitySetNotInCSPace";

	internal const string TypeNotInEntitySet = "TypeNotInEntitySet";

	internal const string TypeNotInAssociationSet = "TypeNotInAssociationSet";

	internal const string DifferentSchemaVersionInCollection = "DifferentSchemaVersionInCollection";

	internal const string InvalidCollectionForMapping = "InvalidCollectionForMapping";

	internal const string OnlyStoreConnectionsSupported = "OnlyStoreConnectionsSupported";

	internal const string StoreItemCollectionMustHaveOneArtifact = "StoreItemCollectionMustHaveOneArtifact";

	internal const string CheckArgumentContainsNullFailed = "CheckArgumentContainsNullFailed";

	internal const string InvalidRelationshipSetName = "InvalidRelationshipSetName";

	internal const string InvalidEntitySetName = "InvalidEntitySetName";

	internal const string OnlyFunctionImportsCanBeAddedToEntityContainer = "OnlyFunctionImportsCanBeAddedToEntityContainer";

	internal const string ItemInvalidIdentity = "ItemInvalidIdentity";

	internal const string ItemDuplicateIdentity = "ItemDuplicateIdentity";

	internal const string NotStringTypeForTypeUsage = "NotStringTypeForTypeUsage";

	internal const string NotBinaryTypeForTypeUsage = "NotBinaryTypeForTypeUsage";

	internal const string NotDateTimeTypeForTypeUsage = "NotDateTimeTypeForTypeUsage";

	internal const string NotDateTimeOffsetTypeForTypeUsage = "NotDateTimeOffsetTypeForTypeUsage";

	internal const string NotTimeTypeForTypeUsage = "NotTimeTypeForTypeUsage";

	internal const string NotDecimalTypeForTypeUsage = "NotDecimalTypeForTypeUsage";

	internal const string ArrayTooSmall = "ArrayTooSmall";

	internal const string MoreThanOneItemMatchesIdentity = "MoreThanOneItemMatchesIdentity";

	internal const string MissingDefaultValueForConstantFacet = "MissingDefaultValueForConstantFacet";

	internal const string MinAndMaxValueMustBeSameForConstantFacet = "MinAndMaxValueMustBeSameForConstantFacet";

	internal const string BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet = "BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet";

	internal const string MinAndMaxValueMustBeDifferentForNonConstantFacet = "MinAndMaxValueMustBeDifferentForNonConstantFacet";

	internal const string MinAndMaxMustBePositive = "MinAndMaxMustBePositive";

	internal const string MinMustBeLessThanMax = "MinMustBeLessThanMax";

	internal const string SameRoleNameOnRelationshipAttribute = "SameRoleNameOnRelationshipAttribute";

	internal const string RoleTypeInEdmRelationshipAttributeIsInvalidType = "RoleTypeInEdmRelationshipAttributeIsInvalidType";

	internal const string TargetRoleNameInNavigationPropertyNotValid = "TargetRoleNameInNavigationPropertyNotValid";

	internal const string RelationshipNameInNavigationPropertyNotValid = "RelationshipNameInNavigationPropertyNotValid";

	internal const string NestedClassNotSupported = "NestedClassNotSupported";

	internal const string NullParameterForEdmRelationshipAttribute = "NullParameterForEdmRelationshipAttribute";

	internal const string NullRelationshipNameforEdmRelationshipAttribute = "NullRelationshipNameforEdmRelationshipAttribute";

	internal const string NavigationPropertyRelationshipEndTypeMismatch = "NavigationPropertyRelationshipEndTypeMismatch";

	internal const string AllArtifactsMustTargetSameProvider_InvariantName = "AllArtifactsMustTargetSameProvider_InvariantName";

	internal const string AllArtifactsMustTargetSameProvider_ManifestToken = "AllArtifactsMustTargetSameProvider_ManifestToken";

	internal const string ProviderManifestTokenNotFound = "ProviderManifestTokenNotFound";

	internal const string FailedToRetrieveProviderManifest = "FailedToRetrieveProviderManifest";

	internal const string InvalidMaxLengthSize = "InvalidMaxLengthSize";

	internal const string ArgumentMustBeCSpaceType = "ArgumentMustBeCSpaceType";

	internal const string ArgumentMustBeOSpaceType = "ArgumentMustBeOSpaceType";

	internal const string FailedToFindOSpaceTypeMapping = "FailedToFindOSpaceTypeMapping";

	internal const string FailedToFindCSpaceTypeMapping = "FailedToFindCSpaceTypeMapping";

	internal const string FailedToFindClrTypeMapping = "FailedToFindClrTypeMapping";

	internal const string GenericTypeNotSupported = "GenericTypeNotSupported";

	internal const string InvalidEDMVersion = "InvalidEDMVersion";

	internal const string Mapping_General_Error = "Mapping_General_Error";

	internal const string Mapping_InvalidContent_General = "Mapping_InvalidContent_General";

	internal const string Mapping_InvalidContent_EntityContainer = "Mapping_InvalidContent_EntityContainer";

	internal const string Mapping_InvalidContent_StorageEntityContainer = "Mapping_InvalidContent_StorageEntityContainer";

	internal const string Mapping_AlreadyMapped_StorageEntityContainer = "Mapping_AlreadyMapped_StorageEntityContainer";

	internal const string Mapping_InvalidContent_Entity_Set = "Mapping_InvalidContent_Entity_Set";

	internal const string Mapping_InvalidContent_Entity_Type = "Mapping_InvalidContent_Entity_Type";

	internal const string Mapping_InvalidContent_AbstractEntity_FunctionMapping = "Mapping_InvalidContent_AbstractEntity_FunctionMapping";

	internal const string Mapping_InvalidContent_AbstractEntity_Type = "Mapping_InvalidContent_AbstractEntity_Type";

	internal const string Mapping_InvalidContent_AbstractEntity_IsOfType = "Mapping_InvalidContent_AbstractEntity_IsOfType";

	internal const string Mapping_InvalidContent_Entity_Type_For_Entity_Set = "Mapping_InvalidContent_Entity_Type_For_Entity_Set";

	internal const string Mapping_Invalid_Association_Type_For_Association_Set = "Mapping_Invalid_Association_Type_For_Association_Set";

	internal const string Mapping_InvalidContent_Table = "Mapping_InvalidContent_Table";

	internal const string Mapping_InvalidContent_Complex_Type = "Mapping_InvalidContent_Complex_Type";

	internal const string Mapping_InvalidContent_Association_Set = "Mapping_InvalidContent_Association_Set";

	internal const string Mapping_InvalidContent_AssociationSet_Condition = "Mapping_InvalidContent_AssociationSet_Condition";

	internal const string Mapping_InvalidContent_ForeignKey_Association_Set = "Mapping_InvalidContent_ForeignKey_Association_Set";

	internal const string Mapping_InvalidContent_ForeignKey_Association_Set_PKtoPK = "Mapping_InvalidContent_ForeignKey_Association_Set_PKtoPK";

	internal const string Mapping_InvalidContent_Association_Type = "Mapping_InvalidContent_Association_Type";

	internal const string Mapping_InvalidContent_EndProperty = "Mapping_InvalidContent_EndProperty";

	internal const string Mapping_InvalidContent_Association_Type_Empty = "Mapping_InvalidContent_Association_Type_Empty";

	internal const string Mapping_InvalidContent_Table_Expected = "Mapping_InvalidContent_Table_Expected";

	internal const string Mapping_InvalidContent_Cdm_Member = "Mapping_InvalidContent_Cdm_Member";

	internal const string Mapping_InvalidContent_Column = "Mapping_InvalidContent_Column";

	internal const string Mapping_InvalidContent_End = "Mapping_InvalidContent_End";

	internal const string Mapping_InvalidContent_Container_SubElement = "Mapping_InvalidContent_Container_SubElement";

	internal const string Mapping_InvalidContent_Duplicate_Cdm_Member = "Mapping_InvalidContent_Duplicate_Cdm_Member";

	internal const string Mapping_InvalidContent_Duplicate_Condition_Member = "Mapping_InvalidContent_Duplicate_Condition_Member";

	internal const string Mapping_InvalidContent_ConditionMapping_Both_Members = "Mapping_InvalidContent_ConditionMapping_Both_Members";

	internal const string Mapping_InvalidContent_ConditionMapping_Either_Members = "Mapping_InvalidContent_ConditionMapping_Either_Members";

	internal const string Mapping_InvalidContent_ConditionMapping_Both_Values = "Mapping_InvalidContent_ConditionMapping_Both_Values";

	internal const string Mapping_InvalidContent_ConditionMapping_Either_Values = "Mapping_InvalidContent_ConditionMapping_Either_Values";

	internal const string Mapping_InvalidContent_ConditionMapping_NonScalar = "Mapping_InvalidContent_ConditionMapping_NonScalar";

	internal const string Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind = "Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind";

	internal const string Mapping_InvalidContent_ConditionMapping_InvalidMember = "Mapping_InvalidContent_ConditionMapping_InvalidMember";

	internal const string Mapping_InvalidContent_ConditionMapping_Computed = "Mapping_InvalidContent_ConditionMapping_Computed";

	internal const string Mapping_InvalidContent_Emtpty_SetMap = "Mapping_InvalidContent_Emtpty_SetMap";

	internal const string Mapping_InvalidContent_TypeMapping_QueryView = "Mapping_InvalidContent_TypeMapping_QueryView";

	internal const string Mapping_Default_OCMapping_Clr_Member = "Mapping_Default_OCMapping_Clr_Member";

	internal const string Mapping_Default_OCMapping_Clr_Member2 = "Mapping_Default_OCMapping_Clr_Member2";

	internal const string Mapping_Default_OCMapping_Invalid_MemberType = "Mapping_Default_OCMapping_Invalid_MemberType";

	internal const string Mapping_Default_OCMapping_MemberKind_Mismatch = "Mapping_Default_OCMapping_MemberKind_Mismatch";

	internal const string Mapping_Default_OCMapping_MultiplicityMismatch = "Mapping_Default_OCMapping_MultiplicityMismatch";

	internal const string Mapping_Default_OCMapping_Member_Count_Mismatch = "Mapping_Default_OCMapping_Member_Count_Mismatch";

	internal const string Mapping_Default_OCMapping_Member_Type_Mismatch = "Mapping_Default_OCMapping_Member_Type_Mismatch";

	internal const string Mapping_Enum_OCMapping_UnderlyingTypesMismatch = "Mapping_Enum_OCMapping_UnderlyingTypesMismatch";

	internal const string Mapping_Enum_OCMapping_MemberMismatch = "Mapping_Enum_OCMapping_MemberMismatch";

	internal const string Mapping_NotFound_EntityContainer = "Mapping_NotFound_EntityContainer";

	internal const string Mapping_Duplicate_CdmAssociationSet_StorageMap = "Mapping_Duplicate_CdmAssociationSet_StorageMap";

	internal const string Mapping_Invalid_CSRootElementMissing = "Mapping_Invalid_CSRootElementMissing";

	internal const string Mapping_ConditionValueTypeMismatch = "Mapping_ConditionValueTypeMismatch";

	internal const string Mapping_Storage_InvalidSpace = "Mapping_Storage_InvalidSpace";

	internal const string Mapping_Invalid_Member_Mapping = "Mapping_Invalid_Member_Mapping";

	internal const string Mapping_Invalid_CSide_ScalarProperty = "Mapping_Invalid_CSide_ScalarProperty";

	internal const string Mapping_Duplicate_Type = "Mapping_Duplicate_Type";

	internal const string Mapping_Duplicate_PropertyMap_CaseInsensitive = "Mapping_Duplicate_PropertyMap_CaseInsensitive";

	internal const string Mapping_Enum_EmptyValue = "Mapping_Enum_EmptyValue";

	internal const string Mapping_Enum_InvalidValue = "Mapping_Enum_InvalidValue";

	internal const string Mapping_InvalidMappingSchema_Parsing = "Mapping_InvalidMappingSchema_Parsing";

	internal const string Mapping_InvalidMappingSchema_validation = "Mapping_InvalidMappingSchema_validation";

	internal const string Mapping_Object_InvalidType = "Mapping_Object_InvalidType";

	internal const string Mapping_Provider_WrongConnectionType = "Mapping_Provider_WrongConnectionType";

	internal const string Mapping_Views_For_Extent_Not_Generated = "Mapping_Views_For_Extent_Not_Generated";

	internal const string Mapping_TableName_QueryView = "Mapping_TableName_QueryView";

	internal const string Mapping_Empty_QueryView = "Mapping_Empty_QueryView";

	internal const string Mapping_Empty_QueryView_OfType = "Mapping_Empty_QueryView_OfType";

	internal const string Mapping_Empty_QueryView_OfTypeOnly = "Mapping_Empty_QueryView_OfTypeOnly";

	internal const string Mapping_QueryView_PropertyMaps = "Mapping_QueryView_PropertyMaps";

	internal const string Mapping_Invalid_QueryView = "Mapping_Invalid_QueryView";

	internal const string Mapping_Invalid_QueryView2 = "Mapping_Invalid_QueryView2";

	internal const string Mapping_Invalid_QueryView_Type = "Mapping_Invalid_QueryView_Type";

	internal const string Mapping_TypeName_For_First_QueryView = "Mapping_TypeName_For_First_QueryView";

	internal const string Mapping_AllQueryViewAtCompileTime = "Mapping_AllQueryViewAtCompileTime";

	internal const string Mapping_QueryViewMultipleTypeInTypeName = "Mapping_QueryViewMultipleTypeInTypeName";

	internal const string Mapping_QueryView_Duplicate_OfType = "Mapping_QueryView_Duplicate_OfType";

	internal const string Mapping_QueryView_Duplicate_OfTypeOnly = "Mapping_QueryView_Duplicate_OfTypeOnly";

	internal const string Mapping_QueryView_TypeName_Not_Defined = "Mapping_QueryView_TypeName_Not_Defined";

	internal const string Mapping_QueryView_For_Base_Type = "Mapping_QueryView_For_Base_Type";

	internal const string Mapping_UnsupportedExpressionKind_QueryView = "Mapping_UnsupportedExpressionKind_QueryView";

	internal const string Mapping_UnsupportedFunctionCall_QueryView = "Mapping_UnsupportedFunctionCall_QueryView";

	internal const string Mapping_UnsupportedScanTarget_QueryView = "Mapping_UnsupportedScanTarget_QueryView";

	internal const string Mapping_UnsupportedPropertyKind_QueryView = "Mapping_UnsupportedPropertyKind_QueryView";

	internal const string Mapping_UnsupportedInitialization_QueryView = "Mapping_UnsupportedInitialization_QueryView";

	internal const string Mapping_EntitySetMismatchOnAssociationSetEnd_QueryView = "Mapping_EntitySetMismatchOnAssociationSetEnd_QueryView";

	internal const string Mapping_Invalid_Query_Views_MissingSetClosure = "Mapping_Invalid_Query_Views_MissingSetClosure";

	internal const string DbMappingViewCacheTypeAttribute_InvalidContextType = "DbMappingViewCacheTypeAttribute_InvalidContextType";

	internal const string DbMappingViewCacheTypeAttribute_CacheTypeNotFound = "DbMappingViewCacheTypeAttribute_CacheTypeNotFound";

	internal const string DbMappingViewCacheTypeAttribute_MultipleInstancesWithSameContextType = "DbMappingViewCacheTypeAttribute_MultipleInstancesWithSameContextType";

	internal const string DbMappingViewCacheFactory_CreateFailure = "DbMappingViewCacheFactory_CreateFailure";

	internal const string Generated_View_Type_Super_Class = "Generated_View_Type_Super_Class";

	internal const string Generated_Views_Invalid_Extent = "Generated_Views_Invalid_Extent";

	internal const string MappingViewCacheFactory_MustNotChange = "MappingViewCacheFactory_MustNotChange";

	internal const string Mapping_ItemWithSameNameExistsBothInCSpaceAndSSpace = "Mapping_ItemWithSameNameExistsBothInCSpaceAndSSpace";

	internal const string Mapping_AbstractTypeMappingToNonAbstractType = "Mapping_AbstractTypeMappingToNonAbstractType";

	internal const string Mapping_EnumTypeMappingToNonEnumType = "Mapping_EnumTypeMappingToNonEnumType";

	internal const string StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping = "StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping";

	internal const string Mapping_InvalidContent_IsTypeOfNotTerminated = "Mapping_InvalidContent_IsTypeOfNotTerminated";

	internal const string Mapping_CannotMapCLRTypeMultipleTimes = "Mapping_CannotMapCLRTypeMultipleTimes";

	internal const string Mapping_ModificationFunction_In_Table_Context = "Mapping_ModificationFunction_In_Table_Context";

	internal const string Mapping_ModificationFunction_Multiple_Types = "Mapping_ModificationFunction_Multiple_Types";

	internal const string Mapping_ModificationFunction_UnknownFunction = "Mapping_ModificationFunction_UnknownFunction";

	internal const string Mapping_ModificationFunction_AmbiguousFunction = "Mapping_ModificationFunction_AmbiguousFunction";

	internal const string Mapping_ModificationFunction_NotValidFunction = "Mapping_ModificationFunction_NotValidFunction";

	internal const string Mapping_ModificationFunction_NotValidFunctionParameter = "Mapping_ModificationFunction_NotValidFunctionParameter";

	internal const string Mapping_ModificationFunction_MissingParameter = "Mapping_ModificationFunction_MissingParameter";

	internal const string Mapping_ModificationFunction_AssociationSetDoesNotExist = "Mapping_ModificationFunction_AssociationSetDoesNotExist";

	internal const string Mapping_ModificationFunction_AssociationSetRoleDoesNotExist = "Mapping_ModificationFunction_AssociationSetRoleDoesNotExist";

	internal const string Mapping_ModificationFunction_AssociationSetFromRoleIsNotEntitySet = "Mapping_ModificationFunction_AssociationSetFromRoleIsNotEntitySet";

	internal const string Mapping_ModificationFunction_AssociationSetCardinality = "Mapping_ModificationFunction_AssociationSetCardinality";

	internal const string Mapping_ModificationFunction_ComplexTypeNotFound = "Mapping_ModificationFunction_ComplexTypeNotFound";

	internal const string Mapping_ModificationFunction_WrongComplexType = "Mapping_ModificationFunction_WrongComplexType";

	internal const string Mapping_ModificationFunction_MissingVersion = "Mapping_ModificationFunction_MissingVersion";

	internal const string Mapping_ModificationFunction_VersionMustBeOriginal = "Mapping_ModificationFunction_VersionMustBeOriginal";

	internal const string Mapping_ModificationFunction_VersionMustBeCurrent = "Mapping_ModificationFunction_VersionMustBeCurrent";

	internal const string Mapping_ModificationFunction_ParameterNotFound = "Mapping_ModificationFunction_ParameterNotFound";

	internal const string Mapping_ModificationFunction_PropertyNotFound = "Mapping_ModificationFunction_PropertyNotFound";

	internal const string Mapping_ModificationFunction_PropertyNotKey = "Mapping_ModificationFunction_PropertyNotKey";

	internal const string Mapping_ModificationFunction_ParameterBoundTwice = "Mapping_ModificationFunction_ParameterBoundTwice";

	internal const string Mapping_ModificationFunction_RedundantEntityTypeMapping = "Mapping_ModificationFunction_RedundantEntityTypeMapping";

	internal const string Mapping_ModificationFunction_MissingSetClosure = "Mapping_ModificationFunction_MissingSetClosure";

	internal const string Mapping_ModificationFunction_MissingEntityType = "Mapping_ModificationFunction_MissingEntityType";

	internal const string Mapping_ModificationFunction_PropertyParameterTypeMismatch = "Mapping_ModificationFunction_PropertyParameterTypeMismatch";

	internal const string Mapping_ModificationFunction_AssociationSetAmbiguous = "Mapping_ModificationFunction_AssociationSetAmbiguous";

	internal const string Mapping_ModificationFunction_MultipleEndsOfAssociationMapped = "Mapping_ModificationFunction_MultipleEndsOfAssociationMapped";

	internal const string Mapping_ModificationFunction_AmbiguousResultBinding = "Mapping_ModificationFunction_AmbiguousResultBinding";

	internal const string Mapping_ModificationFunction_AssociationSetNotMappedForOperation = "Mapping_ModificationFunction_AssociationSetNotMappedForOperation";

	internal const string Mapping_ModificationFunction_AssociationEndMappingInvalidForEntityType = "Mapping_ModificationFunction_AssociationEndMappingInvalidForEntityType";

	internal const string Mapping_ModificationFunction_AssociationEndMappingForeignKeyAssociation = "Mapping_ModificationFunction_AssociationEndMappingForeignKeyAssociation";

	internal const string Mapping_StoreTypeMismatch_ScalarPropertyMapping = "Mapping_StoreTypeMismatch_ScalarPropertyMapping";

	internal const string Mapping_DistinctFlagInReadWriteContainer = "Mapping_DistinctFlagInReadWriteContainer";

	internal const string Mapping_ProviderReturnsNullType = "Mapping_ProviderReturnsNullType";

	internal const string Mapping_DifferentEdmStoreVersion = "Mapping_DifferentEdmStoreVersion";

	internal const string Mapping_DifferentMappingEdmStoreVersion = "Mapping_DifferentMappingEdmStoreVersion";

	internal const string Mapping_FunctionImport_StoreFunctionDoesNotExist = "Mapping_FunctionImport_StoreFunctionDoesNotExist";

	internal const string Mapping_FunctionImport_FunctionImportDoesNotExist = "Mapping_FunctionImport_FunctionImportDoesNotExist";

	internal const string Mapping_FunctionImport_FunctionImportMappedMultipleTimes = "Mapping_FunctionImport_FunctionImportMappedMultipleTimes";

	internal const string Mapping_FunctionImport_TargetFunctionMustBeNonComposable = "Mapping_FunctionImport_TargetFunctionMustBeNonComposable";

	internal const string Mapping_FunctionImport_TargetFunctionMustBeComposable = "Mapping_FunctionImport_TargetFunctionMustBeComposable";

	internal const string Mapping_FunctionImport_TargetParameterHasNoCorrespondingImportParameter = "Mapping_FunctionImport_TargetParameterHasNoCorrespondingImportParameter";

	internal const string Mapping_FunctionImport_ImportParameterHasNoCorrespondingTargetParameter = "Mapping_FunctionImport_ImportParameterHasNoCorrespondingTargetParameter";

	internal const string Mapping_FunctionImport_IncompatibleParameterMode = "Mapping_FunctionImport_IncompatibleParameterMode";

	internal const string Mapping_FunctionImport_IncompatibleParameterType = "Mapping_FunctionImport_IncompatibleParameterType";

	internal const string Mapping_FunctionImport_IncompatibleEnumParameterType = "Mapping_FunctionImport_IncompatibleEnumParameterType";

	internal const string Mapping_FunctionImport_RowsAffectedParameterDoesNotExist = "Mapping_FunctionImport_RowsAffectedParameterDoesNotExist";

	internal const string Mapping_FunctionImport_RowsAffectedParameterHasWrongType = "Mapping_FunctionImport_RowsAffectedParameterHasWrongType";

	internal const string Mapping_FunctionImport_RowsAffectedParameterHasWrongMode = "Mapping_FunctionImport_RowsAffectedParameterHasWrongMode";

	internal const string Mapping_FunctionImport_EntityTypeMappingForFunctionNotReturningEntitySet = "Mapping_FunctionImport_EntityTypeMappingForFunctionNotReturningEntitySet";

	internal const string Mapping_FunctionImport_InvalidContentEntityTypeForEntitySet = "Mapping_FunctionImport_InvalidContentEntityTypeForEntitySet";

	internal const string Mapping_FunctionImport_ConditionValueTypeMismatch = "Mapping_FunctionImport_ConditionValueTypeMismatch";

	internal const string Mapping_FunctionImport_UnsupportedType = "Mapping_FunctionImport_UnsupportedType";

	internal const string Mapping_FunctionImport_ResultMappingCountDoesNotMatchResultCount = "Mapping_FunctionImport_ResultMappingCountDoesNotMatchResultCount";

	internal const string Mapping_FunctionImport_ResultMapping_MappedTypeDoesNotMatchReturnType = "Mapping_FunctionImport_ResultMapping_MappedTypeDoesNotMatchReturnType";

	internal const string Mapping_FunctionImport_ResultMapping_InvalidCTypeCTExpected = "Mapping_FunctionImport_ResultMapping_InvalidCTypeCTExpected";

	internal const string Mapping_FunctionImport_ResultMapping_InvalidCTypeETExpected = "Mapping_FunctionImport_ResultMapping_InvalidCTypeETExpected";

	internal const string Mapping_FunctionImport_ResultMapping_InvalidSType = "Mapping_FunctionImport_ResultMapping_InvalidSType";

	internal const string Mapping_FunctionImport_PropertyNotMapped = "Mapping_FunctionImport_PropertyNotMapped";

	internal const string Mapping_FunctionImport_ImplicitMappingForAbstractReturnType = "Mapping_FunctionImport_ImplicitMappingForAbstractReturnType";

	internal const string Mapping_FunctionImport_ScalarMappingToMulticolumnTVF = "Mapping_FunctionImport_ScalarMappingToMulticolumnTVF";

	internal const string Mapping_FunctionImport_ScalarMappingTypeMismatch = "Mapping_FunctionImport_ScalarMappingTypeMismatch";

	internal const string Mapping_FunctionImport_UnreachableType = "Mapping_FunctionImport_UnreachableType";

	internal const string Mapping_FunctionImport_UnreachableIsTypeOf = "Mapping_FunctionImport_UnreachableIsTypeOf";

	internal const string Mapping_FunctionImport_FunctionAmbiguous = "Mapping_FunctionImport_FunctionAmbiguous";

	internal const string Mapping_FunctionImport_CannotInferTargetFunctionKeys = "Mapping_FunctionImport_CannotInferTargetFunctionKeys";

	internal const string Entity_EntityCantHaveMultipleChangeTrackers = "Entity_EntityCantHaveMultipleChangeTrackers";

	internal const string ComplexObject_NullableComplexTypesNotSupported = "ComplexObject_NullableComplexTypesNotSupported";

	internal const string ComplexObject_ComplexObjectAlreadyAttachedToParent = "ComplexObject_ComplexObjectAlreadyAttachedToParent";

	internal const string ComplexObject_ComplexChangeRequestedOnScalarProperty = "ComplexObject_ComplexChangeRequestedOnScalarProperty";

	internal const string ObjectStateEntry_SetModifiedOnInvalidProperty = "ObjectStateEntry_SetModifiedOnInvalidProperty";

	internal const string ObjectStateEntry_OriginalValuesDoesNotExist = "ObjectStateEntry_OriginalValuesDoesNotExist";

	internal const string ObjectStateEntry_CurrentValuesDoesNotExist = "ObjectStateEntry_CurrentValuesDoesNotExist";

	internal const string ObjectStateEntry_InvalidState = "ObjectStateEntry_InvalidState";

	internal const string ObjectStateEntry_CannotModifyKeyProperty = "ObjectStateEntry_CannotModifyKeyProperty";

	internal const string ObjectStateEntry_CantModifyRelationValues = "ObjectStateEntry_CantModifyRelationValues";

	internal const string ObjectStateEntry_CantModifyRelationState = "ObjectStateEntry_CantModifyRelationState";

	internal const string ObjectStateEntry_CantModifyDetachedDeletedEntries = "ObjectStateEntry_CantModifyDetachedDeletedEntries";

	internal const string ObjectStateEntry_SetModifiedStates = "ObjectStateEntry_SetModifiedStates";

	internal const string ObjectStateEntry_CantSetEntityKey = "ObjectStateEntry_CantSetEntityKey";

	internal const string ObjectStateEntry_CannotAccessKeyEntryValues = "ObjectStateEntry_CannotAccessKeyEntryValues";

	internal const string ObjectStateEntry_CannotModifyKeyEntryState = "ObjectStateEntry_CannotModifyKeyEntryState";

	internal const string ObjectStateEntry_CannotDeleteOnKeyEntry = "ObjectStateEntry_CannotDeleteOnKeyEntry";

	internal const string ObjectStateEntry_EntityMemberChangedWithoutEntityMemberChanging = "ObjectStateEntry_EntityMemberChangedWithoutEntityMemberChanging";

	internal const string ObjectStateEntry_ChangeOnUnmappedProperty = "ObjectStateEntry_ChangeOnUnmappedProperty";

	internal const string ObjectStateEntry_ChangeOnUnmappedComplexProperty = "ObjectStateEntry_ChangeOnUnmappedComplexProperty";

	internal const string ObjectStateEntry_ChangedInDifferentStateFromChanging = "ObjectStateEntry_ChangedInDifferentStateFromChanging";

	internal const string ObjectStateEntry_UnableToEnumerateCollection = "ObjectStateEntry_UnableToEnumerateCollection";

	internal const string ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers = "ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers";

	internal const string ObjectStateEntry_InvalidTypeForComplexTypeProperty = "ObjectStateEntry_InvalidTypeForComplexTypeProperty";

	internal const string ObjectStateEntry_ComplexObjectUsedMultipleTimes = "ObjectStateEntry_ComplexObjectUsedMultipleTimes";

	internal const string ObjectStateEntry_SetOriginalComplexProperties = "ObjectStateEntry_SetOriginalComplexProperties";

	internal const string ObjectStateEntry_NullOriginalValueForNonNullableProperty = "ObjectStateEntry_NullOriginalValueForNonNullableProperty";

	internal const string ObjectStateEntry_SetOriginalPrimaryKey = "ObjectStateEntry_SetOriginalPrimaryKey";

	internal const string ObjectStateManager_NoEntryExistForEntityKey = "ObjectStateManager_NoEntryExistForEntityKey";

	internal const string ObjectStateManager_NoEntryExistsForObject = "ObjectStateManager_NoEntryExistsForObject";

	internal const string ObjectStateManager_EntityNotTracked = "ObjectStateManager_EntityNotTracked";

	internal const string ObjectStateManager_DetachedObjectStateEntriesDoesNotExistInObjectStateManager = "ObjectStateManager_DetachedObjectStateEntriesDoesNotExistInObjectStateManager";

	internal const string ObjectStateManager_ObjectStateManagerContainsThisEntityKey = "ObjectStateManager_ObjectStateManagerContainsThisEntityKey";

	internal const string ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity = "ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity";

	internal const string ObjectStateManager_CannotFixUpKeyToExistingValues = "ObjectStateManager_CannotFixUpKeyToExistingValues";

	internal const string ObjectStateManager_KeyPropertyDoesntMatchValueInKey = "ObjectStateManager_KeyPropertyDoesntMatchValueInKey";

	internal const string ObjectStateManager_KeyPropertyDoesntMatchValueInKeyForAttach = "ObjectStateManager_KeyPropertyDoesntMatchValueInKeyForAttach";

	internal const string ObjectStateManager_InvalidKey = "ObjectStateManager_InvalidKey";

	internal const string ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType = "ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType";

	internal const string ObjectStateManager_AcceptChangesEntityKeyIsNotValid = "ObjectStateManager_AcceptChangesEntityKeyIsNotValid";

	internal const string ObjectStateManager_EntityConflictsWithKeyEntry = "ObjectStateManager_EntityConflictsWithKeyEntry";

	internal const string ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity = "ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity";

	internal const string ObjectStateManager_CannotChangeRelationshipStateEntityDeleted = "ObjectStateManager_CannotChangeRelationshipStateEntityDeleted";

	internal const string ObjectStateManager_CannotChangeRelationshipStateEntityAdded = "ObjectStateManager_CannotChangeRelationshipStateEntityAdded";

	internal const string ObjectStateManager_CannotChangeRelationshipStateKeyEntry = "ObjectStateManager_CannotChangeRelationshipStateKeyEntry";

	internal const string ObjectStateManager_ConflictingChangesOfRelationshipDetected = "ObjectStateManager_ConflictingChangesOfRelationshipDetected";

	internal const string ObjectStateManager_ChangeRelationshipStateNotSupportedForForeignKeyAssociations = "ObjectStateManager_ChangeRelationshipStateNotSupportedForForeignKeyAssociations";

	internal const string ObjectStateManager_ChangeStateFromAddedWithNullKeyIsInvalid = "ObjectStateManager_ChangeStateFromAddedWithNullKeyIsInvalid";

	internal const string ObjectContext_ClientEntityRemovedFromStore = "ObjectContext_ClientEntityRemovedFromStore";

	internal const string ObjectContext_StoreEntityNotPresentInClient = "ObjectContext_StoreEntityNotPresentInClient";

	internal const string ObjectContext_InvalidConnectionString = "ObjectContext_InvalidConnectionString";

	internal const string ObjectContext_InvalidConnection = "ObjectContext_InvalidConnection";

	internal const string ObjectContext_InvalidDefaultContainerName = "ObjectContext_InvalidDefaultContainerName";

	internal const string ObjectContext_NthElementInAddedState = "ObjectContext_NthElementInAddedState";

	internal const string ObjectContext_NthElementIsDuplicate = "ObjectContext_NthElementIsDuplicate";

	internal const string ObjectContext_NthElementIsNull = "ObjectContext_NthElementIsNull";

	internal const string ObjectContext_NthElementNotInObjectStateManager = "ObjectContext_NthElementNotInObjectStateManager";

	internal const string ObjectContext_ObjectNotFound = "ObjectContext_ObjectNotFound";

	internal const string ObjectContext_CannotDeleteEntityNotInObjectStateManager = "ObjectContext_CannotDeleteEntityNotInObjectStateManager";

	internal const string ObjectContext_CannotDetachEntityNotInObjectStateManager = "ObjectContext_CannotDetachEntityNotInObjectStateManager";

	internal const string ObjectContext_EntitySetNotFoundForName = "ObjectContext_EntitySetNotFoundForName";

	internal const string ObjectContext_EntityContainerNotFoundForName = "ObjectContext_EntityContainerNotFoundForName";

	internal const string ObjectContext_InvalidCommandTimeout = "ObjectContext_InvalidCommandTimeout";

	internal const string ObjectContext_NoMappingForEntityType = "ObjectContext_NoMappingForEntityType";

	internal const string ObjectContext_EntityAlreadyExistsInObjectStateManager = "ObjectContext_EntityAlreadyExistsInObjectStateManager";

	internal const string ObjectContext_InvalidEntitySetInKey = "ObjectContext_InvalidEntitySetInKey";

	internal const string ObjectContext_CannotAttachEntityWithoutKey = "ObjectContext_CannotAttachEntityWithoutKey";

	internal const string ObjectContext_CannotAttachEntityWithTemporaryKey = "ObjectContext_CannotAttachEntityWithTemporaryKey";

	internal const string ObjectContext_EntitySetNameOrEntityKeyRequired = "ObjectContext_EntitySetNameOrEntityKeyRequired";

	internal const string ObjectContext_ExecuteFunctionTypeMismatch = "ObjectContext_ExecuteFunctionTypeMismatch";

	internal const string ObjectContext_ExecuteFunctionCalledWithScalarFunction = "ObjectContext_ExecuteFunctionCalledWithScalarFunction";

	internal const string ObjectContext_ExecuteFunctionCalledWithNonQueryFunction = "ObjectContext_ExecuteFunctionCalledWithNonQueryFunction";

	internal const string ObjectContext_ExecuteFunctionCalledWithNullParameter = "ObjectContext_ExecuteFunctionCalledWithNullParameter";

	internal const string ObjectContext_ContainerQualifiedEntitySetNameRequired = "ObjectContext_ContainerQualifiedEntitySetNameRequired";

	internal const string ObjectContext_CannotSetDefaultContainerName = "ObjectContext_CannotSetDefaultContainerName";

	internal const string ObjectContext_QualfiedEntitySetName = "ObjectContext_QualfiedEntitySetName";

	internal const string ObjectContext_EntitiesHaveDifferentType = "ObjectContext_EntitiesHaveDifferentType";

	internal const string ObjectContext_EntityMustBeUnchangedOrModified = "ObjectContext_EntityMustBeUnchangedOrModified";

	internal const string ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted = "ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted";

	internal const string ObjectContext_AcceptAllChangesFailure = "ObjectContext_AcceptAllChangesFailure";

	internal const string ObjectContext_CommitWithConceptualNull = "ObjectContext_CommitWithConceptualNull";

	internal const string ObjectContext_InvalidEntitySetOnEntity = "ObjectContext_InvalidEntitySetOnEntity";

	internal const string ObjectContext_InvalidObjectSetTypeForEntitySet = "ObjectContext_InvalidObjectSetTypeForEntitySet";

	internal const string ObjectContext_InvalidEntitySetInKeyFromName = "ObjectContext_InvalidEntitySetInKeyFromName";

	internal const string ObjectContext_ObjectDisposed = "ObjectContext_ObjectDisposed";

	internal const string ObjectContext_CannotExplicitlyLoadDetachedRelationships = "ObjectContext_CannotExplicitlyLoadDetachedRelationships";

	internal const string ObjectContext_CannotLoadReferencesUsingDifferentContext = "ObjectContext_CannotLoadReferencesUsingDifferentContext";

	internal const string ObjectContext_SelectorExpressionMustBeMemberAccess = "ObjectContext_SelectorExpressionMustBeMemberAccess";

	internal const string ObjectContext_MultipleEntitySetsFoundInSingleContainer = "ObjectContext_MultipleEntitySetsFoundInSingleContainer";

	internal const string ObjectContext_MultipleEntitySetsFoundInAllContainers = "ObjectContext_MultipleEntitySetsFoundInAllContainers";

	internal const string ObjectContext_NoEntitySetFoundForType = "ObjectContext_NoEntitySetFoundForType";

	internal const string ObjectContext_EntityNotInObjectSet_Delete = "ObjectContext_EntityNotInObjectSet_Delete";

	internal const string ObjectContext_EntityNotInObjectSet_Detach = "ObjectContext_EntityNotInObjectSet_Detach";

	internal const string ObjectContext_InvalidEntityState = "ObjectContext_InvalidEntityState";

	internal const string ObjectContext_InvalidRelationshipState = "ObjectContext_InvalidRelationshipState";

	internal const string ObjectContext_EntityNotTrackedOrHasTempKey = "ObjectContext_EntityNotTrackedOrHasTempKey";

	internal const string ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues = "ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues";

	internal const string ObjectContext_InvalidEntitySetForStoreQuery = "ObjectContext_InvalidEntitySetForStoreQuery";

	internal const string ObjectContext_InvalidTypeForStoreQuery = "ObjectContext_InvalidTypeForStoreQuery";

	internal const string ObjectContext_TwoPropertiesMappedToSameColumn = "ObjectContext_TwoPropertiesMappedToSameColumn";

	internal const string RelatedEnd_InvalidOwnerStateForAttach = "RelatedEnd_InvalidOwnerStateForAttach";

	internal const string RelatedEnd_InvalidNthElementNullForAttach = "RelatedEnd_InvalidNthElementNullForAttach";

	internal const string RelatedEnd_InvalidNthElementContextForAttach = "RelatedEnd_InvalidNthElementContextForAttach";

	internal const string RelatedEnd_InvalidNthElementStateForAttach = "RelatedEnd_InvalidNthElementStateForAttach";

	internal const string RelatedEnd_InvalidEntityContextForAttach = "RelatedEnd_InvalidEntityContextForAttach";

	internal const string RelatedEnd_InvalidEntityStateForAttach = "RelatedEnd_InvalidEntityStateForAttach";

	internal const string RelatedEnd_UnableToAddEntity = "RelatedEnd_UnableToAddEntity";

	internal const string RelatedEnd_UnableToRemoveEntity = "RelatedEnd_UnableToRemoveEntity";

	internal const string RelatedEnd_UnableToAddRelationshipWithDeletedEntity = "RelatedEnd_UnableToAddRelationshipWithDeletedEntity";

	internal const string RelatedEnd_CannotSerialize = "RelatedEnd_CannotSerialize";

	internal const string RelatedEnd_CannotAddToFixedSizeArray = "RelatedEnd_CannotAddToFixedSizeArray";

	internal const string RelatedEnd_CannotRemoveFromFixedSizeArray = "RelatedEnd_CannotRemoveFromFixedSizeArray";

	internal const string Materializer_PropertyIsNotNullable = "Materializer_PropertyIsNotNullable";

	internal const string Materializer_PropertyIsNotNullableWithName = "Materializer_PropertyIsNotNullableWithName";

	internal const string Materializer_SetInvalidValue = "Materializer_SetInvalidValue";

	internal const string Materializer_InvalidCastReference = "Materializer_InvalidCastReference";

	internal const string Materializer_InvalidCastNullable = "Materializer_InvalidCastNullable";

	internal const string Materializer_NullReferenceCast = "Materializer_NullReferenceCast";

	internal const string Materializer_RecyclingEntity = "Materializer_RecyclingEntity";

	internal const string Materializer_AddedEntityAlreadyExists = "Materializer_AddedEntityAlreadyExists";

	internal const string Materializer_CannotReEnumerateQueryResults = "Materializer_CannotReEnumerateQueryResults";

	internal const string Materializer_UnsupportedType = "Materializer_UnsupportedType";

	internal const string Collections_NoRelationshipSetMatched = "Collections_NoRelationshipSetMatched";

	internal const string Collections_ExpectedCollectionGotReference = "Collections_ExpectedCollectionGotReference";

	internal const string Collections_InvalidEntityStateSource = "Collections_InvalidEntityStateSource";

	internal const string Collections_InvalidEntityStateLoad = "Collections_InvalidEntityStateLoad";

	internal const string Collections_CannotFillTryDifferentMergeOption = "Collections_CannotFillTryDifferentMergeOption";

	internal const string Collections_UnableToMergeCollections = "Collections_UnableToMergeCollections";

	internal const string EntityReference_ExpectedReferenceGotCollection = "EntityReference_ExpectedReferenceGotCollection";

	internal const string EntityReference_CannotAddMoreThanOneEntityToEntityReference = "EntityReference_CannotAddMoreThanOneEntityToEntityReference";

	internal const string EntityReference_LessThanExpectedRelatedEntitiesFound = "EntityReference_LessThanExpectedRelatedEntitiesFound";

	internal const string EntityReference_MoreThanExpectedRelatedEntitiesFound = "EntityReference_MoreThanExpectedRelatedEntitiesFound";

	internal const string EntityReference_CannotChangeReferentialConstraintProperty = "EntityReference_CannotChangeReferentialConstraintProperty";

	internal const string EntityReference_CannotSetSpecialKeys = "EntityReference_CannotSetSpecialKeys";

	internal const string EntityReference_EntityKeyValueMismatch = "EntityReference_EntityKeyValueMismatch";

	internal const string RelatedEnd_RelatedEndNotFound = "RelatedEnd_RelatedEndNotFound";

	internal const string RelatedEnd_RelatedEndNotAttachedToContext = "RelatedEnd_RelatedEndNotAttachedToContext";

	internal const string RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd = "RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd";

	internal const string RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd = "RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd";

	internal const string RelatedEnd_InvalidContainedType_Collection = "RelatedEnd_InvalidContainedType_Collection";

	internal const string RelatedEnd_InvalidContainedType_Reference = "RelatedEnd_InvalidContainedType_Reference";

	internal const string RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities = "RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities";

	internal const string RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts = "RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts";

	internal const string RelatedEnd_MismatchedMergeOptionOnLoad = "RelatedEnd_MismatchedMergeOptionOnLoad";

	internal const string RelatedEnd_EntitySetIsNotValidForRelationship = "RelatedEnd_EntitySetIsNotValidForRelationship";

	internal const string RelatedEnd_OwnerIsNull = "RelatedEnd_OwnerIsNull";

	internal const string RelationshipManager_UnableToRetrieveReferentialConstraintProperties = "RelationshipManager_UnableToRetrieveReferentialConstraintProperties";

	internal const string RelationshipManager_InconsistentReferentialConstraintProperties = "RelationshipManager_InconsistentReferentialConstraintProperties";

	internal const string RelationshipManager_CircularRelationshipsWithReferentialConstraints = "RelationshipManager_CircularRelationshipsWithReferentialConstraints";

	internal const string RelationshipManager_UnableToFindRelationshipTypeInMetadata = "RelationshipManager_UnableToFindRelationshipTypeInMetadata";

	internal const string RelationshipManager_InvalidTargetRole = "RelationshipManager_InvalidTargetRole";

	internal const string RelationshipManager_UnexpectedNull = "RelationshipManager_UnexpectedNull";

	internal const string RelationshipManager_InvalidRelationshipManagerOwner = "RelationshipManager_InvalidRelationshipManagerOwner";

	internal const string RelationshipManager_OwnerIsNotSourceType = "RelationshipManager_OwnerIsNotSourceType";

	internal const string RelationshipManager_UnexpectedNullContext = "RelationshipManager_UnexpectedNullContext";

	internal const string RelationshipManager_ReferenceAlreadyInitialized = "RelationshipManager_ReferenceAlreadyInitialized";

	internal const string RelationshipManager_RelationshipManagerAttached = "RelationshipManager_RelationshipManagerAttached";

	internal const string RelationshipManager_InitializeIsForDeserialization = "RelationshipManager_InitializeIsForDeserialization";

	internal const string RelationshipManager_CollectionAlreadyInitialized = "RelationshipManager_CollectionAlreadyInitialized";

	internal const string RelationshipManager_CollectionRelationshipManagerAttached = "RelationshipManager_CollectionRelationshipManagerAttached";

	internal const string RelationshipManager_CollectionInitializeIsForDeserialization = "RelationshipManager_CollectionInitializeIsForDeserialization";

	internal const string RelationshipManager_NavigationPropertyNotFound = "RelationshipManager_NavigationPropertyNotFound";

	internal const string RelationshipManager_CannotGetRelatEndForDetachedPocoEntity = "RelationshipManager_CannotGetRelatEndForDetachedPocoEntity";

	internal const string ObjectView_CannotReplacetheEntityorRow = "ObjectView_CannotReplacetheEntityorRow";

	internal const string ObjectView_IndexBasedInsertIsNotSupported = "ObjectView_IndexBasedInsertIsNotSupported";

	internal const string ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList = "ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList";

	internal const string ObjectView_AddNewOperationNotAllowedOnAbstractBindingList = "ObjectView_AddNewOperationNotAllowedOnAbstractBindingList";

	internal const string ObjectView_IncompatibleArgument = "ObjectView_IncompatibleArgument";

	internal const string ObjectView_CannotResolveTheEntitySet = "ObjectView_CannotResolveTheEntitySet";

	internal const string CodeGen_ConstructorNoParameterless = "CodeGen_ConstructorNoParameterless";

	internal const string CodeGen_PropertyDeclaringTypeIsValueType = "CodeGen_PropertyDeclaringTypeIsValueType";

	internal const string CodeGen_PropertyUnsupportedType = "CodeGen_PropertyUnsupportedType";

	internal const string CodeGen_PropertyIsIndexed = "CodeGen_PropertyIsIndexed";

	internal const string CodeGen_PropertyIsStatic = "CodeGen_PropertyIsStatic";

	internal const string CodeGen_PropertyNoGetter = "CodeGen_PropertyNoGetter";

	internal const string CodeGen_PropertyNoSetter = "CodeGen_PropertyNoSetter";

	internal const string PocoEntityWrapper_UnableToSetFieldOrProperty = "PocoEntityWrapper_UnableToSetFieldOrProperty";

	internal const string PocoEntityWrapper_UnexpectedTypeForNavigationProperty = "PocoEntityWrapper_UnexpectedTypeForNavigationProperty";

	internal const string PocoEntityWrapper_UnableToMaterializeArbitaryNavPropType = "PocoEntityWrapper_UnableToMaterializeArbitaryNavPropType";

	internal const string GeneralQueryError = "GeneralQueryError";

	internal const string CtxAlias = "CtxAlias";

	internal const string CtxAliasedNamespaceImport = "CtxAliasedNamespaceImport";

	internal const string CtxAnd = "CtxAnd";

	internal const string CtxAnyElement = "CtxAnyElement";

	internal const string CtxApplyClause = "CtxApplyClause";

	internal const string CtxBetween = "CtxBetween";

	internal const string CtxCase = "CtxCase";

	internal const string CtxCaseElse = "CtxCaseElse";

	internal const string CtxCaseWhenThen = "CtxCaseWhenThen";

	internal const string CtxCast = "CtxCast";

	internal const string CtxCollatedOrderByClauseItem = "CtxCollatedOrderByClauseItem";

	internal const string CtxCollectionTypeDefinition = "CtxCollectionTypeDefinition";

	internal const string CtxCommandExpression = "CtxCommandExpression";

	internal const string CtxCreateRef = "CtxCreateRef";

	internal const string CtxDeref = "CtxDeref";

	internal const string CtxDivide = "CtxDivide";

	internal const string CtxElement = "CtxElement";

	internal const string CtxEquals = "CtxEquals";

	internal const string CtxEscapedIdentifier = "CtxEscapedIdentifier";

	internal const string CtxExcept = "CtxExcept";

	internal const string CtxExists = "CtxExists";

	internal const string CtxExpressionList = "CtxExpressionList";

	internal const string CtxFlatten = "CtxFlatten";

	internal const string CtxFromApplyClause = "CtxFromApplyClause";

	internal const string CtxFromClause = "CtxFromClause";

	internal const string CtxFromClauseItem = "CtxFromClauseItem";

	internal const string CtxFromClauseList = "CtxFromClauseList";

	internal const string CtxFromJoinClause = "CtxFromJoinClause";

	internal const string CtxFunction = "CtxFunction";

	internal const string CtxFunctionDefinition = "CtxFunctionDefinition";

	internal const string CtxGreaterThan = "CtxGreaterThan";

	internal const string CtxGreaterThanEqual = "CtxGreaterThanEqual";

	internal const string CtxGroupByClause = "CtxGroupByClause";

	internal const string CtxGroupPartition = "CtxGroupPartition";

	internal const string CtxHavingClause = "CtxHavingClause";

	internal const string CtxIdentifier = "CtxIdentifier";

	internal const string CtxIn = "CtxIn";

	internal const string CtxIntersect = "CtxIntersect";

	internal const string CtxIsNotNull = "CtxIsNotNull";

	internal const string CtxIsNotOf = "CtxIsNotOf";

	internal const string CtxIsNull = "CtxIsNull";

	internal const string CtxIsOf = "CtxIsOf";

	internal const string CtxJoinClause = "CtxJoinClause";

	internal const string CtxJoinOnClause = "CtxJoinOnClause";

	internal const string CtxKey = "CtxKey";

	internal const string CtxLessThan = "CtxLessThan";

	internal const string CtxLessThanEqual = "CtxLessThanEqual";

	internal const string CtxLike = "CtxLike";

	internal const string CtxLimitSubClause = "CtxLimitSubClause";

	internal const string CtxLiteral = "CtxLiteral";

	internal const string CtxMemberAccess = "CtxMemberAccess";

	internal const string CtxMethod = "CtxMethod";

	internal const string CtxMinus = "CtxMinus";

	internal const string CtxModulus = "CtxModulus";

	internal const string CtxMultiply = "CtxMultiply";

	internal const string CtxMultisetCtor = "CtxMultisetCtor";

	internal const string CtxNamespaceImport = "CtxNamespaceImport";

	internal const string CtxNamespaceImportList = "CtxNamespaceImportList";

	internal const string CtxNavigate = "CtxNavigate";

	internal const string CtxNot = "CtxNot";

	internal const string CtxNotBetween = "CtxNotBetween";

	internal const string CtxNotEqual = "CtxNotEqual";

	internal const string CtxNotIn = "CtxNotIn";

	internal const string CtxNotLike = "CtxNotLike";

	internal const string CtxNullLiteral = "CtxNullLiteral";

	internal const string CtxOfType = "CtxOfType";

	internal const string CtxOfTypeOnly = "CtxOfTypeOnly";

	internal const string CtxOr = "CtxOr";

	internal const string CtxOrderByClause = "CtxOrderByClause";

	internal const string CtxOrderByClauseItem = "CtxOrderByClauseItem";

	internal const string CtxOverlaps = "CtxOverlaps";

	internal const string CtxParen = "CtxParen";

	internal const string CtxPlus = "CtxPlus";

	internal const string CtxTypeNameWithTypeSpec = "CtxTypeNameWithTypeSpec";

	internal const string CtxQueryExpression = "CtxQueryExpression";

	internal const string CtxQueryStatement = "CtxQueryStatement";

	internal const string CtxRef = "CtxRef";

	internal const string CtxRefTypeDefinition = "CtxRefTypeDefinition";

	internal const string CtxRelationship = "CtxRelationship";

	internal const string CtxRelationshipList = "CtxRelationshipList";

	internal const string CtxRowCtor = "CtxRowCtor";

	internal const string CtxRowTypeDefinition = "CtxRowTypeDefinition";

	internal const string CtxSelectRowClause = "CtxSelectRowClause";

	internal const string CtxSelectValueClause = "CtxSelectValueClause";

	internal const string CtxSet = "CtxSet";

	internal const string CtxSimpleIdentifier = "CtxSimpleIdentifier";

	internal const string CtxSkipSubClause = "CtxSkipSubClause";

	internal const string CtxTopSubClause = "CtxTopSubClause";

	internal const string CtxTreat = "CtxTreat";

	internal const string CtxTypeCtor = "CtxTypeCtor";

	internal const string CtxTypeName = "CtxTypeName";

	internal const string CtxUnaryMinus = "CtxUnaryMinus";

	internal const string CtxUnaryPlus = "CtxUnaryPlus";

	internal const string CtxUnion = "CtxUnion";

	internal const string CtxUnionAll = "CtxUnionAll";

	internal const string CtxWhereClause = "CtxWhereClause";

	internal const string CannotConvertNumericLiteral = "CannotConvertNumericLiteral";

	internal const string GenericSyntaxError = "GenericSyntaxError";

	internal const string InFromClause = "InFromClause";

	internal const string InGroupClause = "InGroupClause";

	internal const string InRowCtor = "InRowCtor";

	internal const string InSelectProjectionList = "InSelectProjectionList";

	internal const string InvalidAliasName = "InvalidAliasName";

	internal const string InvalidEmptyIdentifier = "InvalidEmptyIdentifier";

	internal const string InvalidEmptyQuery = "InvalidEmptyQuery";

	internal const string InvalidEscapedIdentifier = "InvalidEscapedIdentifier";

	internal const string InvalidEscapedIdentifierUnbalanced = "InvalidEscapedIdentifierUnbalanced";

	internal const string InvalidOperatorSymbol = "InvalidOperatorSymbol";

	internal const string InvalidPunctuatorSymbol = "InvalidPunctuatorSymbol";

	internal const string InvalidSimpleIdentifier = "InvalidSimpleIdentifier";

	internal const string InvalidSimpleIdentifierNonASCII = "InvalidSimpleIdentifierNonASCII";

	internal const string LocalizedCollection = "LocalizedCollection";

	internal const string LocalizedColumn = "LocalizedColumn";

	internal const string LocalizedComplex = "LocalizedComplex";

	internal const string LocalizedEntity = "LocalizedEntity";

	internal const string LocalizedEntityContainerExpression = "LocalizedEntityContainerExpression";

	internal const string LocalizedFunction = "LocalizedFunction";

	internal const string LocalizedInlineFunction = "LocalizedInlineFunction";

	internal const string LocalizedKeyword = "LocalizedKeyword";

	internal const string LocalizedLeft = "LocalizedLeft";

	internal const string LocalizedLine = "LocalizedLine";

	internal const string LocalizedMetadataMemberExpression = "LocalizedMetadataMemberExpression";

	internal const string LocalizedNamespace = "LocalizedNamespace";

	internal const string LocalizedNear = "LocalizedNear";

	internal const string LocalizedPrimitive = "LocalizedPrimitive";

	internal const string LocalizedReference = "LocalizedReference";

	internal const string LocalizedRight = "LocalizedRight";

	internal const string LocalizedRow = "LocalizedRow";

	internal const string LocalizedTerm = "LocalizedTerm";

	internal const string LocalizedType = "LocalizedType";

	internal const string LocalizedEnumMember = "LocalizedEnumMember";

	internal const string LocalizedValueExpression = "LocalizedValueExpression";

	internal const string AliasNameAlreadyUsed = "AliasNameAlreadyUsed";

	internal const string AmbiguousFunctionArguments = "AmbiguousFunctionArguments";

	internal const string AmbiguousMetadataMemberName = "AmbiguousMetadataMemberName";

	internal const string ArgumentTypesAreIncompatible = "ArgumentTypesAreIncompatible";

	internal const string BetweenLimitsCannotBeUntypedNulls = "BetweenLimitsCannotBeUntypedNulls";

	internal const string BetweenLimitsTypesAreNotCompatible = "BetweenLimitsTypesAreNotCompatible";

	internal const string BetweenLimitsTypesAreNotOrderComparable = "BetweenLimitsTypesAreNotOrderComparable";

	internal const string BetweenValueIsNotOrderComparable = "BetweenValueIsNotOrderComparable";

	internal const string CannotCreateEmptyMultiset = "CannotCreateEmptyMultiset";

	internal const string CannotCreateMultisetofNulls = "CannotCreateMultisetofNulls";

	internal const string CannotInstantiateAbstractType = "CannotInstantiateAbstractType";

	internal const string CannotResolveNameToTypeOrFunction = "CannotResolveNameToTypeOrFunction";

	internal const string ConcatBuiltinNotSupported = "ConcatBuiltinNotSupported";

	internal const string CouldNotResolveIdentifier = "CouldNotResolveIdentifier";

	internal const string CreateRefTypeIdentifierMustBeASubOrSuperType = "CreateRefTypeIdentifierMustBeASubOrSuperType";

	internal const string CreateRefTypeIdentifierMustSpecifyAnEntityType = "CreateRefTypeIdentifierMustSpecifyAnEntityType";

	internal const string DeRefArgIsNotOfRefType = "DeRefArgIsNotOfRefType";

	internal const string DuplicatedInlineFunctionOverload = "DuplicatedInlineFunctionOverload";

	internal const string ElementOperatorIsNotSupported = "ElementOperatorIsNotSupported";

	internal const string MemberDoesNotBelongToEntityContainer = "MemberDoesNotBelongToEntityContainer";

	internal const string ExpressionCannotBeNull = "ExpressionCannotBeNull";

	internal const string OfTypeExpressionElementTypeMustBeEntityType = "OfTypeExpressionElementTypeMustBeEntityType";

	internal const string OfTypeExpressionElementTypeMustBeNominalType = "OfTypeExpressionElementTypeMustBeNominalType";

	internal const string ExpressionMustBeCollection = "ExpressionMustBeCollection";

	internal const string ExpressionMustBeNumericType = "ExpressionMustBeNumericType";

	internal const string ExpressionTypeMustBeBoolean = "ExpressionTypeMustBeBoolean";

	internal const string ExpressionTypeMustBeEqualComparable = "ExpressionTypeMustBeEqualComparable";

	internal const string ExpressionTypeMustBeEntityType = "ExpressionTypeMustBeEntityType";

	internal const string ExpressionTypeMustBeNominalType = "ExpressionTypeMustBeNominalType";

	internal const string ExpressionTypeMustNotBeCollection = "ExpressionTypeMustNotBeCollection";

	internal const string ExprIsNotValidEntitySetForCreateRef = "ExprIsNotValidEntitySetForCreateRef";

	internal const string FailedToResolveAggregateFunction = "FailedToResolveAggregateFunction";

	internal const string GeneralExceptionAsQueryInnerException = "GeneralExceptionAsQueryInnerException";

	internal const string GroupingKeysMustBeEqualComparable = "GroupingKeysMustBeEqualComparable";

	internal const string GroupPartitionOutOfContext = "GroupPartitionOutOfContext";

	internal const string HavingRequiresGroupClause = "HavingRequiresGroupClause";

	internal const string ImcompatibleCreateRefKeyElementType = "ImcompatibleCreateRefKeyElementType";

	internal const string ImcompatibleCreateRefKeyType = "ImcompatibleCreateRefKeyType";

	internal const string InnerJoinMustHaveOnPredicate = "InnerJoinMustHaveOnPredicate";

	internal const string InvalidAssociationTypeForUnion = "InvalidAssociationTypeForUnion";

	internal const string InvalidCaseResultTypes = "InvalidCaseResultTypes";

	internal const string InvalidCaseWhenThenNullType = "InvalidCaseWhenThenNullType";

	internal const string InvalidCast = "InvalidCast";

	internal const string InvalidCastExpressionType = "InvalidCastExpressionType";

	internal const string InvalidCastType = "InvalidCastType";

	internal const string InvalidComplexType = "InvalidComplexType";

	internal const string InvalidCreateRefKeyType = "InvalidCreateRefKeyType";

	internal const string InvalidCtorArgumentType = "InvalidCtorArgumentType";

	internal const string InvalidCtorUseOnType = "InvalidCtorUseOnType";

	internal const string InvalidDateTimeOffsetLiteral = "InvalidDateTimeOffsetLiteral";

	internal const string InvalidDay = "InvalidDay";

	internal const string InvalidDayInMonth = "InvalidDayInMonth";

	internal const string InvalidDeRefProperty = "InvalidDeRefProperty";

	internal const string InvalidDistinctArgumentInCtor = "InvalidDistinctArgumentInCtor";

	internal const string InvalidDistinctArgumentInNonAggFunction = "InvalidDistinctArgumentInNonAggFunction";

	internal const string InvalidEntityRootTypeArgument = "InvalidEntityRootTypeArgument";

	internal const string InvalidEntityTypeArgument = "InvalidEntityTypeArgument";

	internal const string InvalidExpressionResolutionClass = "InvalidExpressionResolutionClass";

	internal const string InvalidFlattenArgument = "InvalidFlattenArgument";

	internal const string InvalidGroupIdentifierReference = "InvalidGroupIdentifierReference";

	internal const string InvalidHour = "InvalidHour";

	internal const string InvalidImplicitRelationshipFromEnd = "InvalidImplicitRelationshipFromEnd";

	internal const string InvalidImplicitRelationshipToEnd = "InvalidImplicitRelationshipToEnd";

	internal const string InvalidInExprArgs = "InvalidInExprArgs";

	internal const string InvalidJoinLeftCorrelation = "InvalidJoinLeftCorrelation";

	internal const string InvalidKeyArgument = "InvalidKeyArgument";

	internal const string InvalidKeyTypeForCollation = "InvalidKeyTypeForCollation";

	internal const string InvalidLiteralFormat = "InvalidLiteralFormat";

	internal const string InvalidMetadataMemberName = "InvalidMetadataMemberName";

	internal const string InvalidMinute = "InvalidMinute";

	internal const string InvalidModeForWithRelationshipClause = "InvalidModeForWithRelationshipClause";

	internal const string InvalidMonth = "InvalidMonth";

	internal const string InvalidNamespaceAlias = "InvalidNamespaceAlias";

	internal const string InvalidNullArithmetic = "InvalidNullArithmetic";

	internal const string InvalidNullComparison = "InvalidNullComparison";

	internal const string InvalidNullLiteralForNonNullableMember = "InvalidNullLiteralForNonNullableMember";

	internal const string InvalidParameterFormat = "InvalidParameterFormat";

	internal const string InvalidPlaceholderRootTypeArgument = "InvalidPlaceholderRootTypeArgument";

	internal const string InvalidPlaceholderTypeArgument = "InvalidPlaceholderTypeArgument";

	internal const string InvalidPredicateForCrossJoin = "InvalidPredicateForCrossJoin";

	internal const string InvalidRelationshipMember = "InvalidRelationshipMember";

	internal const string InvalidMetadataMemberClassResolution = "InvalidMetadataMemberClassResolution";

	internal const string InvalidRootComplexType = "InvalidRootComplexType";

	internal const string InvalidRootRowType = "InvalidRootRowType";

	internal const string InvalidRowType = "InvalidRowType";

	internal const string InvalidSecond = "InvalidSecond";

	internal const string InvalidSelectValueAliasedExpression = "InvalidSelectValueAliasedExpression";

	internal const string InvalidSelectValueList = "InvalidSelectValueList";

	internal const string InvalidTypeForWithRelationshipClause = "InvalidTypeForWithRelationshipClause";

	internal const string InvalidUnarySetOpArgument = "InvalidUnarySetOpArgument";

	internal const string InvalidUnsignedTypeForUnaryMinusOperation = "InvalidUnsignedTypeForUnaryMinusOperation";

	internal const string InvalidYear = "InvalidYear";

	internal const string InvalidWithRelationshipTargetEndMultiplicity = "InvalidWithRelationshipTargetEndMultiplicity";

	internal const string InvalidQueryResultType = "InvalidQueryResultType";

	internal const string IsNullInvalidType = "IsNullInvalidType";

	internal const string KeyMustBeCorrelated = "KeyMustBeCorrelated";

	internal const string LeftSetExpressionArgsMustBeCollection = "LeftSetExpressionArgsMustBeCollection";

	internal const string LikeArgMustBeStringType = "LikeArgMustBeStringType";

	internal const string LiteralTypeNotFoundInMetadata = "LiteralTypeNotFoundInMetadata";

	internal const string MalformedSingleQuotePayload = "MalformedSingleQuotePayload";

	internal const string MalformedStringLiteralPayload = "MalformedStringLiteralPayload";

	internal const string MethodInvocationNotSupported = "MethodInvocationNotSupported";

	internal const string MultipleDefinitionsOfParameter = "MultipleDefinitionsOfParameter";

	internal const string MultipleDefinitionsOfVariable = "MultipleDefinitionsOfVariable";

	internal const string MultisetElemsAreNotTypeCompatible = "MultisetElemsAreNotTypeCompatible";

	internal const string NamespaceAliasAlreadyUsed = "NamespaceAliasAlreadyUsed";

	internal const string NamespaceAlreadyImported = "NamespaceAlreadyImported";

	internal const string NestedAggregateCannotBeUsedInAggregate = "NestedAggregateCannotBeUsedInAggregate";

	internal const string NoAggrFunctionOverloadMatch = "NoAggrFunctionOverloadMatch";

	internal const string NoCanonicalAggrFunctionOverloadMatch = "NoCanonicalAggrFunctionOverloadMatch";

	internal const string NoCanonicalFunctionOverloadMatch = "NoCanonicalFunctionOverloadMatch";

	internal const string NoFunctionOverloadMatch = "NoFunctionOverloadMatch";

	internal const string NotAMemberOfCollection = "NotAMemberOfCollection";

	internal const string NotAMemberOfType = "NotAMemberOfType";

	internal const string NotASuperOrSubType = "NotASuperOrSubType";

	internal const string NullLiteralCannotBePromotedToCollectionOfNulls = "NullLiteralCannotBePromotedToCollectionOfNulls";

	internal const string NumberOfTypeCtorIsLessThenFormalSpec = "NumberOfTypeCtorIsLessThenFormalSpec";

	internal const string NumberOfTypeCtorIsMoreThenFormalSpec = "NumberOfTypeCtorIsMoreThenFormalSpec";

	internal const string OrderByKeyIsNotOrderComparable = "OrderByKeyIsNotOrderComparable";

	internal const string OfTypeOnlyTypeArgumentCannotBeAbstract = "OfTypeOnlyTypeArgumentCannotBeAbstract";

	internal const string ParameterTypeNotSupported = "ParameterTypeNotSupported";

	internal const string ParameterWasNotDefined = "ParameterWasNotDefined";

	internal const string PlaceholderExpressionMustBeCompatibleWithEdm64 = "PlaceholderExpressionMustBeCompatibleWithEdm64";

	internal const string PlaceholderExpressionMustBeConstant = "PlaceholderExpressionMustBeConstant";

	internal const string PlaceholderExpressionMustBeGreaterThanOrEqualToZero = "PlaceholderExpressionMustBeGreaterThanOrEqualToZero";

	internal const string PlaceholderSetArgTypeIsNotEqualComparable = "PlaceholderSetArgTypeIsNotEqualComparable";

	internal const string PlusLeftExpressionInvalidType = "PlusLeftExpressionInvalidType";

	internal const string PlusRightExpressionInvalidType = "PlusRightExpressionInvalidType";

	internal const string PrecisionMustBeGreaterThanScale = "PrecisionMustBeGreaterThanScale";

	internal const string RefArgIsNotOfEntityType = "RefArgIsNotOfEntityType";

	internal const string RefTypeIdentifierMustSpecifyAnEntityType = "RefTypeIdentifierMustSpecifyAnEntityType";

	internal const string RelatedEndExprTypeMustBeReference = "RelatedEndExprTypeMustBeReference";

	internal const string RelatedEndExprTypeMustBePromotoableToToEnd = "RelatedEndExprTypeMustBePromotoableToToEnd";

	internal const string RelationshipFromEndIsAmbiguos = "RelationshipFromEndIsAmbiguos";

	internal const string RelationshipTypeExpected = "RelationshipTypeExpected";

	internal const string RelationshipToEndIsAmbiguos = "RelationshipToEndIsAmbiguos";

	internal const string RelationshipTargetMustBeUnique = "RelationshipTargetMustBeUnique";

	internal const string ResultingExpressionTypeCannotBeNull = "ResultingExpressionTypeCannotBeNull";

	internal const string RightSetExpressionArgsMustBeCollection = "RightSetExpressionArgsMustBeCollection";

	internal const string RowCtorElementCannotBeNull = "RowCtorElementCannotBeNull";

	internal const string SelectDistinctMustBeEqualComparable = "SelectDistinctMustBeEqualComparable";

	internal const string SourceTypeMustBePromotoableToFromEndRelationType = "SourceTypeMustBePromotoableToFromEndRelationType";

	internal const string TopAndLimitCannotCoexist = "TopAndLimitCannotCoexist";

	internal const string TopAndSkipCannotCoexist = "TopAndSkipCannotCoexist";

	internal const string TypeDoesNotSupportSpec = "TypeDoesNotSupportSpec";

	internal const string TypeDoesNotSupportFacet = "TypeDoesNotSupportFacet";

	internal const string TypeArgumentCountMismatch = "TypeArgumentCountMismatch";

	internal const string TypeArgumentMustBeLiteral = "TypeArgumentMustBeLiteral";

	internal const string TypeArgumentBelowMin = "TypeArgumentBelowMin";

	internal const string TypeArgumentExceedsMax = "TypeArgumentExceedsMax";

	internal const string TypeArgumentIsNotValid = "TypeArgumentIsNotValid";

	internal const string TypeKindMismatch = "TypeKindMismatch";

	internal const string TypeMustBeInheritableType = "TypeMustBeInheritableType";

	internal const string TypeMustBeEntityType = "TypeMustBeEntityType";

	internal const string TypeMustBeNominalType = "TypeMustBeNominalType";

	internal const string TypeNameNotFound = "TypeNameNotFound";

	internal const string GroupVarNotFoundInScope = "GroupVarNotFoundInScope";

	internal const string InvalidArgumentTypeForAggregateFunction = "InvalidArgumentTypeForAggregateFunction";

	internal const string InvalidSavePoint = "InvalidSavePoint";

	internal const string InvalidScopeIndex = "InvalidScopeIndex";

	internal const string LiteralTypeNotSupported = "LiteralTypeNotSupported";

	internal const string ParserFatalError = "ParserFatalError";

	internal const string ParserInputError = "ParserInputError";

	internal const string StackOverflowInParser = "StackOverflowInParser";

	internal const string UnknownAstCommandExpression = "UnknownAstCommandExpression";

	internal const string UnknownAstExpressionType = "UnknownAstExpressionType";

	internal const string UnknownBuiltInAstExpressionType = "UnknownBuiltInAstExpressionType";

	internal const string UnknownExpressionResolutionClass = "UnknownExpressionResolutionClass";

	internal const string Cqt_General_UnsupportedExpression = "Cqt_General_UnsupportedExpression";

	internal const string Cqt_General_PolymorphicTypeRequired = "Cqt_General_PolymorphicTypeRequired";

	internal const string Cqt_General_PolymorphicArgRequired = "Cqt_General_PolymorphicArgRequired";

	internal const string Cqt_General_MetadataNotReadOnly = "Cqt_General_MetadataNotReadOnly";

	internal const string Cqt_General_NoProviderBooleanType = "Cqt_General_NoProviderBooleanType";

	internal const string Cqt_General_NoProviderIntegerType = "Cqt_General_NoProviderIntegerType";

	internal const string Cqt_General_NoProviderStringType = "Cqt_General_NoProviderStringType";

	internal const string Cqt_Metadata_EdmMemberIncorrectSpace = "Cqt_Metadata_EdmMemberIncorrectSpace";

	internal const string Cqt_Metadata_EntitySetEntityContainerNull = "Cqt_Metadata_EntitySetEntityContainerNull";

	internal const string Cqt_Metadata_EntitySetIncorrectSpace = "Cqt_Metadata_EntitySetIncorrectSpace";

	internal const string Cqt_Metadata_EntityTypeNullKeyMembersInvalid = "Cqt_Metadata_EntityTypeNullKeyMembersInvalid";

	internal const string Cqt_Metadata_EntityTypeEmptyKeyMembersInvalid = "Cqt_Metadata_EntityTypeEmptyKeyMembersInvalid";

	internal const string Cqt_Metadata_FunctionReturnParameterNull = "Cqt_Metadata_FunctionReturnParameterNull";

	internal const string Cqt_Metadata_FunctionIncorrectSpace = "Cqt_Metadata_FunctionIncorrectSpace";

	internal const string Cqt_Metadata_FunctionParameterIncorrectSpace = "Cqt_Metadata_FunctionParameterIncorrectSpace";

	internal const string Cqt_Metadata_TypeUsageIncorrectSpace = "Cqt_Metadata_TypeUsageIncorrectSpace";

	internal const string Cqt_Exceptions_InvalidCommandTree = "Cqt_Exceptions_InvalidCommandTree";

	internal const string Cqt_Util_CheckListEmptyInvalid = "Cqt_Util_CheckListEmptyInvalid";

	internal const string Cqt_Util_CheckListDuplicateName = "Cqt_Util_CheckListDuplicateName";

	internal const string Cqt_ExpressionLink_TypeMismatch = "Cqt_ExpressionLink_TypeMismatch";

	internal const string Cqt_ExpressionList_IncorrectElementCount = "Cqt_ExpressionList_IncorrectElementCount";

	internal const string Cqt_Copier_EntityContainerNotFound = "Cqt_Copier_EntityContainerNotFound";

	internal const string Cqt_Copier_EntitySetNotFound = "Cqt_Copier_EntitySetNotFound";

	internal const string Cqt_Copier_FunctionNotFound = "Cqt_Copier_FunctionNotFound";

	internal const string Cqt_Copier_PropertyNotFound = "Cqt_Copier_PropertyNotFound";

	internal const string Cqt_Copier_NavPropertyNotFound = "Cqt_Copier_NavPropertyNotFound";

	internal const string Cqt_Copier_EndNotFound = "Cqt_Copier_EndNotFound";

	internal const string Cqt_Copier_TypeNotFound = "Cqt_Copier_TypeNotFound";

	internal const string Cqt_CommandTree_InvalidDataSpace = "Cqt_CommandTree_InvalidDataSpace";

	internal const string Cqt_CommandTree_InvalidParameterName = "Cqt_CommandTree_InvalidParameterName";

	internal const string Cqt_Validator_InvalidIncompatibleParameterReferences = "Cqt_Validator_InvalidIncompatibleParameterReferences";

	internal const string Cqt_Validator_InvalidOtherWorkspaceMetadata = "Cqt_Validator_InvalidOtherWorkspaceMetadata";

	internal const string Cqt_Validator_InvalidIncorrectDataSpaceMetadata = "Cqt_Validator_InvalidIncorrectDataSpaceMetadata";

	internal const string Cqt_Factory_NewCollectionInvalidCommonType = "Cqt_Factory_NewCollectionInvalidCommonType";

	internal const string NoSuchProperty = "NoSuchProperty";

	internal const string Cqt_Factory_NoSuchRelationEnd = "Cqt_Factory_NoSuchRelationEnd";

	internal const string Cqt_Factory_IncompatibleRelationEnds = "Cqt_Factory_IncompatibleRelationEnds";

	internal const string Cqt_Factory_MethodResultTypeNotSupported = "Cqt_Factory_MethodResultTypeNotSupported";

	internal const string Cqt_Aggregate_InvalidFunction = "Cqt_Aggregate_InvalidFunction";

	internal const string Cqt_Binding_CollectionRequired = "Cqt_Binding_CollectionRequired";

	internal const string Cqt_GroupBinding_CollectionRequired = "Cqt_GroupBinding_CollectionRequired";

	internal const string Cqt_Binary_CollectionsRequired = "Cqt_Binary_CollectionsRequired";

	internal const string Cqt_Unary_CollectionRequired = "Cqt_Unary_CollectionRequired";

	internal const string Cqt_And_BooleanArgumentsRequired = "Cqt_And_BooleanArgumentsRequired";

	internal const string Cqt_Apply_DuplicateVariableNames = "Cqt_Apply_DuplicateVariableNames";

	internal const string Cqt_Arithmetic_NumericCommonType = "Cqt_Arithmetic_NumericCommonType";

	internal const string Cqt_Arithmetic_InvalidUnsignedTypeForUnaryMinus = "Cqt_Arithmetic_InvalidUnsignedTypeForUnaryMinus";

	internal const string Cqt_Case_WhensMustEqualThens = "Cqt_Case_WhensMustEqualThens";

	internal const string Cqt_Case_InvalidResultType = "Cqt_Case_InvalidResultType";

	internal const string Cqt_Cast_InvalidCast = "Cqt_Cast_InvalidCast";

	internal const string Cqt_Comparison_ComparableRequired = "Cqt_Comparison_ComparableRequired";

	internal const string Cqt_Constant_InvalidType = "Cqt_Constant_InvalidType";

	internal const string Cqt_Constant_InvalidValueForType = "Cqt_Constant_InvalidValueForType";

	internal const string Cqt_Constant_InvalidConstantType = "Cqt_Constant_InvalidConstantType";

	internal const string Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType = "Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType";

	internal const string Cqt_Distinct_InvalidCollection = "Cqt_Distinct_InvalidCollection";

	internal const string Cqt_DeRef_RefRequired = "Cqt_DeRef_RefRequired";

	internal const string Cqt_Element_InvalidArgumentForUnwrapSingleProperty = "Cqt_Element_InvalidArgumentForUnwrapSingleProperty";

	internal const string Cqt_Function_VoidResultInvalid = "Cqt_Function_VoidResultInvalid";

	internal const string Cqt_Function_NonComposableInExpression = "Cqt_Function_NonComposableInExpression";

	internal const string Cqt_Function_CommandTextInExpression = "Cqt_Function_CommandTextInExpression";

	internal const string Cqt_Function_CanonicalFunction_NotFound = "Cqt_Function_CanonicalFunction_NotFound";

	internal const string Cqt_Function_CanonicalFunction_AmbiguousMatch = "Cqt_Function_CanonicalFunction_AmbiguousMatch";

	internal const string Cqt_GetEntityRef_EntityRequired = "Cqt_GetEntityRef_EntityRequired";

	internal const string Cqt_GetRefKey_RefRequired = "Cqt_GetRefKey_RefRequired";

	internal const string Cqt_GroupBy_AtLeastOneKeyOrAggregate = "Cqt_GroupBy_AtLeastOneKeyOrAggregate";

	internal const string Cqt_GroupBy_KeyNotEqualityComparable = "Cqt_GroupBy_KeyNotEqualityComparable";

	internal const string Cqt_GroupBy_AggregateColumnExistsAsGroupColumn = "Cqt_GroupBy_AggregateColumnExistsAsGroupColumn";

	internal const string Cqt_GroupBy_MoreThanOneGroupAggregate = "Cqt_GroupBy_MoreThanOneGroupAggregate";

	internal const string Cqt_CrossJoin_AtLeastTwoInputs = "Cqt_CrossJoin_AtLeastTwoInputs";

	internal const string Cqt_CrossJoin_DuplicateVariableNames = "Cqt_CrossJoin_DuplicateVariableNames";

	internal const string Cqt_IsNull_CollectionNotAllowed = "Cqt_IsNull_CollectionNotAllowed";

	internal const string Cqt_IsNull_InvalidType = "Cqt_IsNull_InvalidType";

	internal const string Cqt_InvalidTypeForSetOperation = "Cqt_InvalidTypeForSetOperation";

	internal const string Cqt_Join_DuplicateVariableNames = "Cqt_Join_DuplicateVariableNames";

	internal const string Cqt_Limit_ConstantOrParameterRefRequired = "Cqt_Limit_ConstantOrParameterRefRequired";

	internal const string Cqt_Limit_IntegerRequired = "Cqt_Limit_IntegerRequired";

	internal const string Cqt_Limit_NonNegativeLimitRequired = "Cqt_Limit_NonNegativeLimitRequired";

	internal const string Cqt_NewInstance_CollectionTypeRequired = "Cqt_NewInstance_CollectionTypeRequired";

	internal const string Cqt_NewInstance_StructuralTypeRequired = "Cqt_NewInstance_StructuralTypeRequired";

	internal const string Cqt_NewInstance_CannotInstantiateMemberlessType = "Cqt_NewInstance_CannotInstantiateMemberlessType";

	internal const string Cqt_NewInstance_CannotInstantiateAbstractType = "Cqt_NewInstance_CannotInstantiateAbstractType";

	internal const string Cqt_NewInstance_IncompatibleRelatedEntity_SourceTypeNotValid = "Cqt_NewInstance_IncompatibleRelatedEntity_SourceTypeNotValid";

	internal const string Cqt_Not_BooleanArgumentRequired = "Cqt_Not_BooleanArgumentRequired";

	internal const string Cqt_Or_BooleanArgumentsRequired = "Cqt_Or_BooleanArgumentsRequired";

	internal const string Cqt_In_SameResultTypeRequired = "Cqt_In_SameResultTypeRequired";

	internal const string Cqt_Property_InstanceRequiredForInstance = "Cqt_Property_InstanceRequiredForInstance";

	internal const string Cqt_Ref_PolymorphicArgRequired = "Cqt_Ref_PolymorphicArgRequired";

	internal const string Cqt_RelatedEntityRef_TargetEndFromDifferentRelationship = "Cqt_RelatedEntityRef_TargetEndFromDifferentRelationship";

	internal const string Cqt_RelatedEntityRef_TargetEndMustBeAtMostOne = "Cqt_RelatedEntityRef_TargetEndMustBeAtMostOne";

	internal const string Cqt_RelatedEntityRef_TargetEndSameAsSourceEnd = "Cqt_RelatedEntityRef_TargetEndSameAsSourceEnd";

	internal const string Cqt_RelatedEntityRef_TargetEntityNotRef = "Cqt_RelatedEntityRef_TargetEntityNotRef";

	internal const string Cqt_RelatedEntityRef_TargetEntityNotCompatible = "Cqt_RelatedEntityRef_TargetEntityNotCompatible";

	internal const string Cqt_RelNav_NoCompositions = "Cqt_RelNav_NoCompositions";

	internal const string Cqt_RelNav_WrongSourceType = "Cqt_RelNav_WrongSourceType";

	internal const string Cqt_Skip_ConstantOrParameterRefRequired = "Cqt_Skip_ConstantOrParameterRefRequired";

	internal const string Cqt_Skip_IntegerRequired = "Cqt_Skip_IntegerRequired";

	internal const string Cqt_Skip_NonNegativeCountRequired = "Cqt_Skip_NonNegativeCountRequired";

	internal const string Cqt_Sort_NonStringCollationInvalid = "Cqt_Sort_NonStringCollationInvalid";

	internal const string Cqt_Sort_OrderComparable = "Cqt_Sort_OrderComparable";

	internal const string Cqt_UDF_FunctionDefinitionGenerationFailed = "Cqt_UDF_FunctionDefinitionGenerationFailed";

	internal const string Cqt_UDF_FunctionDefinitionWithCircularReference = "Cqt_UDF_FunctionDefinitionWithCircularReference";

	internal const string Cqt_UDF_FunctionDefinitionResultTypeMismatch = "Cqt_UDF_FunctionDefinitionResultTypeMismatch";

	internal const string Cqt_UDF_FunctionHasNoDefinition = "Cqt_UDF_FunctionHasNoDefinition";

	internal const string Cqt_Validator_VarRefInvalid = "Cqt_Validator_VarRefInvalid";

	internal const string Cqt_Validator_VarRefTypeMismatch = "Cqt_Validator_VarRefTypeMismatch";

	internal const string Iqt_General_UnsupportedOp = "Iqt_General_UnsupportedOp";

	internal const string Iqt_CTGen_UnexpectedAggregate = "Iqt_CTGen_UnexpectedAggregate";

	internal const string Iqt_CTGen_UnexpectedVarDefList = "Iqt_CTGen_UnexpectedVarDefList";

	internal const string Iqt_CTGen_UnexpectedVarDef = "Iqt_CTGen_UnexpectedVarDef";

	internal const string ADP_MustUseSequentialAccess = "ADP_MustUseSequentialAccess";

	internal const string ADP_ProviderDoesNotSupportCommandTrees = "ADP_ProviderDoesNotSupportCommandTrees";

	internal const string ADP_ClosedDataReaderError = "ADP_ClosedDataReaderError";

	internal const string ADP_DataReaderClosed = "ADP_DataReaderClosed";

	internal const string ADP_ImplicitlyClosedDataReaderError = "ADP_ImplicitlyClosedDataReaderError";

	internal const string ADP_NoData = "ADP_NoData";

	internal const string ADP_GetSchemaTableIsNotSupported = "ADP_GetSchemaTableIsNotSupported";

	internal const string ADP_InvalidDataReaderFieldCountForScalarType = "ADP_InvalidDataReaderFieldCountForScalarType";

	internal const string ADP_InvalidDataReaderMissingColumnForType = "ADP_InvalidDataReaderMissingColumnForType";

	internal const string ADP_InvalidDataReaderMissingDiscriminatorColumn = "ADP_InvalidDataReaderMissingDiscriminatorColumn";

	internal const string ADP_InvalidDataReaderUnableToDetermineType = "ADP_InvalidDataReaderUnableToDetermineType";

	internal const string ADP_InvalidDataReaderUnableToMaterializeNonScalarType = "ADP_InvalidDataReaderUnableToMaterializeNonScalarType";

	internal const string ADP_KeysRequiredForJoinOverNest = "ADP_KeysRequiredForJoinOverNest";

	internal const string ADP_KeysRequiredForNesting = "ADP_KeysRequiredForNesting";

	internal const string ADP_NestingNotSupported = "ADP_NestingNotSupported";

	internal const string ADP_NoQueryMappingView = "ADP_NoQueryMappingView";

	internal const string ADP_InternalProviderError = "ADP_InternalProviderError";

	internal const string ADP_InvalidEnumerationValue = "ADP_InvalidEnumerationValue";

	internal const string ADP_InvalidBufferSizeOrIndex = "ADP_InvalidBufferSizeOrIndex";

	internal const string ADP_InvalidDataLength = "ADP_InvalidDataLength";

	internal const string ADP_InvalidDataType = "ADP_InvalidDataType";

	internal const string ADP_InvalidDestinationBufferIndex = "ADP_InvalidDestinationBufferIndex";

	internal const string ADP_InvalidSourceBufferIndex = "ADP_InvalidSourceBufferIndex";

	internal const string ADP_NonSequentialChunkAccess = "ADP_NonSequentialChunkAccess";

	internal const string ADP_NonSequentialColumnAccess = "ADP_NonSequentialColumnAccess";

	internal const string ADP_UnknownDataTypeCode = "ADP_UnknownDataTypeCode";

	internal const string DataCategory_Data = "DataCategory_Data";

	internal const string DbParameter_Direction = "DbParameter_Direction";

	internal const string DbParameter_Size = "DbParameter_Size";

	internal const string DataCategory_Update = "DataCategory_Update";

	internal const string DbParameter_SourceColumn = "DbParameter_SourceColumn";

	internal const string DbParameter_SourceVersion = "DbParameter_SourceVersion";

	internal const string ADP_CollectionParameterElementIsNull = "ADP_CollectionParameterElementIsNull";

	internal const string ADP_CollectionParameterElementIsNullOrEmpty = "ADP_CollectionParameterElementIsNullOrEmpty";

	internal const string NonReturnParameterInReturnParameterCollection = "NonReturnParameterInReturnParameterCollection";

	internal const string ReturnParameterInInputParameterCollection = "ReturnParameterInInputParameterCollection";

	internal const string NullEntitySetsForFunctionReturningMultipleResultSets = "NullEntitySetsForFunctionReturningMultipleResultSets";

	internal const string NumberOfEntitySetsDoesNotMatchNumberOfReturnParameters = "NumberOfEntitySetsDoesNotMatchNumberOfReturnParameters";

	internal const string EntityParameterCollectionInvalidParameterName = "EntityParameterCollectionInvalidParameterName";

	internal const string EntityParameterCollectionInvalidIndex = "EntityParameterCollectionInvalidIndex";

	internal const string InvalidEntityParameterType = "InvalidEntityParameterType";

	internal const string EntityParameterContainedByAnotherCollection = "EntityParameterContainedByAnotherCollection";

	internal const string EntityParameterCollectionRemoveInvalidObject = "EntityParameterCollectionRemoveInvalidObject";

	internal const string ADP_ConnectionStringSyntax = "ADP_ConnectionStringSyntax";

	internal const string ExpandingDataDirectoryFailed = "ExpandingDataDirectoryFailed";

	internal const string ADP_InvalidDataDirectory = "ADP_InvalidDataDirectory";

	internal const string ADP_InvalidMultipartNameDelimiterUsage = "ADP_InvalidMultipartNameDelimiterUsage";

	internal const string ADP_InvalidSizeValue = "ADP_InvalidSizeValue";

	internal const string ADP_KeywordNotSupported = "ADP_KeywordNotSupported";

	internal const string ConstantFacetSpecifiedInSchema = "ConstantFacetSpecifiedInSchema";

	internal const string DuplicateAnnotation = "DuplicateAnnotation";

	internal const string EmptyFile = "EmptyFile";

	internal const string EmptySchemaTextReader = "EmptySchemaTextReader";

	internal const string EmptyName = "EmptyName";

	internal const string InvalidName = "InvalidName";

	internal const string MissingName = "MissingName";

	internal const string UnexpectedXmlAttribute = "UnexpectedXmlAttribute";

	internal const string UnexpectedXmlElement = "UnexpectedXmlElement";

	internal const string TextNotAllowed = "TextNotAllowed";

	internal const string UnexpectedXmlNodeType = "UnexpectedXmlNodeType";

	internal const string MalformedXml = "MalformedXml";

	internal const string ValueNotUnderstood = "ValueNotUnderstood";

	internal const string EntityContainerAlreadyExists = "EntityContainerAlreadyExists";

	internal const string TypeNameAlreadyDefinedDuplicate = "TypeNameAlreadyDefinedDuplicate";

	internal const string PropertyNameAlreadyDefinedDuplicate = "PropertyNameAlreadyDefinedDuplicate";

	internal const string DuplicateMemberNameInExtendedEntityContainer = "DuplicateMemberNameInExtendedEntityContainer";

	internal const string DuplicateEntityContainerMemberName = "DuplicateEntityContainerMemberName";

	internal const string PropertyTypeAlreadyDefined = "PropertyTypeAlreadyDefined";

	internal const string InvalidSize = "InvalidSize";

	internal const string InvalidSystemReferenceId = "InvalidSystemReferenceId";

	internal const string BadNamespaceOrAlias = "BadNamespaceOrAlias";

	internal const string MissingNamespaceAttribute = "MissingNamespaceAttribute";

	internal const string InvalidBaseTypeForStructuredType = "InvalidBaseTypeForStructuredType";

	internal const string InvalidPropertyType = "InvalidPropertyType";

	internal const string InvalidBaseTypeForItemType = "InvalidBaseTypeForItemType";

	internal const string InvalidBaseTypeForNestedType = "InvalidBaseTypeForNestedType";

	internal const string DefaultNotAllowed = "DefaultNotAllowed";

	internal const string FacetNotAllowed = "FacetNotAllowed";

	internal const string RequiredFacetMissing = "RequiredFacetMissing";

	internal const string InvalidDefaultBinaryWithNoMaxLength = "InvalidDefaultBinaryWithNoMaxLength";

	internal const string InvalidDefaultIntegral = "InvalidDefaultIntegral";

	internal const string InvalidDefaultDateTime = "InvalidDefaultDateTime";

	internal const string InvalidDefaultTime = "InvalidDefaultTime";

	internal const string InvalidDefaultDateTimeOffset = "InvalidDefaultDateTimeOffset";

	internal const string InvalidDefaultDecimal = "InvalidDefaultDecimal";

	internal const string InvalidDefaultFloatingPoint = "InvalidDefaultFloatingPoint";

	internal const string InvalidDefaultGuid = "InvalidDefaultGuid";

	internal const string InvalidDefaultBoolean = "InvalidDefaultBoolean";

	internal const string DuplicateMemberName = "DuplicateMemberName";

	internal const string GeneratorErrorSeverityError = "GeneratorErrorSeverityError";

	internal const string GeneratorErrorSeverityWarning = "GeneratorErrorSeverityWarning";

	internal const string GeneratorErrorSeverityUnknown = "GeneratorErrorSeverityUnknown";

	internal const string SourceUriUnknown = "SourceUriUnknown";

	internal const string BadPrecisionAndScale = "BadPrecisionAndScale";

	internal const string InvalidNamespaceInUsing = "InvalidNamespaceInUsing";

	internal const string BadNavigationPropertyRelationshipNotRelationship = "BadNavigationPropertyRelationshipNotRelationship";

	internal const string BadNavigationPropertyRolesCannotBeTheSame = "BadNavigationPropertyRolesCannotBeTheSame";

	internal const string BadNavigationPropertyUndefinedRole = "BadNavigationPropertyUndefinedRole";

	internal const string BadNavigationPropertyBadFromRoleType = "BadNavigationPropertyBadFromRoleType";

	internal const string InvalidMemberNameMatchesTypeName = "InvalidMemberNameMatchesTypeName";

	internal const string InvalidKeyKeyDefinedInBaseClass = "InvalidKeyKeyDefinedInBaseClass";

	internal const string InvalidKeyNullablePart = "InvalidKeyNullablePart";

	internal const string InvalidKeyNoProperty = "InvalidKeyNoProperty";

	internal const string KeyMissingOnEntityType = "KeyMissingOnEntityType";

	internal const string InvalidDocumentationBothTextAndStructure = "InvalidDocumentationBothTextAndStructure";

	internal const string ArgumentOutOfRangeExpectedPostiveNumber = "ArgumentOutOfRangeExpectedPostiveNumber";

	internal const string ArgumentOutOfRange = "ArgumentOutOfRange";

	internal const string UnacceptableUri = "UnacceptableUri";

	internal const string UnexpectedTypeInCollection = "UnexpectedTypeInCollection";

	internal const string AllElementsMustBeInSchema = "AllElementsMustBeInSchema";

	internal const string AliasNameIsAlreadyDefined = "AliasNameIsAlreadyDefined";

	internal const string NeedNotUseSystemNamespaceInUsing = "NeedNotUseSystemNamespaceInUsing";

	internal const string CannotUseSystemNamespaceAsAlias = "CannotUseSystemNamespaceAsAlias";

	internal const string EntitySetTypeHasNoKeys = "EntitySetTypeHasNoKeys";

	internal const string TableAndSchemaAreMutuallyExclusiveWithDefiningQuery = "TableAndSchemaAreMutuallyExclusiveWithDefiningQuery";

	internal const string UnexpectedRootElement = "UnexpectedRootElement";

	internal const string UnexpectedRootElementNoNamespace = "UnexpectedRootElementNoNamespace";

	internal const string ParameterNameAlreadyDefinedDuplicate = "ParameterNameAlreadyDefinedDuplicate";

	internal const string FunctionWithNonPrimitiveTypeNotSupported = "FunctionWithNonPrimitiveTypeNotSupported";

	internal const string FunctionWithNonEdmPrimitiveTypeNotSupported = "FunctionWithNonEdmPrimitiveTypeNotSupported";

	internal const string FunctionImportWithUnsupportedReturnTypeV1 = "FunctionImportWithUnsupportedReturnTypeV1";

	internal const string FunctionImportWithUnsupportedReturnTypeV1_1 = "FunctionImportWithUnsupportedReturnTypeV1_1";

	internal const string FunctionImportWithUnsupportedReturnTypeV2 = "FunctionImportWithUnsupportedReturnTypeV2";

	internal const string FunctionImportUnknownEntitySet = "FunctionImportUnknownEntitySet";

	internal const string FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet = "FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet";

	internal const string FunctionImportEntityTypeDoesNotMatchEntitySet = "FunctionImportEntityTypeDoesNotMatchEntitySet";

	internal const string FunctionImportSpecifiesEntitySetButNotEntityType = "FunctionImportSpecifiesEntitySetButNotEntityType";

	internal const string FunctionImportEntitySetAndEntitySetPathDeclared = "FunctionImportEntitySetAndEntitySetPathDeclared";

	internal const string FunctionImportComposableAndSideEffectingNotAllowed = "FunctionImportComposableAndSideEffectingNotAllowed";

	internal const string FunctionImportCollectionAndRefParametersNotAllowed = "FunctionImportCollectionAndRefParametersNotAllowed";

	internal const string FunctionImportNonNullableParametersNotAllowed = "FunctionImportNonNullableParametersNotAllowed";

	internal const string TVFReturnTypeRowHasNonScalarProperty = "TVFReturnTypeRowHasNonScalarProperty";

	internal const string DuplicateEntitySetTable = "DuplicateEntitySetTable";

	internal const string ConcurrencyRedefinedOnSubTypeOfEntitySetType = "ConcurrencyRedefinedOnSubTypeOfEntitySetType";

	internal const string SimilarRelationshipEnd = "SimilarRelationshipEnd";

	internal const string InvalidRelationshipEndMultiplicity = "InvalidRelationshipEndMultiplicity";

	internal const string EndNameAlreadyDefinedDuplicate = "EndNameAlreadyDefinedDuplicate";

	internal const string InvalidRelationshipEndType = "InvalidRelationshipEndType";

	internal const string BadParameterDirection = "BadParameterDirection";

	internal const string BadParameterDirectionForComposableFunctions = "BadParameterDirectionForComposableFunctions";

	internal const string InvalidOperationMultipleEndsInAssociation = "InvalidOperationMultipleEndsInAssociation";

	internal const string InvalidAction = "InvalidAction";

	internal const string DuplicationOperation = "DuplicationOperation";

	internal const string NotInNamespaceAlias = "NotInNamespaceAlias";

	internal const string NotNamespaceQualified = "NotNamespaceQualified";

	internal const string NotInNamespaceNoAlias = "NotInNamespaceNoAlias";

	internal const string InvalidValueForParameterTypeSemanticsAttribute = "InvalidValueForParameterTypeSemanticsAttribute";

	internal const string DuplicatePropertyNameSpecifiedInEntityKey = "DuplicatePropertyNameSpecifiedInEntityKey";

	internal const string InvalidEntitySetType = "InvalidEntitySetType";

	internal const string InvalidRelationshipSetType = "InvalidRelationshipSetType";

	internal const string InvalidEntityContainerNameInExtends = "InvalidEntityContainerNameInExtends";

	internal const string InvalidNamespaceOrAliasSpecified = "InvalidNamespaceOrAliasSpecified";

	internal const string PrecisionOutOfRange = "PrecisionOutOfRange";

	internal const string ScaleOutOfRange = "ScaleOutOfRange";

	internal const string InvalidEntitySetNameReference = "InvalidEntitySetNameReference";

	internal const string InvalidEntityEndName = "InvalidEntityEndName";

	internal const string DuplicateEndName = "DuplicateEndName";

	internal const string AmbiguousEntityContainerEnd = "AmbiguousEntityContainerEnd";

	internal const string MissingEntityContainerEnd = "MissingEntityContainerEnd";

	internal const string InvalidEndEntitySetTypeMismatch = "InvalidEndEntitySetTypeMismatch";

	internal const string InferRelationshipEndFailedNoEntitySetMatch = "InferRelationshipEndFailedNoEntitySetMatch";

	internal const string InferRelationshipEndAmbiguous = "InferRelationshipEndAmbiguous";

	internal const string InferRelationshipEndGivesAlreadyDefinedEnd = "InferRelationshipEndGivesAlreadyDefinedEnd";

	internal const string TooManyAssociationEnds = "TooManyAssociationEnds";

	internal const string InvalidEndRoleInRelationshipConstraint = "InvalidEndRoleInRelationshipConstraint";

	internal const string InvalidFromPropertyInRelationshipConstraint = "InvalidFromPropertyInRelationshipConstraint";

	internal const string InvalidToPropertyInRelationshipConstraint = "InvalidToPropertyInRelationshipConstraint";

	internal const string InvalidPropertyInRelationshipConstraint = "InvalidPropertyInRelationshipConstraint";

	internal const string TypeMismatchRelationshipConstraint = "TypeMismatchRelationshipConstraint";

	internal const string InvalidMultiplicityFromRoleUpperBoundMustBeOne = "InvalidMultiplicityFromRoleUpperBoundMustBeOne";

	internal const string InvalidMultiplicityFromRoleToPropertyNonNullableV1 = "InvalidMultiplicityFromRoleToPropertyNonNullableV1";

	internal const string InvalidMultiplicityFromRoleToPropertyNonNullableV2 = "InvalidMultiplicityFromRoleToPropertyNonNullableV2";

	internal const string InvalidMultiplicityFromRoleToPropertyNullableV1 = "InvalidMultiplicityFromRoleToPropertyNullableV1";

	internal const string InvalidMultiplicityToRoleLowerBoundMustBeZero = "InvalidMultiplicityToRoleLowerBoundMustBeZero";

	internal const string InvalidMultiplicityToRoleUpperBoundMustBeOne = "InvalidMultiplicityToRoleUpperBoundMustBeOne";

	internal const string InvalidMultiplicityToRoleUpperBoundMustBeMany = "InvalidMultiplicityToRoleUpperBoundMustBeMany";

	internal const string MismatchNumberOfPropertiesinRelationshipConstraint = "MismatchNumberOfPropertiesinRelationshipConstraint";

	internal const string MissingConstraintOnRelationshipType = "MissingConstraintOnRelationshipType";

	internal const string SameRoleReferredInReferentialConstraint = "SameRoleReferredInReferentialConstraint";

	internal const string InvalidPrimitiveTypeKind = "InvalidPrimitiveTypeKind";

	internal const string EntityKeyMustBeScalar = "EntityKeyMustBeScalar";

	internal const string EntityKeyTypeCurrentlyNotSupportedInSSDL = "EntityKeyTypeCurrentlyNotSupportedInSSDL";

	internal const string EntityKeyTypeCurrentlyNotSupported = "EntityKeyTypeCurrentlyNotSupported";

	internal const string MissingFacetDescription = "MissingFacetDescription";

	internal const string EndWithManyMultiplicityCannotHaveOperationsSpecified = "EndWithManyMultiplicityCannotHaveOperationsSpecified";

	internal const string EndWithoutMultiplicity = "EndWithoutMultiplicity";

	internal const string EntityContainerCannotExtendItself = "EntityContainerCannotExtendItself";

	internal const string ComposableFunctionOrFunctionImportMustDeclareReturnType = "ComposableFunctionOrFunctionImportMustDeclareReturnType";

	internal const string NonComposableFunctionCannotBeMappedAsComposable = "NonComposableFunctionCannotBeMappedAsComposable";

	internal const string ComposableFunctionImportsReturningEntitiesNotSupported = "ComposableFunctionImportsReturningEntitiesNotSupported";

	internal const string StructuralTypeMappingsMustNotBeNullForFunctionImportsReturningNonScalarValues = "StructuralTypeMappingsMustNotBeNullForFunctionImportsReturningNonScalarValues";

	internal const string InvalidReturnTypeForComposableFunction = "InvalidReturnTypeForComposableFunction";

	internal const string NonComposableFunctionMustNotDeclareReturnType = "NonComposableFunctionMustNotDeclareReturnType";

	internal const string CommandTextFunctionsNotComposable = "CommandTextFunctionsNotComposable";

	internal const string CommandTextFunctionsCannotDeclareStoreFunctionName = "CommandTextFunctionsCannotDeclareStoreFunctionName";

	internal const string NonComposableFunctionHasDisallowedAttribute = "NonComposableFunctionHasDisallowedAttribute";

	internal const string EmptyDefiningQuery = "EmptyDefiningQuery";

	internal const string EmptyCommandText = "EmptyCommandText";

	internal const string AmbiguousFunctionOverload = "AmbiguousFunctionOverload";

	internal const string AmbiguousFunctionAndType = "AmbiguousFunctionAndType";

	internal const string CycleInTypeHierarchy = "CycleInTypeHierarchy";

	internal const string IncorrectProviderManifest = "IncorrectProviderManifest";

	internal const string ComplexTypeAsReturnTypeAndDefinedEntitySet = "ComplexTypeAsReturnTypeAndDefinedEntitySet";

	internal const string ComplexTypeAsReturnTypeAndNestedComplexProperty = "ComplexTypeAsReturnTypeAndNestedComplexProperty";

	internal const string FacetsOnNonScalarType = "FacetsOnNonScalarType";

	internal const string FacetDeclarationRequiresTypeAttribute = "FacetDeclarationRequiresTypeAttribute";

	internal const string TypeMustBeDeclared = "TypeMustBeDeclared";

	internal const string RowTypeWithoutProperty = "RowTypeWithoutProperty";

	internal const string TypeDeclaredAsAttributeAndElement = "TypeDeclaredAsAttributeAndElement";

	internal const string ReferenceToNonEntityType = "ReferenceToNonEntityType";

	internal const string NoCodeGenNamespaceInStructuralAnnotation = "NoCodeGenNamespaceInStructuralAnnotation";

	internal const string CannotLoadDifferentVersionOfSchemaInTheSameItemCollection = "CannotLoadDifferentVersionOfSchemaInTheSameItemCollection";

	internal const string InvalidEnumUnderlyingType = "InvalidEnumUnderlyingType";

	internal const string DuplicateEnumMember = "DuplicateEnumMember";

	internal const string CalculatedEnumValueOutOfRange = "CalculatedEnumValueOutOfRange";

	internal const string EnumMemberValueOutOfItsUnderylingTypeRange = "EnumMemberValueOutOfItsUnderylingTypeRange";

	internal const string SpatialWithUseStrongSpatialTypesFalse = "SpatialWithUseStrongSpatialTypesFalse";

	internal const string ObjectQuery_QueryBuilder_InvalidResultType = "ObjectQuery_QueryBuilder_InvalidResultType";

	internal const string ObjectQuery_QueryBuilder_InvalidQueryArgument = "ObjectQuery_QueryBuilder_InvalidQueryArgument";

	internal const string ObjectQuery_QueryBuilder_NotSupportedLinqSource = "ObjectQuery_QueryBuilder_NotSupportedLinqSource";

	internal const string ObjectQuery_InvalidConnection = "ObjectQuery_InvalidConnection";

	internal const string ObjectQuery_InvalidQueryName = "ObjectQuery_InvalidQueryName";

	internal const string ObjectQuery_UnableToMapResultType = "ObjectQuery_UnableToMapResultType";

	internal const string ObjectQuery_UnableToMaterializeArray = "ObjectQuery_UnableToMaterializeArray";

	internal const string ObjectQuery_UnableToMaterializeArbitaryProjectionType = "ObjectQuery_UnableToMaterializeArbitaryProjectionType";

	internal const string ObjectParameter_InvalidParameterName = "ObjectParameter_InvalidParameterName";

	internal const string ObjectParameter_InvalidParameterType = "ObjectParameter_InvalidParameterType";

	internal const string ObjectParameterCollection_ParameterNameNotFound = "ObjectParameterCollection_ParameterNameNotFound";

	internal const string ObjectParameterCollection_ParameterAlreadyExists = "ObjectParameterCollection_ParameterAlreadyExists";

	internal const string ObjectParameterCollection_DuplicateParameterName = "ObjectParameterCollection_DuplicateParameterName";

	internal const string ObjectParameterCollection_ParametersLocked = "ObjectParameterCollection_ParametersLocked";

	internal const string ProviderReturnedNullForGetDbInformation = "ProviderReturnedNullForGetDbInformation";

	internal const string ProviderReturnedNullForCreateCommandDefinition = "ProviderReturnedNullForCreateCommandDefinition";

	internal const string ProviderDidNotReturnAProviderManifest = "ProviderDidNotReturnAProviderManifest";

	internal const string ProviderDidNotReturnAProviderManifestToken = "ProviderDidNotReturnAProviderManifestToken";

	internal const string ProviderDidNotReturnSpatialServices = "ProviderDidNotReturnSpatialServices";

	internal const string SpatialProviderNotUsable = "SpatialProviderNotUsable";

	internal const string ProviderRequiresStoreCommandTree = "ProviderRequiresStoreCommandTree";

	internal const string ProviderShouldOverrideEscapeLikeArgument = "ProviderShouldOverrideEscapeLikeArgument";

	internal const string ProviderEscapeLikeArgumentReturnedNull = "ProviderEscapeLikeArgumentReturnedNull";

	internal const string ProviderDidNotCreateACommandDefinition = "ProviderDidNotCreateACommandDefinition";

	internal const string ProviderDoesNotSupportCreateDatabaseScript = "ProviderDoesNotSupportCreateDatabaseScript";

	internal const string ProviderDoesNotSupportCreateDatabase = "ProviderDoesNotSupportCreateDatabase";

	internal const string ProviderDoesNotSupportDatabaseExists = "ProviderDoesNotSupportDatabaseExists";

	internal const string ProviderDoesNotSupportDeleteDatabase = "ProviderDoesNotSupportDeleteDatabase";

	internal const string Spatial_GeographyValueNotCompatibleWithSpatialServices = "Spatial_GeographyValueNotCompatibleWithSpatialServices";

	internal const string Spatial_GeometryValueNotCompatibleWithSpatialServices = "Spatial_GeometryValueNotCompatibleWithSpatialServices";

	internal const string Spatial_ProviderValueNotCompatibleWithSpatialServices = "Spatial_ProviderValueNotCompatibleWithSpatialServices";

	internal const string Spatial_WellKnownValueSerializationPropertyNotDirectlySettable = "Spatial_WellKnownValueSerializationPropertyNotDirectlySettable";

	internal const string EntityConnectionString_Name = "EntityConnectionString_Name";

	internal const string EntityConnectionString_Provider = "EntityConnectionString_Provider";

	internal const string EntityConnectionString_Metadata = "EntityConnectionString_Metadata";

	internal const string EntityConnectionString_ProviderConnectionString = "EntityConnectionString_ProviderConnectionString";

	internal const string EntityDataCategory_Context = "EntityDataCategory_Context";

	internal const string EntityDataCategory_NamedConnectionString = "EntityDataCategory_NamedConnectionString";

	internal const string EntityDataCategory_Source = "EntityDataCategory_Source";

	internal const string ObjectQuery_Span_IncludeRequiresEntityOrEntityCollection = "ObjectQuery_Span_IncludeRequiresEntityOrEntityCollection";

	internal const string ObjectQuery_Span_NoNavProp = "ObjectQuery_Span_NoNavProp";

	internal const string ObjectQuery_Span_SpanPathSyntaxError = "ObjectQuery_Span_SpanPathSyntaxError";

	internal const string EntityProxyTypeInfo_ProxyHasWrongWrapper = "EntityProxyTypeInfo_ProxyHasWrongWrapper";

	internal const string EntityProxyTypeInfo_CannotSetEntityCollectionProperty = "EntityProxyTypeInfo_CannotSetEntityCollectionProperty";

	internal const string EntityProxyTypeInfo_ProxyMetadataIsUnavailable = "EntityProxyTypeInfo_ProxyMetadataIsUnavailable";

	internal const string EntityProxyTypeInfo_DuplicateOSpaceType = "EntityProxyTypeInfo_DuplicateOSpaceType";

	internal const string InvalidEdmMemberInstance = "InvalidEdmMemberInstance";

	internal const string EF6Providers_NoProviderFound = "EF6Providers_NoProviderFound";

	internal const string EF6Providers_ProviderTypeMissing = "EF6Providers_ProviderTypeMissing";

	internal const string EF6Providers_InstanceMissing = "EF6Providers_InstanceMissing";

	internal const string EF6Providers_NotDbProviderServices = "EF6Providers_NotDbProviderServices";

	internal const string ProviderInvariantRepeatedInConfig = "ProviderInvariantRepeatedInConfig";

	internal const string DbDependencyResolver_NoProviderInvariantName = "DbDependencyResolver_NoProviderInvariantName";

	internal const string DbDependencyResolver_InvalidKey = "DbDependencyResolver_InvalidKey";

	internal const string DefaultConfigurationUsedBeforeSet = "DefaultConfigurationUsedBeforeSet";

	internal const string AddHandlerToInUseConfiguration = "AddHandlerToInUseConfiguration";

	internal const string ConfigurationSetTwice = "ConfigurationSetTwice";

	internal const string ConfigurationNotDiscovered = "ConfigurationNotDiscovered";

	internal const string SetConfigurationNotDiscovered = "SetConfigurationNotDiscovered";

	internal const string MultipleConfigsInAssembly = "MultipleConfigsInAssembly";

	internal const string CreateInstance_BadMigrationsConfigurationType = "CreateInstance_BadMigrationsConfigurationType";

	internal const string CreateInstance_BadSqlGeneratorType = "CreateInstance_BadSqlGeneratorType";

	internal const string CreateInstance_BadDbConfigurationType = "CreateInstance_BadDbConfigurationType";

	internal const string DbConfigurationTypeNotFound = "DbConfigurationTypeNotFound";

	internal const string DbConfigurationTypeInAttributeNotFound = "DbConfigurationTypeInAttributeNotFound";

	internal const string CreateInstance_NoParameterlessConstructor = "CreateInstance_NoParameterlessConstructor";

	internal const string CreateInstance_AbstractType = "CreateInstance_AbstractType";

	internal const string CreateInstance_GenericType = "CreateInstance_GenericType";

	internal const string ConfigurationLocked = "ConfigurationLocked";

	internal const string EnableMigrationsForContext = "EnableMigrationsForContext";

	internal const string EnableMigrations_MultipleContexts = "EnableMigrations_MultipleContexts";

	internal const string EnableMigrations_MultipleContextsWithName = "EnableMigrations_MultipleContextsWithName";

	internal const string EnableMigrations_NoContext = "EnableMigrations_NoContext";

	internal const string EnableMigrations_NoContextWithName = "EnableMigrations_NoContextWithName";

	internal const string MoreThanOneElement = "MoreThanOneElement";

	internal const string IQueryable_Not_Async = "IQueryable_Not_Async";

	internal const string IQueryable_Provider_Not_Async = "IQueryable_Provider_Not_Async";

	internal const string EmptySequence = "EmptySequence";

	internal const string UnableToMoveHistoryTableWithAuto = "UnableToMoveHistoryTableWithAuto";

	internal const string NoMatch = "NoMatch";

	internal const string MoreThanOneMatch = "MoreThanOneMatch";

	internal const string CreateConfigurationType_NoParameterlessConstructor = "CreateConfigurationType_NoParameterlessConstructor";

	internal const string CollectionEmpty = "CollectionEmpty";

	internal const string DbMigrationsConfiguration_ContextType = "DbMigrationsConfiguration_ContextType";

	internal const string ContextFactoryContextType = "ContextFactoryContextType";

	internal const string DbMigrationsConfiguration_RootedPath = "DbMigrationsConfiguration_RootedPath";

	internal const string ModelBuilder_PropertyFilterTypeMustBePrimitive = "ModelBuilder_PropertyFilterTypeMustBePrimitive";

	internal const string LightweightEntityConfiguration_NonScalarProperty = "LightweightEntityConfiguration_NonScalarProperty";

	internal const string MigrationsPendingException = "MigrationsPendingException";

	internal const string ExecutionStrategy_ExistingTransaction = "ExecutionStrategy_ExistingTransaction";

	internal const string ExecutionStrategy_MinimumMustBeLessThanMaximum = "ExecutionStrategy_MinimumMustBeLessThanMaximum";

	internal const string ExecutionStrategy_NegativeDelay = "ExecutionStrategy_NegativeDelay";

	internal const string ExecutionStrategy_RetryLimitExceeded = "ExecutionStrategy_RetryLimitExceeded";

	internal const string BaseTypeNotMappedToFunctions = "BaseTypeNotMappedToFunctions";

	internal const string InvalidResourceName = "InvalidResourceName";

	internal const string ModificationFunctionParameterNotFound = "ModificationFunctionParameterNotFound";

	internal const string EntityClient_CannotOpenBrokenConnection = "EntityClient_CannotOpenBrokenConnection";

	internal const string ModificationFunctionParameterNotFoundOriginal = "ModificationFunctionParameterNotFoundOriginal";

	internal const string ResultBindingNotFound = "ResultBindingNotFound";

	internal const string ConflictingFunctionsMapping = "ConflictingFunctionsMapping";

	internal const string DbContext_InvalidTransactionForConnection = "DbContext_InvalidTransactionForConnection";

	internal const string DbContext_InvalidTransactionNoConnection = "DbContext_InvalidTransactionNoConnection";

	internal const string DbContext_TransactionAlreadyStarted = "DbContext_TransactionAlreadyStarted";

	internal const string DbContext_TransactionAlreadyEnlistedInUserTransaction = "DbContext_TransactionAlreadyEnlistedInUserTransaction";

	internal const string ExecutionStrategy_StreamingNotSupported = "ExecutionStrategy_StreamingNotSupported";

	internal const string EdmProperty_InvalidPropertyType = "EdmProperty_InvalidPropertyType";

	internal const string ConcurrentMethodInvocation = "ConcurrentMethodInvocation";

	internal const string AssociationSet_EndEntityTypeMismatch = "AssociationSet_EndEntityTypeMismatch";

	internal const string VisitDbInExpressionNotImplemented = "VisitDbInExpressionNotImplemented";

	internal const string InvalidColumnBuilderArgument = "InvalidColumnBuilderArgument";

	internal const string StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed = "StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed";

	internal const string StorageComplexPropertyMapping_OnlyComplexPropertyAllowed = "StorageComplexPropertyMapping_OnlyComplexPropertyAllowed";

	internal const string MetadataItemErrorsFoundDuringGeneration = "MetadataItemErrorsFoundDuringGeneration";

	internal const string AutomaticStaleFunctions = "AutomaticStaleFunctions";

	internal const string ScaffoldSprocInDownNotSupported = "ScaffoldSprocInDownNotSupported";

	internal const string LightweightEntityConfiguration_ConfigurationConflict_ComplexType = "LightweightEntityConfiguration_ConfigurationConflict_ComplexType";

	internal const string LightweightEntityConfiguration_ConfigurationConflict_IgnoreType = "LightweightEntityConfiguration_ConfigurationConflict_IgnoreType";

	internal const string AttemptToAddEdmMemberFromWrongDataSpace = "AttemptToAddEdmMemberFromWrongDataSpace";

	internal const string LightweightEntityConfiguration_InvalidNavigationProperty = "LightweightEntityConfiguration_InvalidNavigationProperty";

	internal const string LightweightEntityConfiguration_InvalidInverseNavigationProperty = "LightweightEntityConfiguration_InvalidInverseNavigationProperty";

	internal const string LightweightEntityConfiguration_MismatchedInverseNavigationProperty = "LightweightEntityConfiguration_MismatchedInverseNavigationProperty";

	internal const string DuplicateParameterName = "DuplicateParameterName";

	internal const string CommandLogFailed = "CommandLogFailed";

	internal const string CommandLogCanceled = "CommandLogCanceled";

	internal const string CommandLogComplete = "CommandLogComplete";

	internal const string CommandLogAsync = "CommandLogAsync";

	internal const string CommandLogNonAsync = "CommandLogNonAsync";

	internal const string SuppressionAfterExecution = "SuppressionAfterExecution";

	internal const string BadContextTypeForDiscovery = "BadContextTypeForDiscovery";

	internal const string ErrorGeneratingCommandTree = "ErrorGeneratingCommandTree";

	internal const string LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity = "LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity";

	internal const string LightweightNavigationPropertyConfiguration_InvalidMultiplicity = "LightweightNavigationPropertyConfiguration_InvalidMultiplicity";

	internal const string LightweightPrimitivePropertyConfiguration_NonNullableProperty = "LightweightPrimitivePropertyConfiguration_NonNullableProperty";

	internal const string TestDoubleNotImplemented = "TestDoubleNotImplemented";

	internal const string TestDoublesCannotBeConverted = "TestDoublesCannotBeConverted";

	internal const string InvalidNavigationPropertyComplexType = "InvalidNavigationPropertyComplexType";

	internal const string ConventionsConfiguration_InvalidConventionType = "ConventionsConfiguration_InvalidConventionType";

	internal const string ConventionsConfiguration_ConventionTypeMissmatch = "ConventionsConfiguration_ConventionTypeMissmatch";

	internal const string LightweightPrimitivePropertyConfiguration_DateTimeScale = "LightweightPrimitivePropertyConfiguration_DateTimeScale";

	internal const string LightweightPrimitivePropertyConfiguration_DecimalNoScale = "LightweightPrimitivePropertyConfiguration_DecimalNoScale";

	internal const string LightweightPrimitivePropertyConfiguration_HasPrecisionNonDateTime = "LightweightPrimitivePropertyConfiguration_HasPrecisionNonDateTime";

	internal const string LightweightPrimitivePropertyConfiguration_HasPrecisionNonDecimal = "LightweightPrimitivePropertyConfiguration_HasPrecisionNonDecimal";

	internal const string LightweightPrimitivePropertyConfiguration_IsRowVersionNonBinary = "LightweightPrimitivePropertyConfiguration_IsRowVersionNonBinary";

	internal const string LightweightPrimitivePropertyConfiguration_IsUnicodeNonString = "LightweightPrimitivePropertyConfiguration_IsUnicodeNonString";

	internal const string LightweightPrimitivePropertyConfiguration_NonLength = "LightweightPrimitivePropertyConfiguration_NonLength";

	internal const string UnableToUpgradeHistoryWhenCustomFactory = "UnableToUpgradeHistoryWhenCustomFactory";

	internal const string CommitFailed = "CommitFailed";

	internal const string InterceptorTypeNotFound = "InterceptorTypeNotFound";

	internal const string InterceptorTypeNotInterceptor = "InterceptorTypeNotInterceptor";

	internal const string ViewGenContainersNotFound = "ViewGenContainersNotFound";

	internal const string HashCalcContainersNotFound = "HashCalcContainersNotFound";

	internal const string ViewGenMultipleContainers = "ViewGenMultipleContainers";

	internal const string HashCalcMultipleContainers = "HashCalcMultipleContainers";

	internal const string BadConnectionWrapping = "BadConnectionWrapping";

	internal const string ConnectionClosedLog = "ConnectionClosedLog";

	internal const string ConnectionCloseErrorLog = "ConnectionCloseErrorLog";

	internal const string ConnectionOpenedLog = "ConnectionOpenedLog";

	internal const string ConnectionOpenErrorLog = "ConnectionOpenErrorLog";

	internal const string ConnectionOpenedLogAsync = "ConnectionOpenedLogAsync";

	internal const string ConnectionOpenErrorLogAsync = "ConnectionOpenErrorLogAsync";

	internal const string TransactionStartedLog = "TransactionStartedLog";

	internal const string TransactionStartErrorLog = "TransactionStartErrorLog";

	internal const string TransactionCommittedLog = "TransactionCommittedLog";

	internal const string TransactionCommitErrorLog = "TransactionCommitErrorLog";

	internal const string TransactionRolledBackLog = "TransactionRolledBackLog";

	internal const string TransactionRollbackErrorLog = "TransactionRollbackErrorLog";

	internal const string ConnectionOpenCanceledLog = "ConnectionOpenCanceledLog";

	internal const string TransactionHandler_AlreadyInitialized = "TransactionHandler_AlreadyInitialized";

	internal const string ConnectionDisposedLog = "ConnectionDisposedLog";

	internal const string TransactionDisposedLog = "TransactionDisposedLog";

	internal const string UnableToLoadEmbeddedResource = "UnableToLoadEmbeddedResource";

	internal const string CannotSetBaseTypeCyclicInheritance = "CannotSetBaseTypeCyclicInheritance";

	internal const string CannotDefineKeysOnBothBaseAndDerivedTypes = "CannotDefineKeysOnBothBaseAndDerivedTypes";

	internal const string StoreTypeNotFound = "StoreTypeNotFound";

	internal const string ProviderDoesNotSupportEscapingLikeArgument = "ProviderDoesNotSupportEscapingLikeArgument";

	internal const string IndexPropertyNotFound = "IndexPropertyNotFound";

	internal const string ConflictingIndexAttributeMatches = "ConflictingIndexAttributeMatches";

	private static EntityRes loader;

	private readonly ResourceManager resources;

	private static CultureInfo Culture => null;

	public static ResourceManager Resources => GetLoader().resources;

	private EntityRes()
	{
		resources = new ResourceManager("System.Data.Entity.Properties.Resources", typeof(DbContext).GetTypeInfo().Assembly);
	}

	private static EntityRes GetLoader()
	{
		if (loader == null)
		{
			EntityRes value = new EntityRes();
			Interlocked.CompareExchange(ref loader, value, null);
		}
		return loader;
	}

	public static string GetString(string name, params object[] args)
	{
		EntityRes entityRes = GetLoader();
		if (entityRes == null)
		{
			return null;
		}
		string @string = entityRes.resources.GetString(name, Culture);
		if (args != null && args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] is string { Length: >1024 } text)
				{
					args[i] = text.Substring(0, 1021) + "...";
				}
			}
			return string.Format(CultureInfo.CurrentCulture, @string, args);
		}
		return @string;
	}

	public static string GetString(string name)
	{
		return GetLoader()?.resources.GetString(name, Culture);
	}

	public static string GetString(string name, out bool usedFallback)
	{
		usedFallback = false;
		return GetString(name);
	}

	public static object GetObject(string name)
	{
		return GetLoader()?.resources.GetObject(name, Culture);
	}
}
