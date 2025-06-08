using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

[Serializable]
public abstract class EdmError
{
	private readonly string _message;

	public string Message => _message;

	internal EdmError(string message)
	{
		Check.NotEmpty(message, "message");
		_message = message;
	}
}
