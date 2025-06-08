using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal abstract class DbExpressionRuleProcessingVisitor : DefaultExpressionVisitor
{
	private bool _stopped;

	protected abstract IEnumerable<DbExpressionRule> GetRules();

	private static Tuple<DbExpression, DbExpressionRule.ProcessedAction> ProcessRules(DbExpression expression, List<DbExpressionRule> rules)
	{
		for (int i = 0; i < rules.Count; i++)
		{
			DbExpressionRule dbExpressionRule = rules[i];
			if (dbExpressionRule.ShouldProcess(expression) && dbExpressionRule.TryProcess(expression, out var result))
			{
				if (dbExpressionRule.OnExpressionProcessed != 0)
				{
					return Tuple.Create(result, dbExpressionRule.OnExpressionProcessed);
				}
				expression = result;
			}
		}
		return Tuple.Create(expression, DbExpressionRule.ProcessedAction.Continue);
	}

	private DbExpression ApplyRules(DbExpression expression)
	{
		List<DbExpressionRule> rules = GetRules().ToList();
		Tuple<DbExpression, DbExpressionRule.ProcessedAction> tuple = ProcessRules(expression, rules);
		while (tuple.Item2 == DbExpressionRule.ProcessedAction.Reset)
		{
			rules = GetRules().ToList();
			tuple = ProcessRules(tuple.Item1, rules);
		}
		if (tuple.Item2 == DbExpressionRule.ProcessedAction.Stop)
		{
			_stopped = true;
		}
		return tuple.Item1;
	}

	protected override DbExpression VisitExpression(DbExpression expression)
	{
		DbExpression dbExpression = ApplyRules(expression);
		if (_stopped)
		{
			return dbExpression;
		}
		dbExpression = base.VisitExpression(dbExpression);
		if (_stopped)
		{
			return dbExpression;
		}
		return ApplyRules(dbExpression);
	}
}
