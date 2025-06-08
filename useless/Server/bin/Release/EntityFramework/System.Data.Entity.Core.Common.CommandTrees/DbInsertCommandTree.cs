using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbInsertCommandTree : DbModificationCommandTree
{
	private readonly ReadOnlyCollection<DbModificationClause> _setClauses;

	private readonly DbExpression _returning;

	public IList<DbModificationClause> SetClauses => _setClauses;

	public DbExpression Returning => _returning;

	public override DbCommandTreeKind CommandTreeKind => DbCommandTreeKind.Insert;

	internal override bool HasReader => Returning != null;

	internal DbInsertCommandTree()
	{
	}

	public DbInsertCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, ReadOnlyCollection<DbModificationClause> setClauses, DbExpression returning)
		: base(metadata, dataSpace, target)
	{
		_setClauses = setClauses;
		_returning = returning;
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		base.DumpStructure(dumper);
		dumper.Begin("SetClauses");
		foreach (DbModificationClause setClause in SetClauses)
		{
			setClause?.DumpStructure(dumper);
		}
		dumper.End("SetClauses");
		if (Returning != null)
		{
			dumper.Dump(Returning, "Returning");
		}
	}

	internal override string PrintTree(ExpressionPrinter printer)
	{
		return printer.Print(this);
	}
}
