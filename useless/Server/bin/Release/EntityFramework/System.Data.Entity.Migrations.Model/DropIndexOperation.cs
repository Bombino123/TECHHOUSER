using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public class DropIndexOperation : IndexOperation
{
	private readonly CreateIndexOperation _inverse;

	public override MigrationOperation Inverse => _inverse;

	public override bool IsDestructiveChange => false;

	public DropIndexOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}

	public DropIndexOperation(CreateIndexOperation inverse, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotNull(inverse, "inverse");
		_inverse = inverse;
	}
}
