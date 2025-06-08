using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class TreeExpr<T_Identifier> : BoolExpr<T_Identifier>
{
	private readonly Set<BoolExpr<T_Identifier>> _children;

	private readonly int _hashCode;

	internal Set<BoolExpr<T_Identifier>> Children => _children;

	protected TreeExpr(IEnumerable<BoolExpr<T_Identifier>> children)
	{
		_children = new Set<BoolExpr<T_Identifier>>(children);
		_children.MakeReadOnly();
		_hashCode = _children.GetElementsHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as BoolExpr<T_Identifier>);
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override string ToString()
	{
		return StringUtil.FormatInvariant("{0}({1})", ExprType, _children);
	}

	protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
	{
		return ((TreeExpr<T_Identifier>)other).Children.SetEquals(Children);
	}
}
