using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class MetadataException : EntityException
{
	private const int HResultMetadata = -2146232007;

	public MetadataException()
		: base(Strings.Metadata_General_Error)
	{
		base.HResult = -2146232007;
	}

	public MetadataException(string message)
		: base(message)
	{
		base.HResult = -2146232007;
	}

	public MetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232007;
	}

	private MetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
