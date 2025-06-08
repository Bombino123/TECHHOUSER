using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wasapi.CoreAudioApi.Interfaces;

namespace NAudio.Wasapi.CoreAudioApi;

internal class ActivateAudioInterfaceCompletionHandler : IActivateAudioInterfaceCompletionHandler, IAgileObject
{
	private Action<IAudioClient2> initializeAction;

	private TaskCompletionSource<IAudioClient2> tcs = new TaskCompletionSource<IAudioClient2>();

	public ActivateAudioInterfaceCompletionHandler(Action<IAudioClient2> initializeAction)
	{
		this.initializeAction = initializeAction;
	}

	public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation)
	{
		activateOperation.GetActivateResult(out var activateResult, out var activateInterface);
		if (activateResult != 0)
		{
			tcs.TrySetException(Marshal.GetExceptionForHR(activateResult, new IntPtr(-1)));
			return;
		}
		IAudioClient2 audioClient = (IAudioClient2)activateInterface;
		try
		{
			initializeAction(audioClient);
			tcs.SetResult(audioClient);
		}
		catch (Exception exception)
		{
			tcs.TrySetException(exception);
		}
	}

	public TaskAwaiter<IAudioClient2> GetAwaiter()
	{
		return tcs.Task.GetAwaiter();
	}
}
