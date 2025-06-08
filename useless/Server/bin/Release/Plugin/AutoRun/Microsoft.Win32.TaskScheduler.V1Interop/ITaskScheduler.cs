using System;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.V1Interop;

[ComImport]
[Guid("148BD527-A2AB-11CE-B11F-00AA00530503")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[SuppressUnmanagedCodeSecurity]
[CoClass(typeof(CTaskScheduler))]
internal interface ITaskScheduler
{
	void SetTargetComputer([In][MarshalAs(UnmanagedType.LPWStr)] string Computer);

	CoTaskMemString GetTargetComputer();

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumWorkItems Enum();

	[return: MarshalAs(UnmanagedType.Interface)]
	ITask Activate([In][NotNull][MarshalAs(UnmanagedType.LPWStr)] string Name, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);

	void Delete([In][NotNull][MarshalAs(UnmanagedType.LPWStr)] string Name);

	[return: MarshalAs(UnmanagedType.Interface)]
	ITask NewWorkItem([In][NotNull][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In][MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);

	void AddWorkItem([In][NotNull][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In][MarshalAs(UnmanagedType.Interface)] ITask WorkItem);

	void IsOfType([In][NotNull][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);
}
