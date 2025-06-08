using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.EntitySql;

public sealed class EntitySqlParser
{
	private readonly Perspective _perspective;

	internal EntitySqlParser(Perspective perspective)
	{
		_perspective = perspective;
	}

	public ParseResult Parse(string query, params DbParameterReferenceExpression[] parameters)
	{
		Check.NotNull(query, "query");
		if (parameters != null)
		{
			IEnumerable<DbParameterReferenceExpression> enumerableArgument = parameters;
			EntityUtil.CheckArgumentContainsNull(ref enumerableArgument, "parameters");
		}
		return CqlQuery.Compile(query, _perspective, null, parameters);
	}

	public DbLambda ParseLambda(string query, params DbVariableReferenceExpression[] variables)
	{
		Check.NotNull(query, "query");
		if (variables != null)
		{
			IEnumerable<DbVariableReferenceExpression> enumerableArgument = variables;
			EntityUtil.CheckArgumentContainsNull(ref enumerableArgument, "variables");
		}
		return CqlQuery.CompileQueryCommandLambda(query, _perspective, null, null, variables);
	}
}
