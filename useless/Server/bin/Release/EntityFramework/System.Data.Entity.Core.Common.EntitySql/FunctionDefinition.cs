using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql;

public sealed class FunctionDefinition
{
	private readonly string _name;

	private readonly DbLambda _lambda;

	private readonly int _startPosition;

	private readonly int _endPosition;

	public string Name => _name;

	public DbLambda Lambda => _lambda;

	public int StartPosition => _startPosition;

	public int EndPosition => _endPosition;

	internal FunctionDefinition(string name, DbLambda lambda, int startPosition, int endPosition)
	{
		_name = name;
		_lambda = lambda;
		_startPosition = startPosition;
		_endPosition = endPosition;
	}
}
