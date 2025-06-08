using System.Runtime.InteropServices;

namespace NAudio.Wasapi.CoreAudioApi.Interfaces;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")]
public interface IActivateAudioInterfaceAsyncOperation
{
	void GetActivateResult(out int activateResult, [MarshalAs(UnmanagedType.IUnknown)] out object activateInterface);
}
