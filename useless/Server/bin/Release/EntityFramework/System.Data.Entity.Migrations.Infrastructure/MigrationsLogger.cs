namespace System.Data.Entity.Migrations.Infrastructure;

public abstract class MigrationsLogger : MarshalByRefObject
{
	public abstract void Info(string message);

	public abstract void Warning(string message);

	public abstract void Verbose(string message);
}
