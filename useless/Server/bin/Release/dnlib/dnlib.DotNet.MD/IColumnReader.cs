namespace dnlib.DotNet.MD;

public interface IColumnReader
{
	bool ReadColumn(MDTable table, uint rid, ColumnInfo column, out uint value);
}
