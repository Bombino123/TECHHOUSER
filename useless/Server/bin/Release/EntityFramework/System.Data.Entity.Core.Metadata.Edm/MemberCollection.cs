using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class MemberCollection : MetadataCollection<EdmMember>
{
	private readonly StructuralType _declaringType;

	public override ReadOnlyCollection<EdmMember> AsReadOnly => new ReadOnlyCollection<EdmMember>(this);

	public override int Count => GetBaseTypeMemberCount() + base.Count;

	public override EdmMember this[int index]
	{
		get
		{
			int relativeIndex = GetRelativeIndex(index);
			if (relativeIndex < 0)
			{
				return ((StructuralType)_declaringType.BaseType).Members[index];
			}
			return base[relativeIndex];
		}
		set
		{
			int relativeIndex = GetRelativeIndex(index);
			if (relativeIndex < 0)
			{
				((StructuralType)_declaringType.BaseType).Members.Source[index] = value;
			}
			else
			{
				base[relativeIndex] = value;
			}
		}
	}

	public MemberCollection(StructuralType declaringType)
		: this(declaringType, null)
	{
	}

	public MemberCollection(StructuralType declaringType, IEnumerable<EdmMember> items)
		: base(items)
	{
		_declaringType = declaringType;
	}

	public override void Add(EdmMember member)
	{
		ValidateMemberForAdd(member, "member");
		base.Add(member);
		member.ChangeDeclaringTypeWithoutCollectionFixup(_declaringType);
	}

	public override bool ContainsIdentity(string identity)
	{
		if (base.ContainsIdentity(identity))
		{
			return true;
		}
		EdmType baseType = _declaringType.BaseType;
		if (baseType != null && ((StructuralType)baseType).Members.Contains(identity))
		{
			return true;
		}
		return false;
	}

	public override int IndexOf(EdmMember item)
	{
		int num = base.IndexOf(item);
		if (num != -1)
		{
			return num + GetBaseTypeMemberCount();
		}
		if (_declaringType.BaseType is StructuralType structuralType)
		{
			return structuralType.Members.IndexOf(item);
		}
		return -1;
	}

	public override void CopyTo(EdmMember[] array, int arrayIndex)
	{
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		int baseTypeMemberCount = GetBaseTypeMemberCount();
		if (base.Count + baseTypeMemberCount > array.Length - arrayIndex)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (baseTypeMemberCount > 0)
		{
			((StructuralType)_declaringType.BaseType).Members.CopyTo(array, arrayIndex);
		}
		base.CopyTo(array, arrayIndex + baseTypeMemberCount);
	}

	public override bool TryGetValue(string identity, bool ignoreCase, out EdmMember item)
	{
		if (!base.TryGetValue(identity, ignoreCase, out item))
		{
			EdmType baseType = _declaringType.BaseType;
			if (baseType != null)
			{
				((StructuralType)baseType).Members.TryGetValue(identity, ignoreCase, out item);
			}
		}
		return item != null;
	}

	internal ReadOnlyMetadataCollection<T> GetDeclaredOnlyMembers<T>() where T : EdmMember
	{
		MetadataCollection<T> metadataCollection = new MetadataCollection<T>();
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is T item)
			{
				metadataCollection.Add(item);
			}
		}
		return new ReadOnlyMetadataCollection<T>(metadataCollection);
	}

	private int GetBaseTypeMemberCount()
	{
		if (_declaringType.BaseType is StructuralType structuralType)
		{
			return structuralType.Members.Count;
		}
		return 0;
	}

	private int GetRelativeIndex(int index)
	{
		int baseTypeMemberCount = GetBaseTypeMemberCount();
		int count = base.Count;
		if (index < 0 || index >= baseTypeMemberCount + count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return index - baseTypeMemberCount;
	}

	private void ValidateMemberForAdd(EdmMember member, string argumentName)
	{
		Check.NotNull(member, argumentName);
		_declaringType.ValidateMemberForAdd(member);
	}
}
