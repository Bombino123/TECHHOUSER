namespace NAudio.CoreAudioApi;

internal struct AudioClientActivationParams
{
	public AudioClientActivationType ActivationType;

	public AudioClientProcessLoopbackParams ProcessLoopbackParams;
}
