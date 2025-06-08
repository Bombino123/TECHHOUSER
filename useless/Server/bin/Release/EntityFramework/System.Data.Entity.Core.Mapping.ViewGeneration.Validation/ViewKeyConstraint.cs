using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ViewKeyConstraint : KeyConstraint<ViewCellRelation, ViewCellSlot>
{
	internal Cell Cell => base.CellRelation.Cell;

	internal ViewKeyConstraint(ViewCellRelation relation, IEnumerable<ViewCellSlot> keySlots)
		: base(relation, keySlots, (IEqualityComparer<ViewCellSlot>)ProjectedSlot.EqualityComparer)
	{
	}

	internal bool Implies(ViewKeyConstraint second)
	{
		if (base.CellRelation != second.CellRelation)
		{
			return false;
		}
		if (base.KeySlots.IsSubsetOf(second.KeySlots))
		{
			return true;
		}
		Set<ViewCellSlot> set = new Set<ViewCellSlot>(second.KeySlots);
		foreach (ViewCellSlot keySlot in base.KeySlots)
		{
			bool flag = false;
			foreach (ViewCellSlot item in set)
			{
				if (ProjectedSlot.EqualityComparer.Equals(keySlot.SSlot, item.SSlot))
				{
					MemberPath memberPath = keySlot.CSlot.MemberPath;
					MemberPath memberPath2 = item.CSlot.MemberPath;
					if (MemberPath.EqualityComparer.Equals(memberPath, memberPath2) || memberPath.IsEquivalentViaRefConstraint(memberPath2))
					{
						set.Remove(item);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	internal static ErrorLog.Record GetErrorRecord(ViewKeyConstraint rightKeyConstraint)
	{
		List<ViewCellSlot> list = new List<ViewCellSlot>(rightKeyConstraint.KeySlots);
		EntitySetBase extent = list[0].SSlot.MemberPath.Extent;
		EntitySetBase extent2 = list[0].CSlot.MemberPath.Extent;
		MemberPath prefix = new MemberPath(extent);
		MemberPath prefix2 = new MemberPath(extent2);
		ExtentKey primaryKeyForEntityType = ExtentKey.GetPrimaryKeyForEntityType(prefix, (EntityType)extent.ElementType);
		ExtentKey extentKey = null;
		string message = Strings.ViewGen_KeyConstraint_Violation(p5: ((!(extent2 is EntitySet)) ? ExtentKey.GetKeyForRelationType(prefix2, (AssociationType)extent2.ElementType) : ExtentKey.GetPrimaryKeyForEntityType(prefix2, (EntityType)extent2.ElementType)).ToUserString(), p0: extent.Name, p1: ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, isFromCside: false), p2: primaryKeyForEntityType.ToUserString(), p3: extent2.Name, p4: ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, isFromCside: true));
		string debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", rightKeyConstraint);
		return new ErrorLog.Record(ViewGenErrorCode.KeyConstraintViolation, message, rightKeyConstraint.CellRelation.Cell, debugMessage);
	}

	internal static ErrorLog.Record GetErrorRecord(IEnumerable<ViewKeyConstraint> rightKeyConstraints)
	{
		ViewKeyConstraint viewKeyConstraint = null;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (ViewKeyConstraint rightKeyConstraint in rightKeyConstraints)
		{
			string value = ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, isFromCside: true);
			if (!flag)
			{
				stringBuilder.Append("; ");
			}
			flag = false;
			stringBuilder.Append(value);
			viewKeyConstraint = rightKeyConstraint;
		}
		List<ViewCellSlot> list = new List<ViewCellSlot>(viewKeyConstraint.KeySlots);
		EntitySetBase extent = list[0].SSlot.MemberPath.Extent;
		EntitySetBase extent2 = list[0].CSlot.MemberPath.Extent;
		ExtentKey primaryKeyForEntityType = ExtentKey.GetPrimaryKeyForEntityType(new MemberPath(extent), (EntityType)extent.ElementType);
		string message;
		if (extent2 is EntitySet)
		{
			message = Strings.ViewGen_KeyConstraint_Update_Violation_EntitySet(stringBuilder.ToString(), extent2.Name, primaryKeyForEntityType.ToUserString(), extent.Name);
		}
		else
		{
			AssociationEndMember endThatShouldBeMappedToKey = Helper.GetEndThatShouldBeMappedToKey(((AssociationSet)extent2).ElementType);
			message = ((endThatShouldBeMappedToKey == null) ? Strings.ViewGen_KeyConstraint_Update_Violation_AssociationSet(extent2.Name, primaryKeyForEntityType.ToUserString(), extent.Name) : Strings.ViewGen_AssociationEndShouldBeMappedToKey(endThatShouldBeMappedToKey.Name, extent.Name));
		}
		string debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", viewKeyConstraint);
		return new ErrorLog.Record(ViewGenErrorCode.KeyConstraintUpdateViolation, message, viewKeyConstraint.CellRelation.Cell, debugMessage);
	}
}
