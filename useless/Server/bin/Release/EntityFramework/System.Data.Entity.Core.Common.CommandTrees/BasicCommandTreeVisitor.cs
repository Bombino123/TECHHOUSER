using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class BasicCommandTreeVisitor : BasicExpressionVisitor
{
	protected virtual void VisitSetClause(DbSetClause setClause)
	{
		Check.NotNull(setClause, "setClause");
		VisitExpression(setClause.Property);
		VisitExpression(setClause.Value);
	}

	protected virtual void VisitModificationClause(DbModificationClause modificationClause)
	{
		Check.NotNull(modificationClause, "modificationClause");
		VisitSetClause((DbSetClause)modificationClause);
	}

	protected virtual void VisitModificationClauses(IList<DbModificationClause> modificationClauses)
	{
		Check.NotNull(modificationClauses, "modificationClauses");
		for (int i = 0; i < modificationClauses.Count; i++)
		{
			VisitModificationClause(modificationClauses[i]);
		}
	}

	public virtual void VisitCommandTree(DbCommandTree commandTree)
	{
		Check.NotNull(commandTree, "commandTree");
		switch (commandTree.CommandTreeKind)
		{
		case DbCommandTreeKind.Delete:
			VisitDeleteCommandTree((DbDeleteCommandTree)commandTree);
			break;
		case DbCommandTreeKind.Function:
			VisitFunctionCommandTree((DbFunctionCommandTree)commandTree);
			break;
		case DbCommandTreeKind.Insert:
			VisitInsertCommandTree((DbInsertCommandTree)commandTree);
			break;
		case DbCommandTreeKind.Query:
			VisitQueryCommandTree((DbQueryCommandTree)commandTree);
			break;
		case DbCommandTreeKind.Update:
			VisitUpdateCommandTree((DbUpdateCommandTree)commandTree);
			break;
		default:
			throw new NotSupportedException();
		}
	}

	protected virtual void VisitDeleteCommandTree(DbDeleteCommandTree deleteTree)
	{
		Check.NotNull(deleteTree, "deleteTree");
		VisitExpressionBindingPre(deleteTree.Target);
		VisitExpression(deleteTree.Predicate);
		VisitExpressionBindingPost(deleteTree.Target);
	}

	protected virtual void VisitFunctionCommandTree(DbFunctionCommandTree functionTree)
	{
		Check.NotNull(functionTree, "functionTree");
	}

	protected virtual void VisitInsertCommandTree(DbInsertCommandTree insertTree)
	{
		Check.NotNull(insertTree, "insertTree");
		VisitExpressionBindingPre(insertTree.Target);
		VisitModificationClauses(insertTree.SetClauses);
		if (insertTree.Returning != null)
		{
			VisitExpression(insertTree.Returning);
		}
		VisitExpressionBindingPost(insertTree.Target);
	}

	protected virtual void VisitQueryCommandTree(DbQueryCommandTree queryTree)
	{
		Check.NotNull(queryTree, "queryTree");
		VisitExpression(queryTree.Query);
	}

	protected virtual void VisitUpdateCommandTree(DbUpdateCommandTree updateTree)
	{
		Check.NotNull(updateTree, "updateTree");
		VisitExpressionBindingPre(updateTree.Target);
		VisitModificationClauses(updateTree.SetClauses);
		VisitExpression(updateTree.Predicate);
		if (updateTree.Returning != null)
		{
			VisitExpression(updateTree.Returning);
		}
		VisitExpressionBindingPost(updateTree.Target);
	}
}
