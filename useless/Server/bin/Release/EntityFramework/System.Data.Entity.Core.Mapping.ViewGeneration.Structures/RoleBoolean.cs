using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class RoleBoolean : TrueFalseLiteral
{
	private readonly MetadataItem m_metadataItem;

	internal RoleBoolean(EntitySetBase extent)
	{
		m_metadataItem = extent;
	}

	internal RoleBoolean(AssociationSetEnd end)
	{
		m_metadataItem = end;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return null;
	}

	internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
	{
		return null;
	}

	internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		if (m_metadataItem is AssociationSetEnd associationSetEnd)
		{
			builder.Append(Strings.ViewGen_AssociationSet_AsUserString(blockAlias, associationSetEnd.Name, associationSetEnd.ParentAssociationSet));
		}
		else
		{
			builder.Append(Strings.ViewGen_EntitySet_AsUserString(blockAlias, m_metadataItem.ToString()));
		}
		return builder;
	}

	internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		if (m_metadataItem is AssociationSetEnd associationSetEnd)
		{
			builder.Append(Strings.ViewGen_AssociationSet_AsUserString_Negated(blockAlias, associationSetEnd.Name, associationSetEnd.ParentAssociationSet));
		}
		else
		{
			builder.Append(Strings.ViewGen_EntitySet_AsUserString_Negated(blockAlias, m_metadataItem.ToString()));
		}
		return builder;
	}

	internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
	{
		throw new NotImplementedException();
	}

	protected override bool IsEqualTo(BoolLiteral right)
	{
		if (!(right is RoleBoolean roleBoolean))
		{
			return false;
		}
		return m_metadataItem == roleBoolean.m_metadataItem;
	}

	public override int GetHashCode()
	{
		return m_metadataItem.GetHashCode();
	}

	internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
	{
		return this;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		if (m_metadataItem is AssociationSetEnd associationSetEnd)
		{
			builder.Append("InEnd:" + associationSetEnd.ParentAssociationSet?.ToString() + "_" + associationSetEnd.Name);
		}
		else
		{
			builder.Append("InSet:" + m_metadataItem);
		}
	}
}
