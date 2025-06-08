using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ITreeGenerator : DbExpressionVisitor<Node>
{
	private abstract class CqtVariableScope
	{
		internal abstract Node this[string varName] { get; }

		internal abstract bool Contains(string varName);

		internal abstract bool IsPredicate(string varName);
	}

	private class ExpressionBindingScope : CqtVariableScope
	{
		private readonly Command _tree;

		private readonly string _varName;

		private readonly Var _var;

		internal override Node this[string name]
		{
			get
			{
				PlanCompiler.Assert(name == _varName, "huh?");
				return _tree.CreateNode(_tree.CreateVarRefOp(_var));
			}
		}

		internal Var ScopeVar => _var;

		internal ExpressionBindingScope(Command iqtTree, string name, Var iqtVar)
		{
			_tree = iqtTree;
			_varName = name;
			_var = iqtVar;
		}

		internal override bool Contains(string name)
		{
			return _varName == name;
		}

		internal override bool IsPredicate(string varName)
		{
			return false;
		}
	}

	private sealed class LambdaScope : CqtVariableScope
	{
		private readonly ITreeGenerator _treeGen;

		private readonly Command _command;

		private readonly Dictionary<string, Tuple<Node, bool>> _arguments;

		private readonly Dictionary<Node, bool> _referencedArgs;

		internal override Node this[string name]
		{
			get
			{
				PlanCompiler.Assert(_arguments.ContainsKey(name), "LambdaScope indexer called for invalid Var");
				Node node = _arguments[name].Item1;
				if (_referencedArgs.ContainsKey(node))
				{
					VarMap varMap = null;
					Node node2 = OpCopier.Copy(_command, node, out varMap);
					if (varMap.Count > 0)
					{
						List<Node> list = new List<Node>(1);
						list.Add(node);
						List<Node> list2 = new List<Node>(1);
						list2.Add(node2);
						MapCopiedNodeVars(list, list2, varMap);
					}
					node = node2;
				}
				else
				{
					_referencedArgs[node] = true;
				}
				return node;
			}
		}

		internal LambdaScope(ITreeGenerator treeGen, Command command, Dictionary<string, Tuple<Node, bool>> args)
		{
			_treeGen = treeGen;
			_command = command;
			_arguments = args;
			_referencedArgs = new Dictionary<Node, bool>(_arguments.Count);
		}

		internal override bool Contains(string name)
		{
			return _arguments.ContainsKey(name);
		}

		internal override bool IsPredicate(string name)
		{
			PlanCompiler.Assert(_arguments.ContainsKey(name), "LambdaScope indexer called for invalid Var");
			return _arguments[name].Item2;
		}

		private void MapCopiedNodeVars(IList<Node> sources, IList<Node> copies, IDictionary<Var, Var> varMappings)
		{
			PlanCompiler.Assert(sources.Count == copies.Count, "Source/Copy Node count mismatch");
			for (int i = 0; i < sources.Count; i++)
			{
				Node node = sources[i];
				Node node2 = copies[i];
				if (node.Children.Count > 0)
				{
					MapCopiedNodeVars(node.Children, node2.Children, varMappings);
				}
				Var value = null;
				if (_treeGen.VarMap.TryGetValue(node, out value))
				{
					PlanCompiler.Assert(varMappings.ContainsKey(value), "No mapping found for Var in Var to Var map from OpCopier");
					_treeGen.VarMap[node2] = varMappings[value];
				}
			}
		}
	}

	private delegate Node VisitExprDelegate(DbExpression e);

	private class IsOfFilter
	{
		private readonly TypeUsage requiredType;

		private readonly bool isExact;

		private IsOfFilter next;

		internal IsOfFilter(DbIsOfExpression template)
		{
			requiredType = template.OfType;
			isExact = template.ExpressionKind == DbExpressionKind.IsOfOnly;
		}

		internal IsOfFilter(DbOfTypeExpression template)
		{
			requiredType = template.OfType;
			isExact = template.ExpressionKind == DbExpressionKind.OfTypeOnly;
		}

		private IsOfFilter(TypeUsage required, bool exact)
		{
			requiredType = required;
			isExact = exact;
		}

		private IsOfFilter Merge(TypeUsage otherRequiredType, bool otherIsExact)
		{
			bool flag = requiredType.EdmEquals(otherRequiredType);
			IsOfFilter isOfFilter;
			if (flag && isExact == otherIsExact)
			{
				isOfFilter = this;
			}
			else if (isExact && otherIsExact)
			{
				isOfFilter = new IsOfFilter(otherRequiredType, otherIsExact);
				isOfFilter.next = this;
			}
			else if (!isExact && !otherIsExact)
			{
				if (otherRequiredType.IsSubtypeOf(requiredType))
				{
					isOfFilter = new IsOfFilter(otherRequiredType, exact: false);
					isOfFilter.next = next;
				}
				else if (requiredType.IsSubtypeOf(otherRequiredType))
				{
					isOfFilter = this;
				}
				else
				{
					isOfFilter = new IsOfFilter(otherRequiredType, otherIsExact);
					isOfFilter.next = this;
				}
			}
			else if (flag)
			{
				isOfFilter = new IsOfFilter(otherRequiredType, exact: true);
				isOfFilter.next = next;
			}
			else
			{
				TypeUsage typeUsage = (isExact ? requiredType : otherRequiredType);
				TypeUsage typeUsage2 = (isExact ? otherRequiredType : requiredType);
				if (typeUsage.IsSubtypeOf(typeUsage2))
				{
					if (typeUsage == requiredType && isExact)
					{
						isOfFilter = this;
					}
					else
					{
						isOfFilter = new IsOfFilter(typeUsage, exact: true);
						isOfFilter.next = next;
					}
				}
				else
				{
					isOfFilter = new IsOfFilter(otherRequiredType, otherIsExact);
					isOfFilter.next = this;
				}
			}
			return isOfFilter;
		}

		internal IsOfFilter Merge(DbIsOfExpression other)
		{
			return Merge(other.OfType, other.ExpressionKind == DbExpressionKind.IsOfOnly);
		}

		internal IsOfFilter Merge(DbOfTypeExpression other)
		{
			return Merge(other.OfType, other.ExpressionKind == DbExpressionKind.OfTypeOnly);
		}

		internal IEnumerable<KeyValuePair<TypeUsage, bool>> ToEnumerable()
		{
			for (IsOfFilter currentFilter = this; currentFilter != null; currentFilter = currentFilter.next)
			{
				yield return new KeyValuePair<TypeUsage, bool>(currentFilter.requiredType, currentFilter.isExact);
			}
		}
	}

	private static readonly Dictionary<DbExpressionKind, OpType> _opMap = InitializeExpressionKindToOpTypeMap();

	private readonly bool _useDatabaseNullSemantics;

	private readonly Command _iqtCommand;

	private readonly Stack<CqtVariableScope> _varScopes = new Stack<CqtVariableScope>();

	private readonly Dictionary<Node, Var> _varMap = new Dictionary<Node, Var>();

	private readonly Stack<EdmFunction> _functionExpansions = new Stack<EdmFunction>();

	private readonly Dictionary<DbExpression, bool> _functionsIsPredicateFlag = new Dictionary<DbExpression, bool>();

	private readonly HashSet<DbFilterExpression> _processedIsOfFilters = new HashSet<DbFilterExpression>();

	private readonly HashSet<DbTreatExpression> _fakeTreats = new HashSet<DbTreatExpression>();

	private readonly DiscriminatorMap _discriminatorMap;

	private readonly DbProjectExpression _discriminatedViewTopProject;

	internal Dictionary<Node, Var> VarMap => _varMap;

	private static Dictionary<DbExpressionKind, OpType> InitializeExpressionKindToOpTypeMap()
	{
		return new Dictionary<DbExpressionKind, OpType>(12)
		{
			[DbExpressionKind.Plus] = OpType.Plus,
			[DbExpressionKind.Minus] = OpType.Minus,
			[DbExpressionKind.Multiply] = OpType.Multiply,
			[DbExpressionKind.Divide] = OpType.Divide,
			[DbExpressionKind.Modulo] = OpType.Modulo,
			[DbExpressionKind.UnaryMinus] = OpType.UnaryMinus,
			[DbExpressionKind.Equals] = OpType.EQ,
			[DbExpressionKind.NotEquals] = OpType.NE,
			[DbExpressionKind.LessThan] = OpType.LT,
			[DbExpressionKind.GreaterThan] = OpType.GT,
			[DbExpressionKind.LessThanOrEquals] = OpType.LE,
			[DbExpressionKind.GreaterThanOrEquals] = OpType.GE
		};
	}

	public static Command Generate(DbQueryCommandTree ctree)
	{
		return Generate(ctree, null);
	}

	internal static Command Generate(DbQueryCommandTree ctree, DiscriminatorMap discriminatorMap)
	{
		return new ITreeGenerator(ctree, discriminatorMap)._iqtCommand;
	}

	private ITreeGenerator(DbQueryCommandTree ctree, DiscriminatorMap discriminatorMap)
	{
		_useDatabaseNullSemantics = ctree.UseDatabaseNullSemantics;
		_iqtCommand = new Command(ctree.MetadataWorkspace);
		if (discriminatorMap != null)
		{
			_discriminatorMap = discriminatorMap;
			PlanCompiler.Assert(ctree.Query.ExpressionKind == DbExpressionKind.Project, "top level QMV expression must be project to match discriminator pattern");
			_discriminatedViewTopProject = (DbProjectExpression)ctree.Query;
		}
		foreach (KeyValuePair<string, TypeUsage> parameter in ctree.Parameters)
		{
			if (!ValidateParameterType(parameter.Value))
			{
				throw new NotSupportedException(Strings.ParameterTypeNotSupported(parameter.Key, parameter.Value.ToString()));
			}
			_iqtCommand.CreateParameterVar(parameter.Key, parameter.Value);
		}
		_iqtCommand.Root = VisitExpr(ctree.Query);
		if (!_iqtCommand.Root.Op.IsRelOp)
		{
			Node definingExpr = ConvertToScalarOpTree(_iqtCommand.Root, ctree.Query);
			Node arg = _iqtCommand.CreateNode(_iqtCommand.CreateSingleRowTableOp());
			Var computedVar;
			Node node = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar);
			ProjectOp op = _iqtCommand.CreateProjectOp(computedVar);
			Node root = _iqtCommand.CreateNode(op, arg, node);
			if (TypeSemantics.IsCollectionType(_iqtCommand.Root.Op.Type))
			{
				UnnestOp unnestOp = _iqtCommand.CreateUnnestOp(computedVar);
				root = _iqtCommand.CreateNode(unnestOp, node.Child0);
				computedVar = unnestOp.Table.Columns[0];
			}
			_iqtCommand.Root = root;
			_varMap[_iqtCommand.Root] = computedVar;
		}
		_iqtCommand.Root = CapWithPhysicalProject(_iqtCommand.Root);
	}

	private static bool ValidateParameterType(TypeUsage paramType)
	{
		if (paramType != null && paramType.EdmType != null)
		{
			if (!TypeSemantics.IsPrimitiveType(paramType))
			{
				return paramType.EdmType is EnumType;
			}
			return true;
		}
		return false;
	}

	private static RowType ExtractElementRowType(TypeUsage typeUsage)
	{
		return TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(typeUsage).TypeUsage);
	}

	private bool IsPredicate(DbExpression expr)
	{
		if (TypeSemantics.IsPrimitiveType(expr.ResultType, PrimitiveTypeKind.Boolean))
		{
			switch (expr.ExpressionKind)
			{
			case DbExpressionKind.All:
			case DbExpressionKind.And:
			case DbExpressionKind.Any:
			case DbExpressionKind.Equals:
			case DbExpressionKind.GreaterThan:
			case DbExpressionKind.GreaterThanOrEquals:
			case DbExpressionKind.IsEmpty:
			case DbExpressionKind.IsNull:
			case DbExpressionKind.IsOf:
			case DbExpressionKind.IsOfOnly:
			case DbExpressionKind.LessThan:
			case DbExpressionKind.LessThanOrEquals:
			case DbExpressionKind.Like:
			case DbExpressionKind.Not:
			case DbExpressionKind.NotEquals:
			case DbExpressionKind.Or:
			case DbExpressionKind.In:
				return true;
			case DbExpressionKind.VariableReference:
			{
				DbVariableReferenceExpression dbVariableReferenceExpression = (DbVariableReferenceExpression)expr;
				return ResolveScope(dbVariableReferenceExpression).IsPredicate(dbVariableReferenceExpression.VariableName);
			}
			case DbExpressionKind.Lambda:
			{
				if (_functionsIsPredicateFlag.TryGetValue(expr, out var value2))
				{
					return value2;
				}
				PlanCompiler.Assert(condition: false, "IsPredicate must be called on a visited lambda expression");
				return false;
			}
			case DbExpressionKind.Function:
				if (((DbFunctionExpression)expr).Function.HasUserDefinedBody)
				{
					if (_functionsIsPredicateFlag.TryGetValue(expr, out var value))
					{
						return value;
					}
					PlanCompiler.Assert(condition: false, "IsPredicate must be called on a visited function expression");
					return false;
				}
				return false;
			default:
				return false;
			}
		}
		return false;
	}

	private Node VisitExpr(DbExpression e)
	{
		return e?.Accept(this);
	}

	private Node VisitExprAsScalar(DbExpression expr)
	{
		if (expr == null)
		{
			return null;
		}
		Node node = VisitExpr(expr);
		return ConvertToScalarOpTree(node, expr);
	}

	private Node ConvertToScalarOpTree(Node node, DbExpression expr)
	{
		if (node.Op.IsRelOp)
		{
			node = ConvertRelOpToScalarOpTree(node, expr.ResultType);
		}
		else if (IsPredicate(expr))
		{
			node = ConvertPredicateToScalarOpTree(node, expr);
		}
		return node;
	}

	private Node ConvertRelOpToScalarOpTree(Node node, TypeUsage resultType)
	{
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(resultType), "RelOp with non-Collection result type");
		CollectOp op = _iqtCommand.CreateCollectOp(resultType);
		Node arg = CapWithPhysicalProject(node);
		node = _iqtCommand.CreateNode(op, arg);
		return node;
	}

	private Node ConvertPredicateToScalarOpTree(Node node, DbExpression expr)
	{
		CaseOp op = _iqtCommand.CreateCaseOp(_iqtCommand.BooleanType);
		bool num = IsNullable(expr);
		List<Node> list = new List<Node>(num ? 5 : 3)
		{
			node,
			_iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true))
		};
		if (num)
		{
			Node arg = VisitExpr(expr);
			list.Add(_iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), arg));
		}
		list.Add(_iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, false)));
		if (num)
		{
			list.Add(_iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.BooleanType)));
		}
		node = _iqtCommand.CreateNode(op, list);
		return node;
	}

	private bool IsNullable(DbExpression expression)
	{
		switch (expression.ExpressionKind)
		{
		case DbExpressionKind.All:
		case DbExpressionKind.Any:
		case DbExpressionKind.IsEmpty:
		case DbExpressionKind.IsNull:
			return false;
		case DbExpressionKind.Not:
			return IsNullable(((DbNotExpression)expression).Argument);
		case DbExpressionKind.And:
		case DbExpressionKind.Or:
		{
			DbBinaryExpression dbBinaryExpression = (DbBinaryExpression)expression;
			if (!IsNullable(dbBinaryExpression.Left))
			{
				return IsNullable(dbBinaryExpression.Right);
			}
			return true;
		}
		default:
			return true;
		}
	}

	private Node VisitExprAsPredicate(DbExpression expr)
	{
		if (expr == null)
		{
			return null;
		}
		Node node = VisitExpr(expr);
		if (!IsPredicate(expr))
		{
			ComparisonOp op = _iqtCommand.CreateComparisonOp(OpType.EQ);
			Node arg = _iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true));
			node = _iqtCommand.CreateNode(op, node, arg);
		}
		else
		{
			PlanCompiler.Assert(!node.Op.IsRelOp, "unexpected relOp as predicate?");
		}
		return node;
	}

	private static IList<Node> VisitExpr(IList<DbExpression> exprs, VisitExprDelegate exprDelegate)
	{
		List<Node> list = new List<Node>();
		for (int i = 0; i < exprs.Count; i++)
		{
			list.Add(exprDelegate(exprs[i]));
		}
		return list;
	}

	private IList<Node> VisitExprAsScalar(IList<DbExpression> exprs)
	{
		return VisitExpr(exprs, VisitExprAsScalar);
	}

	private Node VisitUnary(DbUnaryExpression e, Op op, VisitExprDelegate exprDelegate)
	{
		return _iqtCommand.CreateNode(op, exprDelegate(e.Argument));
	}

	private Node VisitBinary(DbBinaryExpression e, Op op, VisitExprDelegate exprDelegate)
	{
		return _iqtCommand.CreateNode(op, exprDelegate(e.Left), exprDelegate(e.Right));
	}

	private Node EnsureRelOp(Node inputNode)
	{
		Op op = inputNode.Op;
		if (op.IsRelOp)
		{
			return inputNode;
		}
		ScalarOp obj = op as ScalarOp;
		PlanCompiler.Assert(obj != null, "An expression in a CQT produced a non-ScalarOp and non-RelOp output Op");
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(obj.Type), "An expression used as a RelOp argument was neither a RelOp or a collection");
		if (op is CollectOp)
		{
			PlanCompiler.Assert(inputNode.HasChild0, "CollectOp without argument");
			if (inputNode.Child0.Op is PhysicalProjectOp)
			{
				PlanCompiler.Assert(inputNode.Child0.HasChild0, "PhysicalProjectOp without argument");
				PlanCompiler.Assert(inputNode.Child0.Child0.Op.IsRelOp, "PhysicalProjectOp applied to non-RelOp input");
				return inputNode.Child0.Child0;
			}
		}
		Var computedVar;
		Node arg = _iqtCommand.CreateVarDefNode(inputNode, out computedVar);
		UnnestOp unnestOp = _iqtCommand.CreateUnnestOp(computedVar);
		PlanCompiler.Assert(unnestOp.Table.Columns.Count == 1, "Un-nest of collection ScalarOp produced unexpected number of columns (1 expected)");
		Node node = _iqtCommand.CreateNode(unnestOp, arg);
		_varMap[node] = unnestOp.Table.Columns[0];
		Node definingExpr = _iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(unnestOp.Table.Columns[0]));
		Var computedVar2;
		Node arg2 = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar2);
		ProjectOp op2 = _iqtCommand.CreateProjectOp(computedVar2);
		Node node2 = _iqtCommand.CreateNode(op2, node, arg2);
		_varMap[node2] = computedVar2;
		return node2;
	}

	private Node CapWithProject(Node input)
	{
		PlanCompiler.Assert(input.Op.IsRelOp, "unexpected non-RelOp?");
		if (input.Op.OpType == OpType.Project)
		{
			return input;
		}
		Var var = _varMap[input];
		ProjectOp op = _iqtCommand.CreateProjectOp(var);
		Node node = _iqtCommand.CreateNode(op, input, _iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp()));
		_varMap[node] = var;
		return node;
	}

	private Node CapWithPhysicalProject(Node input)
	{
		PlanCompiler.Assert(input.Op.IsRelOp, "unexpected non-RelOp?");
		Var outputVar = _varMap[input];
		PhysicalProjectOp op = _iqtCommand.CreatePhysicalProjectOp(outputVar);
		return _iqtCommand.CreateNode(op, input);
	}

	private Node EnterExpressionBinding(DbExpressionBinding binding)
	{
		return VisitBoundExpressionPushBindingScope(binding.Expression, binding.VariableName);
	}

	private Node EnterGroupExpressionBinding(DbGroupExpressionBinding binding)
	{
		return VisitBoundExpressionPushBindingScope(binding.Expression, binding.VariableName);
	}

	private Node VisitBoundExpressionPushBindingScope(DbExpression boundExpression, string bindingName)
	{
		Var boundVar;
		Node result = VisitBoundExpression(boundExpression, out boundVar);
		PushBindingScope(boundVar, bindingName);
		return result;
	}

	private Node VisitBoundExpression(DbExpression boundExpression, out Var boundVar)
	{
		Node node = VisitExpr(boundExpression);
		PlanCompiler.Assert(node != null, "DbExpressionBinding.Expression produced null conversion");
		node = EnsureRelOp(node);
		boundVar = _varMap[node];
		PlanCompiler.Assert(boundVar != null, "No Var found for Input Op");
		return node;
	}

	private void PushBindingScope(Var boundVar, string bindingName)
	{
		_varScopes.Push(new ExpressionBindingScope(_iqtCommand, bindingName, boundVar));
	}

	private ExpressionBindingScope ExitExpressionBinding()
	{
		ExpressionBindingScope obj = _varScopes.Pop() as ExpressionBindingScope;
		PlanCompiler.Assert(obj != null, "ExitExpressionBinding called without ExpressionBindingScope on top of scope stack");
		return obj;
	}

	private void ExitGroupExpressionBinding()
	{
		PlanCompiler.Assert(_varScopes.Pop() is ExpressionBindingScope, "ExitGroupExpressionBinding called without ExpressionBindingScope on top of scope stack");
	}

	private void EnterLambdaFunction(DbLambda lambda, List<Tuple<Node, bool>> argumentValues, EdmFunction expandingEdmFunction)
	{
		IList<DbVariableReferenceExpression> variables = lambda.Variables;
		Dictionary<string, Tuple<Node, bool>> dictionary = new Dictionary<string, Tuple<Node, bool>>();
		int num = 0;
		foreach (Tuple<Node, bool> argumentValue in argumentValues)
		{
			dictionary.Add(variables[num].VariableName, argumentValue);
			num++;
		}
		if (expandingEdmFunction != null)
		{
			if (_functionExpansions.Contains(expandingEdmFunction))
			{
				throw new EntityCommandCompilationException(Strings.Cqt_UDF_FunctionDefinitionWithCircularReference(expandingEdmFunction.FullName), null);
			}
			_functionExpansions.Push(expandingEdmFunction);
		}
		_varScopes.Push(new LambdaScope(this, _iqtCommand, dictionary));
	}

	private LambdaScope ExitLambdaFunction(EdmFunction expandingEdmFunction)
	{
		LambdaScope obj = _varScopes.Pop() as LambdaScope;
		PlanCompiler.Assert(obj != null, "ExitLambdaFunction called without LambdaScope on top of scope stack");
		if (expandingEdmFunction != null)
		{
			PlanCompiler.Assert(_functionExpansions.Pop() == expandingEdmFunction, "Function expansion stack corruption: unexpected function at the top of the stack");
		}
		return obj;
	}

	private Node ProjectNewRecord(Node inputNode, RowType recType, IEnumerable<Var> colVars)
	{
		List<Node> list = new List<Node>();
		foreach (Var colVar in colVars)
		{
			list.Add(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(colVar)));
		}
		Node definingExpr = _iqtCommand.CreateNode(_iqtCommand.CreateNewRecordOp(recType), list);
		Var computedVar;
		Node arg = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar);
		ProjectOp op = _iqtCommand.CreateProjectOp(computedVar);
		Node node = _iqtCommand.CreateNode(op, inputNode, arg);
		_varMap[node] = computedVar;
		return node;
	}

	public override Node Visit(DbExpression e)
	{
		Check.NotNull(e, "e");
		throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
	}

	public override Node Visit(DbConstantExpression e)
	{
		Check.NotNull(e, "e");
		ConstantBaseOp op = _iqtCommand.CreateConstantOp(e.ResultType, e.GetValue());
		return _iqtCommand.CreateNode(op);
	}

	public override Node Visit(DbNullExpression e)
	{
		Check.NotNull(e, "e");
		NullOp op = _iqtCommand.CreateNullOp(e.ResultType);
		return _iqtCommand.CreateNode(op);
	}

	public override Node Visit(DbVariableReferenceExpression e)
	{
		Check.NotNull(e, "e");
		return ResolveScope(e)[e.VariableName];
	}

	private CqtVariableScope ResolveScope(DbVariableReferenceExpression e)
	{
		foreach (CqtVariableScope varScope in _varScopes)
		{
			if (varScope.Contains(e.VariableName))
			{
				return varScope;
			}
		}
		PlanCompiler.Assert(condition: false, "CQT VarRef could not be resolved in the variable scope stack");
		return null;
	}

	public override Node Visit(DbParameterReferenceExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateVarRefOp(_iqtCommand.GetParameter(e.ParameterName));
		return _iqtCommand.CreateNode(op);
	}

	public override Node Visit(DbFunctionExpression e)
	{
		Check.NotNull(e, "e");
		Node node = null;
		if (e.Function.IsModelDefinedFunction)
		{
			DbLambda generatedFunctionDefinition;
			try
			{
				generatedFunctionDefinition = _iqtCommand.MetadataWorkspace.GetGeneratedFunctionDefinition(e.Function);
			}
			catch (Exception ex)
			{
				if (ex.IsCatchableExceptionType())
				{
					throw new EntityCommandCompilationException(Strings.Cqt_UDF_FunctionDefinitionGenerationFailed(e.Function.FullName), ex);
				}
				throw;
			}
			return VisitLambdaExpression(generatedFunctionDefinition, e.Arguments, e, e.Function);
		}
		List<Node> list = new List<Node>(e.Arguments.Count);
		for (int i = 0; i < e.Arguments.Count; i++)
		{
			list.Add(BuildSoftCast(VisitExprAsScalar(e.Arguments[i]), e.Function.Parameters[i].TypeUsage));
		}
		return _iqtCommand.CreateNode(_iqtCommand.CreateFunctionOp(e.Function), list);
	}

	public override Node Visit(DbLambdaExpression e)
	{
		Check.NotNull(e, "e");
		return VisitLambdaExpression(e.Lambda, e.Arguments, e, null);
	}

	private Node VisitLambdaExpression(DbLambda lambda, IList<DbExpression> arguments, DbExpression applicationExpr, EdmFunction expandingEdmFunction)
	{
		List<Tuple<Node, bool>> list = new List<Tuple<Node, bool>>(arguments.Count);
		foreach (DbExpression argument in arguments)
		{
			list.Add(Tuple.Create(VisitExpr(argument), IsPredicate(argument)));
		}
		EnterLambdaFunction(lambda, list, expandingEdmFunction);
		Node result = VisitExpr(lambda.Body);
		_functionsIsPredicateFlag[applicationExpr] = IsPredicate(lambda.Body);
		ExitLambdaFunction(expandingEdmFunction);
		return result;
	}

	private Node BuildSoftCast(Node node, TypeUsage targetType)
	{
		if (node.Op.IsRelOp)
		{
			targetType = TypeHelpers.GetEdmType<CollectionType>(targetType).TypeUsage;
			Var var = _varMap[node];
			if (Command.EqualTypes(targetType, var.Type))
			{
				return node;
			}
			Node arg = _iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(var));
			Node definingExpr = _iqtCommand.CreateNode(_iqtCommand.CreateSoftCastOp(targetType), arg);
			Var computedVar;
			Node arg2 = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar);
			ProjectOp op = _iqtCommand.CreateProjectOp(computedVar);
			Node node2 = _iqtCommand.CreateNode(op, node, arg2);
			_varMap[node2] = computedVar;
			return node2;
		}
		PlanCompiler.Assert(node.Op.IsScalarOp, "I want a scalar op");
		if (Command.EqualTypes(node.Op.Type, targetType))
		{
			return node;
		}
		SoftCastOp op2 = _iqtCommand.CreateSoftCastOp(targetType);
		return _iqtCommand.CreateNode(op2, node);
	}

	private Node BuildSoftCast(Node node, EdmType targetType)
	{
		return BuildSoftCast(node, TypeUsage.Create(targetType));
	}

	private Node BuildEntityRef(Node arg, TypeUsage entityType)
	{
		TypeUsage type = TypeHelpers.CreateReferenceTypeUsage((EntityType)entityType.EdmType);
		return _iqtCommand.CreateNode(_iqtCommand.CreateGetEntityRefOp(type), arg);
	}

	private static bool TryRewriteKeyPropertyAccess(DbPropertyExpression propertyExpression, out DbExpression rewritten)
	{
		if (propertyExpression.Instance.ExpressionKind == DbExpressionKind.Property && Helper.IsEntityType(propertyExpression.Instance.ResultType.EdmType))
		{
			EntityType entityType = (EntityType)propertyExpression.Instance.ResultType.EdmType;
			DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)propertyExpression.Instance;
			if (Helper.IsNavigationProperty(dbPropertyExpression.Property) && entityType.KeyMembers.Contains(propertyExpression.Property))
			{
				NavigationProperty navigationProperty = (NavigationProperty)dbPropertyExpression.Property;
				DbExpression argument = dbPropertyExpression.Instance.GetEntityRef().Navigate(navigationProperty.FromEndMember, navigationProperty.ToEndMember);
				rewritten = argument.GetRefKey();
				rewritten = rewritten.Property(propertyExpression.Property.Name);
				return true;
			}
		}
		rewritten = null;
		return false;
	}

	public override Node Visit(DbPropertyExpression e)
	{
		Check.NotNull(e, "e");
		if (BuiltInTypeKind.EdmProperty != e.Property.BuiltInTypeKind && e.Property.BuiltInTypeKind != 0 && BuiltInTypeKind.NavigationProperty != e.Property.BuiltInTypeKind)
		{
			throw new NotSupportedException();
		}
		PlanCompiler.Assert(e.Instance != null, "Static properties are not supported");
		Node node = null;
		if (TryRewriteKeyPropertyAccess(e, out var rewritten))
		{
			return VisitExpr(rewritten);
		}
		Node node2 = VisitExpr(e.Instance);
		if (e.Instance.ExpressionKind == DbExpressionKind.NewInstance && Helper.IsStructuralType(e.Instance.ResultType.EdmType))
		{
			IList allStructuralMembers = Helper.GetAllStructuralMembers(e.Instance.ResultType.EdmType);
			int num = -1;
			for (int i = 0; i < allStructuralMembers.Count; i++)
			{
				if (string.Equals(e.Property.Name, ((EdmMember)allStructuralMembers[i]).Name, StringComparison.Ordinal))
				{
					num = i;
					break;
				}
			}
			PlanCompiler.Assert(num > -1, "The specified property was not found");
			node = node2.Children[num];
			return BuildSoftCast(node, e.ResultType);
		}
		Op op = _iqtCommand.CreatePropertyOp(e.Property);
		node2 = BuildSoftCast(node2, e.Property.DeclaringType);
		return _iqtCommand.CreateNode(op, node2);
	}

	public override Node Visit(DbComparisonExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateComparisonOp(_opMap[e.ExpressionKind]);
		Node node = VisitExprAsScalar(e.Left);
		Node node2 = VisitExprAsScalar(e.Right);
		TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(e.Left.ResultType, e.Right.ResultType);
		if (!Command.EqualTypes(e.Left.ResultType, e.Right.ResultType))
		{
			node = BuildSoftCast(node, commonTypeUsage);
			node2 = BuildSoftCast(node2, commonTypeUsage);
		}
		if (TypeSemantics.IsEntityType(commonTypeUsage) && (e.ExpressionKind == DbExpressionKind.Equals || e.ExpressionKind == DbExpressionKind.NotEquals))
		{
			node = BuildEntityRef(node, commonTypeUsage);
			node2 = BuildEntityRef(node2, commonTypeUsage);
		}
		return _iqtCommand.CreateNode(op, node, node2);
	}

	public override Node Visit(DbLikeExpression e)
	{
		Check.NotNull(e, "e");
		return _iqtCommand.CreateNode(_iqtCommand.CreateLikeOp(), VisitExpr(e.Argument), VisitExpr(e.Pattern), VisitExpr(e.Escape));
	}

	private Node CreateLimitNode(Node inputNode, Node limitNode, bool withTies)
	{
		Node node = null;
		if (OpType.ConstrainedSort == inputNode.Op.OpType && OpType.Null == inputNode.Child2.Op.OpType)
		{
			inputNode.Child2 = limitNode;
			if (withTies)
			{
				((ConstrainedSortOp)inputNode.Op).WithTies = true;
			}
			return inputNode;
		}
		if (OpType.Sort == inputNode.Op.OpType)
		{
			return _iqtCommand.CreateNode(_iqtCommand.CreateConstrainedSortOp(((SortOp)inputNode.Op).Keys, withTies), inputNode.Child0, _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType)), limitNode);
		}
		return _iqtCommand.CreateNode(_iqtCommand.CreateConstrainedSortOp(new List<SortKey>(), withTies), inputNode, _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType)), limitNode);
	}

	public override Node Visit(DbLimitExpression expression)
	{
		Check.NotNull(expression, "expression");
		Node node = EnsureRelOp(VisitExpr(expression.Argument));
		Var value = _varMap[node];
		Node limitNode = VisitExprAsScalar(expression.Limit);
		Node node2;
		if (OpType.Project == node.Op.OpType && (node.Child0.Op.OpType == OpType.Sort || node.Child0.Op.OpType == OpType.ConstrainedSort))
		{
			node.Child0 = CreateLimitNode(node.Child0, limitNode, expression.WithTies);
			node2 = node;
		}
		else
		{
			node2 = CreateLimitNode(node, limitNode, expression.WithTies);
		}
		if (node2 != node)
		{
			_varMap[node2] = value;
		}
		return node2;
	}

	public override Node Visit(DbIsNullExpression e)
	{
		Check.NotNull(e, "e");
		bool flag = false;
		if (e.Argument.ExpressionKind == DbExpressionKind.IsNull)
		{
			flag = true;
		}
		else if (e.Argument.ExpressionKind == DbExpressionKind.Not && ((DbNotExpression)e.Argument).Argument.ExpressionKind == DbExpressionKind.IsNull)
		{
			flag = true;
		}
		Op op = _iqtCommand.CreateConditionalOp(OpType.IsNull);
		if (flag)
		{
			return _iqtCommand.CreateNode(op, _iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true)));
		}
		Node node = VisitExprAsScalar(e.Argument);
		if (TypeSemantics.IsEntityType(e.Argument.ResultType))
		{
			node = BuildEntityRef(node, e.Argument.ResultType);
		}
		return _iqtCommand.CreateNode(op, node);
	}

	public override Node Visit(DbArithmeticExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateArithmeticOp(_opMap[e.ExpressionKind], e.ResultType);
		List<Node> list = new List<Node>();
		foreach (DbExpression argument in e.Arguments)
		{
			Node node = VisitExprAsScalar(argument);
			list.Add(BuildSoftCast(node, e.ResultType));
		}
		return _iqtCommand.CreateNode(op, list);
	}

	public override Node Visit(DbAndExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateConditionalOp(OpType.And);
		return VisitBinary(e, op, VisitExprAsPredicate);
	}

	public override Node Visit(DbOrExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateConditionalOp(OpType.Or);
		return VisitBinary(e, op, VisitExprAsPredicate);
	}

	public override Node Visit(DbInExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateConditionalOp(OpType.In);
		List<Node> list = new List<Node>(1 + e.List.Count) { VisitExpr(e.Item) };
		list.AddRange(e.List.Select(VisitExpr));
		return _iqtCommand.CreateNode(op, list);
	}

	public override Node Visit(DbNotExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateConditionalOp(OpType.Not);
		return VisitUnary(e, op, VisitExprAsPredicate);
	}

	public override Node Visit(DbDistinctExpression e)
	{
		Check.NotNull(e, "e");
		Node node = EnsureRelOp(VisitExpr(e.Argument));
		Var var = _varMap[node];
		Op op = _iqtCommand.CreateDistinctOp(var);
		Node node2 = _iqtCommand.CreateNode(op, node);
		_varMap[node2] = var;
		return node2;
	}

	public override Node Visit(DbElementExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateElementOp(e.ResultType);
		Node node = EnsureRelOp(VisitExpr(e.Argument));
		node = BuildSoftCast(node, TypeHelpers.CreateCollectionTypeUsage(e.ResultType));
		Var value = _varMap[node];
		node = _iqtCommand.CreateNode(_iqtCommand.CreateSingleRowOp(), node);
		_varMap[node] = value;
		node = CapWithProject(node);
		return _iqtCommand.CreateNode(op, node);
	}

	public override Node Visit(DbIsEmptyExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateExistsOp();
		Node arg = EnsureRelOp(VisitExpr(e.Argument));
		return _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), _iqtCommand.CreateNode(op, arg));
	}

	private Node VisitSetOpExpression(DbBinaryExpression expression)
	{
		PlanCompiler.Assert(DbExpressionKind.Except == expression.ExpressionKind || DbExpressionKind.Intersect == expression.ExpressionKind || DbExpressionKind.UnionAll == expression.ExpressionKind, "Non-SetOp DbExpression used as argument to VisitSetOpExpression");
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(expression.ResultType), "SetOp DbExpression does not have collection result type?");
		Node node = EnsureRelOp(VisitExpr(expression.Left));
		Node node2 = EnsureRelOp(VisitExpr(expression.Right));
		node = BuildSoftCast(node, expression.ResultType);
		node2 = BuildSoftCast(node2, expression.ResultType);
		Var var = _iqtCommand.CreateSetOpVar(TypeHelpers.GetEdmType<CollectionType>(expression.ResultType).TypeUsage);
		VarMap varMap = new VarMap();
		varMap.Add(var, _varMap[node]);
		VarMap varMap2 = new VarMap();
		varMap2.Add(var, _varMap[node2]);
		Op op = null;
		switch (expression.ExpressionKind)
		{
		case DbExpressionKind.Except:
			op = _iqtCommand.CreateExceptOp(varMap, varMap2);
			break;
		case DbExpressionKind.Intersect:
			op = _iqtCommand.CreateIntersectOp(varMap, varMap2);
			break;
		case DbExpressionKind.UnionAll:
			op = _iqtCommand.CreateUnionAllOp(varMap, varMap2);
			break;
		}
		Node node3 = _iqtCommand.CreateNode(op, node, node2);
		_varMap[node3] = var;
		return node3;
	}

	public override Node Visit(DbUnionAllExpression e)
	{
		Check.NotNull(e, "e");
		return VisitSetOpExpression(e);
	}

	public override Node Visit(DbIntersectExpression e)
	{
		Check.NotNull(e, "e");
		return VisitSetOpExpression(e);
	}

	public override Node Visit(DbExceptExpression e)
	{
		Check.NotNull(e, "e");
		return VisitSetOpExpression(e);
	}

	public override Node Visit(DbTreatExpression e)
	{
		Check.NotNull(e, "e");
		Op op = ((!_fakeTreats.Contains(e)) ? _iqtCommand.CreateTreatOp(e.ResultType) : _iqtCommand.CreateFakeTreatOp(e.ResultType));
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbIsOfExpression e)
	{
		Check.NotNull(e, "e");
		Op op = null;
		op = ((DbExpressionKind.IsOfOnly != e.ExpressionKind) ? _iqtCommand.CreateIsOfOp(e.OfType) : _iqtCommand.CreateIsOfOnlyOp(e.OfType));
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbCastExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateCastOp(e.ResultType);
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbCaseExpression e)
	{
		Check.NotNull(e, "e");
		List<Node> list = new List<Node>();
		for (int i = 0; i < e.When.Count; i++)
		{
			list.Add(VisitExprAsPredicate(e.When[i]));
			list.Add(BuildSoftCast(VisitExprAsScalar(e.Then[i]), e.ResultType));
		}
		list.Add(BuildSoftCast(VisitExprAsScalar(e.Else), e.ResultType));
		return _iqtCommand.CreateNode(_iqtCommand.CreateCaseOp(e.ResultType), list);
	}

	private DbFilterExpression CreateIsOfFilterExpression(DbExpression input, IsOfFilter typeFilter)
	{
		DbExpressionBinding resultBinding = input.Bind();
		DbExpression predicate = Helpers.BuildBalancedTreeInPlace(new List<DbExpression>((from tf in typeFilter.ToEnumerable()
			select (!tf.Value) ? resultBinding.Variable.IsOf(tf.Key) : resultBinding.Variable.IsOfOnly(tf.Key)).ToList()), (DbExpression left, DbExpression right) => left.And(right));
		DbFilterExpression dbFilterExpression = resultBinding.Filter(predicate);
		_processedIsOfFilters.Add(dbFilterExpression);
		return dbFilterExpression;
	}

	private static bool IsIsOfFilter(DbFilterExpression filter)
	{
		if (filter.Predicate.ExpressionKind != DbExpressionKind.IsOf && filter.Predicate.ExpressionKind != DbExpressionKind.IsOfOnly)
		{
			return false;
		}
		DbExpression argument = ((DbIsOfExpression)filter.Predicate).Argument;
		if (argument.ExpressionKind == DbExpressionKind.VariableReference)
		{
			return ((DbVariableReferenceExpression)argument).VariableName == filter.Input.VariableName;
		}
		return false;
	}

	private DbExpression ApplyIsOfFilter(DbExpression current, IsOfFilter typeFilter)
	{
		switch (current.ExpressionKind)
		{
		case DbExpressionKind.Distinct:
			return ApplyIsOfFilter(((DbDistinctExpression)current).Argument, typeFilter).Distinct();
		case DbExpressionKind.Filter:
		{
			DbFilterExpression dbFilterExpression = (DbFilterExpression)current;
			if (IsIsOfFilter(dbFilterExpression))
			{
				DbIsOfExpression other = (DbIsOfExpression)dbFilterExpression.Predicate;
				typeFilter = typeFilter.Merge(other);
				return ApplyIsOfFilter(dbFilterExpression.Input.Expression, typeFilter);
			}
			return ApplyIsOfFilter(dbFilterExpression.Input.Expression, typeFilter).BindAs(dbFilterExpression.Input.VariableName).Filter(dbFilterExpression.Predicate);
		}
		case DbExpressionKind.OfType:
		case DbExpressionKind.OfTypeOnly:
		{
			DbOfTypeExpression dbOfTypeExpression = (DbOfTypeExpression)current;
			typeFilter = typeFilter.Merge(dbOfTypeExpression);
			DbExpressionBinding dbExpressionBinding = ApplyIsOfFilter(dbOfTypeExpression.Argument, typeFilter).Bind();
			DbTreatExpression dbTreatExpression = dbExpressionBinding.Variable.TreatAs(dbOfTypeExpression.OfType);
			_fakeTreats.Add(dbTreatExpression);
			return dbExpressionBinding.Project(dbTreatExpression);
		}
		case DbExpressionKind.Project:
		{
			DbProjectExpression dbProjectExpression = (DbProjectExpression)current;
			if (dbProjectExpression.Projection.ExpressionKind == DbExpressionKind.VariableReference && ((DbVariableReferenceExpression)dbProjectExpression.Projection).VariableName == dbProjectExpression.Input.VariableName)
			{
				return ApplyIsOfFilter(dbProjectExpression.Input.Expression, typeFilter);
			}
			return CreateIsOfFilterExpression(current, typeFilter);
		}
		case DbExpressionKind.Sort:
		{
			DbSortExpression dbSortExpression = (DbSortExpression)current;
			return ApplyIsOfFilter(dbSortExpression.Input.Expression, typeFilter).BindAs(dbSortExpression.Input.VariableName).Sort(dbSortExpression.SortOrder);
		}
		default:
			return CreateIsOfFilterExpression(current, typeFilter);
		}
	}

	public override Node Visit(DbOfTypeExpression e)
	{
		Check.NotNull(e, "e");
		PlanCompiler.Assert(TypeSemantics.IsCollectionType(e.Argument.ResultType), "Non-Collection Type Argument in DbOfTypeExpression");
		DbExpression e2 = ApplyIsOfFilter(e.Argument, new IsOfFilter(e));
		Node node = EnsureRelOp(VisitExpr(e2));
		Var inputVar = _varMap[node];
		Var resultVar;
		Node node2 = _iqtCommand.BuildFakeTreatProject(node, inputVar, e.OfType, out resultVar);
		_varMap[node2] = resultVar;
		return node2;
	}

	public override Node Visit(DbNewInstanceExpression e)
	{
		Check.NotNull(e, "e");
		Op op = null;
		List<Node> list = null;
		if (TypeSemantics.IsCollectionType(e.ResultType))
		{
			op = _iqtCommand.CreateNewMultisetOp(e.ResultType);
		}
		else if (TypeSemantics.IsRowType(e.ResultType))
		{
			op = _iqtCommand.CreateNewRecordOp(e.ResultType);
		}
		else if (TypeSemantics.IsEntityType(e.ResultType))
		{
			List<RelProperty> list2 = new List<RelProperty>();
			list = new List<Node>();
			if (e.HasRelatedEntityReferences)
			{
				foreach (DbRelatedEntityRef relatedEntityReference in e.RelatedEntityReferences)
				{
					RelProperty item = new RelProperty((RelationshipType)relatedEntityReference.TargetEnd.DeclaringType, relatedEntityReference.SourceEnd, relatedEntityReference.TargetEnd);
					list2.Add(item);
					Node item2 = VisitExprAsScalar(relatedEntityReference.TargetEntityReference);
					list.Add(item2);
				}
			}
			op = _iqtCommand.CreateNewEntityOp(e.ResultType, list2);
		}
		else
		{
			op = _iqtCommand.CreateNewInstanceOp(e.ResultType);
		}
		List<Node> list3 = new List<Node>();
		if (TypeSemantics.IsStructuralType(e.ResultType))
		{
			StructuralType edmType = TypeHelpers.GetEdmType<StructuralType>(e.ResultType);
			int num = 0;
			foreach (EdmMember allStructuralMember in TypeHelpers.GetAllStructuralMembers(edmType))
			{
				Node item3 = BuildSoftCast(VisitExprAsScalar(e.Arguments[num]), Helper.GetModelTypeUsage(allStructuralMember));
				list3.Add(item3);
				num++;
			}
		}
		else
		{
			TypeUsage typeUsage = TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
			foreach (DbExpression argument in e.Arguments)
			{
				Node item4 = BuildSoftCast(VisitExprAsScalar(argument), typeUsage);
				list3.Add(item4);
			}
		}
		if (list != null)
		{
			list3.AddRange(list);
		}
		return _iqtCommand.CreateNode(op, list3);
	}

	public override Node Visit(DbRefExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateRefOp(e.EntitySet, e.ResultType);
		Node arg = BuildSoftCast(VisitExprAsScalar(e.Argument), TypeHelpers.CreateKeyRowType(e.EntitySet.ElementType));
		return _iqtCommand.CreateNode(op, arg);
	}

	public override Node Visit(DbRelationshipNavigationExpression e)
	{
		Check.NotNull(e, "e");
		RelProperty relProperty = new RelProperty(e.Relationship, e.NavigateFrom, e.NavigateTo);
		Op op = _iqtCommand.CreateNavigateOp(e.ResultType, relProperty);
		Node arg = VisitExprAsScalar(e.NavigationSource);
		return _iqtCommand.CreateNode(op, arg);
	}

	public override Node Visit(DbDerefExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateDerefOp(e.ResultType);
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbRefKeyExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateGetRefKeyOp(e.ResultType);
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbEntityRefExpression e)
	{
		Check.NotNull(e, "e");
		Op op = _iqtCommand.CreateGetEntityRefOp(e.ResultType);
		return VisitUnary(e, op, VisitExprAsScalar);
	}

	public override Node Visit(DbScanExpression e)
	{
		Check.NotNull(e, "e");
		TableMD tableMetadata = Command.CreateTableDefinition(e.Target);
		ScanTableOp scanTableOp = _iqtCommand.CreateScanTableOp(tableMetadata);
		Node node = _iqtCommand.CreateNode(scanTableOp);
		Var value = scanTableOp.Table.Columns[0];
		_varMap[node] = value;
		return node;
	}

	public override Node Visit(DbFilterExpression e)
	{
		Check.NotNull(e, "e");
		if (!IsIsOfFilter(e) || _processedIsOfFilters.Contains(e))
		{
			Node node = EnterExpressionBinding(e.Input);
			Node arg = VisitExprAsPredicate(e.Predicate);
			ExitExpressionBinding();
			Op op = _iqtCommand.CreateFilterOp();
			Node node2 = _iqtCommand.CreateNode(op, node, arg);
			_varMap[node2] = _varMap[node];
			return node2;
		}
		DbIsOfExpression template = (DbIsOfExpression)e.Predicate;
		DbExpression e2 = ApplyIsOfFilter(e.Input.Expression, new IsOfFilter(template));
		return VisitExpr(e2);
	}

	public override Node Visit(DbProjectExpression e)
	{
		Check.NotNull(e, "e");
		if (e == _discriminatedViewTopProject)
		{
			return GenerateDiscriminatedProject(e);
		}
		return GenerateStandardProject(e);
	}

	private Node GenerateDiscriminatedProject(DbProjectExpression e)
	{
		PlanCompiler.Assert(_discriminatedViewTopProject != null, "if a project matches the pattern, there must be a corresponding discriminator map");
		Node arg = EnterExpressionBinding(e.Input);
		List<RelProperty> list = new List<RelProperty>();
		List<Node> list2 = new List<Node>();
		foreach (KeyValuePair<RelProperty, DbExpression> item2 in _discriminatorMap.RelPropertyMap)
		{
			list.Add(item2.Key);
			list2.Add(VisitExprAsScalar(item2.Value));
		}
		DiscriminatedNewEntityOp op = _iqtCommand.CreateDiscriminatedNewEntityOp(e.Projection.ResultType, new ExplicitDiscriminatorMap(_discriminatorMap), _discriminatorMap.EntitySet, list);
		List<Node> list3 = new List<Node>(_discriminatorMap.PropertyMap.Count + 1);
		list3.Add(CreateNewInstanceArgument(_discriminatorMap.Discriminator.Property, _discriminatorMap.Discriminator));
		foreach (KeyValuePair<EdmProperty, DbExpression> item3 in _discriminatorMap.PropertyMap)
		{
			DbExpression value = item3.Value;
			EdmProperty key = item3.Key;
			Node item = CreateNewInstanceArgument(key, value);
			list3.Add(item);
		}
		list3.AddRange(list2);
		Node definingExpr = _iqtCommand.CreateNode(op, list3);
		ExitExpressionBinding();
		Var computedVar;
		Node arg2 = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar);
		ProjectOp op2 = _iqtCommand.CreateProjectOp(computedVar);
		Node node = _iqtCommand.CreateNode(op2, arg, arg2);
		_varMap[node] = computedVar;
		return node;
	}

	private Node CreateNewInstanceArgument(EdmMember property, DbExpression value)
	{
		return BuildSoftCast(VisitExprAsScalar(value), Helper.GetModelTypeUsage(property));
	}

	private Node GenerateStandardProject(DbProjectExpression e)
	{
		Node arg = EnterExpressionBinding(e.Input);
		Node definingExpr = VisitExprAsScalar(e.Projection);
		ExitExpressionBinding();
		Var computedVar;
		Node arg2 = _iqtCommand.CreateVarDefListNode(definingExpr, out computedVar);
		ProjectOp op = _iqtCommand.CreateProjectOp(computedVar);
		Node node = _iqtCommand.CreateNode(op, arg, arg2);
		_varMap[node] = computedVar;
		return node;
	}

	public override Node Visit(DbCrossJoinExpression e)
	{
		Check.NotNull(e, "e");
		return VisitJoin(e, e.Inputs, null);
	}

	public override Node Visit(DbJoinExpression e)
	{
		Check.NotNull(e, "e");
		List<DbExpressionBinding> list = new List<DbExpressionBinding>();
		list.Add(e.Left);
		list.Add(e.Right);
		return VisitJoin(e, list, e.JoinCondition);
	}

	private Node VisitJoin(DbExpression e, IList<DbExpressionBinding> inputs, DbExpression joinCond)
	{
		PlanCompiler.Assert(DbExpressionKind.CrossJoin == e.ExpressionKind || DbExpressionKind.InnerJoin == e.ExpressionKind || DbExpressionKind.LeftOuterJoin == e.ExpressionKind || DbExpressionKind.FullOuterJoin == e.ExpressionKind, "Unrecognized JoinType specified in DbJoinExpression");
		List<Node> list = new List<Node>();
		List<Var> list2 = new List<Var>();
		for (int i = 0; i < inputs.Count; i++)
		{
			Var boundVar;
			Node item = VisitBoundExpression(inputs[i].Expression, out boundVar);
			list.Add(item);
			list2.Add(boundVar);
		}
		for (int j = 0; j < list.Count; j++)
		{
			PushBindingScope(list2[j], inputs[j].VariableName);
		}
		Node node = VisitExprAsPredicate(joinCond);
		for (int k = 0; k < list.Count; k++)
		{
			ExitExpressionBinding();
		}
		JoinBaseOp joinBaseOp = null;
		switch (e.ExpressionKind)
		{
		case DbExpressionKind.CrossJoin:
			joinBaseOp = _iqtCommand.CreateCrossJoinOp();
			break;
		case DbExpressionKind.InnerJoin:
			joinBaseOp = _iqtCommand.CreateInnerJoinOp();
			break;
		case DbExpressionKind.LeftOuterJoin:
			joinBaseOp = _iqtCommand.CreateLeftOuterJoinOp();
			break;
		case DbExpressionKind.FullOuterJoin:
			joinBaseOp = _iqtCommand.CreateFullOuterJoinOp();
			break;
		}
		PlanCompiler.Assert(joinBaseOp != null, "Unrecognized JoinOp specified in DbJoinExpression, no JoinOp was produced");
		if (e.ExpressionKind != DbExpressionKind.CrossJoin)
		{
			PlanCompiler.Assert(node != null, "Non CrossJoinOps must specify a join condition");
			list.Add(node);
		}
		return ProjectNewRecord(_iqtCommand.CreateNode(joinBaseOp, list), ExtractElementRowType(e.ResultType), list2);
	}

	public override Node Visit(DbApplyExpression e)
	{
		Check.NotNull(e, "e");
		Node node = EnterExpressionBinding(e.Input);
		Node node2 = EnterExpressionBinding(e.Apply);
		ExitExpressionBinding();
		ExitExpressionBinding();
		PlanCompiler.Assert(DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind, "Unrecognized DbExpressionKind specified in DbApplyExpression");
		ApplyBaseOp applyBaseOp = null;
		applyBaseOp = ((DbExpressionKind.CrossApply != e.ExpressionKind) ? ((ApplyBaseOp)_iqtCommand.CreateOuterApplyOp()) : ((ApplyBaseOp)_iqtCommand.CreateCrossApplyOp()));
		Node inputNode = _iqtCommand.CreateNode(applyBaseOp, node, node2);
		return ProjectNewRecord(inputNode, ExtractElementRowType(e.ResultType), new Var[2]
		{
			_varMap[node],
			_varMap[node2]
		});
	}

	public override Node Visit(DbGroupByExpression e)
	{
		Check.NotNull(e, "e");
		VarVec varVec = _iqtCommand.CreateVarVec();
		VarVec varVec2 = _iqtCommand.CreateVarVec();
		ExtractKeys(e, varVec, varVec2, out var inputNode, out var keyVarDefNodes, out var scope);
		int num = -1;
		for (int i = 0; i < e.Aggregates.Count; i++)
		{
			if (e.Aggregates[i].GetType() == typeof(DbGroupAggregate))
			{
				num = i;
				break;
			}
		}
		Node inputNode2 = null;
		List<Node> keyVarDefNodes2 = null;
		VarVec outputVarSet = _iqtCommand.CreateVarVec();
		VarVec varVec3 = _iqtCommand.CreateVarVec();
		if (num >= 0)
		{
			ExtractKeys(e, varVec3, outputVarSet, out inputNode2, out keyVarDefNodes2, out var _);
		}
		scope = new ExpressionBindingScope(_iqtCommand, e.Input.GroupVariableName, scope.ScopeVar);
		_varScopes.Push(scope);
		List<Node> list = new List<Node>();
		Node arg = null;
		for (int j = 0; j < e.Aggregates.Count; j++)
		{
			DbAggregate dbAggregate = e.Aggregates[j];
			IList<Node> argNodes = VisitExprAsScalar(dbAggregate.Arguments);
			Var groupAggVar;
			if (j != num)
			{
				DbFunctionAggregate dbFunctionAggregate = dbAggregate as DbFunctionAggregate;
				PlanCompiler.Assert(dbFunctionAggregate != null, "Unrecognized DbAggregate used in DbGroupByExpression");
				list.Add(ProcessFunctionAggregate(dbFunctionAggregate, argNodes, out groupAggVar));
			}
			else
			{
				arg = ProcessGroupAggregate(keyVarDefNodes, inputNode2, keyVarDefNodes2, varVec3, e.Input.Expression.ResultType, out groupAggVar);
			}
			varVec2.Set(groupAggVar);
		}
		ExitGroupExpressionBinding();
		List<Node> list2 = new List<Node>();
		list2.Add(inputNode);
		list2.Add(_iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp(), keyVarDefNodes));
		list2.Add(_iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp(), list));
		GroupByBaseOp op;
		if (num >= 0)
		{
			list2.Add(_iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp(), arg));
			op = _iqtCommand.CreateGroupByIntoOp(varVec, _iqtCommand.CreateVarVec(_varMap[inputNode]), varVec2);
		}
		else
		{
			op = _iqtCommand.CreateGroupByOp(varVec, varVec2);
		}
		Node inputNode3 = _iqtCommand.CreateNode(op, list2);
		return ProjectNewRecord(inputNode3, ExtractElementRowType(e.ResultType), varVec2);
	}

	private void ExtractKeys(DbGroupByExpression e, VarVec keyVarSet, VarVec outputVarSet, out Node inputNode, out List<Node> keyVarDefNodes, out ExpressionBindingScope scope)
	{
		inputNode = EnterGroupExpressionBinding(e.Input);
		keyVarDefNodes = new List<Node>();
		for (int i = 0; i < e.Keys.Count; i++)
		{
			DbExpression expr = e.Keys[i];
			Node node = VisitExprAsScalar(expr);
			PlanCompiler.Assert(node.Op is ScalarOp, "GroupBy Key is not a ScalarOp");
			keyVarDefNodes.Add(_iqtCommand.CreateVarDefNode(node, out var computedVar));
			outputVarSet.Set(computedVar);
			keyVarSet.Set(computedVar);
		}
		scope = ExitExpressionBinding();
	}

	private Node ProcessFunctionAggregate(DbFunctionAggregate funcAgg, IList<Node> argNodes, out Var aggVar)
	{
		Node definingExpr = _iqtCommand.CreateNode(_iqtCommand.CreateAggregateOp(funcAgg.Function, funcAgg.Distinct), argNodes);
		return _iqtCommand.CreateVarDefNode(definingExpr, out aggVar);
	}

	private Node ProcessGroupAggregate(List<Node> keyVarDefNodes, Node copyOfInput, List<Node> copyOfkeyVarDefNodes, VarVec copyKeyVarSet, TypeUsage inputResultType, out Var groupAggVar)
	{
		Var var = _varMap[copyOfInput];
		Node node = copyOfInput;
		if (keyVarDefNodes.Count > 0)
		{
			VarVec varVec = _iqtCommand.CreateVarVec();
			varVec.Set(var);
			varVec.Or(copyKeyVarSet);
			Node arg = _iqtCommand.CreateNode(_iqtCommand.CreateProjectOp(varVec), node, _iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp(), copyOfkeyVarDefNodes));
			List<Node> list = new List<Node>();
			List<Node> list2 = new List<Node>();
			for (int i = 0; i < keyVarDefNodes.Count; i++)
			{
				Node node2 = keyVarDefNodes[i];
				Node node3 = copyOfkeyVarDefNodes[i];
				Var var2 = ((VarDefOp)node2.Op).Var;
				Var var3 = ((VarDefOp)node3.Op).Var;
				FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(var2)), list);
				FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(var3)), list2);
			}
			PlanCompiler.Assert(list.Count == list2.Count, "The flattened keys lists should have the same number of elements");
			Node node4 = null;
			for (int j = 0; j < list.Count; j++)
			{
				Node node5 = list[j];
				Node node6 = list2[j];
				Node node7 = ((!_useDatabaseNullSemantics) ? _iqtCommand.CreateNode(_iqtCommand.CreateComparisonOp(OpType.EQ), node5, node6) : _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Or), _iqtCommand.CreateNode(_iqtCommand.CreateComparisonOp(OpType.EQ), node5, node6), _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.And), _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.IsNull), OpCopier.Copy(_iqtCommand, node5)), _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.IsNull), OpCopier.Copy(_iqtCommand, node6)))));
				node4 = ((node4 != null) ? _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.And), node4, node7) : node7);
			}
			node = _iqtCommand.CreateNode(_iqtCommand.CreateFilterOp(), arg, node4);
		}
		_varMap[node] = var;
		node = ConvertRelOpToScalarOpTree(node, inputResultType);
		return _iqtCommand.CreateVarDefNode(node, out groupAggVar);
	}

	private void FlattenProperties(Node input, IList<Node> flattenedProperties)
	{
		if (input.Op.Type.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
		{
			IList<EdmProperty> properties = TypeHelpers.GetProperties(input.Op.Type);
			PlanCompiler.Assert(properties.Count != 0, "No nested properties for RowType");
			for (int i = 0; i < properties.Count; i++)
			{
				Node arg = ((i == 0) ? input : OpCopier.Copy(_iqtCommand, input));
				FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreatePropertyOp(properties[i]), arg), flattenedProperties);
			}
		}
		else
		{
			flattenedProperties.Add(input);
		}
	}

	private Node VisitSortArguments(DbExpressionBinding input, IList<DbSortClause> sortOrder, List<SortKey> sortKeys, out Var inputVar)
	{
		Node node = EnterExpressionBinding(input);
		inputVar = _varMap[node];
		VarVec varVec = _iqtCommand.CreateVarVec();
		varVec.Set(inputVar);
		List<Node> list = new List<Node>();
		PlanCompiler.Assert(sortKeys.Count == 0, "Non-empty SortKey list before adding converted SortClauses");
		for (int i = 0; i < sortOrder.Count; i++)
		{
			DbSortClause dbSortClause = sortOrder[i];
			Node node2 = VisitExprAsScalar(dbSortClause.Expression);
			PlanCompiler.Assert(node2.Op is ScalarOp, "DbSortClause Expression converted to non-ScalarOp");
			list.Add(_iqtCommand.CreateVarDefNode(node2, out var computedVar));
			varVec.Set(computedVar);
			SortKey sortKey = null;
			sortKey = ((!string.IsNullOrEmpty(dbSortClause.Collation)) ? Command.CreateSortKey(computedVar, dbSortClause.Ascending, dbSortClause.Collation) : Command.CreateSortKey(computedVar, dbSortClause.Ascending));
			sortKeys.Add(sortKey);
		}
		ExitExpressionBinding();
		return _iqtCommand.CreateNode(_iqtCommand.CreateProjectOp(varVec), node, _iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp(), list));
	}

	public override Node Visit(DbSkipExpression expression)
	{
		Check.NotNull(expression, "expression");
		List<SortKey> sortKeys = new List<SortKey>();
		Var inputVar;
		Node arg = VisitSortArguments(expression.Input, expression.SortOrder, sortKeys, out inputVar);
		Node arg2 = VisitExprAsScalar(expression.Count);
		Node node = _iqtCommand.CreateNode(_iqtCommand.CreateConstrainedSortOp(sortKeys), arg, arg2, _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType)));
		_varMap[node] = inputVar;
		return node;
	}

	public override Node Visit(DbSortExpression e)
	{
		Check.NotNull(e, "e");
		List<SortKey> sortKeys = new List<SortKey>();
		Var inputVar;
		Node arg = VisitSortArguments(e.Input, e.SortOrder, sortKeys, out inputVar);
		SortOp op = _iqtCommand.CreateSortOp(sortKeys);
		Node node = _iqtCommand.CreateNode(op, arg);
		_varMap[node] = inputVar;
		return node;
	}

	public override Node Visit(DbQuantifierExpression e)
	{
		Check.NotNull(e, "e");
		Node node = null;
		PlanCompiler.Assert(DbExpressionKind.Any == e.ExpressionKind || e.ExpressionKind == DbExpressionKind.All, "Invalid DbExpressionKind in DbQuantifierExpression");
		Node node2 = EnterExpressionBinding(e.Input);
		Node node3 = VisitExprAsPredicate(e.Predicate);
		if (e.ExpressionKind == DbExpressionKind.All)
		{
			node3 = _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), node3);
			Node arg = VisitExprAsScalar(e.Predicate);
			arg = _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.IsNull), arg);
			node3 = _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Or), node3, arg);
		}
		ExitExpressionBinding();
		Var value = _varMap[node2];
		node2 = _iqtCommand.CreateNode(_iqtCommand.CreateFilterOp(), node2, node3);
		_varMap[node2] = value;
		node = _iqtCommand.CreateNode(_iqtCommand.CreateExistsOp(), node2);
		if (e.ExpressionKind == DbExpressionKind.All)
		{
			node = _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), node);
		}
		return node;
	}
}
