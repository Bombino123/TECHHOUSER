using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace System.Data.Entity.Validation;

[Serializable]
public class DbUnexpectedValidationException : DataException
{
	public DbUnexpectedValidationException()
	{
	}

	public DbUnexpectedValidationException(string message)
		: base(message)
	{
	}

	public DbUnexpectedValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[ExcludeFromCodeCoverage]
	protected DbUnexpectedValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
