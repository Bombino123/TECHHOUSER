using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbExpressionBinding
{
	private readonly DbExpression _expr;

	private readonly DbVariableReferenceExpression _varRef;

	public DbExpression Expression => _expr;

	public string VariableName => _varRef.VariableName;

	public TypeUsage VariableType => _varRef.ResultType;

	public DbVariableReferenceExpression Variable => _varRef;

	internal DbExpressionBinding(DbExpression input, DbVariableReferenceExpression varRef)
	{
		_expr = input;
		_varRef = varRef;
	}
}
