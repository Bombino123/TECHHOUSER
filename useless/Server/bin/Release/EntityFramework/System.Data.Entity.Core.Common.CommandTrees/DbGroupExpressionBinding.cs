using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbGroupExpressionBinding
{
	private readonly DbExpression _expr;

	private readonly DbVariableReferenceExpression _varRef;

	private readonly DbVariableReferenceExpression _groupVarRef;

	private DbGroupAggregate _groupAggregate;

	public DbExpression Expression => _expr;

	public string VariableName => _varRef.VariableName;

	public TypeUsage VariableType => _varRef.ResultType;

	public DbVariableReferenceExpression Variable => _varRef;

	public string GroupVariableName => _groupVarRef.VariableName;

	public TypeUsage GroupVariableType => _groupVarRef.ResultType;

	public DbVariableReferenceExpression GroupVariable => _groupVarRef;

	public DbGroupAggregate GroupAggregate
	{
		get
		{
			if (_groupAggregate == null)
			{
				_groupAggregate = DbExpressionBuilder.GroupAggregate(GroupVariable);
			}
			return _groupAggregate;
		}
	}

	internal DbGroupExpressionBinding(DbExpression input, DbVariableReferenceExpression inputRef, DbVariableReferenceExpression groupRef)
	{
		_expr = input;
		_varRef = inputRef;
		_groupVarRef = groupRef;
	}
}
