namespace AntdUI;

internal class ObjectItemSearch
{
	public int Weight { get; set; }

	public ObjectItem Value { get; set; }

	public ObjectItemSearch(int weigth, ObjectItem value)
	{
		Weight = weigth;
		Value = value;
	}
}
