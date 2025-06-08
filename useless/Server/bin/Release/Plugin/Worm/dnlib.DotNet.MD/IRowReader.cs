using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public interface IRowReader<TRow> where TRow : struct
{
	bool TryReadRow(uint rid, out TRow row);
}
