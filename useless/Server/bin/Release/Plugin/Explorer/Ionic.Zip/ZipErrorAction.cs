using System.Runtime.InteropServices;

namespace Ionic.Zip;

[ComVisible(true)]
public enum ZipErrorAction
{
	Throw,
	Skip,
	Retry,
	InvokeErrorEvent
}
