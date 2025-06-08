using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal class PatternMatchRuleProcessor : DbExpressionRuleProcessingVisitor
{
	private readonly ReadOnlyCollection<PatternMatchRule> ruleSet;

	private PatternMatchRuleProcessor(ReadOnlyCollection<PatternMatchRule> rules)
	{
		ruleSet = rules;
	}

	private DbExpression Process(DbExpression expression)
	{
		expression = VisitExpression(expression);
		return expression;
	}

	protected override IEnumerable<DbExpressionRule> GetRules()
	{
		return ruleSet;
	}

	internal static Func<DbExpression, DbExpression> Create(params PatternMatchRule[] rules)
	{
		return new PatternMatchRuleProcessor(new ReadOnlyCollection<PatternMatchRule>(rules)).Process;
	}
}
