using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class AssociationTypeMappingGenerator : StructuralTypeMappingGenerator
{
	public AssociationTypeMappingGenerator(DbProviderManifest providerManifest)
		: base(providerManifest)
	{
	}

	public void Generate(AssociationType associationType, DbDatabaseMapping databaseMapping)
	{
		if (associationType.Constraint != null)
		{
			GenerateForeignKeyAssociationType(associationType, databaseMapping);
		}
		else if (associationType.IsManyToMany())
		{
			GenerateManyToManyAssociation(associationType, databaseMapping);
		}
		else
		{
			GenerateIndependentAssociationType(associationType, databaseMapping);
		}
	}

	private static void GenerateForeignKeyAssociationType(AssociationType associationType, DbDatabaseMapping databaseMapping)
	{
		AssociationEndMember dependentEnd = associationType.Constraint.DependentEnd;
		AssociationEndMember otherEnd = associationType.GetOtherEnd(dependentEnd);
		EntityTypeMapping entityTypeMappingInHierarchy = StructuralTypeMappingGenerator.GetEntityTypeMappingInHierarchy(databaseMapping, otherEnd.GetEntityType());
		EntityTypeMapping dependentEntityTypeMapping = StructuralTypeMappingGenerator.GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.GetEntityType());
		ForeignKeyBuilder foreignKeyBuilder = new ForeignKeyBuilder(databaseMapping.Database, associationType.Name)
		{
			PrincipalTable = entityTypeMappingInHierarchy.MappingFragments.Single().Table,
			DeleteAction = ((otherEnd.DeleteBehavior != 0) ? otherEnd.DeleteBehavior : OperationAction.None)
		};
		dependentEntityTypeMapping.MappingFragments.Single().Table.AddForeignKey(foreignKeyBuilder);
		foreignKeyBuilder.DependentColumns = associationType.Constraint.ToProperties.Select((EdmProperty dependentProperty) => dependentEntityTypeMapping.GetPropertyMapping(dependentProperty).ColumnProperty);
		foreignKeyBuilder.SetAssociationType(associationType);
	}

	private void GenerateManyToManyAssociation(AssociationType associationType, DbDatabaseMapping databaseMapping)
	{
		EntityType entityType = associationType.SourceEnd.GetEntityType();
		EntityType entityType2 = associationType.TargetEnd.GetEntityType();
		EntityType dependentTable = databaseMapping.Database.AddTable(entityType.Name + entityType2.Name);
		AssociationSetMapping associationSetMapping = GenerateAssociationSetMapping(associationType, databaseMapping, associationType.SourceEnd, associationType.TargetEnd, dependentTable);
		GenerateIndependentForeignKeyConstraint(databaseMapping, entityType, entityType2, dependentTable, associationSetMapping, associationSetMapping.SourceEndMapping, associationType.SourceEnd.Name, null, isPrimaryKeyColumn: true);
		GenerateIndependentForeignKeyConstraint(databaseMapping, entityType2, entityType, dependentTable, associationSetMapping, associationSetMapping.TargetEndMapping, associationType.TargetEnd.Name, null, isPrimaryKeyColumn: true);
	}

	private void GenerateIndependentAssociationType(AssociationType associationType, DbDatabaseMapping databaseMapping)
	{
		if (!associationType.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd))
		{
			if (!associationType.IsPrincipalConfigured())
			{
				throw Error.UnableToDeterminePrincipal(associationType.SourceEnd.GetEntityType().GetClrType(), associationType.TargetEnd.GetEntityType().GetClrType());
			}
			principalEnd = associationType.SourceEnd;
			dependentEnd = associationType.TargetEnd;
		}
		EntityTypeMapping entityTypeMappingInHierarchy = StructuralTypeMappingGenerator.GetEntityTypeMappingInHierarchy(databaseMapping, dependentEnd.GetEntityType());
		EntityType table = entityTypeMappingInHierarchy.MappingFragments.First().Table;
		AssociationSetMapping associationSetMapping = GenerateAssociationSetMapping(associationType, databaseMapping, principalEnd, dependentEnd, table);
		GenerateIndependentForeignKeyConstraint(databaseMapping, principalEnd.GetEntityType(), dependentEnd.GetEntityType(), table, associationSetMapping, associationSetMapping.SourceEndMapping, associationType.Name, principalEnd);
		foreach (EdmProperty item in dependentEnd.GetEntityType().KeyProperties())
		{
			associationSetMapping.TargetEndMapping.AddPropertyMapping(new ScalarPropertyMapping(item, entityTypeMappingInHierarchy.GetPropertyMapping(item).ColumnProperty));
		}
	}

	private static AssociationSetMapping GenerateAssociationSetMapping(AssociationType associationType, DbDatabaseMapping databaseMapping, AssociationEndMember principalEnd, AssociationEndMember dependentEnd, EntityType dependentTable)
	{
		AssociationSetMapping associationSetMapping = databaseMapping.AddAssociationSetMapping(databaseMapping.Model.GetAssociationSet(associationType), databaseMapping.Database.GetEntitySet(dependentTable));
		associationSetMapping.StoreEntitySet = databaseMapping.Database.GetEntitySet(dependentTable);
		associationSetMapping.SourceEndMapping.AssociationEnd = principalEnd;
		associationSetMapping.TargetEndMapping.AssociationEnd = dependentEnd;
		return associationSetMapping;
	}

	private void GenerateIndependentForeignKeyConstraint(DbDatabaseMapping databaseMapping, EntityType principalEntityType, EntityType dependentEntityType, EntityType dependentTable, AssociationSetMapping associationSetMapping, EndPropertyMapping associationEndMapping, string name, AssociationEndMember principalEnd, bool isPrimaryKeyColumn = false)
	{
		EntityType table = StructuralTypeMappingGenerator.GetEntityTypeMappingInHierarchy(databaseMapping, principalEntityType).MappingFragments.Single().Table;
		ForeignKeyBuilder foreignKeyBuilder = new ForeignKeyBuilder(databaseMapping.Database, name)
		{
			PrincipalTable = table,
			DeleteAction = ((associationEndMapping.AssociationEnd.DeleteBehavior != 0) ? associationEndMapping.AssociationEnd.DeleteBehavior : OperationAction.None)
		};
		NavigationProperty principalNavigationProperty = databaseMapping.Model.EntityTypes.SelectMany((EntityType e) => e.DeclaredNavigationProperties).SingleOrDefault((NavigationProperty n) => n.ResultEnd == principalEnd);
		dependentTable.AddForeignKey(foreignKeyBuilder);
		foreignKeyBuilder.DependentColumns = GenerateIndependentForeignKeyColumns(principalEntityType, dependentEntityType, associationSetMapping, associationEndMapping, dependentTable, isPrimaryKeyColumn, principalNavigationProperty);
	}

	private IEnumerable<EdmProperty> GenerateIndependentForeignKeyColumns(EntityType principalEntityType, EntityType dependentEntityType, AssociationSetMapping associationSetMapping, EndPropertyMapping associationEndMapping, EntityType dependentTable, bool isPrimaryKeyColumn, NavigationProperty principalNavigationProperty)
	{
		foreach (EdmProperty property in principalEntityType.KeyProperties())
		{
			string columnName = ((principalNavigationProperty != null) ? principalNavigationProperty.Name : principalEntityType.Name) + "_" + property.Name;
			EdmProperty foreignKeyColumn = MapTableColumn(property, columnName, isInstancePropertyOnDerivedType: false);
			dependentTable.AddColumn(foreignKeyColumn);
			if (isPrimaryKeyColumn)
			{
				dependentTable.AddKeyMember(foreignKeyColumn);
			}
			foreignKeyColumn.Nullable = associationEndMapping.AssociationEnd.IsOptional() || (associationEndMapping.AssociationEnd.IsRequired() && dependentEntityType.BaseType != null);
			foreignKeyColumn.StoreGeneratedPattern = StoreGeneratedPattern.None;
			yield return foreignKeyColumn;
			associationEndMapping.AddPropertyMapping(new ScalarPropertyMapping(property, foreignKeyColumn));
			if (foreignKeyColumn.Nullable)
			{
				associationSetMapping.AddCondition(new IsNullConditionMapping(foreignKeyColumn, isNull: false));
			}
		}
	}
}
