using System;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Utils;

namespace NAudio.CoreAudioApi;

internal class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
{
	private readonly AudioEndpointVolume parent;

	internal AudioEndpointVolumeCallback(AudioEndpointVolume parent)
	{
		this.parent = parent;
	}

	public void OnNotify(IntPtr notifyData)
	{
		AudioVolumeNotificationDataStruct audioVolumeNotificationDataStruct = MarshalHelpers.PtrToStructure<AudioVolumeNotificationDataStruct>(notifyData);
		IntPtr intPtr = MarshalHelpers.OffsetOf<AudioVolumeNotificationDataStruct>("ChannelVolume");
		IntPtr pointer = (IntPtr)((long)notifyData + (long)intPtr);
		float[] array = new float[audioVolumeNotificationDataStruct.nChannels];
		for (int i = 0; i < audioVolumeNotificationDataStruct.nChannels; i++)
		{
			array[i] = MarshalHelpers.PtrToStructure<float>(pointer);
		}
		AudioVolumeNotificationData notificationData = new AudioVolumeNotificationData(audioVolumeNotificationDataStruct.guidEventContext, audioVolumeNotificationDataStruct.bMuted, audioVolumeNotificationDataStruct.fMasterVolume, array, audioVolumeNotificationDataStruct.guidEventContext);
		parent.FireNotification(notificationData);
	}
}
