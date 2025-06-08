using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class StructuralType : EdmType
{
	private readonly MemberCollection _members;

	private readonly ReadOnlyMetadataCollection<EdmMember> _readOnlyMembers;

	[MetadataProperty(BuiltInTypeKind.EdmMember, true)]
	public ReadOnlyMetadataCollection<EdmMember> Members => _readOnlyMembers;

	internal StructuralType()
	{
		_members = new MemberCollection(this);
		_readOnlyMembers = _members.AsReadOnlyMetadataCollection();
	}

	internal StructuralType(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
		_members = new MemberCollection(this);
		_readOnlyMembers = _members.AsReadOnlyMetadataCollection();
	}

	internal ReadOnlyMetadataCollection<T> GetDeclaredOnlyMembers<T>() where T : EdmMember
	{
		return _members.GetDeclaredOnlyMembers<T>();
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			Members.Source.SetReadOnly();
		}
	}

	internal abstract void ValidateMemberForAdd(EdmMember member);

	public void AddMember(EdmMember member)
	{
		AddMember(member, forceAdd: false);
	}

	internal void AddMember(EdmMember member, bool forceAdd)
	{
		Check.NotNull(member, "member");
		if (!forceAdd)
		{
			Util.ThrowIfReadOnly(this);
		}
		if (DataSpace != member.TypeUsage.EdmType.DataSpace && BuiltInTypeKind != BuiltInTypeKind.RowType)
		{
			throw new ArgumentException(Strings.AttemptToAddEdmMemberFromWrongDataSpace(member.Name, Name, member.TypeUsage.EdmType.DataSpace, DataSpace), "member");
		}
		if (BuiltInTypeKind.RowType == BuiltInTypeKind)
		{
			if (_members.Count == 0)
			{
				DataSpace = member.TypeUsage.EdmType.DataSpace;
			}
			else if (DataSpace != (DataSpace)(-1) && member.TypeUsage.EdmType.DataSpace != DataSpace)
			{
				DataSpace = (DataSpace)(-1);
			}
		}
		if (_members.IsReadOnly && forceAdd)
		{
			_members.ResetReadOnly();
			_members.Add(member);
			_members.SetReadOnly();
		}
		else
		{
			_members.Add(member);
		}
	}

	public virtual void RemoveMember(EdmMember member)
	{
		Check.NotNull(member, "member");
		Util.ThrowIfReadOnly(this);
		_members.Remove(member);
	}

	internal virtual bool HasMember(EdmMember member)
	{
		return _members.Contains(member);
	}

	internal virtual void NotifyItemIdentityChanged(EdmMember item, string initialIdentity)
	{
		_members.HandleIdentityChange(item, initialIdentity);
	}
}
