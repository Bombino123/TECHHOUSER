using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class CTreeGenerator : BasicOpVisitorOfT<DbExpression>
{
	private class VarInfo
	{
		private readonly Var _var;

		private readonly List<string> _propertyChain = new List<string>();

		internal Var Var => _var;

		internal List<string> PropertyPath => _propertyChain;

		internal VarInfo(Var target)
		{
			_var = target;
		}

		internal void PrependProperty(string propName)
		{
			_propertyChain.Insert(0, propName);
		}
	}

	private class VarInfoList : List<VarInfo>
	{
		internal VarInfoList()
		{
		}

		internal VarInfoList(IEnumerable<VarInfo> elements)
			: base(elements)
		{
		}

		internal void PrependProperty(string propName)
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.PropertyPath.Insert(0, propName);
			}
		}

		internal bool TryGetInfo(Var targetVar, out VarInfo varInfo)
		{
			varInfo = null;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VarInfo current = enumerator.Current;
					if (current.Var == targetVar)
					{
						varInfo = current;
						return true;
					}
				}
			}
			return false;
		}
	}

	private abstract class IqtVarScope
	{
		internal abstract bool TryResolveVar(Var targetVar, out DbExpression resultExpr);
	}

	private abstract class BindingScope : IqtVarScope
	{
		private readonly VarInfoList _definedVars;

		internal VarInfoList PublishedVars => _definedVars;

		protected abstract DbVariableReferenceExpression BindingReference { get; }

		internal BindingScope(IEnumerable<VarInfo> boundVars)
		{
			_definedVars = new VarInfoList(boundVars);
		}

		internal override bool TryResolveVar(Var targetVar, out DbExpression resultExpr)
		{
			resultExpr = null;
			VarInfo varInfo = null;
			if (_definedVars.TryGetInfo(targetVar, out varInfo))
			{
				resultExpr = BindingReference;
				foreach (string item in varInfo.PropertyPath)
				{
					resultExpr = resultExpr.Property(item);
				}
				return true;
			}
			return false;
		}
	}

	private class RelOpInfo : BindingScope
	{
		private readonly DbExpressionBinding _binding;

		internal string PublisherName => _binding.VariableName;

		internal DbExpression Publisher => _binding.Expression;

		protected override DbVariableReferenceExpression BindingReference => _binding.Variable;

		internal RelOpInfo(string bindingName, DbExpression publisher, IEnumerable<VarInfo> publishedVars)
			: base(publishedVars)
		{
			PlanCompiler.Assert(TypeSemantics.IsCollectionType(publisher.ResultType), "non-collection type used as RelOpInfo publisher");
			_binding = publisher.BindAs(bindingName);
		}

		internal DbExpressionBinding CreateBinding()
		{
			return _binding;
		}
	}

	private class GroupByScope : BindingScope
	{
		private readonly DbGroupExpressionBinding _binding;

		private bool _referenceGroup;

		internal DbGroupExpressionBinding Binding => _binding;

		protected override DbVariableReferenceExpression BindingReference
		{
			get
			{
				if (!_referenceGroup)
				{
					return _binding.Variable;
				}
				return _binding.GroupVariable;
			}
		}

		internal GroupByScope(DbGroupExpressionBinding binding, IEnumerable<VarInfo> publishedVars)
			: base(publishedVars)
		{
			_binding = binding;
		}

		internal void SwitchToGroupReference()
		{
			PlanCompiler.Assert(!_referenceGroup, "SwitchToGroupReference called more than once on the same GroupByScope?");
			_referenceGroup = true;
		}
	}

	private class VarDefScope : IqtVarScope
	{
		private readonly Dictionary<Var, DbExpression> _definedVars;

		internal VarDefScope(Dictionary<Var, DbExpression> definedVars)
		{
			_definedVars = definedVars;
		}

		internal override bool TryResolveVar(Var targetVar, out DbExpression resultExpr)
		{
			resultExpr = null;
			DbExpression value = null;
			if (_definedVars.TryGetValue(targetVar, out value))
			{
				resultExpr = value;
				return true;
			}
			return false;
		}
	}

	private readonly Command _iqtCommand;

	private readonly DbQueryCommandTree _queryTree;

	private readonly Dictionary<ParameterVar, DbParameterReferenceExpression> _addedParams = new Dictionary<ParameterVar, DbParameterReferenceExpression>();

	private readonly Stack<IqtVarScope> _bindingScopes = new Stack<IqtVarScope>();

	private readonly Stack<VarDefScope> _varScopes = new Stack<VarDefScope>();

	private readonly Dictionary<DbExpression, RelOpInfo> _relOpState = new Dictionary<DbExpression, RelOpInfo>();

	private readonly AliasGenerator _applyAliases = new AliasGenerator("Apply");

	private readonly AliasGenerator _distinctAliases = new AliasGenerator("Distinct");

	private readonly AliasGenerator _exceptAliases = new AliasGenerator("Except");

	private readonly AliasGenerator _extentAliases = new AliasGenerator("Extent");

	private readonly AliasGenerator _filterAliases = new AliasGenerator("Filter");

	private readonly AliasGenerator _groupByAliases = new AliasGenerator("GroupBy");

	private readonly AliasGenerator _intersectAliases = new AliasGenerator("Intersect");

	private readonly AliasGenerator _joinAliases = new AliasGenerator("Join");

	private readonly AliasGenerator _projectAliases = new AliasGenerator("Project");

	private readonly AliasGenerator _sortAliases = new AliasGenerator("Sort");

	private readonly AliasGenerator _unionAllAliases = new AliasGenerator("UnionAll");

	private readonly AliasGenerator _elementAliases = new AliasGenerator("Element");

	private readonly AliasGenerator _singleRowTableAliases = new AliasGenerator("SingleRowTable");

	private readonly AliasGenerator _limitAliases = new AliasGenerator("Limit");

	private readonly AliasGenerator _skipAliases = new AliasGenerator("Skip");

	private DbProviderManifest _providerManifest;

	private DbProviderManifest ProviderManifest => _providerManifest ?? (_providerManifest = ((StoreItemCollection)_iqtCommand.MetadataWorkspace.GetItemCollection(DataSpace.SSpace)).ProviderManifest);

	internal static DbCommandTree Generate(Command itree, Node toConvert)
	{
		return new CTreeGenerator(itree, toConvert)._queryTree;
	}

	private CTreeGenerator(Command itree, Node toConvert)
	{
		_iqtCommand = itree;
		DbExpression query = VisitNode(toConvert);
		_queryTree = DbQueryCommandTree.FromValidExpression(itree.MetadataWorkspace, DataSpace.SSpace, query, useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false);
	}

	private void AssertRelOp(DbExpression expr)
	{
		PlanCompiler.Assert(_relOpState.ContainsKey(expr), "not a relOp expression?");
	}

	private RelOpInfo PublishRelOp(string name, DbExpression expr, VarInfoList publishedVars)
	{
		RelOpInfo relOpInfo = new RelOpInfo(name, expr, publishedVars);
		_relOpState.Add(expr, relOpInfo);
		return relOpInfo;
	}

	private RelOpInfo ConsumeRelOp(DbExpression expr)
	{
		AssertRelOp(expr);
		RelOpInfo result = _relOpState[expr];
		_relOpState.Remove(expr);
		return result;
	}

	private RelOpInfo VisitAsRelOp(Node inputNode)
	{
		PlanCompiler.Assert(inputNode.Op is RelOp, "Non-RelOp used as DbExpressionBinding Input");
		DbExpression expr = VisitNode(inputNode);
		return ConsumeRelOp(expr);
	}

	private void PushExpressionBindingScope(RelOpInfo inputState)
	{
		PlanCompiler.Assert(inputState != null && inputState.PublisherName != null && inputState.PublishedVars != null, "Invalid RelOpInfo produced by DbExpressionBinding Input");
		_bindingScopes.Push(inputState);
	}

	private RelOpInfo EnterExpressionBindingScope(Node inputNode, bool pushScope)
	{
		RelOpInfo relOpInfo = VisitAsRelOp(inputNode);
		if (pushScope)
		{
			PushExpressionBindingScope(relOpInfo);
		}
		return relOpInfo;
	}

	private RelOpInfo EnterExpressionBindingScope(Node inputNode)
	{
		return EnterExpressionBindingScope(inputNode, pushScope: true);
	}

	private void ExitExpressionBindingScope(RelOpInfo scope, bool wasPushed)
	{
		if (wasPushed)
		{
			PlanCompiler.Assert(_bindingScopes.Count > 0, "ExitExpressionBindingScope called on empty ExpressionBindingScope stack");
			PlanCompiler.Assert((RelOpInfo)_bindingScopes.Pop() == scope, "ExitExpressionBindingScope called on incorrect expression");
		}
	}

	private void ExitExpressionBindingScope(RelOpInfo scope)
	{
		ExitExpressionBindingScope(scope, wasPushed: true);
	}

	private GroupByScope EnterGroupByScope(Node inputNode)
	{
		RelOpInfo relOpInfo = VisitAsRelOp(inputNode);
		string publisherName = relOpInfo.PublisherName;
		string groupVarName = string.Format(CultureInfo.InvariantCulture, "{0}Group", new object[1] { publisherName });
		GroupByScope groupByScope = new GroupByScope(relOpInfo.CreateBinding().Expression.GroupBindAs(publisherName, groupVarName), relOpInfo.PublishedVars);
		_bindingScopes.Push(groupByScope);
		return groupByScope;
	}

	private void ExitGroupByScope(GroupByScope scope)
	{
		PlanCompiler.Assert(_bindingScopes.Count > 0, "ExitGroupByScope called on empty ExpressionBindingScope stack");
		PlanCompiler.Assert((GroupByScope)_bindingScopes.Pop() == scope, "ExitGroupByScope called on incorrect expression");
	}

	private void EnterVarDefScope(List<Node> varDefNodes)
	{
		Dictionary<Var, DbExpression> dictionary = new Dictionary<Var, DbExpression>();
		foreach (Node varDefNode in varDefNodes)
		{
			VarDefOp varDefOp = varDefNode.Op as VarDefOp;
			PlanCompiler.Assert(varDefOp != null, "VarDefListOp contained non-VarDefOp child node");
			PlanCompiler.Assert(varDefOp.Var is ComputedVar, "VarDefOp defined non-Computed Var");
			dictionary.Add(varDefOp.Var, VisitNode(varDefNode.Child0));
		}
		_varScopes.Push(new VarDefScope(dictionary));
	}

	private void EnterVarDefListScope(Node varDefListNode)
	{
		PlanCompiler.Assert(varDefListNode.Op is VarDefListOp, "EnterVarDefListScope called with non-VarDefListOp");
		EnterVarDefScope(varDefListNode.Children);
	}

	private void ExitVarDefScope()
	{
		PlanCompiler.Assert(_varScopes.Count > 0, "ExitVarDefScope called on empty VarDefScope stack");
		_varScopes.Pop();
	}

	private DbExpression ResolveVar(Var referencedVar)
	{
		DbExpression resultExpr = null;
		if (referencedVar is ParameterVar parameterVar)
		{
			if (!_addedParams.TryGetValue(parameterVar, out var value))
			{
				value = parameterVar.Type.Parameter(parameterVar.ParameterName);
				_addedParams[parameterVar] = value;
			}
			resultExpr = value;
		}
		else
		{
			if (referencedVar is ComputedVar targetVar && _varScopes.Count > 0 && !_varScopes.Peek().TryResolveVar(targetVar, out resultExpr))
			{
				resultExpr = null;
			}
			if (resultExpr == null)
			{
				DbExpression resultExpr2 = null;
				foreach (IqtVarScope bindingScope in _bindingScopes)
				{
					if (bindingScope.TryResolveVar(referencedVar, out resultExpr2))
					{
						resultExpr = resultExpr2;
						break;
					}
				}
			}
		}
		PlanCompiler.Assert(resultExpr != null, string.Format(CultureInfo.InvariantCulture, "Unresolvable Var used in Command: VarType={0}, Id={1}", new object[2]
		{
			Enum.GetName(typeof(VarType), referencedVar.VarType),
			referencedVar.Id
		}));
		return resultExpr;
	}

	private static void AssertBinary(Node n)
	{
		PlanCompiler.Assert(2 == n.Children.Count, string.Format(CultureInfo.InvariantCulture, "Non-Binary {0} encountered", new object[1] { n.Op.GetType().Name }));
	}

	private DbExpression VisitChild(Node n, int index)
	{
		PlanCompiler.Assert(n.Children.Count > index, "VisitChild called with invalid index");
		return VisitNode(n.Children[index]);
	}

	private new List<DbExpression> VisitChildren(Node n)
	{
		List<DbExpression> list = new List<DbExpression>();
		foreach (Node child in n.Children)
		{
			list.Add(VisitNode(child));
		}
		return list;
	}

	protected override DbExpression VisitConstantOp(ConstantBaseOp op, Node n)
	{
		return op.Type.Constant(op.Value);
	}

	public override DbExpression Visit(ConstantOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public override DbExpression Visit(InternalConstantOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public override DbExpression Visit(NullOp op, Node n)
	{
		return op.Type.Null();
	}

	public override DbExpression Visit(NullSentinelOp op, Node n)
	{
		return VisitConstantOp(op, n);
	}

	public override DbExpression Visit(ConstantPredicateOp op, Node n)
	{
		return DbExpressionBuilder.True.Equal(op.IsTrue ? DbExpressionBuilder.True : DbExpressionBuilder.False);
	}

	public override DbExpression Visit(FunctionOp op, Node n)
	{
		return op.Function.Invoke(VisitChildren(n));
	}

	public override DbExpression Visit(PropertyOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(RelPropertyOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(ArithmeticOp op, Node n)
	{
		DbExpression dbExpression = null;
		if (OpType.UnaryMinus == op.OpType)
		{
			dbExpression = VisitChild(n, 0).UnaryMinus();
		}
		else
		{
			DbExpression left = VisitChild(n, 0);
			DbExpression right = VisitChild(n, 1);
			dbExpression = op.OpType switch
			{
				OpType.Divide => left.Divide(right), 
				OpType.Minus => left.Minus(right), 
				OpType.Modulo => left.Modulo(right), 
				OpType.Multiply => left.Multiply(right), 
				OpType.Plus => left.Plus(right), 
				_ => null, 
			};
		}
		PlanCompiler.Assert(dbExpression != null, string.Format(CultureInfo.InvariantCulture, "ArithmeticOp OpType not recognized: {0}", new object[1] { Enum.GetName(typeof(OpType), op.OpType) }));
		return dbExpression;
	}

	public override DbExpression Visit(CaseOp op, Node n)
	{
		int num = n.Children.Count;
		PlanCompiler.Assert(num > 1, "Invalid CaseOp: At least 2 child Nodes (1 When/Then pair) must be present");
		List<DbExpression> list = new List<DbExpression>();
		List<DbExpression> list2 = new List<DbExpression>();
		DbExpression dbExpression = null;
		if (n.Children.Count % 2 == 0)
		{
			dbExpression = op.Type.Null();
		}
		else
		{
			num--;
			dbExpression = VisitChild(n, n.Children.Count - 1);
		}
		for (int i = 0; i < num; i += 2)
		{
			list.Add(VisitChild(n, i));
			list2.Add(VisitChild(n, i + 1));
		}
		return DbExpressionBuilder.Case(list, list2, dbExpression);
	}

	public override DbExpression Visit(ComparisonOp op, Node n)
	{
		AssertBinary(n);
		DbExpression left = VisitChild(n, 0);
		DbExpression right = VisitChild(n, 1);
		DbExpression dbExpression = null;
		dbExpression = op.OpType switch
		{
			OpType.EQ => left.Equal(right), 
			OpType.NE => left.NotEqual(right), 
			OpType.LT => left.LessThan(right), 
			OpType.GT => left.GreaterThan(right), 
			OpType.LE => left.LessThanOrEqual(right), 
			OpType.GE => left.GreaterThanOrEqual(right), 
			_ => null, 
		};
		PlanCompiler.Assert(dbExpression != null, string.Format(CultureInfo.InvariantCulture, "ComparisonOp OpType not recognized: {0}", new object[1] { Enum.GetName(typeof(OpType), op.OpType) }));
		return dbExpression;
	}

	public override DbExpression Visit(ConditionalOp op, Node n)
	{
		DbExpression dbExpression = VisitChild(n, 0);
		DbExpression dbExpression2 = null;
		switch (op.OpType)
		{
		case OpType.IsNull:
			dbExpression2 = dbExpression.IsNull();
			break;
		case OpType.And:
			dbExpression2 = dbExpression.And(VisitChild(n, 1));
			break;
		case OpType.Or:
			dbExpression2 = dbExpression.Or(VisitChild(n, 1));
			break;
		case OpType.In:
		{
			int count = n.Children.Count;
			List<DbExpression> list = new List<DbExpression>(count - 1);
			for (int i = 1; i < count; i++)
			{
				list.Add(VisitChild(n, i));
			}
			dbExpression2 = DbExpressionBuilder.CreateInExpression(dbExpression, list);
			break;
		}
		case OpType.Not:
			dbExpression2 = ((!(dbExpression is DbNotExpression dbNotExpression)) ? dbExpression.Not() : dbNotExpression.Argument);
			break;
		default:
			dbExpression2 = null;
			break;
		}
		PlanCompiler.Assert(dbExpression2 != null, string.Format(CultureInfo.InvariantCulture, "ConditionalOp OpType not recognized: {0}", new object[1] { Enum.GetName(typeof(OpType), op.OpType) }));
		return dbExpression2;
	}

	public override DbExpression Visit(LikeOp op, Node n)
	{
		return VisitChild(n, 0).Like(VisitChild(n, 1), VisitChild(n, 2));
	}

	public override DbExpression Visit(AggregateOp op, Node n)
	{
		PlanCompiler.Assert(condition: false, "AggregateOp encountered outside of GroupByOp");
		throw new NotSupportedException(Strings.Iqt_CTGen_UnexpectedAggregate);
	}

	public override DbExpression Visit(NavigateOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(NewEntityOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(NewInstanceOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(DiscriminatedNewEntityOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(NewMultisetOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(NewRecordOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(RefOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(VarRefOp op, Node n)
	{
		return ResolveVar(op.Var);
	}

	public override DbExpression Visit(TreatOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(CastOp op, Node n)
	{
		return VisitChild(n, 0).CastTo(op.Type);
	}

	public override DbExpression Visit(SoftCastOp op, Node n)
	{
		return VisitChild(n, 0);
	}

	public override DbExpression Visit(IsOfOp op, Node n)
	{
		if (op.IsOfOnly)
		{
			return VisitChild(n, 0).IsOfOnly(op.IsOfType);
		}
		return VisitChild(n, 0).IsOf(op.IsOfType);
	}

	public override DbExpression Visit(ExistsOp op, Node n)
	{
		DbExpression dbExpression = VisitNode(n.Child0);
		ConsumeRelOp(dbExpression);
		return dbExpression.IsEmpty().Not();
	}

	public override DbExpression Visit(ElementOp op, Node n)
	{
		DbExpression dbExpression = VisitNode(n.Child0);
		AssertRelOp(dbExpression);
		ConsumeRelOp(dbExpression);
		return DbExpressionBuilder.CreateElementExpressionUnwrapSingleProperty(dbExpression);
	}

	public override DbExpression Visit(GetRefKeyOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(GetEntityRefOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(CollectOp op, Node n)
	{
		throw new NotSupportedException();
	}

	private static string GenerateNameForVar(Var projectedVar, Dictionary<string, AliasGenerator> aliasMap, AliasGenerator defaultAliasGenerator, Dictionary<string, string> alreadyUsedNames)
	{
		AliasGenerator value;
		if (projectedVar.TryGetName(out var name))
		{
			if (!aliasMap.TryGetValue(name, out value))
			{
				value = (aliasMap[name] = new AliasGenerator(name));
			}
			else
			{
				name = value.Next();
			}
		}
		else
		{
			value = defaultAliasGenerator;
			name = value.Next();
		}
		while (alreadyUsedNames.ContainsKey(name))
		{
			name = value.Next();
		}
		alreadyUsedNames[name] = name;
		return name;
	}

	private DbExpression CreateProject(RelOpInfo sourceInfo, IEnumerable<Var> outputVars)
	{
		VarInfoList varInfoList = new VarInfoList();
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
		AliasGenerator defaultAliasGenerator = new AliasGenerator("C");
		Dictionary<string, AliasGenerator> aliasMap = new Dictionary<string, AliasGenerator>(StringComparer.InvariantCultureIgnoreCase);
		Dictionary<string, string> alreadyUsedNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		foreach (Var outputVar in outputVars)
		{
			string text = GenerateNameForVar(outputVar, aliasMap, defaultAliasGenerator, alreadyUsedNames);
			DbExpression value = ResolveVar(outputVar);
			list.Add(new KeyValuePair<string, DbExpression>(text, value));
			VarInfo varInfo = new VarInfo(outputVar);
			varInfo.PrependProperty(text);
			varInfoList.Add(varInfo);
		}
		DbExpression dbExpression = sourceInfo.CreateBinding().Project(DbExpressionBuilder.NewRow(list));
		PublishRelOp(_projectAliases.Next(), dbExpression, varInfoList);
		return dbExpression;
	}

	private static VarInfoList GetTableVars(Table targetTable)
	{
		VarInfoList varInfoList = new VarInfoList();
		if (targetTable.TableMetadata.Flattened)
		{
			for (int i = 0; i < targetTable.Columns.Count; i++)
			{
				VarInfo varInfo = new VarInfo(targetTable.Columns[i]);
				varInfo.PrependProperty(targetTable.TableMetadata.Columns[i].Name);
				varInfoList.Add(varInfo);
			}
		}
		else
		{
			varInfoList.Add(new VarInfo(targetTable.Columns[0]));
		}
		return varInfoList;
	}

	public override DbExpression Visit(ScanTableOp op, Node n)
	{
		PlanCompiler.Assert(op.Table.TableMetadata.Extent != null, "Invalid TableMetadata used in ScanTableOp - no Extent specified");
		PlanCompiler.Assert(!n.HasChild0, "views are not expected here");
		VarInfoList tableVars = GetTableVars(op.Table);
		DbExpression dbExpression = op.Table.TableMetadata.Extent.Scan();
		PublishRelOp(_extentAliases.Next(), dbExpression, tableVars);
		return dbExpression;
	}

	public override DbExpression Visit(ScanViewOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(UnnestOp op, Node n)
	{
		PlanCompiler.Assert(n.Child0.Op.OpType == OpType.VarDef, "an un-nest's child must be a VarDef");
		Node child = n.Child0.Child0;
		DbExpression dbExpression = child.Op.Accept(this, child);
		PlanCompiler.Assert(dbExpression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType, "the input to un-nest must yield a collection after plan compilation");
		VarInfoList tableVars = GetTableVars(op.Table);
		PublishRelOp(_extentAliases.Next(), dbExpression, tableVars);
		return dbExpression;
	}

	private RelOpInfo BuildEmptyProjection(Node relOpNode)
	{
		if (relOpNode.Op.OpType == OpType.Project)
		{
			relOpNode = relOpNode.Child0;
		}
		RelOpInfo relOpInfo = EnterExpressionBindingScope(relOpNode);
		DbExpression value = DbExpressionBuilder.Constant(1);
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
		list.Add(new KeyValuePair<string, DbExpression>("C0", value));
		DbExpression expr = relOpInfo.CreateBinding().Project(DbExpressionBuilder.NewRow(list));
		PublishRelOp(_projectAliases.Next(), expr, new VarInfoList());
		ExitExpressionBindingScope(relOpInfo);
		return ConsumeRelOp(expr);
	}

	private RelOpInfo BuildProjection(Node relOpNode, IEnumerable<Var> projectionVars)
	{
		DbExpression dbExpression = null;
		if (relOpNode.Op is ProjectOp)
		{
			dbExpression = VisitProject(relOpNode, projectionVars);
		}
		else
		{
			RelOpInfo relOpInfo = EnterExpressionBindingScope(relOpNode);
			dbExpression = CreateProject(relOpInfo, projectionVars);
			ExitExpressionBindingScope(relOpInfo);
		}
		return ConsumeRelOp(dbExpression);
	}

	private DbExpression VisitProject(Node n, IEnumerable<Var> varList)
	{
		RelOpInfo relOpInfo = EnterExpressionBindingScope(n.Child0);
		if (n.Children.Count > 1)
		{
			EnterVarDefListScope(n.Child1);
		}
		DbExpression result = CreateProject(relOpInfo, varList);
		if (n.Children.Count > 1)
		{
			ExitVarDefScope();
		}
		ExitExpressionBindingScope(relOpInfo);
		return result;
	}

	public override DbExpression Visit(ProjectOp op, Node n)
	{
		return VisitProject(n, op.Outputs);
	}

	public override DbExpression Visit(FilterOp op, Node n)
	{
		RelOpInfo relOpInfo = EnterExpressionBindingScope(n.Child0);
		DbExpression dbExpression = VisitNode(n.Child1);
		PlanCompiler.Assert(TypeSemantics.IsPrimitiveType(dbExpression.ResultType, PrimitiveTypeKind.Boolean), "Invalid FilterOp Predicate (non-ScalarOp or non-Boolean result)");
		DbExpression dbExpression2 = relOpInfo.CreateBinding().Filter(dbExpression);
		ExitExpressionBindingScope(relOpInfo);
		PublishRelOp(_filterAliases.Next(), dbExpression2, relOpInfo.PublishedVars);
		return dbExpression2;
	}

	private List<DbSortClause> VisitSortKeys(IList<System.Data.Entity.Core.Query.InternalTrees.SortKey> sortKeys)
	{
		VarVec varVec = _iqtCommand.CreateVarVec();
		List<DbSortClause> list = new List<DbSortClause>();
		foreach (System.Data.Entity.Core.Query.InternalTrees.SortKey sortKey in sortKeys)
		{
			if (!varVec.IsSet(sortKey.Var))
			{
				varVec.Set(sortKey.Var);
				DbSortClause dbSortClause = null;
				DbExpression key = ResolveVar(sortKey.Var);
				dbSortClause = (string.IsNullOrEmpty(sortKey.Collation) ? (sortKey.AscendingSort ? key.ToSortClause() : key.ToSortClauseDescending()) : (sortKey.AscendingSort ? key.ToSortClause(sortKey.Collation) : key.ToSortClauseDescending(sortKey.Collation)));
				list.Add(dbSortClause);
			}
		}
		return list;
	}

	public override DbExpression Visit(SortOp op, Node n)
	{
		RelOpInfo relOpInfo = EnterExpressionBindingScope(n.Child0);
		PlanCompiler.Assert(!n.HasChild1, "SortOp can have only one child");
		DbExpression dbExpression = relOpInfo.CreateBinding().Sort(VisitSortKeys(op.Keys));
		ExitExpressionBindingScope(relOpInfo);
		PublishRelOp(_sortAliases.Next(), dbExpression, relOpInfo.PublishedVars);
		return dbExpression;
	}

	private static DbExpression CreateLimitExpression(DbExpression argument, DbExpression limit, bool withTies)
	{
		PlanCompiler.Assert(!withTies, "Limit with Ties is not currently supported");
		return argument.Limit(limit);
	}

	public override DbExpression Visit(ConstrainedSortOp op, Node n)
	{
		DbExpression dbExpression = null;
		RelOpInfo relOpInfo = null;
		string name = null;
		bool flag = OpType.Null == n.Child1.Op.OpType;
		bool flag2 = OpType.Null == n.Child2.Op.OpType;
		PlanCompiler.Assert(!flag || !flag2, "ConstrainedSortOp with no Skip Count and no Limit?");
		if (op.Keys.Count == 0)
		{
			PlanCompiler.Assert(flag, "ConstrainedSortOp without SortKeys cannot have Skip Count");
			DbExpression dbExpression2 = VisitNode(n.Child0);
			relOpInfo = ConsumeRelOp(dbExpression2);
			dbExpression = CreateLimitExpression(dbExpression2, VisitNode(n.Child2), op.WithTies);
			name = _limitAliases.Next();
		}
		else
		{
			relOpInfo = EnterExpressionBindingScope(n.Child0);
			List<DbSortClause> sortOrder = VisitSortKeys(op.Keys);
			ExitExpressionBindingScope(relOpInfo);
			if (!flag && !flag2)
			{
				dbExpression = CreateLimitExpression(relOpInfo.CreateBinding().Skip(sortOrder, VisitChild(n, 1)), VisitChild(n, 2), op.WithTies);
				name = _limitAliases.Next();
			}
			else if (!flag && flag2)
			{
				dbExpression = relOpInfo.CreateBinding().Skip(sortOrder, VisitChild(n, 1));
				name = _skipAliases.Next();
			}
			else if (flag && !flag2)
			{
				dbExpression = CreateLimitExpression(relOpInfo.CreateBinding().Sort(sortOrder), VisitChild(n, 2), op.WithTies);
				name = _limitAliases.Next();
			}
		}
		PublishRelOp(name, dbExpression, relOpInfo.PublishedVars);
		return dbExpression;
	}

	public override DbExpression Visit(GroupByOp op, Node n)
	{
		VarInfoList varInfoList = new VarInfoList();
		GroupByScope groupByScope = EnterGroupByScope(n.Child0);
		EnterVarDefListScope(n.Child1);
		AliasGenerator aliasGenerator = new AliasGenerator("K");
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
		List<Var> list2 = new List<Var>(op.Outputs);
		foreach (Var key in op.Keys)
		{
			string text = aliasGenerator.Next();
			list.Add(new KeyValuePair<string, DbExpression>(text, ResolveVar(key)));
			VarInfo varInfo = new VarInfo(key);
			varInfo.PrependProperty(text);
			varInfoList.Add(varInfo);
			list2.Remove(key);
		}
		ExitVarDefScope();
		groupByScope.SwitchToGroupReference();
		Dictionary<Var, DbAggregate> dictionary = new Dictionary<Var, DbAggregate>();
		Node child = n.Child2;
		PlanCompiler.Assert(child.Op is VarDefListOp, "Invalid Aggregates VarDefListOp Node encountered in GroupByOp");
		foreach (Node child3 in child.Children)
		{
			VarDefOp obj = child3.Op as VarDefOp;
			PlanCompiler.Assert(obj != null, "Non-VarDefOp Node encountered as child of Aggregates VarDefListOp Node");
			Var var = obj.Var;
			PlanCompiler.Assert(var is ComputedVar, "Non-ComputedVar encountered in Aggregate VarDefOp");
			Node child2 = child3.Child0;
			IEnumerable<DbExpression> arguments = child2.Children.Select(base.VisitNode);
			AggregateOp aggregateOp = child2.Op as AggregateOp;
			PlanCompiler.Assert(aggregateOp != null, "Non-Aggregate Node encountered as child of Aggregate VarDefOp Node");
			DbFunctionAggregate value = ((!aggregateOp.IsDistinctAggregate) ? aggregateOp.AggFunc.Aggregate(arguments) : aggregateOp.AggFunc.AggregateDistinct(arguments));
			PlanCompiler.Assert(list2.Contains(var), "Defined aggregate Var not in Output Aggregate Vars list?");
			dictionary.Add(var, value);
		}
		ExitGroupByScope(groupByScope);
		AliasGenerator aliasGenerator2 = new AliasGenerator("A");
		List<KeyValuePair<string, DbAggregate>> list3 = new List<KeyValuePair<string, DbAggregate>>();
		foreach (Var item in list2)
		{
			string text2 = aliasGenerator2.Next();
			list3.Add(new KeyValuePair<string, DbAggregate>(text2, dictionary[item]));
			VarInfo varInfo2 = new VarInfo(item);
			varInfo2.PrependProperty(text2);
			varInfoList.Add(varInfo2);
		}
		DbExpression dbExpression = groupByScope.Binding.GroupBy(list, list3);
		PublishRelOp(_groupByAliases.Next(), dbExpression, varInfoList);
		return dbExpression;
	}

	public override DbExpression Visit(GroupByIntoOp op, Node n)
	{
		throw new NotSupportedException();
	}

	private RelOpInfo VisitJoinInput(Node joinInputNode)
	{
		if (joinInputNode.Op.OpType == OpType.Filter && joinInputNode.Child0.Op.OpType == OpType.ScanTable)
		{
			ScanTableOp scanTableOp = (ScanTableOp)joinInputNode.Child0.Op;
			if (scanTableOp.Table.ReferencedColumns.IsEmpty)
			{
				return BuildEmptyProjection(joinInputNode);
			}
			return BuildProjection(joinInputNode, scanTableOp.Table.ReferencedColumns);
		}
		return EnterExpressionBindingScope(joinInputNode, pushScope: false);
	}

	private DbExpression VisitBinaryJoin(Node joinNode, DbExpressionKind joinKind)
	{
		RelOpInfo relOpInfo = VisitJoinInput(joinNode.Child0);
		RelOpInfo relOpInfo2 = VisitJoinInput(joinNode.Child1);
		bool wasPushed = false;
		DbExpression dbExpression = null;
		if (joinNode.Children.Count > 2)
		{
			wasPushed = true;
			PushExpressionBindingScope(relOpInfo);
			PushExpressionBindingScope(relOpInfo2);
			dbExpression = VisitNode(joinNode.Child2);
		}
		else
		{
			dbExpression = DbExpressionBuilder.True;
		}
		DbExpression dbExpression2 = DbExpressionBuilder.CreateJoinExpressionByKind(joinKind, dbExpression, relOpInfo.CreateBinding(), relOpInfo2.CreateBinding());
		VarInfoList varInfoList = new VarInfoList();
		ExitExpressionBindingScope(relOpInfo2, wasPushed);
		relOpInfo2.PublishedVars.PrependProperty(relOpInfo2.PublisherName);
		varInfoList.AddRange(relOpInfo2.PublishedVars);
		ExitExpressionBindingScope(relOpInfo, wasPushed);
		relOpInfo.PublishedVars.PrependProperty(relOpInfo.PublisherName);
		varInfoList.AddRange(relOpInfo.PublishedVars);
		PublishRelOp(_joinAliases.Next(), dbExpression2, varInfoList);
		return dbExpression2;
	}

	public override DbExpression Visit(CrossJoinOp op, Node n)
	{
		List<DbExpressionBinding> list = new List<DbExpressionBinding>();
		VarInfoList varInfoList = new VarInfoList();
		foreach (Node child in n.Children)
		{
			RelOpInfo relOpInfo = VisitJoinInput(child);
			list.Add(relOpInfo.CreateBinding());
			ExitExpressionBindingScope(relOpInfo, wasPushed: false);
			relOpInfo.PublishedVars.PrependProperty(relOpInfo.PublisherName);
			varInfoList.AddRange(relOpInfo.PublishedVars);
		}
		DbExpression dbExpression = DbExpressionBuilder.CrossJoin(list);
		PublishRelOp(_joinAliases.Next(), dbExpression, varInfoList);
		return dbExpression;
	}

	public override DbExpression Visit(InnerJoinOp op, Node n)
	{
		return VisitBinaryJoin(n, DbExpressionKind.InnerJoin);
	}

	public override DbExpression Visit(LeftOuterJoinOp op, Node n)
	{
		return VisitBinaryJoin(n, DbExpressionKind.LeftOuterJoin);
	}

	public override DbExpression Visit(FullOuterJoinOp op, Node n)
	{
		return VisitBinaryJoin(n, DbExpressionKind.FullOuterJoin);
	}

	private DbExpression VisitApply(Node applyNode, DbExpressionKind applyKind)
	{
		RelOpInfo relOpInfo = EnterExpressionBindingScope(applyNode.Child0);
		RelOpInfo relOpInfo2 = EnterExpressionBindingScope(applyNode.Child1, pushScope: false);
		DbExpression dbExpression = DbExpressionBuilder.CreateApplyExpressionByKind(applyKind, relOpInfo.CreateBinding(), relOpInfo2.CreateBinding());
		ExitExpressionBindingScope(relOpInfo2, wasPushed: false);
		ExitExpressionBindingScope(relOpInfo);
		relOpInfo.PublishedVars.PrependProperty(relOpInfo.PublisherName);
		relOpInfo2.PublishedVars.PrependProperty(relOpInfo2.PublisherName);
		VarInfoList varInfoList = new VarInfoList();
		varInfoList.AddRange(relOpInfo.PublishedVars);
		varInfoList.AddRange(relOpInfo2.PublishedVars);
		PublishRelOp(_applyAliases.Next(), dbExpression, varInfoList);
		return dbExpression;
	}

	public override DbExpression Visit(CrossApplyOp op, Node n)
	{
		return VisitApply(n, DbExpressionKind.CrossApply);
	}

	public override DbExpression Visit(OuterApplyOp op, Node n)
	{
		return VisitApply(n, DbExpressionKind.OuterApply);
	}

	private DbExpression VisitSetOpArgument(Node argNode, VarVec outputVars, VarMap argVars)
	{
		List<Var> list = new List<Var>();
		RelOpInfo relOpInfo;
		if (outputVars.IsEmpty)
		{
			relOpInfo = BuildEmptyProjection(argNode);
		}
		else
		{
			foreach (Var outputVar in outputVars)
			{
				list.Add(argVars[outputVar]);
			}
			relOpInfo = BuildProjection(argNode, list);
		}
		return relOpInfo.Publisher;
	}

	private DbExpression VisitSetOp(SetOp op, Node n, AliasGenerator alias, Func<DbExpression, DbExpression, DbExpression> setOpExpressionBuilder)
	{
		AssertBinary(n);
		int num;
		DbExpression dbExpression;
		if (op.OpType == OpType.UnionAll || op.OpType == OpType.Intersect)
		{
			num = (ProviderManifest.SupportsIntersectAndUnionAllFlattening() ? 1 : 0);
			if (num != 0 && n.Child0.Op.OpType == op.OpType)
			{
				dbExpression = VisitSetOp((SetOp)n.Child0.Op, n.Child0, alias, setOpExpressionBuilder);
				goto IL_007e;
			}
		}
		else
		{
			num = 0;
		}
		dbExpression = VisitSetOpArgument(n.Child0, op.Outputs, op.VarMap[0]);
		goto IL_007e;
		IL_007e:
		DbExpression dbExpression2 = dbExpression;
		DbExpression dbExpression3 = ((num != 0 && n.Child1.Op.OpType == op.OpType) ? VisitSetOp((SetOp)n.Child1.Op, n.Child1, alias, setOpExpressionBuilder) : VisitSetOpArgument(n.Child1, op.Outputs, op.VarMap[1]));
		CollectionType edmType = TypeHelpers.GetEdmType<CollectionType>(TypeHelpers.GetCommonTypeUsage(dbExpression2.ResultType, dbExpression3.ResultType));
		IEnumerator<EdmProperty> enumerator = null;
		RowType type = null;
		if (TypeHelpers.TryGetEdmType<RowType>(edmType.TypeUsage, out type))
		{
			enumerator = type.Properties.GetEnumerator();
		}
		VarInfoList varInfoList = new VarInfoList();
		foreach (Var output in op.Outputs)
		{
			VarInfo varInfo = new VarInfo(output);
			if (type != null)
			{
				if (!enumerator.MoveNext())
				{
					PlanCompiler.Assert(condition: false, "Record columns don't match output vars");
				}
				varInfo.PrependProperty(enumerator.Current.Name);
			}
			varInfoList.Add(varInfo);
		}
		DbExpression dbExpression4 = setOpExpressionBuilder(dbExpression2, dbExpression3);
		PublishRelOp(alias.Next(), dbExpression4, varInfoList);
		return dbExpression4;
	}

	public override DbExpression Visit(UnionAllOp op, Node n)
	{
		return VisitSetOp(op, n, _unionAllAliases, DbExpressionBuilder.UnionAll);
	}

	public override DbExpression Visit(IntersectOp op, Node n)
	{
		return VisitSetOp(op, n, _intersectAliases, DbExpressionBuilder.Intersect);
	}

	public override DbExpression Visit(ExceptOp op, Node n)
	{
		return VisitSetOp(op, n, _exceptAliases, DbExpressionBuilder.Except);
	}

	public override DbExpression Visit(DerefOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(DistinctOp op, Node n)
	{
		RelOpInfo relOpInfo = BuildProjection(n.Child0, op.Keys);
		DbExpression dbExpression = relOpInfo.Publisher.Distinct();
		PublishRelOp(_distinctAliases.Next(), dbExpression, relOpInfo.PublishedVars);
		return dbExpression;
	}

	public override DbExpression Visit(SingleRowOp op, Node n)
	{
		RelOpInfo relOpInfo;
		DbExpression dbExpression;
		if (n.Child0.Op.OpType != OpType.Project)
		{
			ExtendedNodeInfo extendedNodeInfo = _iqtCommand.GetExtendedNodeInfo(n.Child0);
			relOpInfo = ((!extendedNodeInfo.Definitions.IsEmpty) ? BuildProjection(n.Child0, extendedNodeInfo.Definitions) : BuildEmptyProjection(n.Child0));
			dbExpression = relOpInfo.Publisher;
		}
		else
		{
			dbExpression = VisitNode(n.Child0);
			AssertRelOp(dbExpression);
			relOpInfo = ConsumeRelOp(dbExpression);
		}
		DbElementExpression item = dbExpression.Element();
		DbNewInstanceExpression dbNewInstanceExpression = DbExpressionBuilder.NewCollection(new List<DbExpression> { item });
		PublishRelOp(_elementAliases.Next(), dbNewInstanceExpression, relOpInfo.PublishedVars);
		return dbNewInstanceExpression;
	}

	public override DbExpression Visit(SingleRowTableOp op, Node n)
	{
		DbExpression[] elements = new DbConstantExpression[1] { DbExpressionBuilder.Constant(1) };
		DbNewInstanceExpression dbNewInstanceExpression = DbExpressionBuilder.NewCollection(elements);
		PublishRelOp(_singleRowTableAliases.Next(), dbNewInstanceExpression, new VarInfoList());
		return dbNewInstanceExpression;
	}

	public override DbExpression Visit(VarDefOp op, Node n)
	{
		PlanCompiler.Assert(condition: false, "Unexpected VarDefOp");
		throw new NotSupportedException(Strings.Iqt_CTGen_UnexpectedVarDef);
	}

	public override DbExpression Visit(VarDefListOp op, Node n)
	{
		PlanCompiler.Assert(condition: false, "Unexpected VarDefListOp");
		throw new NotSupportedException(Strings.Iqt_CTGen_UnexpectedVarDefList);
	}

	public override DbExpression Visit(PhysicalProjectOp op, Node n)
	{
		PlanCompiler.Assert(n.Children.Count == 1, "more than one input to physicalProjectOp?");
		VarList varList = new VarList();
		foreach (Var output in op.Outputs)
		{
			if (!varList.Contains(output))
			{
				varList.Add(output);
			}
		}
		op.Outputs.Clear();
		op.Outputs.AddRange(varList);
		return BuildProjection(n.Child0, op.Outputs).Publisher;
	}

	public override DbExpression Visit(SingleStreamNestOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override DbExpression Visit(MultiStreamNestOp op, Node n)
	{
		throw new NotSupportedException();
	}
}
