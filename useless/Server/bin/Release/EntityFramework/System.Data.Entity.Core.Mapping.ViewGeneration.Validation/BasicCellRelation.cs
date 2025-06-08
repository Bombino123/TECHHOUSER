using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class BasicCellRelation : CellRelation
{
	private readonly CellQuery m_cellQuery;

	private readonly List<MemberProjectedSlot> m_slots;

	private readonly ViewCellRelation m_viewCellRelation;

	internal ViewCellRelation ViewCellRelation => m_viewCellRelation;

	internal BasicCellRelation(CellQuery cellQuery, ViewCellRelation viewCellRelation, IEnumerable<MemberProjectedSlot> slots)
		: base(viewCellRelation.CellNumber)
	{
		m_cellQuery = cellQuery;
		m_slots = new List<MemberProjectedSlot>(slots);
		m_viewCellRelation = viewCellRelation;
	}

	internal void PopulateKeyConstraints(SchemaConstraints<BasicKeyConstraint> constraints)
	{
		if (m_cellQuery.Extent is EntitySet)
		{
			PopulateKeyConstraintsForEntitySet(constraints);
		}
		else
		{
			PopulateKeyConstraintsForRelationshipSet(constraints);
		}
	}

	private void PopulateKeyConstraintsForEntitySet(SchemaConstraints<BasicKeyConstraint> constraints)
	{
		MemberPath prefix = new MemberPath(m_cellQuery.Extent);
		EntityType entityType = (EntityType)m_cellQuery.Extent.ElementType;
		List<ExtentKey> keysForEntityType = ExtentKey.GetKeysForEntityType(prefix, entityType);
		AddKeyConstraints(keysForEntityType, constraints);
	}

	private void PopulateKeyConstraintsForRelationshipSet(SchemaConstraints<BasicKeyConstraint> constraints)
	{
		AssociationSet associationSet = m_cellQuery.Extent as AssociationSet;
		Set<MemberPath> set = new Set<MemberPath>(MemberPath.EqualityComparer);
		bool flag = false;
		foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
		{
			AssociationEndMember correspondingAssociationEndMember = associationSetEnd.CorrespondingAssociationEndMember;
			List<ExtentKey> keysForEntityType = ExtentKey.GetKeysForEntityType(new MemberPath(associationSet, correspondingAssociationEndMember), associationSetEnd.EntitySet.ElementType);
			if (MetadataHelper.DoesEndFormKey(associationSet, correspondingAssociationEndMember))
			{
				AddKeyConstraints(keysForEntityType, constraints);
				flag = true;
			}
			set.AddRange(keysForEntityType[0].KeyFields);
		}
		if (!flag)
		{
			ExtentKey extentKey = new ExtentKey(set);
			ExtentKey[] keys = new ExtentKey[1] { extentKey };
			AddKeyConstraints(keys, constraints);
		}
	}

	private void AddKeyConstraints(IEnumerable<ExtentKey> keys, SchemaConstraints<BasicKeyConstraint> constraints)
	{
		foreach (ExtentKey key in keys)
		{
			List<MemberProjectedSlot> slots = MemberProjectedSlot.GetSlots(m_slots, key.KeyFields);
			if (slots != null)
			{
				BasicKeyConstraint constraint = new BasicKeyConstraint(this, slots);
				constraints.Add(constraint);
			}
		}
	}

	protected override int GetHash()
	{
		return m_cellQuery.GetHashCode();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("BasicRel: ");
		StringUtil.FormatStringBuilder(builder, "{0}", m_slots[0]);
	}
}
