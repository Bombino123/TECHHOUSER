using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public abstract class ForeignKeyDiscoveryConvention : IConceptualModelConvention<AssociationType>, IConvention
{
	protected virtual bool SupportsMultipleAssociations => false;

	protected abstract bool MatchDependentKeyProperty(AssociationType associationType, AssociationEndMember dependentAssociationEnd, EdmProperty dependentProperty, EntityType principalEntityType, EdmProperty principalKeyProperty);

	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.Constraint != null || item.IsIndependent() || (item.IsOneToOne() && item.IsSelfReferencing()) || !item.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd))
		{
			return;
		}
		IEnumerable<EdmProperty> source = principalEnd.GetEntityType().KeyProperties();
		if (!source.Any() || (!SupportsMultipleAssociations && model.ConceptualModel.GetAssociationTypesBetween(principalEnd.GetEntityType(), dependentEnd.GetEntityType()).Count() > 1))
		{
			return;
		}
		IEnumerable<EdmProperty> enumerable = from p in source
			from d in dependentEnd.GetEntityType().DeclaredProperties
			where MatchDependentKeyProperty(item, dependentEnd, d, principalEnd.GetEntityType(), p) && p.UnderlyingPrimitiveType == d.UnderlyingPrimitiveType
			select d;
		if (!enumerable.Any() || enumerable.Count() != source.Count())
		{
			return;
		}
		IEnumerable<EdmProperty> source2 = dependentEnd.GetEntityType().KeyProperties();
		bool flag = source2.Count() == enumerable.Count() && source2.All(enumerable.Contains<EdmProperty>);
		if (((dependentEnd.IsMany() || item.IsSelfReferencing()) && flag) || (!dependentEnd.IsMany() && !flag))
		{
			return;
		}
		ReferentialConstraint referentialConstraint = new ReferentialConstraint(principalEnd, dependentEnd, source.ToList(), enumerable.ToList());
		item.Constraint = referentialConstraint;
		if (principalEnd.IsRequired())
		{
			referentialConstraint.ToProperties.Each((EdmProperty p) => p.Nullable = false);
		}
	}
}
