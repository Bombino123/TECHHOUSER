using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal sealed class DbExpressionList : ReadOnlyCollection<DbExpression>
{
	internal DbExpressionList(IList<DbExpression> elements)
		: base(elements)
	{
	}
}
