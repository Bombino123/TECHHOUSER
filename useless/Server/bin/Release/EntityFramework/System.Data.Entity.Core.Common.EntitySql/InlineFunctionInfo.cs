using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.EntitySql.AST;

namespace System.Data.Entity.Core.Common.EntitySql;

internal abstract class InlineFunctionInfo
{
	internal readonly System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition FunctionDefAst;

	internal readonly List<DbVariableReferenceExpression> Parameters;

	internal InlineFunctionInfo(System.Data.Entity.Core.Common.EntitySql.AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
	{
		FunctionDefAst = functionDef;
		Parameters = parameters;
	}

	internal abstract DbLambda GetLambda(SemanticResolver sr);
}
