using System.Runtime.InteropServices;

namespace dnlib.Threading;

[ComVisible(true)]
public interface ICancellationToken
{
	void ThrowIfCancellationRequested();
}
