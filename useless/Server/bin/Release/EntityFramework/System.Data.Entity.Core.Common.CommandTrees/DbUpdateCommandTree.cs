using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbUpdateCommandTree : DbModificationCommandTree
{
	private readonly DbExpression _predicate;

	private readonly DbExpression _returning;

	private readonly ReadOnlyCollection<DbModificationClause> _setClauses;

	public IList<DbModificationClause> SetClauses => _setClauses;

	public DbExpression Returning => _returning;

	public DbExpression Predicate => _predicate;

	public override DbCommandTreeKind CommandTreeKind => DbCommandTreeKind.Update;

	internal override bool HasReader => Returning != null;

	internal DbUpdateCommandTree()
	{
	}

	public DbUpdateCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate, ReadOnlyCollection<DbModificationClause> setClauses, DbExpression returning)
		: base(metadata, dataSpace, target)
	{
		_predicate = predicate;
		_setClauses = setClauses;
		_returning = returning;
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		base.DumpStructure(dumper);
		if (Predicate != null)
		{
			dumper.Dump(Predicate, "Predicate");
		}
		dumper.Begin("SetClauses", null);
		foreach (DbModificationClause setClause in SetClauses)
		{
			setClause?.DumpStructure(dumper);
		}
		dumper.End("SetClauses");
		dumper.Dump(Returning, "Returning");
	}

	internal override string PrintTree(ExpressionPrinter printer)
	{
		return printer.Print(this);
	}
}
