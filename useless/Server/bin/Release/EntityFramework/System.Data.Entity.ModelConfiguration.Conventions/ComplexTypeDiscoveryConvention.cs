using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ComplexTypeDiscoveryConvention : IConceptualModelConvention<EdmModel>, IConvention
{
	public virtual void Apply(EdmModel item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		foreach (var item2 in (from entityType in item.EntityTypes
			where entityType.KeyProperties.Count == 0 && entityType.BaseType == null
			let entityTypeConfiguration = entityType.GetConfiguration() as EntityTypeConfiguration
			where (entityTypeConfiguration == null || (!entityTypeConfiguration.IsExplicitEntity && entityTypeConfiguration.IsStructuralConfigurationOnly)) && !entityType.Members.Where(Helper.IsNavigationProperty).Any()
			let matchingAssociations = from associationType in item.AssociationTypes
				where associationType.SourceEnd.GetEntityType() == entityType || associationType.TargetEnd.GetEntityType() == entityType
				let declaringEnd = (associationType.SourceEnd.GetEntityType() == entityType) ? associationType.SourceEnd : associationType.TargetEnd
				let declaringEntity = associationType.GetOtherEnd(declaringEnd).GetEntityType()
				let navigationProperties = from NavigationProperty n in declaringEntity.Members.Where(Helper.IsNavigationProperty)
					where n.ResultEnd.GetEntityType() == entityType
					select n
				select new
				{
					DeclaringEnd = declaringEnd,
					AssociationType = associationType,
					DeclaringEntityType = declaringEntity,
					NavigationProperties = navigationProperties.ToList()
				}
			where matchingAssociations.All(a => a.AssociationType.Constraint == null && a.AssociationType.GetConfiguration() == null && !a.AssociationType.IsSelfReferencing() && a.DeclaringEnd.IsOptional() && a.NavigationProperties.All((NavigationProperty n) => n.GetConfiguration() == null))
			select new
			{
				EntityType = entityType,
				MatchingAssociations = matchingAssociations.ToList()
			}).ToList())
		{
			ComplexType complexType = item.AddComplexType(item2.EntityType.Name, item2.EntityType.NamespaceName);
			foreach (EdmProperty declaredProperty in item2.EntityType.DeclaredProperties)
			{
				complexType.AddMember(declaredProperty);
			}
			foreach (MetadataProperty annotation in item2.EntityType.Annotations)
			{
				complexType.GetMetadataProperties().Add(annotation);
			}
			foreach (var matchingAssociation in item2.MatchingAssociations)
			{
				foreach (NavigationProperty navigationProperty in matchingAssociation.NavigationProperties)
				{
					if (!matchingAssociation.DeclaringEntityType.Members.Where(Helper.IsNavigationProperty).Contains(navigationProperty))
					{
						continue;
					}
					matchingAssociation.DeclaringEntityType.RemoveMember(navigationProperty);
					EdmProperty edmProperty = matchingAssociation.DeclaringEntityType.AddComplexProperty(navigationProperty.Name, complexType);
					foreach (MetadataProperty annotation2 in navigationProperty.Annotations)
					{
						edmProperty.GetMetadataProperties().Add(annotation2);
					}
				}
				item.RemoveAssociationType(matchingAssociation.AssociationType);
			}
			item.RemoveEntityType(item2.EntityType);
		}
	}
}
