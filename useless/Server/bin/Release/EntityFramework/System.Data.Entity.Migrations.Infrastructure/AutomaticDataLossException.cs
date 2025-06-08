using System.Data.Entity.Utilities;
using System.Runtime.Serialization;

namespace System.Data.Entity.Migrations.Infrastructure;

[Serializable]
public sealed class AutomaticDataLossException : MigrationsException
{
	public AutomaticDataLossException()
	{
	}

	public AutomaticDataLossException(string message)
		: base(message)
	{
		Check.NotEmpty(message, "message");
	}

	public AutomaticDataLossException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private AutomaticDataLossException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
