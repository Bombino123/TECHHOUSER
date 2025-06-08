namespace System.Data.Entity.Internal;

internal class NavigationEntryMetadata : MemberEntryMetadata
{
	private readonly bool _isCollection;

	public override MemberEntryType MemberEntryType
	{
		get
		{
			if (!_isCollection)
			{
				return MemberEntryType.ReferenceNavigationProperty;
			}
			return MemberEntryType.CollectionNavigationProperty;
		}
	}

	public override Type MemberType
	{
		get
		{
			if (!_isCollection)
			{
				return base.ElementType;
			}
			return DbHelpers.CollectionType(base.ElementType);
		}
	}

	public NavigationEntryMetadata(Type declaringType, Type propertyType, string propertyName, bool isCollection)
		: base(declaringType, propertyType, propertyName)
	{
		_isCollection = isCollection;
	}

	public override InternalMemberEntry CreateMemberEntry(InternalEntityEntry internalEntityEntry, InternalPropertyEntry parentPropertyEntry)
	{
		if (!_isCollection)
		{
			return new InternalReferenceEntry(internalEntityEntry, this);
		}
		return new InternalCollectionEntry(internalEntityEntry, this);
	}
}
