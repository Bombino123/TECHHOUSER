using System;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioMute
{
	private IAudioMute audioMuteInterface;

	public bool IsMuted
	{
		get
		{
			audioMuteInterface.GetMute(out var mute);
			return mute;
		}
		set
		{
			Guid empty = Guid.Empty;
			IAudioMute audioMute = audioMuteInterface;
			Guid eventContext = empty;
			audioMute.SetMute(value, ref eventContext);
		}
	}

	internal AudioMute(IAudioMute audioMute)
	{
		audioMuteInterface = audioMute;
	}
}
