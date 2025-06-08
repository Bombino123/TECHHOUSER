namespace System.Data.Entity.Internal;

internal abstract class MemberEntryMetadata
{
	private readonly Type _declaringType;

	private readonly Type _elementType;

	private readonly string _memberName;

	public abstract MemberEntryType MemberEntryType { get; }

	public string MemberName => _memberName;

	public Type DeclaringType => _declaringType;

	public Type ElementType => _elementType;

	public abstract Type MemberType { get; }

	protected MemberEntryMetadata(Type declaringType, Type elementType, string memberName)
	{
		_declaringType = declaringType;
		_elementType = elementType;
		_memberName = memberName;
	}

	public abstract InternalMemberEntry CreateMemberEntry(InternalEntityEntry internalEntityEntry, InternalPropertyEntry parentPropertyEntry);
}
