namespace System.Data.Entity.Migrations.Model;

public class NotSupportedOperation : MigrationOperation
{
	internal static readonly NotSupportedOperation Instance = new NotSupportedOperation();

	public override bool IsDestructiveChange => false;

	private NotSupportedOperation()
		: base(null)
	{
	}
}
