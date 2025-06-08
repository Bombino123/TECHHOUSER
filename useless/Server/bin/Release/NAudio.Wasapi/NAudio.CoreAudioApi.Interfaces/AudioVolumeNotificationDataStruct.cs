using System;

namespace NAudio.CoreAudioApi.Interfaces;

internal struct AudioVolumeNotificationDataStruct
{
	public Guid guidEventContext;

	public bool bMuted;

	public float fMasterVolume;

	public uint nChannels;

	public float ChannelVolume;
}
