using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class MemberPath : InternalBase, IEquatable<MemberPath>
{
	private sealed class Comparer : IEqualityComparer<MemberPath>
	{
		public bool Equals(MemberPath left, MemberPath right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			if (!left.m_extent.Equals(right.m_extent) || left.m_path.Count != right.m_path.Count)
			{
				return false;
			}
			for (int i = 0; i < left.m_path.Count; i++)
			{
				if (!left.m_path[i].Equals(right.m_path[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(MemberPath key)
		{
			int num = key.m_extent.GetHashCode();
			foreach (EdmMember item in key.m_path)
			{
				num ^= item.GetHashCode();
			}
			return num;
		}
	}

	private readonly EntitySetBase m_extent;

	private readonly List<EdmMember> m_path;

	internal static readonly IEqualityComparer<MemberPath> EqualityComparer = new Comparer();

	internal EdmMember RootEdmMember
	{
		get
		{
			if (m_path.Count <= 0)
			{
				return null;
			}
			return m_path[0];
		}
	}

	internal EdmMember LeafEdmMember
	{
		get
		{
			if (m_path.Count <= 0)
			{
				return null;
			}
			return m_path[m_path.Count - 1];
		}
	}

	internal string LeafName
	{
		get
		{
			if (m_path.Count == 0)
			{
				return m_extent.Name;
			}
			return LeafEdmMember.Name;
		}
	}

	internal bool IsComputed
	{
		get
		{
			if (m_path.Count == 0)
			{
				return false;
			}
			return RootEdmMember.IsStoreGeneratedComputed;
		}
	}

	internal object DefaultValue
	{
		get
		{
			if (m_path.Count == 0)
			{
				return null;
			}
			if (LeafEdmMember.TypeUsage.Facets.TryGetValue("DefaultValue", ignoreCase: false, out var item))
			{
				return item.Value;
			}
			return null;
		}
	}

	internal bool IsPartOfKey
	{
		get
		{
			if (m_path.Count == 0)
			{
				return false;
			}
			return MetadataHelper.IsPartOfEntityTypeKey(LeafEdmMember);
		}
	}

	internal bool IsNullable
	{
		get
		{
			if (m_path.Count == 0)
			{
				return false;
			}
			return MetadataHelper.IsMemberNullable(LeafEdmMember);
		}
	}

	internal EntitySet EntitySet
	{
		get
		{
			if (m_path.Count == 0)
			{
				return m_extent as EntitySet;
			}
			if (m_path.Count == 1 && RootEdmMember is AssociationEndMember endMember)
			{
				return MetadataHelper.GetEntitySetAtEnd((AssociationSet)m_extent, endMember);
			}
			return null;
		}
	}

	internal EntitySetBase Extent => m_extent;

	internal EdmType EdmType
	{
		get
		{
			if (m_path.Count > 0)
			{
				return LeafEdmMember.TypeUsage.EdmType;
			}
			return m_extent.ElementType;
		}
	}

	internal string CqlFieldAlias
	{
		get
		{
			string text = PathToString(true);
			if (!text.Contains("_"))
			{
				text = text.Replace('.', '_');
			}
			StringBuilder stringBuilder = new StringBuilder();
			CqlWriter.AppendEscapedName(stringBuilder, text);
			return stringBuilder.ToString();
		}
	}

	internal MemberPath(EntitySetBase extent, IEnumerable<EdmMember> path)
	{
		m_extent = extent;
		m_path = path.ToList();
	}

	internal MemberPath(EntitySetBase extent)
		: this(extent, Enumerable.Empty<EdmMember>())
	{
	}

	internal MemberPath(EntitySetBase extent, EdmMember member)
		: this(extent, Enumerable.Repeat(member, 1))
	{
	}

	internal MemberPath(MemberPath prefix, EdmMember last)
	{
		m_extent = prefix.m_extent;
		m_path = new List<EdmMember>(prefix.m_path);
		m_path.Add(last);
	}

	internal bool IsAlwaysDefined(Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
	{
		if (m_path.Count == 0)
		{
			return true;
		}
		EdmMember member = m_path.Last();
		for (int i = 0; i < m_path.Count - 1; i++)
		{
			if (MetadataHelper.IsMemberNullable(m_path[i]))
			{
				return false;
			}
		}
		if (m_path[0].DeclaringType is AssociationType)
		{
			return true;
		}
		if (!(m_extent.ElementType is EntityType entityType))
		{
			return true;
		}
		EntityType entityType2 = m_path[0].DeclaringType as EntityType;
		EntityType entityType3 = entityType2.BaseType as EntityType;
		if (entityType.EdmEquals(entityType2) || MetadataHelper.IsParentOf(entityType2, entityType) || entityType3 == null)
		{
			return true;
		}
		if (!entityType3.Abstract && !MetadataHelper.DoesMemberExist(entityType3, member))
		{
			return false;
		}
		return !RecurseToFindMemberAbsentInConcreteType(entityType3, entityType2, member, entityType, inheritanceGraph);
	}

	private static bool RecurseToFindMemberAbsentInConcreteType(EntityType current, EntityType avoidEdge, EdmMember member, EntityType entitySetType, Dictionary<EntityType, Set<EntityType>> inheritanceGraph)
	{
		foreach (EntityType item in inheritanceGraph[current].Where((EntityType type) => !type.EdmEquals(avoidEdge)))
		{
			if (entitySetType.BaseType == null || !entitySetType.BaseType.EdmEquals(item))
			{
				if (!item.Abstract && !MetadataHelper.DoesMemberExist(item, member))
				{
					return true;
				}
				if (RecurseToFindMemberAbsentInConcreteType(item, current, member, entitySetType, inheritanceGraph))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void GetIdentifiers(CqlIdentifiers identifiers)
	{
		identifiers.AddIdentifier(m_extent.Name);
		identifiers.AddIdentifier(m_extent.ElementType.Name);
		foreach (EdmMember item in m_path)
		{
			identifiers.AddIdentifier(item.Name);
		}
	}

	internal static bool AreAllMembersNullable(IEnumerable<MemberPath> members)
	{
		foreach (MemberPath member in members)
		{
			if (member.m_path.Count == 0)
			{
				return false;
			}
			if (!member.IsNullable)
			{
				return false;
			}
		}
		return true;
	}

	internal static string PropertiesToUserString(IEnumerable<MemberPath> members, bool fullPath)
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MemberPath member in members)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			if (fullPath)
			{
				stringBuilder.Append(member.PathToString(false));
			}
			else
			{
				stringBuilder.Append(member.LeafName);
			}
		}
		return stringBuilder.ToString();
	}

	internal StringBuilder AsEsql(StringBuilder inputBuilder, string blockAlias)
	{
		StringBuilder builder = new StringBuilder();
		CqlWriter.AppendEscapedName(builder, blockAlias);
		AsCql(delegate(string memberName)
		{
			builder.Append('.');
			CqlWriter.AppendEscapedName(builder, memberName);
		}, delegate
		{
			builder.Insert(0, "Key(");
			builder.Append(")");
		}, delegate(StructuralType treatAsType)
		{
			builder.Insert(0, "TREAT(");
			builder.Append(" AS ");
			CqlWriter.AppendEscapedTypeName(builder, treatAsType);
			builder.Append(')');
		});
		inputBuilder.Append((object?)builder);
		return inputBuilder;
	}

	internal DbExpression AsCqt(DbExpression row)
	{
		AsCql(delegate(string memberName)
		{
			row = row.Property(memberName);
		}, delegate
		{
			row = row.GetRefKey();
		}, delegate(StructuralType treatAsType)
		{
			TypeUsage treatType = TypeUsage.Create(treatAsType);
			row = row.TreatAs(treatType);
		});
		return row;
	}

	internal void AsCql(Action<string> accessMember, Action getKey, Action<StructuralType> treatAs)
	{
		EdmType edmType = m_extent.ElementType;
		foreach (EdmMember item in m_path)
		{
			RefType refType;
			StructuralType type;
			if (Helper.IsRefType(edmType))
			{
				refType = (RefType)edmType;
				type = refType.ElementType;
			}
			else
			{
				refType = null;
				type = (StructuralType)edmType;
			}
			bool flag = MetadataHelper.DoesMemberExist(type, item);
			if (refType != null)
			{
				getKey();
			}
			else if (!flag)
			{
				treatAs(item.DeclaringType);
			}
			accessMember(item.Name);
			edmType = item.TypeUsage.EdmType;
		}
	}

	public bool Equals(MemberPath right)
	{
		return EqualityComparer.Equals(this, right);
	}

	public override bool Equals(object obj)
	{
		MemberPath right = obj as MemberPath;
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

	internal bool IsScalarType()
	{
		if (EdmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
		{
			return EdmType.BuiltInTypeKind == BuiltInTypeKind.EnumType;
		}
		return true;
	}

	internal static IEnumerable<MemberPath> GetKeyMembers(EntitySetBase extent, MemberDomainMap domainMap)
	{
		MemberPath memberPath = new MemberPath(extent);
		return new List<MemberPath>(memberPath.GetMembers(memberPath.Extent.ElementType, null, null, true, domainMap));
	}

	internal IEnumerable<MemberPath> GetMembers(EdmType edmType, bool? isScalar, bool? isConditional, bool? isPartOfKey, MemberDomainMap domainMap)
	{
		MemberPath currentPath = this;
		StructuralType structuralType = (StructuralType)edmType;
		foreach (EdmMember edmMember in structuralType.Members)
		{
			if (edmMember is AssociationEndMember)
			{
				foreach (MemberPath member in new MemberPath(currentPath, edmMember).GetMembers(((RefType)edmMember.TypeUsage.EdmType).ElementType, isScalar, isConditional, true, domainMap))
				{
					yield return member;
				}
			}
			bool flag = MetadataHelper.IsNonRefSimpleMember(edmMember);
			if ((isScalar.HasValue && isScalar != flag) || !(edmMember is EdmProperty edmProperty))
			{
				continue;
			}
			bool flag2 = MetadataHelper.IsPartOfEntityTypeKey(edmProperty);
			if (!isPartOfKey.HasValue || isPartOfKey == flag2)
			{
				MemberPath memberPath = new MemberPath(currentPath, edmProperty);
				bool flag3 = domainMap.IsConditionMember(memberPath);
				if (!isConditional.HasValue || isConditional == flag3)
				{
					yield return memberPath;
				}
			}
		}
	}

	internal bool IsEquivalentViaRefConstraint(MemberPath path1)
	{
		if (EdmType is EntityTypeBase || path1.EdmType is EntityTypeBase || !MetadataHelper.IsNonRefSimpleMember(LeafEdmMember) || !MetadataHelper.IsNonRefSimpleMember(path1.LeafEdmMember))
		{
			return false;
		}
		AssociationSet associationSet = Extent as AssociationSet;
		AssociationSet associationSet2 = path1.Extent as AssociationSet;
		EntitySet entitySet = Extent as EntitySet;
		EntitySet entitySet2 = path1.Extent as EntitySet;
		bool result = false;
		if (associationSet != null && associationSet2 != null)
		{
			if (!associationSet.Equals(associationSet2))
			{
				return false;
			}
			result = AreAssociationEndPathsEquivalentViaRefConstraint(this, path1, associationSet);
		}
		else if (entitySet != null && entitySet2 != null)
		{
			foreach (AssociationSet associationsForEntitySet in MetadataHelper.GetAssociationsForEntitySets(entitySet, entitySet2))
			{
				MemberPath correspondingAssociationPath = GetCorrespondingAssociationPath(associationsForEntitySet);
				MemberPath correspondingAssociationPath2 = path1.GetCorrespondingAssociationPath(associationsForEntitySet);
				if (AreAssociationEndPathsEquivalentViaRefConstraint(correspondingAssociationPath, correspondingAssociationPath2, associationsForEntitySet))
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			AssociationSet assocSet = ((associationSet != null) ? associationSet : associationSet2);
			MemberPath assocPath = ((Extent is AssociationSet) ? this : path1);
			MemberPath correspondingAssociationPath3 = ((Extent is EntitySet) ? this : path1).GetCorrespondingAssociationPath(assocSet);
			result = correspondingAssociationPath3 != null && AreAssociationEndPathsEquivalentViaRefConstraint(assocPath, correspondingAssociationPath3, assocSet);
		}
		return result;
	}

	private static bool AreAssociationEndPathsEquivalentViaRefConstraint(MemberPath assocPath0, MemberPath assocPath1, AssociationSet assocSet)
	{
		AssociationEndMember associationEndMember = assocPath0.RootEdmMember as AssociationEndMember;
		AssociationEndMember associationEndMember2 = assocPath1.RootEdmMember as AssociationEndMember;
		EdmProperty edmProperty = assocPath0.LeafEdmMember as EdmProperty;
		EdmProperty edmProperty2 = assocPath1.LeafEdmMember as EdmProperty;
		if (associationEndMember == null || associationEndMember2 == null || edmProperty == null || edmProperty2 == null)
		{
			return false;
		}
		AssociationType elementType = assocSet.ElementType;
		bool result = false;
		foreach (ReferentialConstraint referentialConstraint in elementType.ReferentialConstraints)
		{
			bool flag = associationEndMember.Name == referentialConstraint.FromRole.Name && associationEndMember2.Name == referentialConstraint.ToRole.Name;
			bool flag2 = associationEndMember2.Name == referentialConstraint.FromRole.Name && associationEndMember.Name == referentialConstraint.ToRole.Name;
			if (flag || flag2)
			{
				ReadOnlyMetadataCollection<EdmProperty> readOnlyMetadataCollection = (flag ? referentialConstraint.FromProperties : referentialConstraint.ToProperties);
				ReadOnlyMetadataCollection<EdmProperty> obj = (flag ? referentialConstraint.ToProperties : referentialConstraint.FromProperties);
				int num = readOnlyMetadataCollection.IndexOf(edmProperty);
				int num2 = obj.IndexOf(edmProperty2);
				if (num == num2 && num != -1)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private MemberPath GetCorrespondingAssociationPath(AssociationSet assocSet)
	{
		AssociationEndMember someEndForEntitySet = MetadataHelper.GetSomeEndForEntitySet(assocSet, m_extent);
		if (someEndForEntitySet == null)
		{
			return null;
		}
		List<EdmMember> list = new List<EdmMember>();
		list.Add(someEndForEntitySet);
		list.AddRange(m_path);
		return new MemberPath(assocSet, list);
	}

	internal EntitySet GetScopeOfRelationEnd()
	{
		if (m_path.Count == 0)
		{
			return null;
		}
		if (!(LeafEdmMember is AssociationEndMember endMember))
		{
			return null;
		}
		return MetadataHelper.GetEntitySetAtEnd((AssociationSet)m_extent, endMember);
	}

	internal string PathToString(bool? forAlias)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (forAlias.HasValue)
		{
			if (forAlias == true)
			{
				if (m_path.Count == 0)
				{
					return m_extent.ElementType.Name;
				}
				stringBuilder.Append(m_path[0].DeclaringType.Name);
			}
			else
			{
				stringBuilder.Append(m_extent.Name);
			}
		}
		for (int i = 0; i < m_path.Count; i++)
		{
			stringBuilder.Append('.');
			stringBuilder.Append(m_path[i].Name);
		}
		return stringBuilder.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(PathToString(false));
	}

	internal void ToCompactString(StringBuilder builder, string instanceToken)
	{
		builder.Append(instanceToken + PathToString(null));
	}
}
