using System.Runtime.Serialization;

namespace System.Data.Entity.Migrations.Infrastructure;

[Serializable]
public sealed class AutomaticMigrationsDisabledException : MigrationsException
{
	public AutomaticMigrationsDisabledException()
	{
	}

	public AutomaticMigrationsDisabledException(string message)
		: base(message)
	{
	}

	public AutomaticMigrationsDisabledException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private AutomaticMigrationsDisabledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
