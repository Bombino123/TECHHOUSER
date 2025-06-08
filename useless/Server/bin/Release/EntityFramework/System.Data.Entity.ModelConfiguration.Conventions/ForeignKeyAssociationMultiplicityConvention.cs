using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ForeignKeyAssociationMultiplicityConvention : IConceptualModelConvention<AssociationType>, IConvention
{
	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		ReferentialConstraint constraint = item.Constraint;
		if (constraint == null)
		{
			return;
		}
		NavigationPropertyConfiguration navigationPropertyConfiguration = item.Annotations.GetConfiguration() as NavigationPropertyConfiguration;
		if (constraint.ToProperties.All((EdmProperty p) => !p.Nullable))
		{
			AssociationEndMember principalEnd = item.GetOtherEnd(constraint.DependentEnd);
			NavigationProperty navigationProperty = model.ConceptualModel.EntityTypes.SelectMany((EntityType et) => et.DeclaredNavigationProperties).SingleOrDefault((NavigationProperty np) => np.ResultEnd == principalEnd);
			PropertyInfo clrPropertyInfo;
			if (navigationPropertyConfiguration == null || navigationProperty == null || !((clrPropertyInfo = navigationProperty.Annotations.GetClrPropertyInfo()) != null) || ((!(clrPropertyInfo == navigationPropertyConfiguration.NavigationProperty) || !navigationPropertyConfiguration.RelationshipMultiplicity.HasValue) && (!(clrPropertyInfo == navigationPropertyConfiguration.InverseNavigationProperty) || !navigationPropertyConfiguration.InverseEndKind.HasValue)))
			{
				principalEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
			}
		}
	}
}
