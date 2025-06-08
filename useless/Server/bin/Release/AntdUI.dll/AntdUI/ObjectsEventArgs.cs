namespace AntdUI;

public class ObjectsEventArgs : VEventArgs<object[]>
{
	public ObjectsEventArgs(object[] value)
		: base(value)
	{
	}
}
