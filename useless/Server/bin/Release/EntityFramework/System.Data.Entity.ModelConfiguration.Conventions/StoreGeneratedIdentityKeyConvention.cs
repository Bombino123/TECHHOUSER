using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class StoreGeneratedIdentityKeyConvention : IConceptualModelConvention<EntityType>, IConvention
{
	private static readonly IEnumerable<PrimitiveTypeKind> _applicableTypes = new PrimitiveTypeKind[3]
	{
		PrimitiveTypeKind.Int16,
		PrimitiveTypeKind.Int32,
		PrimitiveTypeKind.Int64
	};

	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.BaseType == null && item.KeyProperties.Count == 1 && !(from p in item.DeclaredProperties
			let sgp = p.GetStoreGeneratedPattern()
			where sgp.HasValue && sgp == StoreGeneratedPattern.Identity
			select sgp).Any())
		{
			EdmProperty property = item.KeyProperties.Single();
			if (!property.GetStoreGeneratedPattern().HasValue && property.PrimitiveType != null && _applicableTypes.Contains(property.PrimitiveType.PrimitiveTypeKind) && !model.ConceptualModel.AssociationTypes.Any((AssociationType a) => IsNonTableSplittingForeignKey(a, property)) && !ParentOfTpc(item, model.ConceptualModel))
			{
				property.SetStoreGeneratedPattern(StoreGeneratedPattern.Identity);
			}
		}
	}

	private static bool IsNonTableSplittingForeignKey(AssociationType association, EdmProperty property)
	{
		if (association.Constraint != null && association.Constraint.ToProperties.Contains(property))
		{
			EntityTypeConfiguration entityTypeConfiguration = (EntityTypeConfiguration)association.SourceEnd.GetEntityType().GetConfiguration();
			EntityTypeConfiguration entityTypeConfiguration2 = (EntityTypeConfiguration)association.TargetEnd.GetEntityType().GetConfiguration();
			if (entityTypeConfiguration != null && entityTypeConfiguration2 != null && entityTypeConfiguration.GetTableName() != null && entityTypeConfiguration2.GetTableName() != null)
			{
				return !entityTypeConfiguration.GetTableName().Equals(entityTypeConfiguration2.GetTableName());
			}
			return true;
		}
		return false;
	}

	private static bool ParentOfTpc(EntityType entityType, EdmModel model)
	{
		return (from et in model.EntityTypes
			where et.GetRootType() == entityType
			select et into e
			let configuration = e.GetConfiguration() as EntityTypeConfiguration
			where configuration != null && configuration.IsMappingAnyInheritedProperty(e)
			select e).Any();
	}
}
