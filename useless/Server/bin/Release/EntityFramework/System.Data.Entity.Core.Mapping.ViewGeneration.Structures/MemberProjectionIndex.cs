using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class MemberProjectionIndex : InternalBase
{
	private readonly Dictionary<MemberPath, int> m_indexMap;

	private readonly List<MemberPath> m_members;

	internal int Count => m_members.Count;

	internal MemberPath this[int index] => m_members[index];

	internal IEnumerable<int> KeySlots
	{
		get
		{
			List<int> list = new List<int>();
			for (int i = 0; i < Count; i++)
			{
				if (IsKeySlot(i, 0))
				{
					list.Add(i);
				}
			}
			return list;
		}
	}

	internal IEnumerable<MemberPath> Members => m_members;

	internal static MemberProjectionIndex Create(EntitySetBase extent, EdmItemCollection edmItemCollection)
	{
		MemberProjectionIndex memberProjectionIndex = new MemberProjectionIndex();
		GatherPartialSignature(memberProjectionIndex, edmItemCollection, new MemberPath(extent), needKeysOnly: false);
		return memberProjectionIndex;
	}

	private MemberProjectionIndex()
	{
		m_indexMap = new Dictionary<MemberPath, int>(MemberPath.EqualityComparer);
		m_members = new List<MemberPath>();
	}

	internal int IndexOf(MemberPath member)
	{
		if (m_indexMap.TryGetValue(member, out var value))
		{
			return value;
		}
		return -1;
	}

	internal int CreateIndex(MemberPath member)
	{
		if (!m_indexMap.TryGetValue(member, out var value))
		{
			value = m_indexMap.Count;
			m_indexMap[member] = value;
			m_members.Add(member);
		}
		return value;
	}

	internal MemberPath GetMemberPath(int slotNum, int numBoolSlots)
	{
		if (!IsBoolSlot(slotNum, numBoolSlots))
		{
			return this[slotNum];
		}
		return null;
	}

	internal int BoolIndexToSlot(int boolIndex, int numBoolSlots)
	{
		return Count + boolIndex;
	}

	internal int SlotToBoolIndex(int slotNum, int numBoolSlots)
	{
		return slotNum - Count;
	}

	internal bool IsKeySlot(int slotNum, int numBoolSlots)
	{
		if (slotNum < Count)
		{
			return this[slotNum].IsPartOfKey;
		}
		return false;
	}

	internal bool IsBoolSlot(int slotNum, int numBoolSlots)
	{
		return slotNum >= Count;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append('<');
		StringUtil.ToCommaSeparatedString(builder, m_members);
		builder.Append('>');
	}

	private static void GatherPartialSignature(MemberProjectionIndex index, EdmItemCollection edmItemCollection, MemberPath member, bool needKeysOnly)
	{
		EdmType edmType = member.EdmType;
		if (edmType is ComplexType && needKeysOnly)
		{
			return;
		}
		index.CreateIndex(member);
		foreach (EdmType item in MetadataHelper.GetTypeAndSubtypesOf(edmType, edmItemCollection, includeAbstractTypes: false))
		{
			StructuralType possibleType = item as StructuralType;
			GatherSignatureFromTypeStructuralMembers(index, edmItemCollection, member, possibleType, needKeysOnly);
		}
	}

	private static void GatherSignatureFromTypeStructuralMembers(MemberProjectionIndex index, EdmItemCollection edmItemCollection, MemberPath member, StructuralType possibleType, bool needKeysOnly)
	{
		foreach (EdmMember allStructuralMember in Helper.GetAllStructuralMembers(possibleType))
		{
			if (MetadataHelper.IsNonRefSimpleMember(allStructuralMember))
			{
				if (!needKeysOnly || MetadataHelper.IsPartOfEntityTypeKey(allStructuralMember))
				{
					MemberPath member2 = new MemberPath(member, allStructuralMember);
					index.CreateIndex(member2);
				}
			}
			else
			{
				MemberPath member3 = new MemberPath(member, allStructuralMember);
				GatherPartialSignature(index, edmItemCollection, member3, needKeysOnly || Helper.IsAssociationEndMember(allStructuralMember));
			}
		}
	}
}
