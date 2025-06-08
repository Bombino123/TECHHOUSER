using System.Data.Entity.Core;
using System.Data.Entity.Internal;
using System.Runtime.Serialization;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public class DbUpdateConcurrencyException : DbUpdateException
{
	internal DbUpdateConcurrencyException(InternalContext context, OptimisticConcurrencyException innerException)
		: base(context, innerException, involvesIndependentAssociations: false)
	{
	}

	public DbUpdateConcurrencyException()
	{
	}

	public DbUpdateConcurrencyException(string message)
		: base(message)
	{
	}

	public DbUpdateConcurrencyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected DbUpdateConcurrencyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
