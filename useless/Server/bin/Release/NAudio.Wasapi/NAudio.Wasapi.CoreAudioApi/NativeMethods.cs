using System;
using System.Runtime.InteropServices;
using NAudio.Wasapi.CoreAudioApi.Interfaces;

namespace NAudio.Wasapi.CoreAudioApi;

internal static class NativeMethods
{
	[DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = false)]
	public static extern void ActivateAudioInterfaceAsync([In][MarshalAs(UnmanagedType.LPWStr)] string deviceInterfacePath, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid, [In] IntPtr activationParams, [In] IActivateAudioInterfaceCompletionHandler completionHandler, out IActivateAudioInterfaceAsyncOperation activationOperation);
}
