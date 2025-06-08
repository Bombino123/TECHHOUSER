using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class AssociationMappingOperations
{
	private static void MoveAssociationSetMappingDependents(AssociationSetMapping associationSetMapping, EndPropertyMapping dependentMapping, EntitySet toSet, bool useExistingColumns)
	{
		EntityType toTable = toSet.ElementType;
		dependentMapping.PropertyMappings.Each(delegate(ScalarPropertyMapping pm)
		{
			EdmProperty oldColumn = pm.Column;
			pm.Column = TableOperations.MoveColumnAndAnyConstraints(associationSetMapping.Table, toTable, oldColumn, useExistingColumns);
			associationSetMapping.Conditions.Where((ConditionPropertyMapping cc) => cc.Column == oldColumn).Each((ConditionPropertyMapping cc) => cc.Column = pm.Column);
		});
		associationSetMapping.StoreEntitySet = toSet;
	}

	public static void MoveAllDeclaredAssociationSetMappings(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType fromTable, EntityType toTable, bool useExistingColumns)
	{
		AssociationSetMapping[] array = (from a in databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping asm) => asm.AssociationSetMappings)
			where a.Table == fromTable && (a.AssociationSet.ElementType.SourceEnd.GetEntityType() == entityType || a.AssociationSet.ElementType.TargetEnd.GetEntityType() == entityType)
			select a).ToArray();
		foreach (AssociationSetMapping associationSetMapping in array)
		{
			if (!associationSetMapping.AssociationSet.ElementType.TryGuessPrincipalAndDependentEnds(out var _, out var dependentEnd))
			{
				dependentEnd = associationSetMapping.AssociationSet.ElementType.TargetEnd;
			}
			if (dependentEnd.GetEntityType() != entityType)
			{
				continue;
			}
			EndPropertyMapping endPropertyMapping = ((dependentEnd == associationSetMapping.TargetEndMapping.AssociationEnd) ? associationSetMapping.SourceEndMapping : associationSetMapping.TargetEndMapping);
			MoveAssociationSetMappingDependents(associationSetMapping, endPropertyMapping, databaseMapping.Database.GetEntitySet(toTable), useExistingColumns);
			((endPropertyMapping == associationSetMapping.TargetEndMapping) ? associationSetMapping.SourceEndMapping : associationSetMapping.TargetEndMapping).PropertyMappings.Each(delegate(ScalarPropertyMapping pm)
			{
				if (pm.Column.DeclaringType != toTable)
				{
					pm.Column = toTable.Properties.Single((EdmProperty p) => string.Equals(p.GetPreferredName(), pm.Column.GetPreferredName(), StringComparison.Ordinal));
				}
			});
		}
	}
}
