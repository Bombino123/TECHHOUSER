namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal abstract class DbExpressionRule
{
	internal enum ProcessedAction
	{
		Continue,
		Reset,
		Stop
	}

	internal abstract ProcessedAction OnExpressionProcessed { get; }

	internal abstract bool ShouldProcess(DbExpression expression);

	internal abstract bool TryProcess(DbExpression expression, out DbExpression result);
}
