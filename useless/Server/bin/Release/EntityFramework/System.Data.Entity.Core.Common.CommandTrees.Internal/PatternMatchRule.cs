namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal class PatternMatchRule : DbExpressionRule
{
	private readonly Func<DbExpression, bool> isMatch;

	private readonly Func<DbExpression, DbExpression> process;

	private readonly ProcessedAction processed;

	internal override ProcessedAction OnExpressionProcessed => processed;

	private PatternMatchRule(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
	{
		isMatch = matchFunc;
		process = processor;
		processed = onProcessed;
	}

	internal override bool ShouldProcess(DbExpression expression)
	{
		return isMatch(expression);
	}

	internal override bool TryProcess(DbExpression expression, out DbExpression result)
	{
		result = process(expression);
		return result != null;
	}

	internal static PatternMatchRule Create(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor)
	{
		return Create(matchFunc, processor, ProcessedAction.Reset);
	}

	internal static PatternMatchRule Create(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
	{
		return new PatternMatchRule(matchFunc, processor, onProcessed);
	}
}
