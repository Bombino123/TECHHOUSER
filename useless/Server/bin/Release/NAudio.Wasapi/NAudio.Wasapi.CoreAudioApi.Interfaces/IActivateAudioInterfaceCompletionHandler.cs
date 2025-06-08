using System.Runtime.InteropServices;

namespace NAudio.Wasapi.CoreAudioApi.Interfaces;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("41D949AB-9862-444A-80F6-C261334DA5EB")]
public interface IActivateAudioInterfaceCompletionHandler
{
	void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation);
}
