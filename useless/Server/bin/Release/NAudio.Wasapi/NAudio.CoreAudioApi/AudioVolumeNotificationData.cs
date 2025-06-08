using System;

namespace NAudio.CoreAudioApi;

public class AudioVolumeNotificationData
{
	public Guid EventContext { get; }

	public bool Muted { get; }

	public Guid Guid { get; }

	public float MasterVolume { get; }

	public int Channels { get; }

	public float[] ChannelVolume { get; }

	public AudioVolumeNotificationData(Guid eventContext, bool muted, float masterVolume, float[] channelVolume, Guid guid)
	{
		EventContext = eventContext;
		Muted = muted;
		MasterVolume = masterVolume;
		Channels = channelVolume.Length;
		ChannelVolume = channelVolume;
		Guid = guid;
	}
}
