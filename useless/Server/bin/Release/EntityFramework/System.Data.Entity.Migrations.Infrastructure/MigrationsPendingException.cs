using System.Runtime.Serialization;

namespace System.Data.Entity.Migrations.Infrastructure;

[Serializable]
public sealed class MigrationsPendingException : MigrationsException
{
	public MigrationsPendingException()
	{
	}

	public MigrationsPendingException(string message)
		: base(message)
	{
	}

	public MigrationsPendingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private MigrationsPendingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
