using System.CodeDom.Compiler;
using System.Data.Entity.Core;
using System.Data.Entity.Migrations.Infrastructure;

namespace System.Data.Entity.Resources;

[GeneratedCode("Resources.tt", "1.0.0.0")]
internal static class Error
{
	internal static Exception AutomaticDataLoss()
	{
		return new AutomaticDataLossException(Strings.AutomaticDataLoss);
	}

	internal static Exception MetadataOutOfDate()
	{
		return new MigrationsException(Strings.MetadataOutOfDate);
	}

	internal static Exception MigrationNotFound(object p0)
	{
		return new MigrationsException(Strings.MigrationNotFound(p0));
	}

	internal static Exception PartialFkOperation(object p0, object p1)
	{
		return new MigrationsException(Strings.PartialFkOperation(p0, p1));
	}

	internal static Exception AutoNotValidTarget(object p0)
	{
		return new MigrationsException(Strings.AutoNotValidTarget(p0));
	}

	internal static Exception AutoNotValidForScriptWindows(object p0)
	{
		return new MigrationsException(Strings.AutoNotValidForScriptWindows(p0));
	}

	internal static Exception ContextNotConstructible(object p0)
	{
		return new MigrationsException(Strings.ContextNotConstructible(p0));
	}

	internal static Exception AmbiguousMigrationName(object p0)
	{
		return new MigrationsException(Strings.AmbiguousMigrationName(p0));
	}

	internal static Exception AutomaticDisabledException()
	{
		return new AutomaticMigrationsDisabledException(Strings.AutomaticDisabledException);
	}

	internal static Exception DownScriptWindowsNotSupported()
	{
		return new MigrationsException(Strings.DownScriptWindowsNotSupported);
	}

	internal static Exception AssemblyMigrator_NoConfigurationWithName(object p0, object p1)
	{
		return new MigrationsException(Strings.AssemblyMigrator_NoConfigurationWithName(p0, p1));
	}

	internal static Exception AssemblyMigrator_MultipleConfigurationsWithName(object p0, object p1)
	{
		return new MigrationsException(Strings.AssemblyMigrator_MultipleConfigurationsWithName(p0, p1));
	}

	internal static Exception AssemblyMigrator_NoConfiguration(object p0)
	{
		return new MigrationsException(Strings.AssemblyMigrator_NoConfiguration(p0));
	}

	internal static Exception AssemblyMigrator_MultipleConfigurations(object p0)
	{
		return new MigrationsException(Strings.AssemblyMigrator_MultipleConfigurations(p0));
	}

	internal static Exception MigrationsNamespaceNotUnderRootNamespace(object p0, object p1)
	{
		return new MigrationsException(Strings.MigrationsNamespaceNotUnderRootNamespace(p0, p1));
	}

	internal static Exception UnableToDispatchAddOrUpdate(object p0)
	{
		return new InvalidOperationException(Strings.UnableToDispatchAddOrUpdate(p0));
	}

	internal static Exception NoSqlGeneratorForProvider(object p0)
	{
		return new MigrationsException(Strings.NoSqlGeneratorForProvider(p0));
	}

	internal static Exception EntityTypeConfigurationMismatch(object p0)
	{
		return new InvalidOperationException(Strings.EntityTypeConfigurationMismatch(p0));
	}

	internal static Exception ComplexTypeConfigurationMismatch(object p0)
	{
		return new InvalidOperationException(Strings.ComplexTypeConfigurationMismatch(p0));
	}

	internal static Exception KeyPropertyNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.KeyPropertyNotFound(p0, p1));
	}

	internal static Exception ForeignKeyPropertyNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ForeignKeyPropertyNotFound(p0, p1));
	}

	internal static Exception PropertyNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.PropertyNotFound(p0, p1));
	}

	internal static Exception NavigationPropertyNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.NavigationPropertyNotFound(p0, p1));
	}

	internal static Exception InvalidPropertyExpression(object p0)
	{
		return new InvalidOperationException(Strings.InvalidPropertyExpression(p0));
	}

	internal static Exception InvalidComplexPropertyExpression(object p0)
	{
		return new InvalidOperationException(Strings.InvalidComplexPropertyExpression(p0));
	}

	internal static Exception InvalidPropertiesExpression(object p0)
	{
		return new InvalidOperationException(Strings.InvalidPropertiesExpression(p0));
	}

	internal static Exception InvalidComplexPropertiesExpression(object p0)
	{
		return new InvalidOperationException(Strings.InvalidComplexPropertiesExpression(p0));
	}

	internal static Exception DuplicateStructuralTypeConfiguration(object p0)
	{
		return new InvalidOperationException(Strings.DuplicateStructuralTypeConfiguration(p0));
	}

	internal static Exception ConflictingPropertyConfiguration(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.ConflictingPropertyConfiguration(p0, p1, p2));
	}

	internal static Exception ConflictingTypeAnnotation(object p0, object p1, object p2, object p3)
	{
		return new InvalidOperationException(Strings.ConflictingTypeAnnotation(p0, p1, p2, p3));
	}

	internal static Exception ConflictingColumnConfiguration(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.ConflictingColumnConfiguration(p0, p1, p2));
	}

	internal static Exception CodeFirstInvalidComplexType(object p0)
	{
		return new InvalidOperationException(Strings.CodeFirstInvalidComplexType(p0));
	}

	internal static Exception InvalidEntityType(object p0)
	{
		return new InvalidOperationException(Strings.InvalidEntityType(p0));
	}

	internal static Exception NavigationInverseItself(object p0, object p1)
	{
		return new InvalidOperationException(Strings.NavigationInverseItself(p0, p1));
	}

	internal static Exception ConflictingConstraint(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingConstraint(p0, p1));
	}

	internal static Exception ConflictingInferredColumnType(object p0, object p1, object p2)
	{
		return new MappingException(Strings.ConflictingInferredColumnType(p0, p1, p2));
	}

	internal static Exception ConflictingMapping(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingMapping(p0, p1));
	}

	internal static Exception ConflictingCascadeDeleteOperation(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingCascadeDeleteOperation(p0, p1));
	}

	internal static Exception ConflictingMultiplicities(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingMultiplicities(p0, p1));
	}

	internal static Exception MaxLengthAttributeConvention_InvalidMaxLength(object p0, object p1)
	{
		return new InvalidOperationException(Strings.MaxLengthAttributeConvention_InvalidMaxLength(p0, p1));
	}

	internal static Exception StringLengthAttributeConvention_InvalidMaximumLength(object p0, object p1)
	{
		return new InvalidOperationException(Strings.StringLengthAttributeConvention_InvalidMaximumLength(p0, p1));
	}

	internal static Exception ModelGeneration_UnableToDetermineKeyOrder(object p0)
	{
		return new InvalidOperationException(Strings.ModelGeneration_UnableToDetermineKeyOrder(p0));
	}

	internal static Exception ForeignKeyAttributeConvention_EmptyKey(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ForeignKeyAttributeConvention_EmptyKey(p0, p1));
	}

	internal static Exception ForeignKeyAttributeConvention_InvalidKey(object p0, object p1, object p2, object p3)
	{
		return new InvalidOperationException(Strings.ForeignKeyAttributeConvention_InvalidKey(p0, p1, p2, p3));
	}

	internal static Exception ForeignKeyAttributeConvention_InvalidNavigationProperty(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.ForeignKeyAttributeConvention_InvalidNavigationProperty(p0, p1, p2));
	}

	internal static Exception ForeignKeyAttributeConvention_OrderRequired(object p0)
	{
		return new InvalidOperationException(Strings.ForeignKeyAttributeConvention_OrderRequired(p0));
	}

	internal static Exception InversePropertyAttributeConvention_PropertyNotFound(object p0, object p1, object p2, object p3)
	{
		return new InvalidOperationException(Strings.InversePropertyAttributeConvention_PropertyNotFound(p0, p1, p2, p3));
	}

	internal static Exception InversePropertyAttributeConvention_SelfInverseDetected(object p0, object p1)
	{
		return new InvalidOperationException(Strings.InversePropertyAttributeConvention_SelfInverseDetected(p0, p1));
	}

	internal static Exception KeyRegisteredOnDerivedType(object p0, object p1)
	{
		return new InvalidOperationException(Strings.KeyRegisteredOnDerivedType(p0, p1));
	}

	internal static Exception InvalidTableMapping(object p0, object p1)
	{
		return new InvalidOperationException(Strings.InvalidTableMapping(p0, p1));
	}

	internal static Exception InvalidTableMapping_NoTableName(object p0)
	{
		return new InvalidOperationException(Strings.InvalidTableMapping_NoTableName(p0));
	}

	internal static Exception InvalidChainedMappingSyntax(object p0)
	{
		return new InvalidOperationException(Strings.InvalidChainedMappingSyntax(p0));
	}

	internal static Exception InvalidNotNullCondition(object p0, object p1)
	{
		return new InvalidOperationException(Strings.InvalidNotNullCondition(p0, p1));
	}

	internal static Exception InvalidDiscriminatorType(object p0)
	{
		return new ArgumentException(Strings.InvalidDiscriminatorType(p0));
	}

	internal static Exception ConventionNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConventionNotFound(p0, p1));
	}

	internal static Exception InvalidEntitySplittingProperties(object p0)
	{
		return new InvalidOperationException(Strings.InvalidEntitySplittingProperties(p0));
	}

	internal static Exception InvalidDatabaseName(object p0)
	{
		return new ArgumentException(Strings.InvalidDatabaseName(p0));
	}

	internal static Exception EntityMappingConfiguration_DuplicateMapInheritedProperties(object p0)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_DuplicateMapInheritedProperties(p0));
	}

	internal static Exception EntityMappingConfiguration_DuplicateMappedProperties(object p0)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_DuplicateMappedProperties(p0));
	}

	internal static Exception EntityMappingConfiguration_DuplicateMappedProperty(object p0, object p1)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_DuplicateMappedProperty(p0, p1));
	}

	internal static Exception EntityMappingConfiguration_CannotMapIgnoredProperty(object p0, object p1)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_CannotMapIgnoredProperty(p0, p1));
	}

	internal static Exception EntityMappingConfiguration_InvalidTableSharing(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_InvalidTableSharing(p0, p1, p2));
	}

	internal static Exception EntityMappingConfiguration_TPCWithIAsOnNonLeafType(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.EntityMappingConfiguration_TPCWithIAsOnNonLeafType(p0, p1, p2));
	}

	internal static Exception CannotIgnoreMappedBaseProperty(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.CannotIgnoreMappedBaseProperty(p0, p1, p2));
	}

	internal static Exception ModelBuilder_KeyPropertiesMustBePrimitive(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ModelBuilder_KeyPropertiesMustBePrimitive(p0, p1));
	}

	internal static Exception TableNotFound(object p0)
	{
		return new InvalidOperationException(Strings.TableNotFound(p0));
	}

	internal static Exception IncorrectColumnCount(object p0)
	{
		return new InvalidOperationException(Strings.IncorrectColumnCount(p0));
	}

	internal static Exception CircularComplexTypeHierarchy()
	{
		return new InvalidOperationException(Strings.CircularComplexTypeHierarchy);
	}

	internal static Exception UnableToDeterminePrincipal(object p0, object p1)
	{
		return new InvalidOperationException(Strings.UnableToDeterminePrincipal(p0, p1));
	}

	internal static Exception UnmappedAbstractType(object p0)
	{
		return new InvalidOperationException(Strings.UnmappedAbstractType(p0));
	}

	internal static Exception UnsupportedHybridInheritanceMapping(object p0)
	{
		return new NotSupportedException(Strings.UnsupportedHybridInheritanceMapping(p0));
	}

	internal static Exception OrphanedConfiguredTableDetected(object p0)
	{
		return new InvalidOperationException(Strings.OrphanedConfiguredTableDetected(p0));
	}

	internal static Exception DuplicateConfiguredColumnOrder(object p0)
	{
		return new InvalidOperationException(Strings.DuplicateConfiguredColumnOrder(p0));
	}

	internal static Exception UnsupportedUseOfV3Type(object p0, object p1)
	{
		return new NotSupportedException(Strings.UnsupportedUseOfV3Type(p0, p1));
	}

	internal static Exception MultiplePropertiesMatchedAsKeys(object p0, object p1)
	{
		return new InvalidOperationException(Strings.MultiplePropertiesMatchedAsKeys(p0, p1));
	}

	internal static Exception DbPropertyEntry_CannotGetCurrentValue(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyEntry_CannotGetCurrentValue(p0, p1));
	}

	internal static Exception DbPropertyEntry_CannotSetCurrentValue(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyEntry_CannotSetCurrentValue(p0, p1));
	}

	internal static Exception DbPropertyEntry_NotSupportedForDetached(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.DbPropertyEntry_NotSupportedForDetached(p0, p1, p2));
	}

	internal static Exception DbPropertyEntry_SettingEntityRefNotSupported(object p0, object p1, object p2)
	{
		return new NotSupportedException(Strings.DbPropertyEntry_SettingEntityRefNotSupported(p0, p1, p2));
	}

	internal static Exception DbPropertyEntry_NotSupportedForPropertiesNotInTheModel(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.DbPropertyEntry_NotSupportedForPropertiesNotInTheModel(p0, p1, p2));
	}

	internal static Exception DbEntityEntry_NotSupportedForDetached(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbEntityEntry_NotSupportedForDetached(p0, p1));
	}

	internal static Exception DbSet_BadTypeForAddAttachRemove(object p0, object p1, object p2)
	{
		return new ArgumentException(Strings.DbSet_BadTypeForAddAttachRemove(p0, p1, p2));
	}

	internal static Exception DbSet_BadTypeForCreate(object p0, object p1)
	{
		return new ArgumentException(Strings.DbSet_BadTypeForCreate(p0, p1));
	}

	internal static Exception DbEntity_BadTypeForCast(object p0, object p1, object p2)
	{
		return new InvalidCastException(Strings.DbEntity_BadTypeForCast(p0, p1, p2));
	}

	internal static Exception DbMember_BadTypeForCast(object p0, object p1, object p2, object p3, object p4)
	{
		return new InvalidCastException(Strings.DbMember_BadTypeForCast(p0, p1, p2, p3, p4));
	}

	internal static Exception DbEntityEntry_UsedReferenceForCollectionProp(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_UsedReferenceForCollectionProp(p0, p1));
	}

	internal static Exception DbEntityEntry_UsedCollectionForReferenceProp(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_UsedCollectionForReferenceProp(p0, p1));
	}

	internal static Exception DbEntityEntry_NotANavigationProperty(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_NotANavigationProperty(p0, p1));
	}

	internal static Exception DbEntityEntry_NotAScalarProperty(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_NotAScalarProperty(p0, p1));
	}

	internal static Exception DbEntityEntry_NotAComplexProperty(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_NotAComplexProperty(p0, p1));
	}

	internal static Exception DbEntityEntry_NotAProperty(object p0, object p1)
	{
		return new ArgumentException(Strings.DbEntityEntry_NotAProperty(p0, p1));
	}

	internal static Exception DbEntityEntry_DottedPartNotComplex(object p0, object p1, object p2)
	{
		return new ArgumentException(Strings.DbEntityEntry_DottedPartNotComplex(p0, p1, p2));
	}

	internal static Exception DbEntityEntry_DottedPathMustBeProperty(object p0)
	{
		return new ArgumentException(Strings.DbEntityEntry_DottedPathMustBeProperty(p0));
	}

	internal static Exception DbEntityEntry_WrongGenericForNavProp(object p0, object p1, object p2, object p3)
	{
		return new ArgumentException(Strings.DbEntityEntry_WrongGenericForNavProp(p0, p1, p2, p3));
	}

	internal static Exception DbEntityEntry_WrongGenericForCollectionNavProp(object p0, object p1, object p2, object p3)
	{
		return new ArgumentException(Strings.DbEntityEntry_WrongGenericForCollectionNavProp(p0, p1, p2, p3));
	}

	internal static Exception DbEntityEntry_WrongGenericForProp(object p0, object p1, object p2, object p3)
	{
		return new ArgumentException(Strings.DbEntityEntry_WrongGenericForProp(p0, p1, p2, p3));
	}

	internal static Exception DbPropertyValues_CannotGetValuesForState(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_CannotGetValuesForState(p0, p1));
	}

	internal static Exception DbPropertyValues_CannotSetNullValue(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_CannotSetNullValue(p0, p1, p2));
	}

	internal static Exception DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull(p0, p1));
	}

	internal static Exception DbPropertyValues_WrongTypeForAssignment(object p0, object p1, object p2, object p3)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_WrongTypeForAssignment(p0, p1, p2, p3));
	}

	internal static Exception DbPropertyValues_PropertyValueNamesAreReadonly()
	{
		return new NotSupportedException(Strings.DbPropertyValues_PropertyValueNamesAreReadonly);
	}

	internal static Exception DbPropertyValues_PropertyDoesNotExist(object p0, object p1)
	{
		return new ArgumentException(Strings.DbPropertyValues_PropertyDoesNotExist(p0, p1));
	}

	internal static Exception DbPropertyValues_AttemptToSetValuesFromWrongObject(object p0, object p1)
	{
		return new ArgumentException(Strings.DbPropertyValues_AttemptToSetValuesFromWrongObject(p0, p1));
	}

	internal static Exception DbPropertyValues_AttemptToSetValuesFromWrongType(object p0, object p1)
	{
		return new ArgumentException(Strings.DbPropertyValues_AttemptToSetValuesFromWrongType(p0, p1));
	}

	internal static Exception DbPropertyValues_AttemptToSetNonValuesOnComplexProperty()
	{
		return new ArgumentException(Strings.DbPropertyValues_AttemptToSetNonValuesOnComplexProperty);
	}

	internal static Exception DbPropertyValues_ComplexObjectCannotBeNull(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_ComplexObjectCannotBeNull(p0, p1));
	}

	internal static Exception DbPropertyValues_NestedPropertyValuesNull(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_NestedPropertyValuesNull(p0, p1));
	}

	internal static Exception DbPropertyValues_CannotSetPropertyOnNullCurrentValue(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_CannotSetPropertyOnNullCurrentValue(p0, p1));
	}

	internal static Exception DbPropertyValues_CannotSetPropertyOnNullOriginalValue(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbPropertyValues_CannotSetPropertyOnNullOriginalValue(p0, p1));
	}

	internal static Exception DatabaseInitializationStrategy_ModelMismatch(object p0)
	{
		return new InvalidOperationException(Strings.DatabaseInitializationStrategy_ModelMismatch(p0));
	}

	internal static Exception Database_DatabaseAlreadyExists(object p0)
	{
		return new InvalidOperationException(Strings.Database_DatabaseAlreadyExists(p0));
	}

	internal static Exception Database_NonCodeFirstCompatibilityCheck()
	{
		return new NotSupportedException(Strings.Database_NonCodeFirstCompatibilityCheck);
	}

	internal static Exception Database_NoDatabaseMetadata()
	{
		return new NotSupportedException(Strings.Database_NoDatabaseMetadata);
	}

	internal static Exception ContextConfiguredMultipleTimes(object p0)
	{
		return new InvalidOperationException(Strings.ContextConfiguredMultipleTimes(p0));
	}

	internal static Exception DbContext_ContextUsedInModelCreating()
	{
		return new InvalidOperationException(Strings.DbContext_ContextUsedInModelCreating);
	}

	internal static Exception DbContext_MESTNotSupported()
	{
		return new InvalidOperationException(Strings.DbContext_MESTNotSupported);
	}

	internal static Exception DbContext_Disposed()
	{
		return new InvalidOperationException(Strings.DbContext_Disposed);
	}

	internal static Exception DbContext_ProviderReturnedNullConnection()
	{
		return new InvalidOperationException(Strings.DbContext_ProviderReturnedNullConnection);
	}

	internal static Exception DbContext_ProviderNameMissing(object p0)
	{
		return new InvalidOperationException(Strings.DbContext_ProviderNameMissing(p0));
	}

	internal static Exception DbContext_ConnectionFactoryReturnedNullConnection()
	{
		return new InvalidOperationException(Strings.DbContext_ConnectionFactoryReturnedNullConnection);
	}

	internal static Exception DbSet_WrongEntityTypeFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.DbSet_WrongEntityTypeFound(p0, p1));
	}

	internal static Exception DbSet_MultipleAddedEntitiesFound()
	{
		return new InvalidOperationException(Strings.DbSet_MultipleAddedEntitiesFound);
	}

	internal static Exception DbSet_DbSetUsedWithComplexType(object p0)
	{
		return new InvalidOperationException(Strings.DbSet_DbSetUsedWithComplexType(p0));
	}

	internal static Exception DbSet_PocoAndNonPocoMixedInSameAssembly(object p0)
	{
		return new InvalidOperationException(Strings.DbSet_PocoAndNonPocoMixedInSameAssembly(p0));
	}

	internal static Exception DbSet_EntityTypeNotInModel(object p0)
	{
		return new InvalidOperationException(Strings.DbSet_EntityTypeNotInModel(p0));
	}

	internal static Exception DbQuery_BindingToDbQueryNotSupported()
	{
		return new NotSupportedException(Strings.DbQuery_BindingToDbQueryNotSupported);
	}

	internal static Exception DbContext_ConnectionStringNotFound(object p0)
	{
		return new InvalidOperationException(Strings.DbContext_ConnectionStringNotFound(p0));
	}

	internal static Exception DbContext_ConnectionHasModel()
	{
		return new InvalidOperationException(Strings.DbContext_ConnectionHasModel);
	}

	internal static Exception DbCollectionEntry_CannotSetCollectionProp(object p0, object p1)
	{
		return new NotSupportedException(Strings.DbCollectionEntry_CannotSetCollectionProp(p0, p1));
	}

	internal static Exception CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported()
	{
		return new NotSupportedException(Strings.CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported);
	}

	internal static Exception Mapping_MESTNotSupported(object p0, object p1, object p2)
	{
		return new InvalidOperationException(Strings.Mapping_MESTNotSupported(p0, p1, p2));
	}

	internal static Exception DbModelBuilder_MissingRequiredCtor(object p0)
	{
		return new InvalidOperationException(Strings.DbModelBuilder_MissingRequiredCtor(p0));
	}

	internal static Exception SqlConnectionFactory_MdfNotSupported(object p0)
	{
		return new NotSupportedException(Strings.SqlConnectionFactory_MdfNotSupported(p0));
	}

	internal static Exception EdmxWriter_EdmxFromObjectContextNotSupported()
	{
		return new NotSupportedException(Strings.EdmxWriter_EdmxFromObjectContextNotSupported);
	}

	internal static Exception EdmxWriter_EdmxFromModelFirstNotSupported()
	{
		return new NotSupportedException(Strings.EdmxWriter_EdmxFromModelFirstNotSupported);
	}

	internal static Exception EdmxWriter_EdmxFromRawCompiledModelNotSupported()
	{
		return new NotSupportedException(Strings.EdmxWriter_EdmxFromRawCompiledModelNotSupported);
	}

	internal static Exception DbContextServices_MissingDefaultCtor(object p0)
	{
		return new InvalidOperationException(Strings.DbContextServices_MissingDefaultCtor(p0));
	}

	internal static Exception CannotCallGenericSetWithProxyType()
	{
		return new InvalidOperationException(Strings.CannotCallGenericSetWithProxyType);
	}

	internal static Exception MaxLengthAttribute_InvalidMaxLength()
	{
		return new InvalidOperationException(Strings.MaxLengthAttribute_InvalidMaxLength);
	}

	internal static Exception MinLengthAttribute_InvalidMinLength()
	{
		return new InvalidOperationException(Strings.MinLengthAttribute_InvalidMinLength);
	}

	internal static Exception DbConnectionInfo_ConnectionStringNotFound(object p0)
	{
		return new InvalidOperationException(Strings.DbConnectionInfo_ConnectionStringNotFound(p0));
	}

	internal static Exception EagerInternalContext_CannotSetConnectionInfo()
	{
		return new InvalidOperationException(Strings.EagerInternalContext_CannotSetConnectionInfo);
	}

	internal static Exception LazyInternalContext_CannotReplaceEfConnectionWithDbConnection()
	{
		return new InvalidOperationException(Strings.LazyInternalContext_CannotReplaceEfConnectionWithDbConnection);
	}

	internal static Exception LazyInternalContext_CannotReplaceDbConnectionWithEfConnection()
	{
		return new InvalidOperationException(Strings.LazyInternalContext_CannotReplaceDbConnectionWithEfConnection);
	}

	internal static Exception EntityKey_UnexpectedNull()
	{
		return new InvalidOperationException(Strings.EntityKey_UnexpectedNull);
	}

	internal static Exception EntityClient_ConnectionStringNeededBeforeOperation()
	{
		return new InvalidOperationException(Strings.EntityClient_ConnectionStringNeededBeforeOperation);
	}

	internal static Exception EntityClient_ConnectionNotOpen()
	{
		return new InvalidOperationException(Strings.EntityClient_ConnectionNotOpen);
	}

	internal static Exception EntityClient_NoConnectionForAdapter()
	{
		return new InvalidOperationException(Strings.EntityClient_NoConnectionForAdapter);
	}

	internal static Exception EntityClient_ClosedConnectionForUpdate()
	{
		return new InvalidOperationException(Strings.EntityClient_ClosedConnectionForUpdate);
	}

	internal static Exception EntityClient_NoStoreConnectionForUpdate()
	{
		return new InvalidOperationException(Strings.EntityClient_NoStoreConnectionForUpdate);
	}

	internal static Exception Mapping_Default_OCMapping_Member_Type_Mismatch(object p0, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
	{
		return new MappingException(Strings.Mapping_Default_OCMapping_Member_Type_Mismatch(p0, p1, p2, p3, p4, p5, p6, p7));
	}

	internal static Exception ObjectStateManager_ConflictingChangesOfRelationshipDetected(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(p0, p1));
	}

	internal static Exception RelatedEnd_InvalidOwnerStateForAttach()
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidOwnerStateForAttach);
	}

	internal static Exception RelatedEnd_InvalidNthElementNullForAttach(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidNthElementNullForAttach(p0));
	}

	internal static Exception RelatedEnd_InvalidNthElementContextForAttach(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidNthElementContextForAttach(p0));
	}

	internal static Exception RelatedEnd_InvalidNthElementStateForAttach(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidNthElementStateForAttach(p0));
	}

	internal static Exception RelatedEnd_InvalidEntityContextForAttach()
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidEntityContextForAttach);
	}

	internal static Exception RelatedEnd_InvalidEntityStateForAttach()
	{
		return new InvalidOperationException(Strings.RelatedEnd_InvalidEntityStateForAttach);
	}

	internal static Exception RelatedEnd_UnableToAddRelationshipWithDeletedEntity()
	{
		return new InvalidOperationException(Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
	}

	internal static Exception Collections_NoRelationshipSetMatched(object p0)
	{
		return new InvalidOperationException(Strings.Collections_NoRelationshipSetMatched(p0));
	}

	internal static Exception Collections_InvalidEntityStateSource()
	{
		return new InvalidOperationException(Strings.Collections_InvalidEntityStateSource);
	}

	internal static Exception Collections_InvalidEntityStateLoad(object p0)
	{
		return new InvalidOperationException(Strings.Collections_InvalidEntityStateLoad(p0));
	}

	internal static Exception EntityReference_LessThanExpectedRelatedEntitiesFound()
	{
		return new InvalidOperationException(Strings.EntityReference_LessThanExpectedRelatedEntitiesFound);
	}

	internal static Exception EntityReference_MoreThanExpectedRelatedEntitiesFound()
	{
		return new InvalidOperationException(Strings.EntityReference_MoreThanExpectedRelatedEntitiesFound);
	}

	internal static Exception EntityReference_CannotSetSpecialKeys()
	{
		return new InvalidOperationException(Strings.EntityReference_CannotSetSpecialKeys);
	}

	internal static Exception RelatedEnd_RelatedEndNotFound()
	{
		return new InvalidOperationException(Strings.RelatedEnd_RelatedEndNotFound);
	}

	internal static Exception RelatedEnd_RelatedEndNotAttachedToContext(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_RelatedEndNotAttachedToContext(p0));
	}

	internal static Exception RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd()
	{
		return new InvalidOperationException(Strings.RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd);
	}

	internal static Exception RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd()
	{
		return new InvalidOperationException(Strings.RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd);
	}

	internal static Exception RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(p0));
	}

	internal static Exception RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts()
	{
		return new InvalidOperationException(Strings.RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts);
	}

	internal static Exception RelatedEnd_MismatchedMergeOptionOnLoad(object p0)
	{
		return new InvalidOperationException(Strings.RelatedEnd_MismatchedMergeOptionOnLoad(p0));
	}

	internal static Exception RelatedEnd_EntitySetIsNotValidForRelationship(object p0, object p1, object p2, object p3, object p4)
	{
		return new InvalidOperationException(Strings.RelatedEnd_EntitySetIsNotValidForRelationship(p0, p1, p2, p3, p4));
	}

	internal static Exception RelatedEnd_OwnerIsNull()
	{
		return new InvalidOperationException(Strings.RelatedEnd_OwnerIsNull);
	}

	internal static Exception RelationshipManager_NavigationPropertyNotFound(object p0)
	{
		return new InvalidOperationException(Strings.RelationshipManager_NavigationPropertyNotFound(p0));
	}

	internal static Exception ADP_ClosedDataReaderError()
	{
		return new InvalidOperationException(Strings.ADP_ClosedDataReaderError);
	}

	internal static Exception ADP_DataReaderClosed(object p0)
	{
		return new InvalidOperationException(Strings.ADP_DataReaderClosed(p0));
	}

	internal static Exception ADP_ImplicitlyClosedDataReaderError()
	{
		return new InvalidOperationException(Strings.ADP_ImplicitlyClosedDataReaderError);
	}

	internal static Exception ADP_NoData()
	{
		return new InvalidOperationException(Strings.ADP_NoData);
	}

	internal static Exception InvalidEdmMemberInstance()
	{
		return new ArgumentException(Strings.InvalidEdmMemberInstance);
	}

	internal static Exception EnableMigrations_MultipleContextsWithName(object p0, object p1)
	{
		return new MigrationsException(Strings.EnableMigrations_MultipleContextsWithName(p0, p1));
	}

	internal static Exception EnableMigrations_NoContext(object p0)
	{
		return new MigrationsException(Strings.EnableMigrations_NoContext(p0));
	}

	internal static Exception EnableMigrations_NoContextWithName(object p0, object p1)
	{
		return new MigrationsException(Strings.EnableMigrations_NoContextWithName(p0, p1));
	}

	internal static Exception MoreThanOneElement()
	{
		return new InvalidOperationException(Strings.MoreThanOneElement);
	}

	internal static Exception IQueryable_Not_Async(object p0)
	{
		return new InvalidOperationException(Strings.IQueryable_Not_Async(p0));
	}

	internal static Exception IQueryable_Provider_Not_Async()
	{
		return new InvalidOperationException(Strings.IQueryable_Provider_Not_Async);
	}

	internal static Exception EmptySequence()
	{
		return new InvalidOperationException(Strings.EmptySequence);
	}

	internal static Exception UnableToMoveHistoryTableWithAuto()
	{
		return new MigrationsException(Strings.UnableToMoveHistoryTableWithAuto);
	}

	internal static Exception NoMatch()
	{
		return new InvalidOperationException(Strings.NoMatch);
	}

	internal static Exception MoreThanOneMatch()
	{
		return new InvalidOperationException(Strings.MoreThanOneMatch);
	}

	internal static Exception ModelBuilder_PropertyFilterTypeMustBePrimitive(object p0)
	{
		return new InvalidOperationException(Strings.ModelBuilder_PropertyFilterTypeMustBePrimitive(p0));
	}

	internal static Exception MigrationsPendingException(object p0)
	{
		return new MigrationsPendingException(Strings.MigrationsPendingException(p0));
	}

	internal static Exception BaseTypeNotMappedToFunctions(object p0, object p1)
	{
		return new InvalidOperationException(Strings.BaseTypeNotMappedToFunctions(p0, p1));
	}

	internal static Exception InvalidResourceName(object p0)
	{
		return new ArgumentException(Strings.InvalidResourceName(p0));
	}

	internal static Exception ModificationFunctionParameterNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ModificationFunctionParameterNotFound(p0, p1));
	}

	internal static Exception EntityClient_CannotOpenBrokenConnection()
	{
		return new InvalidOperationException(Strings.EntityClient_CannotOpenBrokenConnection);
	}

	internal static Exception ModificationFunctionParameterNotFoundOriginal(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ModificationFunctionParameterNotFoundOriginal(p0, p1));
	}

	internal static Exception ResultBindingNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ResultBindingNotFound(p0, p1));
	}

	internal static Exception ConflictingFunctionsMapping(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingFunctionsMapping(p0, p1));
	}

	internal static Exception AutomaticStaleFunctions(object p0)
	{
		return new MigrationsException(Strings.AutomaticStaleFunctions(p0));
	}

	internal static Exception UnableToUpgradeHistoryWhenCustomFactory()
	{
		return new MigrationsException(Strings.UnableToUpgradeHistoryWhenCustomFactory);
	}

	internal static Exception StoreTypeNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.StoreTypeNotFound(p0, p1));
	}

	internal static Exception IndexPropertyNotFound(object p0, object p1)
	{
		return new InvalidOperationException(Strings.IndexPropertyNotFound(p0, p1));
	}

	internal static Exception ConflictingIndexAttributeMatches(object p0, object p1)
	{
		return new InvalidOperationException(Strings.ConflictingIndexAttributeMatches(p0, p1));
	}

	internal static Exception ArgumentOutOfRange(string paramName)
	{
		return new ArgumentOutOfRangeException(paramName);
	}

	internal static Exception NotImplemented()
	{
		return new NotImplementedException();
	}

	internal static Exception NotSupported()
	{
		return new NotSupportedException();
	}
}
