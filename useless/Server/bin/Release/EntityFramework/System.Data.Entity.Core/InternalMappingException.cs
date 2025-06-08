using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
internal class InternalMappingException : EntityException
{
	private readonly ErrorLog m_errorLog;

	internal ErrorLog ErrorLog => m_errorLog;

	internal InternalMappingException()
	{
	}

	internal InternalMappingException(string message)
		: base(message)
	{
	}

	internal InternalMappingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected InternalMappingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal InternalMappingException(string message, ErrorLog errorLog)
		: base(message)
	{
		m_errorLog = errorLog;
	}

	internal InternalMappingException(string message, ErrorLog.Record record)
		: base(message)
	{
		m_errorLog = new ErrorLog();
		m_errorLog.AddEntry(record);
	}
}
