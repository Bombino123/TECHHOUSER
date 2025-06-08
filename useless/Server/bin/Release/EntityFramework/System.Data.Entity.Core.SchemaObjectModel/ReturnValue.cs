namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ReturnValue<T>
{
	private bool _succeeded;

	private T _value;

	internal bool Succeeded => _succeeded;

	internal T Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			_succeeded = true;
		}
	}
}
