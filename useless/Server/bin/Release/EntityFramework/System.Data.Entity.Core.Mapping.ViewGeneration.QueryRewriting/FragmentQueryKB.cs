using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class FragmentQueryKB : KnowledgeBase<DomainConstraint<BoolLiteral, Constant>>
{
	private BoolExpr<DomainConstraint<BoolLiteral, Constant>> _kbExpression = TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value;

	internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> KbExpression => _kbExpression;

	internal override void AddFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
	{
		base.AddFact(fact);
		_kbExpression = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(_kbExpression, fact);
	}

	internal void CreateVariableConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
	{
		CreateVariableConstraintsRecursion(extent.ElementType, new MemberPath(extent), domainMap, edmItemCollection);
	}

	internal void CreateAssociationConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
	{
		if (!(extent is AssociationSet associationSet))
		{
			return;
		}
		BoolExpression boolExpression = BoolExpression.CreateLiteral(new RoleBoolean(associationSet), domainMap);
		HashSet<Pair<EdmMember, EntityType>> associationkeys = new HashSet<Pair<EdmMember, EntityType>>();
		foreach (AssociationEndMember associationEndMember in associationSet.ElementType.AssociationEndMembers)
		{
			EntityType type = (EntityType)((RefType)associationEndMember.TypeUsage.EdmType).ElementType;
			type.KeyMembers.All((EdmMember member) => !associationkeys.Add(new Pair<EdmMember, EntityType>(member, type)) || true);
		}
		foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
		{
			HashSet<EdmType> hashSet = new HashSet<EdmType>();
			hashSet.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(associationSetEnd.CorrespondingAssociationEndMember.TypeUsage.EdmType, edmItemCollection, includeAbstractTypes: false));
			BoolExpression boolExpression2 = CreateIsOfTypeCondition(new MemberPath(associationSetEnd.EntitySet), hashSet, domainMap);
			BoolExpression boolExpression3 = BoolExpression.CreateLiteral(new RoleBoolean(associationSetEnd), domainMap);
			BoolExpression boolExpression4 = BoolExpression.CreateAnd(BoolExpression.CreateLiteral(new RoleBoolean(associationSetEnd.EntitySet), domainMap), boolExpression2);
			AddImplication(boolExpression3.Tree, boolExpression4.Tree);
			if (MetadataHelper.IsEveryOtherEndAtLeastOne(associationSet, associationSetEnd.CorrespondingAssociationEndMember))
			{
				AddImplication(boolExpression4.Tree, boolExpression3.Tree);
			}
			if (MetadataHelper.DoesEndKeySubsumeAssociationSetKey(associationSet, associationSetEnd.CorrespondingAssociationEndMember, associationkeys))
			{
				AddEquivalence(boolExpression3.Tree, boolExpression.Tree);
			}
		}
		foreach (ReferentialConstraint referentialConstraint in associationSet.ElementType.ReferentialConstraints)
		{
			AssociationEndMember endMember = (AssociationEndMember)referentialConstraint.ToRole;
			EntitySet entitySetAtEnd = MetadataHelper.GetEntitySetAtEnd(associationSet, endMember);
			if (Helpers.IsSetEqual(Helpers.AsSuperTypeList<EdmProperty, EdmMember>(referentialConstraint.ToProperties), entitySetAtEnd.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default) && referentialConstraint.FromRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One))
			{
				BoolExpression boolExpression5 = BoolExpression.CreateLiteral(new RoleBoolean(associationSet.AssociationSetEnds[0]), domainMap);
				BoolExpression boolExpression6 = BoolExpression.CreateLiteral(new RoleBoolean(associationSet.AssociationSetEnds[1]), domainMap);
				AddEquivalence(boolExpression5.Tree, boolExpression6.Tree);
			}
		}
	}

	internal void CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(AssociationSet assocSet, MemberDomainMap domainMap)
	{
		foreach (ReferentialConstraint referentialConstraint in assocSet.ElementType.ReferentialConstraints)
		{
			AssociationEndMember endMember = (AssociationEndMember)referentialConstraint.ToRole;
			AssociationEndMember endMember2 = (AssociationEndMember)referentialConstraint.FromRole;
			EntitySet entitySetAtEnd = MetadataHelper.GetEntitySetAtEnd(assocSet, endMember);
			EntitySet entitySetAtEnd2 = MetadataHelper.GetEntitySetAtEnd(assocSet, endMember2);
			if (Helpers.IsSetEqual(Helpers.AsSuperTypeList<EdmProperty, EdmMember>(referentialConstraint.ToProperties), entitySetAtEnd.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default))
			{
				BoolExpression boolExpression = BoolExpression.CreateLiteral(new RoleBoolean(entitySetAtEnd2), domainMap);
				BoolExpression boolExpression2 = BoolExpression.CreateLiteral(new RoleBoolean(entitySetAtEnd), domainMap);
				AddEquivalence(boolExpression.Tree, boolExpression2.Tree);
			}
		}
	}

	private void CreateVariableConstraintsRecursion(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
	{
		HashSet<EdmType> hashSet = new HashSet<EdmType>();
		hashSet.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(edmType, edmItemCollection, includeAbstractTypes: true));
		foreach (EdmType item in hashSet)
		{
			HashSet<EdmType> hashSet2 = new HashSet<EdmType>();
			hashSet2.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(item, edmItemCollection, includeAbstractTypes: false));
			if (hashSet2.Count == 0)
			{
				continue;
			}
			BoolExpression boolExpression = BoolExpression.CreateNot(CreateIsOfTypeCondition(currentPath, hashSet2, domainMap));
			if (!boolExpression.IsSatisfiable())
			{
				continue;
			}
			foreach (EdmProperty declaredOnlyMember in ((StructuralType)item).GetDeclaredOnlyMembers<EdmProperty>())
			{
				MemberPath memberPath = new MemberPath(currentPath, declaredOnlyMember);
				bool flag = MetadataHelper.IsNonRefSimpleMember(declaredOnlyMember);
				if (domainMap.IsConditionMember(memberPath) || domainMap.IsProjectedConditionMember(memberPath))
				{
					List<Constant> possibleDiscreteValues = new List<Constant>(domainMap.GetDomain(memberPath));
					AddEquivalence(right: ((!flag) ? BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(memberPath), new Domain(Constant.Undefined, possibleDiscreteValues)), domainMap) : BoolExpression.CreateLiteral(new ScalarRestriction(new MemberProjectedSlot(memberPath), new Domain(Constant.Undefined, possibleDiscreteValues)), domainMap)).Tree, left: boolExpression.Tree);
				}
				if (!flag)
				{
					CreateVariableConstraintsRecursion(memberPath.EdmType, memberPath, domainMap, edmItemCollection);
				}
			}
		}
	}

	private static BoolExpression CreateIsOfTypeCondition(MemberPath currentPath, IEnumerable<EdmType> derivedTypes, MemberDomainMap domainMap)
	{
		Domain domain = new Domain(derivedTypes.Select((Func<EdmType, Constant>)((EdmType derivedType) => new TypeConstant(derivedType))), domainMap.GetDomain(currentPath));
		return BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(currentPath), domain), domainMap);
	}
}
