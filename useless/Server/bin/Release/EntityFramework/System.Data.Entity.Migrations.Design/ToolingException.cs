using System.Runtime.Serialization;

namespace System.Data.Entity.Migrations.Design;

[Serializable]
[Obsolete("Use System.Data.Entity.Infrastructure.Design.IErrorHandler instead.")]
public class ToolingException : Exception
{
	[Serializable]
	private struct ToolingExceptionState : ISafeSerializationData
	{
		public string InnerType { get; set; }

		public string InnerStackTrace { get; set; }

		public void CompleteDeserialization(object deserialized)
		{
			((ToolingException)deserialized)._state = this;
		}
	}

	[NonSerialized]
	private ToolingExceptionState _state;

	public string InnerType => _state.InnerType;

	public string InnerStackTrace => _state.InnerStackTrace;

	public ToolingException()
	{
		SubscribeToSerializeObjectState();
	}

	public ToolingException(string message)
		: base(message)
	{
		SubscribeToSerializeObjectState();
	}

	public ToolingException(string message, string innerType, string innerStackTrace)
		: base(message)
	{
		_state.InnerType = innerType;
		_state.InnerStackTrace = innerStackTrace;
		SubscribeToSerializeObjectState();
	}

	public ToolingException(string message, Exception innerException)
		: base(message, innerException)
	{
		SubscribeToSerializeObjectState();
	}

	private void SubscribeToSerializeObjectState()
	{
		base.SerializeObjectState += delegate(object _, SafeSerializationEventArgs a)
		{
			a.AddSerializedState(_state);
		};
	}
}
