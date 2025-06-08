using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class ConditionComparer : IEqualityComparer<Dictionary<MemberPath, Set<Constant>>>
{
	public bool Equals(Dictionary<MemberPath, Set<Constant>> one, Dictionary<MemberPath, Set<Constant>> two)
	{
		Set<MemberPath> set = new Set<MemberPath>(one.Keys, MemberPath.EqualityComparer);
		Set<MemberPath> equals = new Set<MemberPath>(two.Keys, MemberPath.EqualityComparer);
		if (!set.SetEquals(equals))
		{
			return false;
		}
		foreach (MemberPath item in set)
		{
			Set<Constant> set2 = one[item];
			Set<Constant> equals2 = two[item];
			if (!set2.SetEquals(equals2))
			{
				return false;
			}
		}
		return true;
	}

	public int GetHashCode(Dictionary<MemberPath, Set<Constant>> obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MemberPath key in obj.Keys)
		{
			stringBuilder.Append(key);
		}
		return stringBuilder.ToString().GetHashCode();
	}
}
