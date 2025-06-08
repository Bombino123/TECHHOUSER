using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class ProjectedSlot : InternalBase, IEquatable<ProjectedSlot>
{
	private sealed class Comparer : IEqualityComparer<ProjectedSlot>
	{
		public bool Equals(ProjectedSlot left, ProjectedSlot right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return left.IsEqualTo(right);
		}

		public int GetHashCode(ProjectedSlot key)
		{
			return key.GetHash();
		}
	}

	internal static readonly IEqualityComparer<ProjectedSlot> EqualityComparer = new Comparer();

	protected virtual bool IsEqualTo(ProjectedSlot right)
	{
		return base.Equals(right);
	}

	protected virtual int GetHash()
	{
		return base.GetHashCode();
	}

	public bool Equals(ProjectedSlot right)
	{
		return EqualityComparer.Equals(this, right);
	}

	public override bool Equals(object obj)
	{
		ProjectedSlot right = obj as ProjectedSlot;
		if (obj == null)
		{
			return false;
		}
		return Equals(right);
	}

	public override int GetHashCode()
	{
		return EqualityComparer.GetHashCode(this);
	}

	internal virtual ProjectedSlot DeepQualify(CqlBlock block)
	{
		return new QualifiedSlot(block, this);
	}

	internal virtual string GetCqlFieldAlias(MemberPath outputMember)
	{
		return outputMember.CqlFieldAlias;
	}

	internal abstract StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel);

	internal abstract DbExpression AsCqt(DbExpression row, MemberPath outputMember);

	internal static bool TryMergeRemapSlots(ProjectedSlot[] slots1, ProjectedSlot[] slots2, out ProjectedSlot[] result)
	{
		if (!TryMergeSlots(slots1, slots2, out var slots3))
		{
			result = null;
			return false;
		}
		result = slots3;
		return true;
	}

	private static bool TryMergeSlots(ProjectedSlot[] slots1, ProjectedSlot[] slots2, out ProjectedSlot[] slots)
	{
		slots = new ProjectedSlot[slots1.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			ProjectedSlot projectedSlot = slots1[i];
			ProjectedSlot projectedSlot2 = slots2[i];
			if (projectedSlot == null)
			{
				slots[i] = projectedSlot2;
				continue;
			}
			if (projectedSlot2 == null)
			{
				slots[i] = projectedSlot;
				continue;
			}
			MemberProjectedSlot memberProjectedSlot = projectedSlot as MemberProjectedSlot;
			MemberProjectedSlot memberProjectedSlot2 = projectedSlot2 as MemberProjectedSlot;
			if (memberProjectedSlot != null && memberProjectedSlot2 != null && !EqualityComparer.Equals(memberProjectedSlot, memberProjectedSlot2))
			{
				return false;
			}
			ProjectedSlot projectedSlot3 = ((memberProjectedSlot != null) ? projectedSlot : projectedSlot2);
			slots[i] = projectedSlot3;
		}
		return true;
	}
}
