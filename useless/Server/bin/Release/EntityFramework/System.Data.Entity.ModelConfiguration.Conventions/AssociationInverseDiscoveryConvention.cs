using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class AssociationInverseDiscoveryConvention : IConceptualModelConvention<EdmModel>, IConvention
{
	public virtual void Apply(EdmModel item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		foreach (var item2 in from g in (from a1 in item.AssociationTypes
				from a2 in item.AssociationTypes
				where a1 != a2
				where a1.SourceEnd.GetEntityType() == a2.TargetEnd.GetEntityType() && a1.TargetEnd.GetEntityType() == a2.SourceEnd.GetEntityType()
				let a1Configuration = a1.GetConfiguration() as NavigationPropertyConfiguration
				let a2Configuration = a2.GetConfiguration() as NavigationPropertyConfiguration
				where (a1Configuration == null || (!a1Configuration.InverseEndKind.HasValue && a1Configuration.InverseNavigationProperty == null)) && (a2Configuration == null || (!a2Configuration.InverseEndKind.HasValue && a2Configuration.InverseNavigationProperty == null))
				select new { a1, a2 }).Distinct((a, b) => a.a1 == b.a2 && a.a2 == b.a1).GroupBy((a, b) => a.a1.SourceEnd.GetEntityType() == b.a2.TargetEnd.GetEntityType() && a.a1.TargetEnd.GetEntityType() == b.a2.SourceEnd.GetEntityType())
			where g.Count() == 1
			select g.Single())
		{
			AssociationType associationType = ((item2.a2.GetConfiguration() != null) ? item2.a2 : item2.a1);
			AssociationType associationType2 = ((associationType == item2.a1) ? item2.a2 : item2.a1);
			associationType.SourceEnd.RelationshipMultiplicity = associationType2.TargetEnd.RelationshipMultiplicity;
			if (associationType2.Constraint != null)
			{
				associationType.Constraint = associationType2.Constraint;
				associationType.Constraint.FromRole = associationType.SourceEnd;
				associationType.Constraint.ToRole = associationType.TargetEnd;
			}
			PropertyInfo clrPropertyInfo = associationType2.SourceEnd.GetClrPropertyInfo();
			if (clrPropertyInfo != null)
			{
				associationType.TargetEnd.SetClrPropertyInfo(clrPropertyInfo);
			}
			FixNavigationProperties(item, associationType, associationType2);
			item.RemoveAssociationType(associationType2);
		}
	}

	private static void FixNavigationProperties(EdmModel model, AssociationType unifiedAssociation, AssociationType redundantAssociation)
	{
		foreach (NavigationProperty item in from np in model.EntityTypes.SelectMany((EntityType e) => e.NavigationProperties)
			where np.Association == redundantAssociation
			select np)
		{
			item.RelationshipType = unifiedAssociation;
			item.FromEndMember = unifiedAssociation.TargetEnd;
			item.ToEndMember = unifiedAssociation.SourceEnd;
		}
	}
}
