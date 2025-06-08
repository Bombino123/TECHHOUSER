using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class NullSemantics : BasicOpVisitorOfNode
{
	private struct VariableNullabilityTable
	{
		private bool[] _entries;

		public bool this[Var variable]
		{
			get
			{
				if (variable.Id < _entries.Length)
				{
					return _entries[variable.Id];
				}
				return true;
			}
			set
			{
				EnsureCapacity(variable.Id + 1);
				_entries[variable.Id] = value;
			}
		}

		public VariableNullabilityTable(int capacity)
		{
			_entries = Enumerable.Repeat(element: true, capacity).ToArray();
		}

		private void EnsureCapacity(int minimum)
		{
			if (_entries.Length < minimum)
			{
				int num = _entries.Length * 2;
				if (num < minimum)
				{
					num = minimum;
				}
				bool[] array = Enumerable.Repeat(element: true, num).ToArray();
				Array.Copy(_entries, 0, array, 0, _entries.Length);
				_entries = array;
			}
		}
	}

	private Command _command;

	private bool _modified;

	private bool _negated;

	private VariableNullabilityTable _variableNullabilityTable = new VariableNullabilityTable(32);

	private NullSemantics(Command command)
	{
		_command = command;
	}

	public static bool Process(Command command)
	{
		NullSemantics nullSemantics = new NullSemantics(command);
		command.Root = nullSemantics.VisitNode(command.Root);
		return nullSemantics._modified;
	}

	protected override Node VisitDefault(Node n)
	{
		bool negated = _negated;
		switch (n.Op.OpType)
		{
		case OpType.Not:
			_negated = !_negated;
			n = base.VisitDefault(n);
			break;
		case OpType.Or:
			n = HandleOr(n);
			break;
		case OpType.And:
			n = base.VisitDefault(n);
			break;
		case OpType.EQ:
			_negated = false;
			n = HandleEQ(n, negated);
			break;
		case OpType.NE:
			n = HandleNE(n);
			break;
		default:
			_negated = false;
			n = base.VisitDefault(n);
			break;
		}
		_negated = negated;
		return n;
	}

	private Node HandleOr(Node n)
	{
		Node node = ((n.Child0.Op.OpType == OpType.IsNull) ? n.Child0 : null);
		if (node == null || node.Child0.Op.OpType != OpType.VarRef)
		{
			return base.VisitDefault(n);
		}
		Var var = ((VarRefOp)node.Child0.Op).Var;
		bool value = _variableNullabilityTable[var];
		_variableNullabilityTable[var] = false;
		n.Child1 = VisitNode(n.Child1);
		_variableNullabilityTable[var] = value;
		return n;
	}

	private Node HandleEQ(Node n, bool negated)
	{
		bool modified = _modified;
		Node child = n.Child0;
		Node node2 = (n.Child0 = VisitNode(n.Child0));
		int num;
		if (child == node2)
		{
			Node child2 = n.Child1;
			node2 = (n.Child1 = VisitNode(n.Child1));
			if (child2 == node2)
			{
				num = ((n != (n = ImplementEquality(n, negated))) ? 1 : 0);
				goto IL_0055;
			}
		}
		num = 1;
		goto IL_0055;
		IL_0055:
		_modified = (byte)((modified ? 1u : 0u) | (uint)num) != 0;
		return n;
	}

	private Node HandleNE(Node n)
	{
		ComparisonOp comparisonOp = (ComparisonOp)n.Op;
		n = _command.CreateNode(_command.CreateConditionalOp(OpType.Not), _command.CreateNode(_command.CreateComparisonOp(OpType.EQ, comparisonOp.UseDatabaseNullSemantics), n.Child0, n.Child1));
		_modified = true;
		return base.VisitDefault(n);
	}

	private bool IsNullableVarRef(Node n)
	{
		if (n.Op.OpType == OpType.VarRef)
		{
			return _variableNullabilityTable[((VarRefOp)n.Op).Var];
		}
		return false;
	}

	private Node ImplementEquality(Node n, bool negated)
	{
		if (((ComparisonOp)n.Op).UseDatabaseNullSemantics)
		{
			return n;
		}
		Node child = n.Child0;
		Node child2 = n.Child1;
		switch (child.Op.OpType)
		{
		case OpType.Constant:
		case OpType.InternalConstant:
		case OpType.NullSentinel:
			switch (child2.Op.OpType)
			{
			case OpType.Constant:
			case OpType.InternalConstant:
			case OpType.NullSentinel:
				return n;
			case OpType.Null:
				return False();
			default:
				if (!negated)
				{
					return n;
				}
				return And(n, Not(IsNull(Clone(child2))));
			}
		case OpType.Null:
			switch (child2.Op.OpType)
			{
			case OpType.Constant:
			case OpType.InternalConstant:
			case OpType.NullSentinel:
				return False();
			case OpType.Null:
				return True();
			default:
				return IsNull(child2);
			}
		default:
			switch (child2.Op.OpType)
			{
			case OpType.Constant:
			case OpType.InternalConstant:
			case OpType.NullSentinel:
				if (!negated || !IsNullableVarRef(n))
				{
					return n;
				}
				return And(n, Not(IsNull(Clone(child))));
			case OpType.Null:
				return IsNull(child);
			default:
				if (!negated)
				{
					return Or(n, And(IsNull(Clone(child)), IsNull(Clone(child2))));
				}
				return And(n, NotXor(Clone(child), Clone(child2)));
			}
		}
	}

	private Node Clone(Node x)
	{
		return OpCopier.Copy(_command, x);
	}

	private Node False()
	{
		return _command.CreateNode(_command.CreateFalseOp());
	}

	private Node True()
	{
		return _command.CreateNode(_command.CreateTrueOp());
	}

	private Node IsNull(Node x)
	{
		return _command.CreateNode(_command.CreateConditionalOp(OpType.IsNull), x);
	}

	private Node Not(Node x)
	{
		return _command.CreateNode(_command.CreateConditionalOp(OpType.Not), x);
	}

	private Node And(Node x, Node y)
	{
		return _command.CreateNode(_command.CreateConditionalOp(OpType.And), x, y);
	}

	private Node Or(Node x, Node y)
	{
		return _command.CreateNode(_command.CreateConditionalOp(OpType.Or), x, y);
	}

	private Node Boolean(bool value)
	{
		return _command.CreateNode(_command.CreateConstantOp(_command.BooleanType, value));
	}

	private Node NotXor(Node x, Node y)
	{
		return _command.CreateNode(_command.CreateComparisonOp(OpType.EQ), _command.CreateNode(_command.CreateCaseOp(_command.BooleanType), IsNull(x), Boolean(value: true), Boolean(value: false)), _command.CreateNode(_command.CreateCaseOp(_command.BooleanType), IsNull(y), Boolean(value: true), Boolean(value: false)));
	}
}
