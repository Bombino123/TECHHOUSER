using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class PropertyPushdownHelper : BasicOpVisitor
{
	private readonly Dictionary<Node, PropertyRefList> m_nodePropertyRefMap;

	private readonly Dictionary<Var, PropertyRefList> m_varPropertyRefMap;

	private PropertyPushdownHelper()
	{
		m_varPropertyRefMap = new Dictionary<Var, PropertyRefList>();
		m_nodePropertyRefMap = new Dictionary<Node, PropertyRefList>();
	}

	internal static void Process(Command itree, out Dictionary<Var, PropertyRefList> varPropertyRefs, out Dictionary<Node, PropertyRefList> nodePropertyRefs)
	{
		PropertyPushdownHelper propertyPushdownHelper = new PropertyPushdownHelper();
		propertyPushdownHelper.Process(itree.Root);
		varPropertyRefs = propertyPushdownHelper.m_varPropertyRefMap;
		nodePropertyRefs = propertyPushdownHelper.m_nodePropertyRefMap;
	}

	private void Process(Node rootNode)
	{
		rootNode.Op.Accept(this, rootNode);
	}

	private PropertyRefList GetPropertyRefList(Node node)
	{
		if (!m_nodePropertyRefMap.TryGetValue(node, out var value))
		{
			value = new PropertyRefList();
			m_nodePropertyRefMap[node] = value;
		}
		return value;
	}

	private void AddPropertyRefs(Node node, PropertyRefList propertyRefs)
	{
		GetPropertyRefList(node).Append(propertyRefs);
	}

	private PropertyRefList GetPropertyRefList(Var v)
	{
		if (!m_varPropertyRefMap.TryGetValue(v, out var value))
		{
			value = new PropertyRefList();
			m_varPropertyRefMap[v] = value;
		}
		return value;
	}

	private void AddPropertyRefs(Var v, PropertyRefList propertyRefs)
	{
		GetPropertyRefList(v).Append(propertyRefs);
	}

	private static PropertyRefList GetIdentityProperties(EntityType type)
	{
		PropertyRefList keyProperties = GetKeyProperties(type);
		keyProperties.Add(EntitySetIdPropertyRef.Instance);
		return keyProperties;
	}

	private static PropertyRefList GetKeyProperties(EntityType entityType)
	{
		PropertyRefList propertyRefList = new PropertyRefList();
		foreach (EdmMember keyMember in entityType.KeyMembers)
		{
			EdmProperty obj = keyMember as EdmProperty;
			PlanCompiler.Assert(obj != null, "EntityType had non-EdmProperty key member?");
			SimplePropertyRef property = new SimplePropertyRef(obj);
			propertyRefList.Add(property);
		}
		return propertyRefList;
	}

	protected override void VisitDefault(Node n)
	{
		foreach (Node child in n.Children)
		{
			if (child.Op is ScalarOp scalarOp && TypeUtils.IsStructuredType(scalarOp.Type))
			{
				AddPropertyRefs(child, PropertyRefList.All);
			}
		}
		VisitChildren(n);
	}

	public override void Visit(SoftCastOp op, Node n)
	{
		PropertyRefList propertyRefList = null;
		if (TypeSemantics.IsReferenceType(op.Type))
		{
			propertyRefList = PropertyRefList.All;
		}
		else if (TypeSemantics.IsNominalType(op.Type))
		{
			propertyRefList = m_nodePropertyRefMap[n].Clone();
		}
		else if (TypeSemantics.IsRowType(op.Type))
		{
			propertyRefList = PropertyRefList.All;
		}
		if (propertyRefList != null)
		{
			AddPropertyRefs(n.Child0, propertyRefList);
		}
		VisitChildren(n);
	}

	public override void Visit(CaseOp op, Node n)
	{
		PropertyRefList propertyRefList = GetPropertyRefList(n);
		for (int i = 1; i < n.Children.Count - 1; i += 2)
		{
			PropertyRefList propertyRefs = propertyRefList.Clone();
			AddPropertyRefs(n.Children[i], propertyRefs);
		}
		AddPropertyRefs(n.Children[n.Children.Count - 1], propertyRefList.Clone());
		VisitChildren(n);
	}

	public override void Visit(CollectOp op, Node n)
	{
		VisitChildren(n);
	}

	public override void Visit(ComparisonOp op, Node n)
	{
		TypeUsage type = (n.Child0.Op as ScalarOp).Type;
		if (!TypeUtils.IsStructuredType(type))
		{
			VisitChildren(n);
			return;
		}
		if (TypeSemantics.IsRowType(type) || TypeSemantics.IsReferenceType(type))
		{
			VisitDefault(n);
			return;
		}
		PlanCompiler.Assert(TypeSemantics.IsEntityType(type), "unexpected childOpType?");
		PropertyRefList identityProperties = GetIdentityProperties(TypeHelpers.GetEdmType<EntityType>(type));
		foreach (Node child in n.Children)
		{
			AddPropertyRefs(child, identityProperties);
		}
		VisitChildren(n);
	}

	public override void Visit(ElementOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override void Visit(GetEntityRefOp op, Node n)
	{
		ScalarOp obj = n.Child0.Op as ScalarOp;
		PlanCompiler.Assert(obj != null, "input to GetEntityRefOp is not a ScalarOp?");
		PropertyRefList identityProperties = GetIdentityProperties(TypeHelpers.GetEdmType<EntityType>(obj.Type));
		AddPropertyRefs(n.Child0, identityProperties);
		VisitNode(n.Child0);
	}

	public override void Visit(IsOfOp op, Node n)
	{
		PropertyRefList propertyRefList = new PropertyRefList();
		propertyRefList.Add(TypeIdPropertyRef.Instance);
		AddPropertyRefs(n.Child0, propertyRefList);
		VisitChildren(n);
	}

	private void VisitPropertyOp(Op op, Node n, PropertyRef propertyRef)
	{
		PropertyRefList propertyRefList = new PropertyRefList();
		if (!TypeUtils.IsStructuredType(op.Type))
		{
			propertyRefList.Add(propertyRef);
		}
		else
		{
			PropertyRefList propertyRefList2 = GetPropertyRefList(n);
			if (propertyRefList2.AllProperties)
			{
				propertyRefList = propertyRefList2;
			}
			else
			{
				foreach (PropertyRef property in propertyRefList2.Properties)
				{
					propertyRefList.Add(property.CreateNestedPropertyRef(propertyRef));
				}
			}
		}
		AddPropertyRefs(n.Child0, propertyRefList);
		VisitChildren(n);
	}

	public override void Visit(RelPropertyOp op, Node n)
	{
		VisitPropertyOp(op, n, new RelPropertyRef(op.PropertyInfo));
	}

	public override void Visit(PropertyOp op, Node n)
	{
		VisitPropertyOp(op, n, new SimplePropertyRef(op.PropertyInfo));
	}

	public override void Visit(TreatOp op, Node n)
	{
		PropertyRefList propertyRefList = GetPropertyRefList(n).Clone();
		propertyRefList.Add(TypeIdPropertyRef.Instance);
		AddPropertyRefs(n.Child0, propertyRefList);
		VisitChildren(n);
	}

	public override void Visit(VarRefOp op, Node n)
	{
		if (TypeUtils.IsStructuredType(op.Var.Type))
		{
			PropertyRefList propertyRefList = GetPropertyRefList(n);
			AddPropertyRefs(op.Var, propertyRefList);
		}
	}

	public override void Visit(VarDefOp op, Node n)
	{
		if (TypeUtils.IsStructuredType(op.Var.Type))
		{
			PropertyRefList propertyRefList = GetPropertyRefList(op.Var);
			AddPropertyRefs(n.Child0, propertyRefList);
		}
		VisitChildren(n);
	}

	public override void Visit(VarDefListOp op, Node n)
	{
		VisitChildren(n);
	}

	protected override void VisitApplyOp(ApplyBaseOp op, Node n)
	{
		VisitNode(n.Child1);
		VisitNode(n.Child0);
	}

	public override void Visit(DistinctOp op, Node n)
	{
		foreach (Var key in op.Keys)
		{
			if (TypeUtils.IsStructuredType(key.Type))
			{
				AddPropertyRefs(key, PropertyRefList.All);
			}
		}
		VisitChildren(n);
	}

	public override void Visit(FilterOp op, Node n)
	{
		VisitNode(n.Child1);
		VisitNode(n.Child0);
	}

	protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		foreach (Var key in op.Keys)
		{
			if (TypeUtils.IsStructuredType(key.Type))
			{
				AddPropertyRefs(key, PropertyRefList.All);
			}
		}
		VisitChildrenReverse(n);
	}

	protected override void VisitJoinOp(JoinBaseOp op, Node n)
	{
		if (n.Op.OpType == OpType.CrossJoin)
		{
			VisitChildren(n);
			return;
		}
		VisitNode(n.Child2);
		VisitNode(n.Child0);
		VisitNode(n.Child1);
	}

	public override void Visit(ProjectOp op, Node n)
	{
		VisitNode(n.Child1);
		VisitNode(n.Child0);
	}

	public override void Visit(ScanTableOp op, Node n)
	{
		PlanCompiler.Assert(!n.HasChild0, "scanTableOp with an input?");
	}

	public override void Visit(ScanViewOp op, Node n)
	{
		PlanCompiler.Assert(op.Table.Columns.Count == 1, "ScanViewOp with multiple columns?");
		Var v = op.Table.Columns[0];
		PropertyRefList propertyRefList = GetPropertyRefList(v);
		Var singletonVar = NominalTypeEliminator.GetSingletonVar(n.Child0);
		PlanCompiler.Assert(singletonVar != null, "cannot determine single Var from ScanViewOp's input");
		AddPropertyRefs(singletonVar, propertyRefList.Clone());
		VisitChildren(n);
	}

	protected override void VisitSetOp(SetOp op, Node n)
	{
		VarMap[] varMap = op.VarMap;
		for (int i = 0; i < varMap.Length; i++)
		{
			foreach (KeyValuePair<Var, Var> item in varMap[i])
			{
				if (TypeUtils.IsStructuredType(item.Key.Type))
				{
					PropertyRefList propertyRefList = GetPropertyRefList(item.Key);
					if (op.OpType == OpType.Intersect || op.OpType == OpType.Except)
					{
						propertyRefList = PropertyRefList.All;
						AddPropertyRefs(item.Key, propertyRefList);
					}
					else
					{
						propertyRefList = propertyRefList.Clone();
					}
					AddPropertyRefs(item.Value, propertyRefList);
				}
			}
		}
		VisitChildren(n);
	}

	protected override void VisitSortOp(SortBaseOp op, Node n)
	{
		foreach (SortKey key in op.Keys)
		{
			if (TypeUtils.IsStructuredType(key.Var.Type))
			{
				AddPropertyRefs(key.Var, PropertyRefList.All);
			}
		}
		if (n.HasChild1)
		{
			VisitNode(n.Child1);
		}
		VisitNode(n.Child0);
	}

	public override void Visit(UnnestOp op, Node n)
	{
		VisitChildren(n);
	}

	public override void Visit(PhysicalProjectOp op, Node n)
	{
		foreach (Var output in op.Outputs)
		{
			if (TypeUtils.IsStructuredType(output.Type))
			{
				AddPropertyRefs(output, PropertyRefList.All);
			}
		}
		VisitChildren(n);
	}

	public override void Visit(MultiStreamNestOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override void Visit(SingleStreamNestOp op, Node n)
	{
		throw new NotSupportedException();
	}
}
