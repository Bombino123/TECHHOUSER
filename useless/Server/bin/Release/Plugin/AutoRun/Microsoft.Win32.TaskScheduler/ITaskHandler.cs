using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.TaskScheduler;

[ComImport]
[Guid("839D7762-5121-4009-9234-4F0D19394F04")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[SuppressUnmanagedCodeSecurity]
[ComVisible(true)]
public interface ITaskHandler
{
	void Start([In][MarshalAs(UnmanagedType.IUnknown)] object pHandlerServices, [In][MarshalAs(UnmanagedType.BStr)] string data);

	void Stop([MarshalAs(UnmanagedType.Error)] out int pRetCode);

	void Pause();

	void Resume();
}
