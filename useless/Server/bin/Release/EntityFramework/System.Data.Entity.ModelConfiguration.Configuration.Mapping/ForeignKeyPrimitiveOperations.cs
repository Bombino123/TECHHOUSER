using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class ForeignKeyPrimitiveOperations
{
	public static void UpdatePrincipalTables(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType fromTable, EntityType toTable, bool isMappingAnyInheritedProperty)
	{
		if (fromTable != toTable)
		{
			UpdatePrincipalTables(databaseMapping, toTable, entityType, removeFks: false);
			if (isMappingAnyInheritedProperty)
			{
				UpdatePrincipalTables(databaseMapping, toTable, (EntityType)entityType.BaseType, removeFks: true);
			}
		}
	}

	private static void UpdatePrincipalTables(DbDatabaseMapping databaseMapping, EntityType toTable, EntityType entityType, bool removeFks)
	{
		foreach (AssociationType item in databaseMapping.Model.AssociationTypes.Where((AssociationType at) => at.SourceEnd.GetEntityType().Equals(entityType) || at.TargetEnd.GetEntityType().Equals(entityType)))
		{
			UpdatePrincipalTables(databaseMapping, toTable, removeFks, item, entityType);
		}
	}

	private static void UpdatePrincipalTables(DbDatabaseMapping databaseMapping, EntityType toTable, bool removeFks, AssociationType associationType, EntityType et)
	{
		List<AssociationEndMember> list = new List<AssociationEndMember>();
		if (associationType.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var _))
		{
			list.Add(principalEnd);
		}
		else if (associationType.SourceEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && associationType.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			list.Add(associationType.SourceEnd);
			list.Add(associationType.TargetEnd);
		}
		else
		{
			list.Add(associationType.SourceEnd);
		}
		foreach (AssociationEndMember item in list)
		{
			if (item.GetEntityType() != et)
			{
				continue;
			}
			IEnumerable<KeyValuePair<EntityType, IEnumerable<EdmProperty>>> enumerable;
			if (associationType.Constraint != null)
			{
				EntityType entityType = associationType.GetOtherEnd(item).GetEntityType();
				enumerable = from df in (from t in databaseMapping.Model.GetSelfAndAllDerivedTypes(entityType)
						select databaseMapping.GetEntityTypeMapping(t) into dm
						where dm != null
						select dm).SelectMany((EntityTypeMapping dm) => dm.MappingFragments.Where((MappingFragment tmf) => associationType.Constraint.ToProperties.All((EdmProperty p) => tmf.ColumnMappings.Any((ColumnMappingBuilder pm) => pm.PropertyPath.First() == p)))).Distinct((MappingFragment f1, MappingFragment f2) => f1.Table == f2.Table)
					select new KeyValuePair<EntityType, IEnumerable<EdmProperty>>(df.Table, from pm in df.ColumnMappings
						where associationType.Constraint.ToProperties.Contains(pm.PropertyPath.First())
						select pm.ColumnProperty);
			}
			else
			{
				AssociationSetMapping associationSetMapping = databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Single((AssociationSetMapping asm) => asm.AssociationSet.ElementType == associationType);
				EntityType table = associationSetMapping.Table;
				IEnumerable<EdmProperty> value = ((associationSetMapping.SourceEndMapping.AssociationEnd == item) ? associationSetMapping.SourceEndMapping.PropertyMappings : associationSetMapping.TargetEndMapping.PropertyMappings).Select((ScalarPropertyMapping pm) => pm.Column);
				enumerable = new KeyValuePair<EntityType, IEnumerable<EdmProperty>>[1]
				{
					new KeyValuePair<EntityType, IEnumerable<EdmProperty>>(table, value)
				};
			}
			foreach (KeyValuePair<EntityType, IEnumerable<EdmProperty>> tableInfo in enumerable)
			{
				ForeignKeyBuilder[] array = tableInfo.Key.ForeignKeyBuilders.Where((ForeignKeyBuilder fk) => fk.DependentColumns.SequenceEqual(tableInfo.Value)).ToArray();
				foreach (ForeignKeyBuilder foreignKeyBuilder in array)
				{
					if (removeFks)
					{
						tableInfo.Key.RemoveForeignKey(foreignKeyBuilder);
					}
					else if (foreignKeyBuilder.GetAssociationType() == null || foreignKeyBuilder.GetAssociationType() == associationType)
					{
						foreignKeyBuilder.PrincipalTable = toTable;
					}
				}
			}
		}
	}

	private static void MoveForeignKeyConstraint(EntityType fromTable, EntityType toTable, ForeignKeyBuilder fk)
	{
		fromTable.RemoveForeignKey(fk);
		if (fk.PrincipalTable != toTable || !fk.DependentColumns.All((EdmProperty c) => c.IsPrimaryKeyColumn))
		{
			IList<EdmProperty> dependentColumns = GetDependentColumns(fk.DependentColumns.ToArray(), toTable.Properties);
			if (!ContainsEquivalentForeignKey(toTable, fk.PrincipalTable, dependentColumns))
			{
				toTable.AddForeignKey(fk);
				fk.DependentColumns = dependentColumns;
			}
		}
	}

	private static void CopyForeignKeyConstraint(EdmModel database, EntityType toTable, ForeignKeyBuilder fk, Func<EdmProperty, EdmProperty> selector = null)
	{
		ForeignKeyBuilder foreignKeyBuilder = new ForeignKeyBuilder(database, database.EntityTypes.SelectMany((EntityType t) => t.ForeignKeyBuilders).UniquifyName(fk.Name))
		{
			PrincipalTable = fk.PrincipalTable,
			DeleteAction = fk.DeleteAction
		};
		foreignKeyBuilder.SetPreferredName(fk.Name);
		IList<EdmProperty> dependentColumns = GetDependentColumns((selector != null) ? fk.DependentColumns.Select(selector) : fk.DependentColumns, toTable.Properties);
		if (!ContainsEquivalentForeignKey(toTable, foreignKeyBuilder.PrincipalTable, dependentColumns))
		{
			toTable.AddForeignKey(foreignKeyBuilder);
			foreignKeyBuilder.DependentColumns = dependentColumns;
		}
	}

	private static bool ContainsEquivalentForeignKey(EntityType dependentTable, EntityType principalTable, IEnumerable<EdmProperty> columns)
	{
		return dependentTable.ForeignKeyBuilders.Any((ForeignKeyBuilder fk) => fk.PrincipalTable == principalTable && fk.DependentColumns.SequenceEqual(columns));
	}

	private static IList<EdmProperty> GetDependentColumns(IEnumerable<EdmProperty> sourceColumns, IEnumerable<EdmProperty> destinationColumns)
	{
		return sourceColumns.Select((EdmProperty sc) => destinationColumns.SingleOrDefault((EdmProperty dc) => string.Equals(dc.Name, sc.Name, StringComparison.Ordinal)) ?? destinationColumns.Single((EdmProperty dc) => string.Equals(dc.GetUnpreferredUniqueName(), sc.Name, StringComparison.Ordinal))).ToList();
	}

	private static IEnumerable<ForeignKeyBuilder> FindAllForeignKeyConstraintsForColumn(EntityType fromTable, EntityType toTable, EdmProperty column)
	{
		return fromTable.ForeignKeyBuilders.Where((ForeignKeyBuilder fk) => fk.DependentColumns.Contains(column) && fk.DependentColumns.All((EdmProperty c) => toTable.Properties.Any((EdmProperty nc) => string.Equals(nc.Name, c.Name, StringComparison.Ordinal) || string.Equals(nc.GetUnpreferredUniqueName(), c.Name, StringComparison.Ordinal))));
	}

	public static void CopyAllForeignKeyConstraintsForColumn(EdmModel database, EntityType fromTable, EntityType toTable, EdmProperty column, EdmProperty movedColumn)
	{
		FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column).ToArray().Each(delegate(ForeignKeyBuilder fk)
		{
			CopyForeignKeyConstraint(database, toTable, fk, (EdmProperty c) => (c != column) ? c : movedColumn);
		});
	}

	public static void MoveAllDeclaredForeignKeyConstraintsForPrimaryKeyColumns(EntityType entityType, EntityType fromTable, EntityType toTable)
	{
		foreach (EdmProperty keyProperty in fromTable.KeyProperties)
		{
			FindAllForeignKeyConstraintsForColumn(fromTable, toTable, keyProperty).ToArray().Each(delegate(ForeignKeyBuilder fk)
			{
				AssociationType associationType = fk.GetAssociationType();
				if (associationType != null && associationType.Constraint.ToRole.GetEntityType() == entityType && !fk.GetIsTypeConstraint())
				{
					MoveForeignKeyConstraint(fromTable, toTable, fk);
				}
			});
		}
	}

	public static void CopyAllForeignKeyConstraintsForPrimaryKeyColumns(EdmModel database, EntityType fromTable, EntityType toTable)
	{
		foreach (EdmProperty keyProperty in fromTable.KeyProperties)
		{
			FindAllForeignKeyConstraintsForColumn(fromTable, toTable, keyProperty).ToArray().Each(delegate(ForeignKeyBuilder fk)
			{
				if (!fk.GetIsTypeConstraint())
				{
					CopyForeignKeyConstraint(database, toTable, fk);
				}
			});
		}
	}

	public static void MoveAllForeignKeyConstraintsForColumn(EntityType fromTable, EntityType toTable, EdmProperty column)
	{
		FindAllForeignKeyConstraintsForColumn(fromTable, toTable, column).ToArray().Each(delegate(ForeignKeyBuilder fk)
		{
			MoveForeignKeyConstraint(fromTable, toTable, fk);
		});
	}

	public static void RemoveAllForeignKeyConstraintsForColumn(EntityType table, EdmProperty column, DbDatabaseMapping databaseMapping)
	{
		table.ForeignKeyBuilders.Where((ForeignKeyBuilder fk) => fk.DependentColumns.Contains(column)).ToArray().Each(delegate(ForeignKeyBuilder fk)
		{
			table.RemoveForeignKey(fk);
			ForeignKeyBuilder foreignKeyBuilder = databaseMapping.Database.EntityTypes.SelectMany((EntityType t) => t.ForeignKeyBuilders).SingleOrDefault((ForeignKeyBuilder fk2) => object.Equals(fk2.GetPreferredName(), fk.Name));
			if (foreignKeyBuilder != null)
			{
				foreignKeyBuilder.Name = foreignKeyBuilder.GetPreferredName();
			}
		});
	}
}
