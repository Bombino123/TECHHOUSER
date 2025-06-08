using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbDeleteCommandTree : DbModificationCommandTree
{
	private readonly DbExpression _predicate;

	public DbExpression Predicate => _predicate;

	public override DbCommandTreeKind CommandTreeKind => DbCommandTreeKind.Delete;

	internal override bool HasReader => false;

	internal DbDeleteCommandTree()
	{
	}

	public DbDeleteCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate)
		: base(metadata, dataSpace, target)
	{
		_predicate = predicate;
	}

	internal override void DumpStructure(ExpressionDumper dumper)
	{
		base.DumpStructure(dumper);
		if (Predicate != null)
		{
			dumper.Dump(Predicate, "Predicate");
		}
	}

	internal override string PrintTree(ExpressionPrinter printer)
	{
		return printer.Print(this);
	}
}
