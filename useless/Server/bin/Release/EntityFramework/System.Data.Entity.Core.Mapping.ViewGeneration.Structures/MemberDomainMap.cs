using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class MemberDomainMap : InternalBase
{
	private class CellConstantSetInfo : Set<Constant>
	{
		internal CellConstantSetInfo(Set<Constant> iconstants)
			: base(iconstants)
		{
		}
	}

	private readonly Dictionary<MemberPath, Set<Constant>> m_conditionDomainMap;

	private readonly Dictionary<MemberPath, Set<Constant>> m_nonConditionDomainMap;

	private readonly Set<MemberPath> m_projectedConditionMembers = new Set<MemberPath>();

	private readonly EdmItemCollection m_edmItemCollection;

	private MemberDomainMap(Dictionary<MemberPath, Set<Constant>> domainMap, Dictionary<MemberPath, Set<Constant>> nonConditionDomainMap, EdmItemCollection edmItemCollection)
	{
		m_conditionDomainMap = domainMap;
		m_nonConditionDomainMap = nonConditionDomainMap;
		m_edmItemCollection = edmItemCollection;
	}

	internal MemberDomainMap(ViewTarget viewTarget, bool isValidationEnabled, IEnumerable<Cell> extentCells, EdmItemCollection edmItemCollection, ConfigViewGenerator config, Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
	{
		m_conditionDomainMap = new Dictionary<MemberPath, Set<Constant>>(MemberPath.EqualityComparer);
		m_edmItemCollection = edmItemCollection;
		Dictionary<MemberPath, Set<Constant>> dictionary = null;
		dictionary = ((viewTarget != ViewTarget.UpdateView) ? Domain.ComputeConstantDomainSetsForSlotsInQueryViews(extentCells, m_edmItemCollection, isValidationEnabled) : Domain.ComputeConstantDomainSetsForSlotsInUpdateViews(extentCells, m_edmItemCollection));
		foreach (Cell extentCell in extentCells)
		{
			foreach (MemberRestriction item in extentCell.GetLeftQuery(viewTarget).GetConjunctsFromWhereClause())
			{
				MemberPath memberPath = item.RestrictedMemberSlot.MemberPath;
				if (!dictionary.TryGetValue(memberPath, out var value))
				{
					value = Domain.DeriveDomainFromMemberPath(memberPath, edmItemCollection, isValidationEnabled);
				}
				if (value.Contains(Constant.Null) || !item.Domain.Values.All((Constant conditionConstant) => conditionConstant.Equals(Constant.NotNull)))
				{
					if (value.Count <= 0 || (!value.Contains(Constant.Null) && item.Domain.Values.Contains(Constant.Null)))
					{
						string message = Strings.ViewGen_InvalidCondition(memberPath.PathToString(false));
						ExceptionHelpers.ThrowMappingException(new ErrorLog.Record(ViewGenErrorCode.InvalidCondition, message, extentCell, string.Empty), config);
					}
					if (!memberPath.IsAlwaysDefined(inheritanceGraph))
					{
						value.Add(Constant.Undefined);
					}
					AddToDomainMap(memberPath, value);
				}
			}
		}
		m_nonConditionDomainMap = new Dictionary<MemberPath, Set<Constant>>(MemberPath.EqualityComparer);
		foreach (Cell extentCell2 in extentCells)
		{
			foreach (MemberProjectedSlot allQuerySlot in extentCell2.GetLeftQuery(viewTarget).GetAllQuerySlots())
			{
				MemberPath memberPath2 = allQuerySlot.MemberPath;
				if (!m_conditionDomainMap.ContainsKey(memberPath2) && !m_nonConditionDomainMap.ContainsKey(memberPath2))
				{
					Set<Constant> set = Domain.DeriveDomainFromMemberPath(memberPath2, m_edmItemCollection, leaveDomainUnbounded: true);
					if (!memberPath2.IsAlwaysDefined(inheritanceGraph))
					{
						set.Add(Constant.Undefined);
					}
					set = Domain.ExpandNegationsInDomain(set, set);
					m_nonConditionDomainMap.Add(memberPath2, new CellConstantSetInfo(set));
				}
			}
		}
	}

	internal bool IsProjectedConditionMember(MemberPath memberPath)
	{
		return m_projectedConditionMembers.Contains(memberPath);
	}

	internal MemberDomainMap GetOpenDomain()
	{
		Dictionary<MemberPath, Set<Constant>> dictionary = m_conditionDomainMap.ToDictionary((KeyValuePair<MemberPath, Set<Constant>> p) => p.Key, (KeyValuePair<MemberPath, Set<Constant>> p) => new Set<Constant>(p.Value, Constant.EqualityComparer));
		ExpandDomainsIfNeeded(dictionary);
		return new MemberDomainMap(dictionary, m_nonConditionDomainMap, m_edmItemCollection);
	}

	internal MemberDomainMap MakeCopy()
	{
		return new MemberDomainMap(m_conditionDomainMap.ToDictionary((KeyValuePair<MemberPath, Set<Constant>> p) => p.Key, (KeyValuePair<MemberPath, Set<Constant>> p) => new Set<Constant>(p.Value, Constant.EqualityComparer)), m_nonConditionDomainMap, m_edmItemCollection);
	}

	internal void ExpandDomainsToIncludeAllPossibleValues()
	{
		ExpandDomainsIfNeeded(m_conditionDomainMap);
	}

	private void ExpandDomainsIfNeeded(Dictionary<MemberPath, Set<Constant>> domainMapForMembers)
	{
		foreach (MemberPath key in domainMapForMembers.Keys)
		{
			Set<Constant> set = domainMapForMembers[key];
			if (key.IsScalarType() && !set.Any((Constant c) => c is NegatedConstant))
			{
				if (MetadataHelper.HasDiscreteDomain(key.EdmType))
				{
					Set<Constant> other = Domain.DeriveDomainFromMemberPath(key, m_edmItemCollection, leaveDomainUnbounded: true);
					set.Unite(other);
				}
				else
				{
					NegatedConstant element = new NegatedConstant(set);
					set.Add(element);
				}
			}
		}
	}

	internal void ReduceEnumerableDomainToEnumeratedValues(ConfigViewGenerator config)
	{
		ReduceEnumerableDomainToEnumeratedValues(m_conditionDomainMap, config, m_edmItemCollection);
		ReduceEnumerableDomainToEnumeratedValues(m_nonConditionDomainMap, config, m_edmItemCollection);
	}

	private static void ReduceEnumerableDomainToEnumeratedValues(Dictionary<MemberPath, Set<Constant>> domainMap, ConfigViewGenerator config, EdmItemCollection edmItemCollection)
	{
		foreach (MemberPath key in domainMap.Keys)
		{
			if (!MetadataHelper.HasDiscreteDomain(key.EdmType))
			{
				continue;
			}
			Set<Constant> other = Domain.DeriveDomainFromMemberPath(key, edmItemCollection, leaveDomainUnbounded: true);
			Set<Constant> set = domainMap[key].Difference(other);
			set.Remove(Constant.Undefined);
			if (set.Count > 0)
			{
				if (config.IsNormalTracing)
				{
					Helpers.FormatTraceLine("Changed domain of {0} from {1} - subtract {2}", key, domainMap[key], set);
				}
				domainMap[key].Subtract(set);
			}
		}
	}

	internal static void PropagateUpdateDomainToQueryDomain(IEnumerable<Cell> cells, MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap)
	{
		foreach (Cell cell in cells)
		{
			CellQuery cQuery = cell.CQuery;
			CellQuery sQuery = cell.SQuery;
			for (int i = 0; i < cQuery.NumProjectedSlots; i++)
			{
				MemberProjectedSlot memberProjectedSlot = cQuery.ProjectedSlotAt(i) as MemberProjectedSlot;
				MemberProjectedSlot memberProjectedSlot2 = sQuery.ProjectedSlotAt(i) as MemberProjectedSlot;
				if (memberProjectedSlot != null && memberProjectedSlot2 != null)
				{
					MemberPath memberPath = memberProjectedSlot.MemberPath;
					MemberPath memberPath2 = memberProjectedSlot2.MemberPath;
					Set<Constant> domainInternal = queryDomainMap.GetDomainInternal(memberPath);
					Set<Constant> domainInternal2 = updateDomainMap.GetDomainInternal(memberPath2);
					domainInternal.Unite(domainInternal2.Where((Constant constant) => !constant.IsNull() && !(constant is NegatedConstant)));
					if (updateDomainMap.IsConditionMember(memberPath2) && !queryDomainMap.IsConditionMember(memberPath))
					{
						queryDomainMap.m_projectedConditionMembers.Add(memberPath);
					}
				}
			}
		}
		ExpandNegationsInDomainMap(queryDomainMap.m_conditionDomainMap);
		ExpandNegationsInDomainMap(queryDomainMap.m_nonConditionDomainMap);
	}

	private static void ExpandNegationsInDomainMap(Dictionary<MemberPath, Set<Constant>> domainMap)
	{
		MemberPath[] array = domainMap.Keys.ToArray();
		foreach (MemberPath key in array)
		{
			domainMap[key] = Domain.ExpandNegationsInDomain(domainMap[key]);
		}
	}

	internal bool IsConditionMember(MemberPath path)
	{
		return m_conditionDomainMap.ContainsKey(path);
	}

	internal IEnumerable<MemberPath> ConditionMembers(EntitySetBase extent)
	{
		foreach (MemberPath key in m_conditionDomainMap.Keys)
		{
			if (key.Extent.Equals(extent))
			{
				yield return key;
			}
		}
	}

	internal IEnumerable<MemberPath> NonConditionMembers(EntitySetBase extent)
	{
		foreach (MemberPath key in m_nonConditionDomainMap.Keys)
		{
			if (key.Extent.Equals(extent))
			{
				yield return key;
			}
		}
	}

	internal void AddSentinel(MemberPath path)
	{
		GetDomainInternal(path).Add(Constant.AllOtherConstants);
	}

	internal void RemoveSentinel(MemberPath path)
	{
		GetDomainInternal(path).Remove(Constant.AllOtherConstants);
	}

	internal IEnumerable<Constant> GetDomain(MemberPath path)
	{
		return GetDomainInternal(path);
	}

	private Set<Constant> GetDomainInternal(MemberPath path)
	{
		if (!m_conditionDomainMap.TryGetValue(path, out var value))
		{
			return m_nonConditionDomainMap[path];
		}
		return value;
	}

	internal void UpdateConditionMemberDomain(MemberPath path, IEnumerable<Constant> domainValues)
	{
		Set<Constant> set = m_conditionDomainMap[path];
		set.Clear();
		set.Unite(domainValues);
	}

	private void AddToDomainMap(MemberPath member, IEnumerable<Constant> domainValues)
	{
		if (!m_conditionDomainMap.TryGetValue(member, out var value))
		{
			value = new Set<Constant>(Constant.EqualityComparer);
		}
		value.Unite(domainValues);
		m_conditionDomainMap[member] = Domain.ExpandNegationsInDomain(value, value);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		foreach (MemberPath key in m_conditionDomainMap.Keys)
		{
			builder.Append('(');
			key.ToCompactString(builder);
			IEnumerable<Constant> domain = GetDomain(key);
			builder.Append(": ");
			StringUtil.ToCommaSeparatedStringSorted(builder, domain);
			builder.Append(") ");
		}
	}
}
