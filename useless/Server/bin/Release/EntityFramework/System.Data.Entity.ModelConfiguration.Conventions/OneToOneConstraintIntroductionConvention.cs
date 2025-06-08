using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class OneToOneConstraintIntroductionConvention : IConceptualModelConvention<AssociationType>, IConvention
{
	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.IsOneToOne() && !item.IsSelfReferencing() && !item.IsIndependent() && item.Constraint == null)
		{
			IEnumerable<EdmProperty> source = item.SourceEnd.GetEntityType().KeyProperties();
			IEnumerable<EdmProperty> source2 = item.TargetEnd.GetEntityType().KeyProperties();
			if (source.Count() == source2.Count() && source.Select((EdmProperty p) => p.UnderlyingPrimitiveType).SequenceEqual(source2.Select((EdmProperty p) => p.UnderlyingPrimitiveType)) && (item.TryGuessPrincipalAndDependentEnds(out var _, out var dependentEnd) || item.IsPrincipalConfigured()))
			{
				dependentEnd = dependentEnd ?? item.TargetEnd;
				AssociationEndMember otherEnd = item.GetOtherEnd(dependentEnd);
				ReferentialConstraint constraint = new ReferentialConstraint(otherEnd, dependentEnd, otherEnd.GetEntityType().KeyProperties().ToList(), dependentEnd.GetEntityType().KeyProperties().ToList());
				item.Constraint = constraint;
			}
		}
	}
}
