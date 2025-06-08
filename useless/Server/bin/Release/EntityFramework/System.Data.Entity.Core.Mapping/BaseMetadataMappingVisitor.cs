using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal abstract class BaseMetadataMappingVisitor
{
	internal static class IdentityHelper
	{
		public static string GetIdentity(EntitySetBaseMapping mapping)
		{
			return mapping.Set.Identity;
		}

		public static string GetIdentity(TypeMapping mapping)
		{
			if (mapping is EntityTypeMapping mapping2)
			{
				return GetIdentity(mapping2);
			}
			return GetIdentity((AssociationTypeMapping)mapping);
		}

		public static string GetIdentity(EntityTypeMapping mapping)
		{
			IOrderedEnumerable<string> first = mapping.Types.Select((EntityTypeBase it) => it.Identity).OrderBy<string, string>((string it) => it, StringComparer.Ordinal);
			IOrderedEnumerable<string> second = mapping.IsOfTypes.Select((EntityTypeBase it) => it.Identity).OrderBy<string, string>((string it) => it, StringComparer.Ordinal);
			return string.Join(",", first.Concat(second));
		}

		public static string GetIdentity(AssociationTypeMapping mapping)
		{
			return mapping.AssociationType.Identity;
		}

		public static string GetIdentity(ComplexTypeMapping mapping)
		{
			IOrderedEnumerable<string> first = mapping.AllProperties.Select((PropertyMapping it) => GetIdentity(it)).OrderBy<string, string>((string it) => it, StringComparer.Ordinal);
			IOrderedEnumerable<string> second = mapping.Types.Select((ComplexType it) => it.Identity).OrderBy<string, string>((string it) => it, StringComparer.Ordinal);
			IOrderedEnumerable<string> second2 = mapping.IsOfTypes.Select((ComplexType it) => it.Identity).OrderBy<string, string>((string it) => it, StringComparer.Ordinal);
			return string.Join(",", first.Concat(second).Concat(second2));
		}

		public static string GetIdentity(MappingFragment mapping)
		{
			return mapping.TableSet.Identity;
		}

		public static string GetIdentity(PropertyMapping mapping)
		{
			if (mapping is ScalarPropertyMapping mapping2)
			{
				return GetIdentity(mapping2);
			}
			if (mapping is ComplexPropertyMapping mapping3)
			{
				return GetIdentity(mapping3);
			}
			if (mapping is EndPropertyMapping mapping4)
			{
				return GetIdentity(mapping4);
			}
			return GetIdentity((ConditionPropertyMapping)mapping);
		}

		public static string GetIdentity(ScalarPropertyMapping mapping)
		{
			return "ScalarProperty(Identity=" + mapping.Property.Identity + ",ColumnIdentity=" + mapping.Column.Identity + ")";
		}

		public static string GetIdentity(ComplexPropertyMapping mapping)
		{
			return "ComplexProperty(Identity=" + mapping.Property.Identity + ")";
		}

		public static string GetIdentity(ConditionPropertyMapping mapping)
		{
			if (mapping.Property == null)
			{
				return "ConditionProperty(ColumnIdentity=" + mapping.Column.Identity + ")";
			}
			return "ConditionProperty(Identity=" + mapping.Property.Identity + ")";
		}

		public static string GetIdentity(EndPropertyMapping mapping)
		{
			return "EndProperty(Identity=" + mapping.AssociationEnd.Identity + ")";
		}
	}

	private readonly bool _sortSequence;

	protected BaseMetadataMappingVisitor(bool sortSequence)
	{
		_sortSequence = sortSequence;
	}

	protected virtual void Visit(EntityContainerMapping entityContainerMapping)
	{
		Visit(entityContainerMapping.EdmEntityContainer);
		Visit(entityContainerMapping.StorageEntityContainer);
		foreach (EntitySetBaseMapping item in GetSequence(entityContainerMapping.EntitySetMaps, (EntitySetBaseMapping it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(EntitySetBase entitySetBase)
	{
		switch (entitySetBase.BuiltInTypeKind)
		{
		case BuiltInTypeKind.EntitySet:
			Visit((EntitySet)entitySetBase);
			break;
		case BuiltInTypeKind.AssociationSet:
			Visit((AssociationSet)entitySetBase);
			break;
		}
	}

	protected virtual void Visit(EntitySetBaseMapping setMapping)
	{
		foreach (TypeMapping item in GetSequence(setMapping.TypeMappings, (TypeMapping it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item);
		}
		Visit(setMapping.EntityContainerMapping);
	}

	protected virtual void Visit(EntityContainer entityContainer)
	{
		foreach (EntitySetBase item in GetSequence(entityContainer.BaseEntitySets, (EntitySetBase it) => it.Identity))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(EntitySet entitySet)
	{
		Visit(entitySet.ElementType);
		Visit(entitySet.EntityContainer);
	}

	protected virtual void Visit(AssociationSet associationSet)
	{
		Visit(associationSet.ElementType);
		Visit(associationSet.EntityContainer);
		foreach (AssociationSetEnd item in GetSequence(associationSet.AssociationSetEnds, (AssociationSetEnd it) => it.Identity))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(EntityType entityType)
	{
		foreach (EdmMember item in GetSequence(entityType.KeyMembers, (EdmMember it) => it.Identity))
		{
			Visit(item);
		}
		foreach (EdmMember item2 in GetSequence(entityType.GetDeclaredOnlyMembers<EdmMember>(), (EdmMember it) => it.Identity))
		{
			Visit(item2);
		}
		foreach (NavigationProperty item3 in GetSequence(entityType.NavigationProperties, (NavigationProperty it) => it.Identity))
		{
			Visit(item3);
		}
		foreach (EdmProperty item4 in GetSequence(entityType.Properties, (EdmProperty it) => it.Identity))
		{
			Visit(item4);
		}
	}

	protected virtual void Visit(AssociationType associationType)
	{
		foreach (AssociationEndMember item in GetSequence(associationType.AssociationEndMembers, (AssociationEndMember it) => it.Identity))
		{
			Visit(item);
		}
		Visit(associationType.BaseType);
		foreach (EdmMember item2 in GetSequence(associationType.KeyMembers, (EdmMember it) => it.Identity))
		{
			Visit(item2);
		}
		foreach (EdmMember item3 in GetSequence(associationType.GetDeclaredOnlyMembers<EdmMember>(), (EdmMember it) => it.Identity))
		{
			Visit(item3);
		}
		foreach (ReferentialConstraint item4 in GetSequence(associationType.ReferentialConstraints, (ReferentialConstraint it) => it.Identity))
		{
			Visit(item4);
		}
		foreach (RelationshipEndMember item5 in GetSequence(associationType.RelationshipEndMembers, (RelationshipEndMember it) => it.Identity))
		{
			Visit(item5);
		}
	}

	protected virtual void Visit(AssociationSetEnd associationSetEnd)
	{
		Visit(associationSetEnd.CorrespondingAssociationEndMember);
		Visit(associationSetEnd.EntitySet);
		Visit(associationSetEnd.ParentAssociationSet);
	}

	protected virtual void Visit(EdmProperty edmProperty)
	{
		Visit(edmProperty.TypeUsage);
	}

	protected virtual void Visit(NavigationProperty navigationProperty)
	{
		Visit(navigationProperty.FromEndMember);
		Visit(navigationProperty.RelationshipType);
		Visit(navigationProperty.ToEndMember);
		Visit(navigationProperty.TypeUsage);
	}

	protected virtual void Visit(EdmMember edmMember)
	{
		Visit(edmMember.TypeUsage);
	}

	protected virtual void Visit(AssociationEndMember associationEndMember)
	{
		Visit(associationEndMember.TypeUsage);
	}

	protected virtual void Visit(ReferentialConstraint referentialConstraint)
	{
		foreach (EdmProperty item in GetSequence(referentialConstraint.FromProperties, (EdmProperty it) => it.Identity))
		{
			Visit(item);
		}
		Visit(referentialConstraint.FromRole);
		foreach (EdmProperty item2 in GetSequence(referentialConstraint.ToProperties, (EdmProperty it) => it.Identity))
		{
			Visit(item2);
		}
		Visit(referentialConstraint.ToRole);
	}

	protected virtual void Visit(RelationshipEndMember relationshipEndMember)
	{
		Visit(relationshipEndMember.TypeUsage);
	}

	protected virtual void Visit(TypeUsage typeUsage)
	{
		Visit(typeUsage.EdmType);
		foreach (Facet item in GetSequence(typeUsage.Facets, (Facet it) => it.Identity))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(RelationshipType relationshipType)
	{
		if (relationshipType != null && relationshipType.BuiltInTypeKind == BuiltInTypeKind.AssociationType)
		{
			Visit((AssociationType)relationshipType);
		}
	}

	protected virtual void Visit(EdmType edmType)
	{
		if (edmType != null)
		{
			switch (edmType.BuiltInTypeKind)
			{
			case BuiltInTypeKind.EntityType:
				Visit((EntityType)edmType);
				break;
			case BuiltInTypeKind.AssociationType:
				Visit((AssociationType)edmType);
				break;
			case BuiltInTypeKind.EdmFunction:
				Visit((EdmFunction)edmType);
				break;
			case BuiltInTypeKind.ComplexType:
				Visit((ComplexType)edmType);
				break;
			case BuiltInTypeKind.PrimitiveType:
				Visit((PrimitiveType)edmType);
				break;
			case BuiltInTypeKind.RefType:
				Visit((RefType)edmType);
				break;
			case BuiltInTypeKind.CollectionType:
				Visit((CollectionType)edmType);
				break;
			case BuiltInTypeKind.EnumType:
				Visit((EnumType)edmType);
				break;
			}
		}
	}

	protected virtual void Visit(Facet facet)
	{
		Visit(facet.FacetType);
	}

	protected virtual void Visit(EdmFunction edmFunction)
	{
		Visit(edmFunction.BaseType);
		foreach (EntitySet item in GetSequence(edmFunction.EntitySets, (EntitySet it) => it.Identity))
		{
			if (item != null)
			{
				Visit(item);
			}
		}
		foreach (FunctionParameter item2 in GetSequence(edmFunction.Parameters, (FunctionParameter it) => it.Identity))
		{
			Visit(item2);
		}
		foreach (FunctionParameter item3 in GetSequence(edmFunction.ReturnParameters, (FunctionParameter it) => it.Identity))
		{
			Visit(item3);
		}
	}

	protected virtual void Visit(PrimitiveType primitiveType)
	{
	}

	protected virtual void Visit(ComplexType complexType)
	{
		Visit(complexType.BaseType);
		foreach (EdmMember item in GetSequence(complexType.Members, (EdmMember it) => it.Identity))
		{
			Visit(item);
		}
		foreach (EdmProperty item2 in GetSequence(complexType.Properties, (EdmProperty it) => it.Identity))
		{
			Visit(item2);
		}
	}

	protected virtual void Visit(RefType refType)
	{
		Visit(refType.BaseType);
		Visit(refType.ElementType);
	}

	protected virtual void Visit(EnumType enumType)
	{
		foreach (EnumMember item in GetSequence(enumType.Members, (EnumMember it) => it.Identity))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(EnumMember enumMember)
	{
	}

	protected virtual void Visit(CollectionType collectionType)
	{
		Visit(collectionType.BaseType);
		Visit(collectionType.TypeUsage);
	}

	protected virtual void Visit(EntityTypeBase entityTypeBase)
	{
		if (entityTypeBase != null)
		{
			switch (entityTypeBase.BuiltInTypeKind)
			{
			case BuiltInTypeKind.AssociationType:
				Visit((AssociationType)entityTypeBase);
				break;
			case BuiltInTypeKind.EntityType:
				Visit((EntityType)entityTypeBase);
				break;
			}
		}
	}

	protected virtual void Visit(FunctionParameter functionParameter)
	{
		Visit(functionParameter.DeclaringFunction);
		Visit(functionParameter.TypeUsage);
	}

	protected virtual void Visit(DbProviderManifest providerManifest)
	{
	}

	protected virtual void Visit(TypeMapping typeMapping)
	{
		foreach (EntityTypeBase item in GetSequence(typeMapping.IsOfTypes, (EntityTypeBase it) => it.Identity))
		{
			Visit(item);
		}
		foreach (MappingFragment item2 in GetSequence(typeMapping.MappingFragments, (MappingFragment it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item2);
		}
		Visit(typeMapping.SetMapping);
		foreach (EntityTypeBase item3 in GetSequence(typeMapping.Types, (EntityTypeBase it) => it.Identity))
		{
			Visit(item3);
		}
	}

	protected virtual void Visit(MappingFragment mappingFragment)
	{
		foreach (PropertyMapping item in GetSequence(mappingFragment.AllProperties, (PropertyMapping it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item);
		}
		Visit((EntitySetBase)mappingFragment.TableSet);
	}

	protected virtual void Visit(PropertyMapping propertyMapping)
	{
		if (propertyMapping.GetType() == typeof(ComplexPropertyMapping))
		{
			Visit((ComplexPropertyMapping)propertyMapping);
		}
		else if (propertyMapping.GetType() == typeof(ConditionPropertyMapping))
		{
			Visit((ConditionPropertyMapping)propertyMapping);
		}
		else if (propertyMapping.GetType() == typeof(ScalarPropertyMapping))
		{
			Visit((ScalarPropertyMapping)propertyMapping);
		}
	}

	protected virtual void Visit(ComplexPropertyMapping complexPropertyMapping)
	{
		Visit(complexPropertyMapping.Property);
		foreach (ComplexTypeMapping item in GetSequence(complexPropertyMapping.TypeMappings, (ComplexTypeMapping it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item);
		}
	}

	protected virtual void Visit(ConditionPropertyMapping conditionPropertyMapping)
	{
		Visit(conditionPropertyMapping.Column);
		Visit(conditionPropertyMapping.Property);
	}

	protected virtual void Visit(ScalarPropertyMapping scalarPropertyMapping)
	{
		Visit(scalarPropertyMapping.Column);
		Visit(scalarPropertyMapping.Property);
	}

	protected virtual void Visit(ComplexTypeMapping complexTypeMapping)
	{
		foreach (PropertyMapping item in GetSequence(complexTypeMapping.AllProperties, (PropertyMapping it) => IdentityHelper.GetIdentity(it)))
		{
			Visit(item);
		}
		foreach (ComplexType item2 in GetSequence(complexTypeMapping.IsOfTypes, (ComplexType it) => it.Identity))
		{
			Visit(item2);
		}
		foreach (ComplexType item3 in GetSequence(complexTypeMapping.Types, (ComplexType it) => it.Identity))
		{
			Visit(item3);
		}
	}

	protected IEnumerable<T> GetSequence<T>(IEnumerable<T> sequence, Func<T, string> keySelector)
	{
		if (!_sortSequence)
		{
			return sequence;
		}
		return sequence.OrderBy<T, string>(keySelector, StringComparer.Ordinal);
	}
}
