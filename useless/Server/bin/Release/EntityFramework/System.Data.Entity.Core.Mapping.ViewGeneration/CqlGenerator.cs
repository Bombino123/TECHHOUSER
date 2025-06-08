using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal sealed class CqlGenerator : InternalBase
{
	private readonly CellTreeNode m_view;

	private readonly Dictionary<MemberPath, CaseStatement> m_caseStatements;

	private readonly MemberProjectionIndex m_projectedSlotMap;

	private readonly int m_numBools;

	private int m_currentBlockNum;

	private readonly BoolExpression m_topLevelWhereClause;

	private readonly CqlIdentifiers m_identifiers;

	private readonly StorageMappingItemCollection m_mappingItemCollection;

	private int TotalSlots => m_projectedSlotMap.Count + m_numBools;

	internal CqlGenerator(CellTreeNode view, Dictionary<MemberPath, CaseStatement> caseStatements, CqlIdentifiers identifiers, MemberProjectionIndex projectedSlotMap, int numCellsInView, BoolExpression topLevelWhereClause, StorageMappingItemCollection mappingItemCollection)
	{
		m_view = view;
		m_caseStatements = caseStatements;
		m_projectedSlotMap = projectedSlotMap;
		m_numBools = numCellsInView;
		m_topLevelWhereClause = topLevelWhereClause;
		m_identifiers = identifiers;
		m_mappingItemCollection = mappingItemCollection;
	}

	internal string GenerateEsql()
	{
		CqlBlock cqlBlock = GenerateCqlBlockTree();
		StringBuilder stringBuilder = new StringBuilder(1024);
		cqlBlock.AsEsql(stringBuilder, isTopLevel: true, 1);
		return stringBuilder.ToString();
	}

	internal DbQueryCommandTree GenerateCqt()
	{
		DbExpression query = GenerateCqlBlockTree().AsCqt(isTopLevel: true);
		return DbQueryCommandTree.FromValidExpression(m_mappingItemCollection.Workspace, DataSpace.SSpace, query, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false);
	}

	private CqlBlock GenerateCqlBlockTree()
	{
		bool[] requiredSlots = GetRequiredSlots();
		List<WithRelationship> withRelationships = new List<WithRelationship>();
		CqlBlock viewBlock = m_view.ToCqlBlock(requiredSlots, m_identifiers, ref m_currentBlockNum, ref withRelationships);
		foreach (CaseStatement value in m_caseStatements.Values)
		{
			value.Simplify();
		}
		return ConstructCaseBlocks(viewBlock, withRelationships);
	}

	private bool[] GetRequiredSlots()
	{
		bool[] array = new bool[TotalSlots];
		foreach (CaseStatement value in m_caseStatements.Values)
		{
			GetRequiredSlotsForCaseMember(value.MemberPath, array);
		}
		for (int i = TotalSlots - m_numBools; i < TotalSlots; i++)
		{
			array[i] = true;
		}
		foreach (CaseStatement value2 in m_caseStatements.Values)
		{
			if (!value2.MemberPath.IsPartOfKey && !value2.DependsOnMemberValue)
			{
				array[m_projectedSlotMap.IndexOf(value2.MemberPath)] = false;
			}
		}
		return array;
	}

	private CqlBlock ConstructCaseBlocks(CqlBlock viewBlock, IEnumerable<WithRelationship> withRelationships)
	{
		bool[] array = new bool[TotalSlots];
		array[0] = true;
		m_topLevelWhereClause.GetRequiredSlots(m_projectedSlotMap, array);
		return ConstructCaseBlocks(viewBlock, 0, array, withRelationships);
	}

	private CqlBlock ConstructCaseBlocks(CqlBlock viewBlock, int startSlotNum, bool[] parentRequiredSlots, IEnumerable<WithRelationship> withRelationships)
	{
		int count = m_projectedSlotMap.Count;
		int num = FindNextCaseStatementSlot(startSlotNum, parentRequiredSlots, count);
		if (num == -1)
		{
			return viewBlock;
		}
		MemberPath memberPath = m_projectedSlotMap[num];
		bool[] array = new bool[TotalSlots];
		GetRequiredSlotsForCaseMember(memberPath, array);
		for (int i = 0; i < TotalSlots; i++)
		{
			if (parentRequiredSlots[i])
			{
				array[i] = true;
			}
		}
		CaseStatement caseStatement = m_caseStatements[memberPath];
		array[num] = caseStatement.DependsOnMemberValue;
		CqlBlock cqlBlock = ConstructCaseBlocks(viewBlock, num + 1, array, null);
		SlotInfo[] array2 = CreateSlotInfosForCaseStatement(parentRequiredSlots, num, cqlBlock, caseStatement, withRelationships);
		m_currentBlockNum++;
		BoolExpression whereClause = ((startSlotNum == 0) ? m_topLevelWhereClause : BoolExpression.True);
		if (startSlotNum == 0)
		{
			for (int j = 1; j < array2.Length; j++)
			{
				array2[j].ResetIsRequiredByParent();
			}
		}
		return new CaseCqlBlock(array2, num, cqlBlock, whereClause, m_identifiers, m_currentBlockNum);
	}

	private SlotInfo[] CreateSlotInfosForCaseStatement(bool[] parentRequiredSlots, int foundSlot, CqlBlock childBlock, CaseStatement thisCaseStatement, IEnumerable<WithRelationship> withRelationships)
	{
		int num = childBlock.Slots.Count - TotalSlots;
		SlotInfo[] array = new SlotInfo[TotalSlots + num];
		for (int i = 0; i < TotalSlots; i++)
		{
			bool flag = childBlock.IsProjected(i);
			bool flag2 = parentRequiredSlots[i];
			ProjectedSlot slotValue = childBlock.SlotValue(i);
			MemberPath outputMemberPath = GetOutputMemberPath(i);
			if (i == foundSlot)
			{
				slotValue = new CaseStatementProjectedSlot(thisCaseStatement.DeepQualify(childBlock), withRelationships);
				flag = true;
			}
			else if (flag && flag2)
			{
				slotValue = childBlock.QualifySlotWithBlockAlias(i);
			}
			SlotInfo slotInfo = new SlotInfo(flag2 && flag, flag, slotValue, outputMemberPath);
			array[i] = slotInfo;
		}
		for (int j = TotalSlots; j < TotalSlots + num; j++)
		{
			QualifiedSlot slotValue2 = childBlock.QualifySlotWithBlockAlias(j);
			array[j] = new SlotInfo(isRequiredByParent: true, isProjected: true, slotValue2, childBlock.MemberPath(j));
		}
		return array;
	}

	private int FindNextCaseStatementSlot(int startSlotNum, bool[] parentRequiredSlots, int numMembers)
	{
		int result = -1;
		for (int i = startSlotNum; i < numMembers; i++)
		{
			MemberPath key = m_projectedSlotMap[i];
			if (parentRequiredSlots[i] && m_caseStatements.ContainsKey(key))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void GetRequiredSlotsForCaseMember(MemberPath caseMemberPath, bool[] requiredSlots)
	{
		CaseStatement caseStatement = m_caseStatements[caseMemberPath];
		bool flag = false;
		foreach (CaseStatement.WhenThen clause in caseStatement.Clauses)
		{
			clause.Condition.GetRequiredSlots(m_projectedSlotMap, requiredSlots);
			if (!(clause.Value is ConstantProjectedSlot))
			{
				flag = true;
			}
		}
		EdmType edmType = caseMemberPath.EdmType;
		if (Helper.IsEntityType(edmType) || Helper.IsComplexType(edmType))
		{
			foreach (EdmType instantiatedType in caseStatement.InstantiatedTypes)
			{
				foreach (EdmMember allStructuralMember in Helper.GetAllStructuralMembers(instantiatedType))
				{
					int slotIndex = GetSlotIndex(caseMemberPath, allStructuralMember);
					requiredSlots[slotIndex] = true;
				}
			}
			return;
		}
		if (caseMemberPath.IsScalarType())
		{
			if (flag)
			{
				int num = m_projectedSlotMap.IndexOf(caseMemberPath);
				requiredSlots[num] = true;
			}
			return;
		}
		if (Helper.IsAssociationType(edmType))
		{
			foreach (AssociationEndMember associationEndMember in ((AssociationSet)caseMemberPath.Extent).ElementType.AssociationEndMembers)
			{
				int slotIndex2 = GetSlotIndex(caseMemberPath, associationEndMember);
				requiredSlots[slotIndex2] = true;
			}
			return;
		}
		foreach (EdmMember keyMember in (edmType as RefType).ElementType.KeyMembers)
		{
			int slotIndex3 = GetSlotIndex(caseMemberPath, keyMember);
			requiredSlots[slotIndex3] = true;
		}
	}

	private MemberPath GetOutputMemberPath(int slotNum)
	{
		return m_projectedSlotMap.GetMemberPath(slotNum, TotalSlots - m_projectedSlotMap.Count);
	}

	private int GetSlotIndex(MemberPath member, EdmMember child)
	{
		MemberPath member2 = new MemberPath(member, child);
		return m_projectedSlotMap.IndexOf(member2);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("View: ");
		m_view.ToCompactString(builder);
		builder.Append("ProjectedSlotMap: ");
		m_projectedSlotMap.ToCompactString(builder);
		builder.Append("Case statements: ");
		foreach (MemberPath key in m_caseStatements.Keys)
		{
			m_caseStatements[key].ToCompactString(builder);
			builder.AppendLine();
		}
	}
}
