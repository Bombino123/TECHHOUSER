using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class OptimisticConcurrencyException : UpdateException
{
	public OptimisticConcurrencyException()
	{
	}

	public OptimisticConcurrencyException(string message)
		: base(message)
	{
	}

	public OptimisticConcurrencyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public OptimisticConcurrencyException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
		: base(message, innerException, stateEntries)
	{
	}

	private OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
