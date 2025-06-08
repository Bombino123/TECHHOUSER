using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class LazyLoadBehavior
{
	internal static Func<TProxy, TItem, bool> GetInterceptorDelegate<TProxy, TItem>(EdmMember member, Func<object, object> getEntityWrapperDelegate) where TProxy : class where TItem : class
	{
		Func<TProxy, TItem, bool> result = (TProxy proxy, TItem item) => true;
		if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
		{
			NavigationProperty navProperty = (NavigationProperty)member;
			result = ((navProperty.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many) ? ((Func<TProxy, TItem, bool>)((TProxy proxy, TItem item) => LoadProperty(item, navProperty.RelationshipType.Identity, navProperty.ToEndMember.Identity, mustBeNull: true, getEntityWrapperDelegate(proxy)))) : ((Func<TProxy, TItem, bool>)((TProxy proxy, TItem item) => LoadProperty(item, navProperty.RelationshipType.Identity, navProperty.ToEndMember.Identity, mustBeNull: false, getEntityWrapperDelegate(proxy)))));
		}
		return result;
	}

	internal static bool IsLazyLoadCandidate(EntityType ospaceEntityType, EdmMember member)
	{
		bool result = false;
		if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
		{
			RelationshipMultiplicity relationshipMultiplicity = ((NavigationProperty)member).ToEndMember.RelationshipMultiplicity;
			Type propertyType = ospaceEntityType.ClrType.GetTopProperty(member.Name).PropertyType;
			switch (relationshipMultiplicity)
			{
			case RelationshipMultiplicity.Many:
				result = propertyType.TryGetElementType(typeof(ICollection<>)) != null;
				break;
			case RelationshipMultiplicity.ZeroOrOne:
			case RelationshipMultiplicity.One:
				result = true;
				break;
			}
		}
		return result;
	}

	private static bool LoadProperty<TItem>(TItem propertyValue, string relationshipName, string targetRoleName, bool mustBeNull, object wrapperObject) where TItem : class
	{
		IEntityWrapper entityWrapper = (IEntityWrapper)wrapperObject;
		if (entityWrapper != null && entityWrapper.Context != null)
		{
			RelationshipManager relationshipManager = entityWrapper.RelationshipManager;
			if (relationshipManager != null && (!mustBeNull || propertyValue == null))
			{
				relationshipManager.GetRelatedEndInternal(relationshipName, targetRoleName).DeferredLoad();
			}
		}
		return propertyValue != null;
	}
}
