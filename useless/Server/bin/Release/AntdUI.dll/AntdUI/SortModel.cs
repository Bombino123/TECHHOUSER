namespace AntdUI;

internal class SortModel
{
	public int i { get; set; }

	public string v { get; set; }

	public SortModel(int _i, string? _v)
	{
		i = _i;
		v = _v ?? "";
	}
}
