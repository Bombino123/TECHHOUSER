using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public class UpdateException : DataException
{
	[NonSerialized]
	private readonly ReadOnlyCollection<ObjectStateEntry> _stateEntries;

	public ReadOnlyCollection<ObjectStateEntry> StateEntries => _stateEntries;

	public UpdateException()
	{
	}

	public UpdateException(string message)
		: base(message)
	{
	}

	public UpdateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public UpdateException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
		: base(message, innerException)
	{
		List<ObjectStateEntry> list = new List<ObjectStateEntry>(stateEntries);
		_stateEntries = new ReadOnlyCollection<ObjectStateEntry>(list);
	}

	protected UpdateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
