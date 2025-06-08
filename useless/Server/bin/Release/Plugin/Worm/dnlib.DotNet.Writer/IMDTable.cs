using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IMDTable
{
	Table Table { get; }

	bool IsEmpty { get; }

	int Rows { get; }

	bool IsSorted { get; set; }

	bool IsReadOnly { get; }

	TableInfo TableInfo { get; set; }

	void SetReadOnly();
}
