using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ForeignKeyNavigationPropertyAttributeConvention : IConceptualModelConvention<NavigationProperty>, IConvention
{
	public virtual void Apply(NavigationProperty item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		AssociationType association = item.Association;
		if (association.Constraint != null)
		{
			return;
		}
		ForeignKeyAttribute foreignKeyAttribute = item.GetClrAttributes<ForeignKeyAttribute>().SingleOrDefault();
		if (foreignKeyAttribute == null || (!association.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd) && !association.IsPrincipalConfigured()))
		{
			return;
		}
		dependentEnd = dependentEnd ?? association.TargetEnd;
		principalEnd = principalEnd ?? association.SourceEnd;
		IEnumerable<string> dependentPropertyNames = from p in foreignKeyAttribute.Name.Split(new char[1] { ',' })
			select p.Trim();
		EntityType declaringEntityType = model.ConceptualModel.EntityTypes.Single((EntityType e) => e.DeclaredNavigationProperties.Contains(item));
		List<EdmProperty> toProperties = GetDependentProperties(dependentEnd.GetEntityType(), dependentPropertyNames, declaringEntityType, item).ToList();
		ReferentialConstraint constraint = new ReferentialConstraint(principalEnd, dependentEnd, principalEnd.GetEntityType().KeyProperties().ToList(), toProperties);
		IEnumerable<EdmProperty> source = dependentEnd.GetEntityType().KeyProperties();
		if (source.Count() == constraint.ToProperties.Count() && source.All((EdmProperty kp) => constraint.ToProperties.Contains(kp)))
		{
			principalEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
			if (dependentEnd.RelationshipMultiplicity.IsMany())
			{
				dependentEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
			}
		}
		if (principalEnd.IsRequired())
		{
			constraint.ToProperties.Each((EdmProperty p) => p.Nullable = false);
		}
		association.Constraint = constraint;
	}

	private static IEnumerable<EdmProperty> GetDependentProperties(EntityType dependentType, IEnumerable<string> dependentPropertyNames, EntityType declaringEntityType, NavigationProperty navigationProperty)
	{
		foreach (string dependentPropertyName in dependentPropertyNames)
		{
			if (string.IsNullOrWhiteSpace(dependentPropertyName))
			{
				throw Error.ForeignKeyAttributeConvention_EmptyKey(navigationProperty.Name, declaringEntityType.GetClrType());
			}
			EdmProperty edmProperty = dependentType.Properties.SingleOrDefault((EdmProperty p) => p.Name.Equals(dependentPropertyName, StringComparison.Ordinal));
			if (edmProperty == null)
			{
				throw Error.ForeignKeyAttributeConvention_InvalidKey(navigationProperty.Name, declaringEntityType.GetClrType(), dependentPropertyName, dependentType.GetClrType());
			}
			yield return edmProperty;
		}
	}
}
