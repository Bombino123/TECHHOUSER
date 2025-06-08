using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class Domain : InternalBase
{
	private readonly Set<Constant> m_domain;

	private readonly Set<Constant> m_possibleValues;

	internal IEnumerable<Constant> AllPossibleValues => AllPossibleValuesInternal;

	private Set<Constant> AllPossibleValuesInternal
	{
		get
		{
			NegatedConstant negatedConstant = new NegatedConstant(m_possibleValues);
			return m_possibleValues.Union(new Constant[1] { negatedConstant });
		}
	}

	internal int Count => m_domain.Count;

	internal IEnumerable<Constant> Values => m_domain;

	internal Domain(Constant value, IEnumerable<Constant> possibleDiscreteValues)
		: this(new Constant[1] { value }, possibleDiscreteValues)
	{
	}

	internal Domain(IEnumerable<Constant> values, IEnumerable<Constant> possibleDiscreteValues)
	{
		m_possibleValues = DeterminePossibleValues(values, possibleDiscreteValues);
		m_domain = ExpandNegationsInDomain(values, m_possibleValues);
		AssertInvariant();
	}

	internal Domain(Domain domain)
	{
		m_domain = new Set<Constant>(domain.m_domain, Constant.EqualityComparer);
		m_possibleValues = new Set<Constant>(domain.m_possibleValues, Constant.EqualityComparer);
		AssertInvariant();
	}

	internal static Set<Constant> DeriveDomainFromMemberPath(MemberPath memberPath, EdmItemCollection edmItemCollection, bool leaveDomainUnbounded)
	{
		Set<Constant> set = DeriveDomainFromType(memberPath.EdmType, edmItemCollection, leaveDomainUnbounded);
		if (memberPath.IsNullable)
		{
			set.Add(Constant.Null);
		}
		return set;
	}

	private static Set<Constant> DeriveDomainFromType(EdmType type, EdmItemCollection edmItemCollection, bool leaveDomainUnbounded)
	{
		Set<Constant> set = null;
		if (Helper.IsScalarType(type))
		{
			if (MetadataHelper.HasDiscreteDomain(type))
			{
				set = new Set<Constant>(CreateList(true, false), Constant.EqualityComparer);
			}
			else
			{
				set = new Set<Constant>(Constant.EqualityComparer);
				if (leaveDomainUnbounded)
				{
					set.Add(Constant.NotNull);
				}
			}
		}
		else
		{
			if (Helper.IsRefType(type))
			{
				type = ((RefType)type).ElementType;
			}
			List<Constant> list = new List<Constant>();
			foreach (EdmType item2 in MetadataHelper.GetTypeAndSubtypesOf(type, edmItemCollection, includeAbstractTypes: false))
			{
				TypeConstant item = new TypeConstant(item2);
				list.Add(item);
			}
			set = new Set<Constant>(list, Constant.EqualityComparer);
		}
		return set;
	}

	internal static bool TryGetDefaultValueForMemberPath(MemberPath memberPath, out Constant defaultConstant)
	{
		object defaultValue = memberPath.DefaultValue;
		defaultConstant = Constant.Null;
		if (defaultValue != null)
		{
			defaultConstant = new ScalarConstant(defaultValue);
			return true;
		}
		if (memberPath.IsNullable || memberPath.IsComputed)
		{
			return true;
		}
		return false;
	}

	internal static Constant GetDefaultValueForMemberPath(MemberPath memberPath, IEnumerable<LeftCellWrapper> wrappersForErrorReporting, ConfigViewGenerator config)
	{
		Constant defaultConstant = null;
		if (!TryGetDefaultValueForMemberPath(memberPath, out defaultConstant))
		{
			string message = Strings.ViewGen_No_Default_Value(memberPath.Extent.Name, memberPath.PathToString(false));
			ExceptionHelpers.ThrowMappingException(new ErrorLog.Record(ViewGenErrorCode.NoDefaultValue, message, wrappersForErrorReporting, string.Empty), config);
		}
		return defaultConstant;
	}

	internal int GetHash()
	{
		int num = 0;
		foreach (Constant item in m_domain)
		{
			num ^= Constant.EqualityComparer.GetHashCode(item);
		}
		return num;
	}

	internal bool IsEqualTo(Domain second)
	{
		return m_domain.SetEquals(second.m_domain);
	}

	internal bool ContainsNotNull()
	{
		return GetNegatedConstant(m_domain)?.Contains(Constant.Null) ?? false;
	}

	internal bool Contains(Constant constant)
	{
		return m_domain.Contains(constant);
	}

	internal static Set<Constant> ExpandNegationsInDomain(IEnumerable<Constant> domain, IEnumerable<Constant> otherPossibleValues)
	{
		Set<Constant> set = DeterminePossibleValues(domain, otherPossibleValues);
		Set<Constant> set2 = new Set<Constant>(Constant.EqualityComparer);
		foreach (Constant item in domain)
		{
			if (item is NegatedConstant negatedConstant)
			{
				set2.Add(new NegatedConstant(set));
				Set<Constant> elements = set.Difference(negatedConstant.Elements);
				set2.AddRange(elements);
			}
			else
			{
				set2.Add(item);
			}
		}
		return set2;
	}

	internal static Set<Constant> ExpandNegationsInDomain(IEnumerable<Constant> domain)
	{
		return ExpandNegationsInDomain(domain, domain);
	}

	private static Set<Constant> DeterminePossibleValues(IEnumerable<Constant> domain)
	{
		Set<Constant> set = new Set<Constant>(Constant.EqualityComparer);
		foreach (Constant item in domain)
		{
			if (item is NegatedConstant negatedConstant)
			{
				foreach (Constant element in negatedConstant.Elements)
				{
					set.Add(element);
				}
			}
			else
			{
				set.Add(item);
			}
		}
		return set;
	}

	internal static Dictionary<MemberPath, Set<Constant>> ComputeConstantDomainSetsForSlotsInQueryViews(IEnumerable<Cell> cells, EdmItemCollection edmItemCollection, bool isValidationEnabled)
	{
		Dictionary<MemberPath, Set<Constant>> dictionary = new Dictionary<MemberPath, Set<Constant>>(MemberPath.EqualityComparer);
		foreach (Cell cell in cells)
		{
			foreach (MemberRestriction item in cell.CQuery.GetConjunctsFromWhereClause())
			{
				MemberProjectedSlot restrictedMemberSlot = item.RestrictedMemberSlot;
				Set<Constant> set = DeriveDomainFromMemberPath(restrictedMemberSlot.MemberPath, edmItemCollection, isValidationEnabled);
				set.AddRange(item.Domain.Values.Where((Constant c) => !c.Equals(Constant.Null) && !c.Equals(Constant.NotNull)));
				if (!dictionary.TryGetValue(restrictedMemberSlot.MemberPath, out var value))
				{
					dictionary[restrictedMemberSlot.MemberPath] = set;
				}
				else
				{
					value.AddRange(set);
				}
			}
		}
		return dictionary;
	}

	private static bool GetRestrictedOrUnrestrictedDomain(MemberProjectedSlot slot, CellQuery cellQuery, EdmItemCollection edmItemCollection, out Set<Constant> domain)
	{
		return TryGetDomainRestrictedByWhereClause(DeriveDomainFromMemberPath(slot.MemberPath, edmItemCollection, leaveDomainUnbounded: true), slot, cellQuery, out domain);
	}

	internal static Dictionary<MemberPath, Set<Constant>> ComputeConstantDomainSetsForSlotsInUpdateViews(IEnumerable<Cell> cells, EdmItemCollection edmItemCollection)
	{
		Dictionary<MemberPath, Set<Constant>> dictionary = new Dictionary<MemberPath, Set<Constant>>(MemberPath.EqualityComparer);
		foreach (Cell cell in cells)
		{
			CellQuery cQuery = cell.CQuery;
			CellQuery sQuery = cell.SQuery;
			foreach (MemberProjectedSlot item in from oneOfConst in sQuery.GetConjunctsFromWhereClause()
				select oneOfConst.RestrictedMemberSlot)
			{
				if (!GetRestrictedOrUnrestrictedDomain(item, sQuery, edmItemCollection, out var domain))
				{
					int projectedPosition = sQuery.GetProjectedPosition(item);
					if (projectedPosition >= 0 && !GetRestrictedOrUnrestrictedDomain(cQuery.ProjectedSlotAt(projectedPosition) as MemberProjectedSlot, cQuery, edmItemCollection, out domain))
					{
						continue;
					}
				}
				MemberPath memberPath = item.MemberPath;
				if (TryGetDefaultValueForMemberPath(memberPath, out var defaultConstant))
				{
					domain.Add(defaultConstant);
				}
				if (!dictionary.TryGetValue(memberPath, out var value))
				{
					dictionary[memberPath] = domain;
				}
				else
				{
					value.AddRange(domain);
				}
			}
		}
		return dictionary;
	}

	private static bool TryGetDomainRestrictedByWhereClause(IEnumerable<Constant> domain, MemberProjectedSlot slot, CellQuery cellQuery, out Set<Constant> result)
	{
		IEnumerable<Set<Constant>> enumerable = from restriction in cellQuery.GetConjunctsFromWhereClause()
			where MemberPath.EqualityComparer.Equals(restriction.RestrictedMemberSlot.MemberPath, slot.MemberPath)
			select new Set<Constant>(restriction.Domain.Values, Constant.EqualityComparer);
		if (!enumerable.Any())
		{
			result = new Set<Constant>(domain);
			return false;
		}
		Set<Constant> possibleDiscreteValues = DeterminePossibleValues(enumerable.SelectMany((Set<Constant> m) => m.Select((Constant c) => c)), domain);
		Domain domain2 = new Domain(domain, possibleDiscreteValues);
		foreach (Set<Constant> item in enumerable)
		{
			domain2 = domain2.Intersect(new Domain(item, possibleDiscreteValues));
		}
		result = new Set<Constant>(domain2.Values, Constant.EqualityComparer);
		return !domain.SequenceEqual(result);
	}

	private Domain Intersect(Domain second)
	{
		Domain domain = new Domain(this);
		domain.m_domain.Intersect(second.m_domain);
		return domain;
	}

	private static NegatedConstant GetNegatedConstant(IEnumerable<Constant> constants)
	{
		NegatedConstant result = null;
		foreach (Constant constant in constants)
		{
			if (constant is NegatedConstant negatedConstant)
			{
				result = negatedConstant;
			}
		}
		return result;
	}

	private static Set<Constant> DeterminePossibleValues(IEnumerable<Constant> domain1, IEnumerable<Constant> domain2)
	{
		return DeterminePossibleValues(new Set<Constant>(domain1, Constant.EqualityComparer).Union(domain2));
	}

	[Conditional("DEBUG")]
	private static void CheckTwoDomainInvariants(Domain domain1, Domain domain2)
	{
		domain1.AssertInvariant();
		domain2.AssertInvariant();
	}

	private static IEnumerable<Constant> CreateList(object value1, object value2)
	{
		yield return new ScalarConstant(value1);
		yield return new ScalarConstant(value2);
	}

	internal void AssertInvariant()
	{
		GetNegatedConstant(m_domain);
		GetNegatedConstant(m_possibleValues);
	}

	internal string ToUserString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (Constant item in m_domain)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.ToUserString());
			flag = false;
		}
		return stringBuilder.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(ToUserString());
	}
}
