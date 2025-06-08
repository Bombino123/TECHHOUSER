using NAudio.CoreAudioApi;

namespace NAudio.Wave;

public class WasapiLoopbackCapture : WasapiCapture
{
	public WasapiLoopbackCapture()
		: this(GetDefaultLoopbackCaptureDevice())
	{
	}

	public WasapiLoopbackCapture(MMDevice captureDevice)
		: base(captureDevice)
	{
	}

	public static MMDevice GetDefaultLoopbackCaptureDevice()
	{
		return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
	}

	protected override AudioClientStreamFlags GetAudioClientStreamFlags()
	{
		return AudioClientStreamFlags.Loopback | base.GetAudioClientStreamFlags();
	}
}
