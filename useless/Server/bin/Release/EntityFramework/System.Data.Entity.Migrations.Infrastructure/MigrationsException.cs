using System.Runtime.Serialization;

namespace System.Data.Entity.Migrations.Infrastructure;

[Serializable]
public class MigrationsException : Exception
{
	public MigrationsException()
	{
	}

	public MigrationsException(string message)
		: base(message)
	{
	}

	public MigrationsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected MigrationsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
