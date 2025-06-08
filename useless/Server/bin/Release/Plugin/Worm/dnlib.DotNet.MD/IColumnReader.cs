using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public interface IColumnReader
{
	bool ReadColumn(MDTable table, uint rid, ColumnInfo column, out uint value);
}
