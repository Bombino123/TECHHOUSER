using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class RewritingValidator
{
	private class WhereClauseVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, CellTreeNode>
	{
		private readonly ViewgenContext _viewgenContext;

		private readonly CellTreeNode _topLevelTree;

		private readonly Dictionary<MemberValueBinding, CellTreeNode> _memberValueTrees;

		internal WhereClauseVisitor(CellTreeNode topLevelTree, Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees)
		{
			_topLevelTree = topLevelTree;
			_memberValueTrees = memberValueTrees;
			_viewgenContext = topLevelTree.ViewgenContext;
		}

		internal CellTreeNode GetCellTreeNode(BoolExpression whereClause)
		{
			return whereClause.Tree.Accept(this);
		}

		internal override CellTreeNode VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			IEnumerable<CellTreeNode> enumerable = AcceptChildren(expression.Children);
			OpCellTreeNode opCellTreeNode = new OpCellTreeNode(_viewgenContext, CellTreeOpType.IJ);
			foreach (CellTreeNode item in enumerable)
			{
				if (item == null)
				{
					return null;
				}
				if (item != _topLevelTree)
				{
					opCellTreeNode.Add(item);
				}
			}
			if (opCellTreeNode.Children.Count != 0)
			{
				return opCellTreeNode;
			}
			return _topLevelTree;
		}

		internal override CellTreeNode VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			return _topLevelTree;
		}

		internal override CellTreeNode VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			MemberRestriction memberRestriction = (MemberRestriction)expression.Identifier.Variable.Identifier;
			Set<Constant> range = expression.Identifier.Range;
			OpCellTreeNode opCellTreeNode = new OpCellTreeNode(_viewgenContext, CellTreeOpType.Union);
			CellTreeNode singleNode = null;
			foreach (Constant item in range)
			{
				if (TryGetCellTreeNode(memberRestriction.RestrictedMemberSlot.MemberPath, item, out singleNode))
				{
					opCellTreeNode.Add(singleNode);
				}
			}
			return opCellTreeNode.Children.Count switch
			{
				0 => null, 
				1 => singleNode, 
				_ => opCellTreeNode, 
			};
		}

		internal override CellTreeNode VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			throw new NotImplementedException();
		}

		internal override CellTreeNode VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			throw new NotImplementedException();
		}

		internal override CellTreeNode VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
		{
			throw new NotImplementedException();
		}

		private bool TryGetCellTreeNode(MemberPath memberPath, Constant value, out CellTreeNode singleNode)
		{
			return _memberValueTrees.TryGetValue(new MemberValueBinding(memberPath, value), out singleNode);
		}

		private IEnumerable<CellTreeNode> AcceptChildren(IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> children)
		{
			foreach (BoolExpr<DomainConstraint<BoolLiteral, Constant>> child in children)
			{
				yield return child.Accept(this);
			}
		}
	}

	internal class DomainConstraintVisitor : CellTreeNode.SimpleCellTreeVisitor<bool, bool>
	{
		private readonly LeftCellWrapper m_wrapper;

		private readonly ViewgenContext m_viewgenContext;

		private readonly ErrorLog m_errorLog;

		private DomainConstraintVisitor(LeftCellWrapper wrapper, ViewgenContext context, ErrorLog errorLog)
		{
			m_wrapper = wrapper;
			m_viewgenContext = context;
			m_errorLog = errorLog;
		}

		internal static void CheckConstraints(CellTreeNode node, LeftCellWrapper wrapper, ViewgenContext context, ErrorLog errorLog)
		{
			DomainConstraintVisitor visitor = new DomainConstraintVisitor(wrapper, context, errorLog);
			node.Accept(visitor, param: true);
		}

		internal override bool VisitLeaf(LeafCellTreeNode node, bool dummy)
		{
			CellQuery rightCellQuery = m_wrapper.RightCellQuery;
			CellQuery rightCellQuery2 = node.LeftCellWrapper.RightCellQuery;
			List<MemberPath> list = new List<MemberPath>();
			if (rightCellQuery != rightCellQuery2)
			{
				for (int i = 0; i < rightCellQuery.NumProjectedSlots; i++)
				{
					if (rightCellQuery.ProjectedSlotAt(i) is MemberProjectedSlot memberProjectedSlot && rightCellQuery2.ProjectedSlotAt(i) is MemberProjectedSlot memberProjectedSlot2)
					{
						MemberPath memberPath = m_viewgenContext.MemberMaps.ProjectedSlotMap[i];
						if (!memberPath.IsPartOfKey && !MemberPath.EqualityComparer.Equals(memberProjectedSlot.MemberPath, memberProjectedSlot2.MemberPath))
						{
							list.Add(memberPath);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				string message = Strings.ViewGen_NonKeyProjectedWithOverlappingPartitions(MemberPath.PropertiesToUserString(list, fullPath: false));
				ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.NonKeyProjectedWithOverlappingPartitions, message, new LeftCellWrapper[2] { m_wrapper, node.LeftCellWrapper }, string.Empty);
				m_errorLog.AddEntry(record);
			}
			return true;
		}

		internal override bool VisitOpNode(OpCellTreeNode node, bool dummy)
		{
			if (node.OpType == CellTreeOpType.LASJ)
			{
				node.Children[0].Accept(this, dummy);
			}
			else
			{
				foreach (CellTreeNode child in node.Children)
				{
					child.Accept(this, dummy);
				}
			}
			return true;
		}
	}

	private struct MemberValueBinding : IEquatable<MemberValueBinding>
	{
		internal readonly MemberPath Member;

		internal readonly Constant Value;

		public MemberValueBinding(MemberPath member, Constant value)
		{
			Member = member;
			Value = value;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}={1}", new object[2] { Member, Value });
		}

		public bool Equals(MemberValueBinding other)
		{
			if (MemberPath.EqualityComparer.Equals(Member, other.Member))
			{
				return Constant.EqualityComparer.Equals(Value, other.Value);
			}
			return false;
		}
	}

	private readonly ViewgenContext _viewgenContext;

	private readonly MemberDomainMap _domainMap;

	private readonly CellTreeNode _basicView;

	private readonly IEnumerable<MemberPath> _keyAttributes;

	private readonly ErrorLog _errorLog;

	internal RewritingValidator(ViewgenContext context, CellTreeNode basicView)
	{
		_viewgenContext = context;
		_basicView = basicView;
		_domainMap = _viewgenContext.MemberMaps.UpdateDomainMap;
		_keyAttributes = MemberPath.GetKeyMembers(_viewgenContext.Extent, _domainMap);
		_errorLog = new ErrorLog();
	}

	internal void Validate()
	{
		Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees = CreateMemberValueTrees(complementElse: false);
		Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees2 = CreateMemberValueTrees(complementElse: true);
		WhereClauseVisitor whereClauseVisitor = new WhereClauseVisitor(_basicView, memberValueTrees);
		WhereClauseVisitor whereClauseVisitor2 = new WhereClauseVisitor(_basicView, memberValueTrees2);
		foreach (LeftCellWrapper item in _viewgenContext.AllWrappersForExtent)
		{
			Cell onlyInputCell = item.OnlyInputCell;
			CellTreeNode cellTreeNode = new LeafCellTreeNode(_viewgenContext, item);
			CellTreeNode cellTreeNode2 = whereClauseVisitor2.GetCellTreeNode(onlyInputCell.SQuery.WhereClause);
			if (cellTreeNode2 == null)
			{
				continue;
			}
			CellTreeNode cellTreeNode3 = ((cellTreeNode2 == _basicView) ? _basicView : new OpCellTreeNode(_viewgenContext, CellTreeOpType.IJ, cellTreeNode2, _basicView));
			BoolExpression inExtentCondition = BoolExpression.CreateLiteral(item.CreateRoleBoolean(), _viewgenContext.MemberMaps.QueryDomainMap);
			if (!CheckEquivalence(cellTreeNode.RightFragmentQuery, cellTreeNode3.RightFragmentQuery, inExtentCondition, out var unsatisfiedConstraint))
			{
				string p = StringUtil.FormatInvariant("{0}", _viewgenContext.Extent);
				cellTreeNode.RightFragmentQuery.Condition.ExpensiveSimplify();
				cellTreeNode3.RightFragmentQuery.Condition.ExpensiveSimplify();
				string message = Strings.ViewGen_CQ_PartitionConstraint(p);
				ReportConstraintViolation(message, unsatisfiedConstraint, ViewGenErrorCode.PartitionConstraintViolation, cellTreeNode.GetLeaves().Concat(cellTreeNode3.GetLeaves()));
			}
			CellTreeNode cellTreeNode4 = whereClauseVisitor.GetCellTreeNode(onlyInputCell.SQuery.WhereClause);
			if (cellTreeNode4 != null)
			{
				DomainConstraintVisitor.CheckConstraints(cellTreeNode4, item, _viewgenContext, _errorLog);
				if (_errorLog.Count > 0)
				{
					continue;
				}
				CheckConstraintsOnProjectedConditionMembers(memberValueTrees, item, cellTreeNode3, inExtentCondition);
				if (_errorLog.Count > 0)
				{
					continue;
				}
			}
			CheckConstraintsOnNonNullableMembers(item);
		}
		if (_errorLog.Count > 0)
		{
			ExceptionHelpers.ThrowMappingException(_errorLog, _viewgenContext.Config);
		}
	}

	private bool CheckEquivalence(FragmentQuery cQuery, FragmentQuery sQuery, BoolExpression inExtentCondition, out BoolExpression unsatisfiedConstraint)
	{
		FragmentQuery fragmentQuery = _viewgenContext.RightFragmentQP.Difference(cQuery, sQuery);
		FragmentQuery fragmentQuery2 = _viewgenContext.RightFragmentQP.Difference(sQuery, cQuery);
		FragmentQuery fragmentQuery3 = FragmentQuery.Create(BoolExpression.CreateAnd(fragmentQuery.Condition, inExtentCondition));
		FragmentQuery fragmentQuery4 = FragmentQuery.Create(BoolExpression.CreateAnd(fragmentQuery2.Condition, inExtentCondition));
		unsatisfiedConstraint = null;
		bool flag = true;
		bool flag2 = true;
		if (_viewgenContext.RightFragmentQP.IsSatisfiable(fragmentQuery3))
		{
			unsatisfiedConstraint = fragmentQuery3.Condition;
			flag = false;
		}
		if (_viewgenContext.RightFragmentQP.IsSatisfiable(fragmentQuery4))
		{
			unsatisfiedConstraint = fragmentQuery4.Condition;
			flag2 = false;
		}
		if (flag && flag2)
		{
			return true;
		}
		unsatisfiedConstraint.ExpensiveSimplify();
		return false;
	}

	private void ReportConstraintViolation(string message, BoolExpression extraConstraint, ViewGenErrorCode errorCode, IEnumerable<LeftCellWrapper> relevantWrappers)
	{
		if (!ErrorPatternMatcher.FindMappingErrors(_viewgenContext, _domainMap, _errorLog))
		{
			extraConstraint.ExpensiveSimplify();
			HashSet<LeftCellWrapper> hashSet = new HashSet<LeftCellWrapper>(relevantWrappers);
			new List<LeftCellWrapper>(hashSet).Sort(LeftCellWrapper.OriginalCellIdComparer);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(message);
			EntityConfigurationToUserString(extraConstraint, stringBuilder);
			_errorLog.AddEntry(new ErrorLog.Record(errorCode, stringBuilder.ToString(), hashSet, ""));
		}
	}

	private Dictionary<MemberValueBinding, CellTreeNode> CreateMemberValueTrees(bool complementElse)
	{
		Dictionary<MemberValueBinding, CellTreeNode> dictionary = new Dictionary<MemberValueBinding, CellTreeNode>();
		foreach (MemberPath item in _domainMap.ConditionMembers(_viewgenContext.Extent))
		{
			List<Constant> list = new List<Constant>(_domainMap.GetDomain(item));
			OpCellTreeNode opCellTreeNode = new OpCellTreeNode(_viewgenContext, CellTreeOpType.Union);
			for (int i = 0; i < list.Count; i++)
			{
				Constant constant = list[i];
				MemberValueBinding key = new MemberValueBinding(item, constant);
				FragmentQuery query = QueryRewriter.CreateMemberConditionQuery(item, constant, _keyAttributes, _domainMap);
				if (_viewgenContext.TryGetCachedRewriting(query, out var rewriting))
				{
					CellTreeNode child = (dictionary[key] = QueryRewriter.TileToCellTree(rewriting, _viewgenContext));
					if (i < list.Count - 1)
					{
						opCellTreeNode.Add(child);
					}
				}
			}
			if (complementElse && list.Count > 1)
			{
				Constant value = list[list.Count - 1];
				MemberValueBinding key2 = new MemberValueBinding(item, value);
				dictionary[key2] = new OpCellTreeNode(_viewgenContext, CellTreeOpType.LASJ, _basicView, opCellTreeNode);
			}
		}
		return dictionary;
	}

	private void CheckConstraintsOnProjectedConditionMembers(Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees, LeftCellWrapper wrapper, CellTreeNode sQueryTree, BoolExpression inExtentCondition)
	{
		foreach (MemberPath item in _domainMap.ConditionMembers(_viewgenContext.Extent))
		{
			int slotNum = _viewgenContext.MemberMaps.ProjectedSlotMap.IndexOf(item);
			if (!(wrapper.RightCellQuery.ProjectedSlotAt(slotNum) is MemberProjectedSlot memberProjectedSlot))
			{
				continue;
			}
			foreach (Constant item2 in _domainMap.GetDomain(item))
			{
				if (memberValueTrees.TryGetValue(new MemberValueBinding(item, item2), out var value))
				{
					FragmentQuery cQuery = FragmentQuery.Create(PropagateCellConstantsToWhereClause(wrapper, wrapper.RightCellQuery.WhereClause, item2, item, _viewgenContext.MemberMaps));
					CellTreeNode cellTreeNode = ((sQueryTree == _basicView) ? value : new OpCellTreeNode(_viewgenContext, CellTreeOpType.IJ, value, sQueryTree));
					if (!CheckEquivalence(cQuery, cellTreeNode.RightFragmentQuery, inExtentCondition, out var unsatisfiedConstraint))
					{
						string message = Strings.ViewGen_CQ_DomainConstraint(memberProjectedSlot.ToUserString());
						ReportConstraintViolation(message, unsatisfiedConstraint, ViewGenErrorCode.DomainConstraintViolation, cellTreeNode.GetLeaves().Concat(new LeftCellWrapper[1] { wrapper }));
					}
				}
			}
		}
	}

	internal static BoolExpression PropagateCellConstantsToWhereClause(LeftCellWrapper wrapper, BoolExpression expression, Constant constant, MemberPath member, MemberMaps memberMaps)
	{
		MemberProjectedSlot cSideMappedSlotForSMember = wrapper.GetCSideMappedSlotForSMember(member);
		if (cSideMappedSlotForSMember == null)
		{
			return expression;
		}
		NegatedConstant negatedConstant = constant as NegatedConstant;
		IEnumerable<Constant> domain = memberMaps.QueryDomainMap.GetDomain(cSideMappedSlotForSMember.MemberPath);
		Set<Constant> set = new Set<Constant>(Constant.EqualityComparer);
		if (negatedConstant != null)
		{
			set.Unite(domain);
			set.Difference(negatedConstant.Elements);
		}
		else
		{
			set.Add(constant);
		}
		MemberRestriction literal = new ScalarRestriction(cSideMappedSlotForSMember.MemberPath, set, domain);
		return BoolExpression.CreateAnd(expression, BoolExpression.CreateLiteral(literal, memberMaps.QueryDomainMap));
	}

	private static FragmentQuery AddNullConditionOnCSideFragment(LeftCellWrapper wrapper, MemberPath member, MemberMaps memberMaps)
	{
		MemberProjectedSlot cSideMappedSlotForSMember = wrapper.GetCSideMappedSlotForSMember(member);
		if (cSideMappedSlotForSMember == null || !cSideMappedSlotForSMember.MemberPath.IsNullable)
		{
			return null;
		}
		BoolExpression whereClause = wrapper.RightCellQuery.WhereClause;
		IEnumerable<Constant> domain = memberMaps.QueryDomainMap.GetDomain(cSideMappedSlotForSMember.MemberPath);
		Set<Constant> set = new Set<Constant>(Constant.EqualityComparer);
		set.Add(Constant.Null);
		MemberRestriction literal = new ScalarRestriction(cSideMappedSlotForSMember.MemberPath, set, domain);
		return FragmentQuery.Create(BoolExpression.CreateAnd(whereClause, BoolExpression.CreateLiteral(literal, memberMaps.QueryDomainMap)));
	}

	private void CheckConstraintsOnNonNullableMembers(LeftCellWrapper wrapper)
	{
		foreach (MemberPath item in _domainMap.NonConditionMembers(_viewgenContext.Extent))
		{
			bool flag = item.EdmType is SimpleType;
			if (!item.IsNullable && flag)
			{
				FragmentQuery fragmentQuery = AddNullConditionOnCSideFragment(wrapper, item, _viewgenContext.MemberMaps);
				if (fragmentQuery != null && _viewgenContext.RightFragmentQP.IsSatisfiable(fragmentQuery))
				{
					_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.NullableMappingForNonNullableColumn, Strings.Viewgen_NullableMappingForNonNullableColumn(wrapper.LeftExtent.ToString(), item.ToFullString()), wrapper.Cells, ""));
				}
			}
		}
	}

	internal static void EntityConfigurationToUserString(BoolExpression condition, StringBuilder builder)
	{
		EntityConfigurationToUserString(condition, builder, writeRoundTrippingMessage: true);
	}

	internal static void EntityConfigurationToUserString(BoolExpression condition, StringBuilder builder, bool writeRoundTrippingMessage)
	{
		condition.AsUserString(builder, "PK", writeRoundTrippingMessage);
	}
}
