using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class BindingContext
{
	private readonly Stack<Binding> _scopes;

	internal BindingContext()
	{
		_scopes = new Stack<Binding>();
	}

	internal void PushBindingScope(Binding binding)
	{
		_scopes.Push(binding);
	}

	internal void PopBindingScope()
	{
		_scopes.Pop();
	}

	internal bool TryGetBoundExpression(Expression linqExpression, out DbExpression cqtExpression)
	{
		cqtExpression = (from binding in _scopes
			where binding.LinqExpression == linqExpression
			select binding.CqtExpression).FirstOrDefault();
		return cqtExpression != null;
	}
}
