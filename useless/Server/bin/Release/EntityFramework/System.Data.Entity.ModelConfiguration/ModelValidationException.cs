using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Runtime.Serialization;

namespace System.Data.Entity.ModelConfiguration;

[Serializable]
public class ModelValidationException : Exception
{
	public ModelValidationException()
	{
	}

	public ModelValidationException(string message)
		: base(message)
	{
	}

	public ModelValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	internal ModelValidationException(IEnumerable<DataModelErrorEventArgs> validationErrors)
		: base(validationErrors.ToErrorMessage())
	{
	}

	protected ModelValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
