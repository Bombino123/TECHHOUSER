using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal sealed class ParameterRetriever : BasicCommandTreeVisitor
{
	private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings = new Dictionary<string, DbParameterReferenceExpression>();

	private ParameterRetriever()
	{
	}

	internal static ReadOnlyCollection<DbParameterReferenceExpression> GetParameters(DbCommandTree tree)
	{
		ParameterRetriever parameterRetriever = new ParameterRetriever();
		parameterRetriever.VisitCommandTree(tree);
		return new ReadOnlyCollection<DbParameterReferenceExpression>(parameterRetriever.paramMappings.Values.ToList());
	}

	public override void Visit(DbParameterReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
		paramMappings[expression.ParameterName] = expression;
	}
}
