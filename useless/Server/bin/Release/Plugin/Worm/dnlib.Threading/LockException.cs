using System;
using System.Runtime.Serialization;

namespace dnlib.Threading;

[Serializable]
internal class LockException : Exception
{
	public LockException()
	{
	}

	public LockException(string msg)
		: base(msg)
	{
	}

	protected LockException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
