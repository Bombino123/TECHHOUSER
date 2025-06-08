using System.Data.Entity.Utilities;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class PropertyConstraintException : ConstraintException
{
	public string PropertyName { get; }

	public PropertyConstraintException()
	{
	}

	public PropertyConstraintException(string message)
		: base(message)
	{
	}

	public PropertyConstraintException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public PropertyConstraintException(string message, string propertyName)
		: base(message)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyName = propertyName;
	}

	public PropertyConstraintException(string message, string propertyName, Exception innerException)
		: base(message, innerException)
	{
		Check.NotEmpty(propertyName, "propertyName");
		PropertyName = propertyName;
	}

	private PropertyConstraintException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		PropertyName = info.GetString("PropertyName");
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PropertyName", PropertyName);
	}
}
