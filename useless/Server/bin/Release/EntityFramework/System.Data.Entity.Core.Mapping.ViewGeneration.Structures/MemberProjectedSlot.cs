using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class MemberProjectedSlot : ProjectedSlot
{
	private readonly MemberPath m_memberPath;

	internal MemberPath MemberPath => m_memberPath;

	internal MemberProjectedSlot(MemberPath node)
	{
		m_memberPath = node;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
	{
		if (NeedToCastCqlValue(outputMember, out var outputMemberTypeUsage))
		{
			builder.Append("CAST(");
			m_memberPath.AsEsql(builder, blockAlias);
			builder.Append(" AS ");
			CqlWriter.AppendEscapedTypeName(builder, outputMemberTypeUsage.EdmType);
			builder.Append(')');
		}
		else
		{
			m_memberPath.AsEsql(builder, blockAlias);
		}
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		DbExpression dbExpression = m_memberPath.AsCqt(row);
		if (NeedToCastCqlValue(outputMember, out var outputMemberTypeUsage))
		{
			dbExpression = dbExpression.CastTo(outputMemberTypeUsage);
		}
		return dbExpression;
	}

	private bool NeedToCastCqlValue(MemberPath outputMember, out TypeUsage outputMemberTypeUsage)
	{
		TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(m_memberPath.LeafEdmMember);
		outputMemberTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
		return !modelTypeUsage.EdmType.Equals(outputMemberTypeUsage.EdmType);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_memberPath.ToCompactString(builder);
	}

	internal string ToUserString()
	{
		return m_memberPath.PathToString(false);
	}

	protected override bool IsEqualTo(ProjectedSlot right)
	{
		if (!(right is MemberProjectedSlot memberProjectedSlot))
		{
			return false;
		}
		return MemberPath.EqualityComparer.Equals(m_memberPath, memberProjectedSlot.m_memberPath);
	}

	protected override int GetHash()
	{
		return MemberPath.EqualityComparer.GetHashCode(m_memberPath);
	}

	internal MemberProjectedSlot RemapSlot(Dictionary<MemberPath, MemberPath> remap)
	{
		MemberPath value = null;
		if (remap.TryGetValue(MemberPath, out value))
		{
			return new MemberProjectedSlot(value);
		}
		return new MemberProjectedSlot(MemberPath);
	}

	internal static List<MemberProjectedSlot> GetKeySlots(IEnumerable<MemberProjectedSlot> slots, MemberPath prefix)
	{
		EntitySet entitySet = prefix.EntitySet;
		List<ExtentKey> keysForEntityType = ExtentKey.GetKeysForEntityType(prefix, entitySet.ElementType);
		return GetSlots(slots, keysForEntityType[0].KeyFields);
	}

	internal static List<MemberProjectedSlot> GetSlots(IEnumerable<MemberProjectedSlot> slots, IEnumerable<MemberPath> members)
	{
		List<MemberProjectedSlot> list = new List<MemberProjectedSlot>();
		foreach (MemberPath member in members)
		{
			MemberProjectedSlot slotForMember = GetSlotForMember(Helpers.AsSuperTypeList<MemberProjectedSlot, ProjectedSlot>(slots), member);
			if (slotForMember == null)
			{
				return null;
			}
			list.Add(slotForMember);
		}
		return list;
	}

	internal static MemberProjectedSlot GetSlotForMember(IEnumerable<ProjectedSlot> slots, MemberPath member)
	{
		foreach (MemberProjectedSlot slot in slots)
		{
			if (MemberPath.EqualityComparer.Equals(slot.MemberPath, member))
			{
				return slot;
			}
		}
		return null;
	}
}
