using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioEndpointVolume : IDisposable
{
	private readonly IAudioEndpointVolume audioEndPointVolume;

	private AudioEndpointVolumeCallback callBack;

	private Guid notificationGuid = Guid.Empty;

	public Guid NotificationGuid
	{
		get
		{
			return notificationGuid;
		}
		set
		{
			notificationGuid = value;
		}
	}

	public AudioEndpointVolumeVolumeRange VolumeRange { get; }

	public EEndpointHardwareSupport HardwareSupport { get; }

	public AudioEndpointVolumeStepInformation StepInformation { get; }

	public AudioEndpointVolumeChannels Channels { get; }

	public float MasterVolumeLevel
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMasterVolumeLevel(out var pfLevelDB));
			return pfLevelDB;
		}
		set
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMasterVolumeLevel(value, ref notificationGuid));
		}
	}

	public float MasterVolumeLevelScalar
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMasterVolumeLevelScalar(out var pfLevel));
			return pfLevel;
		}
		set
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMasterVolumeLevelScalar(value, ref notificationGuid));
		}
	}

	public bool Mute
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMute(out var pbMute));
			return pbMute;
		}
		set
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMute(value, ref notificationGuid));
		}
	}

	public event AudioEndpointVolumeNotificationDelegate OnVolumeNotification;

	public void VolumeStepUp()
	{
		Marshal.ThrowExceptionForHR(audioEndPointVolume.VolumeStepUp(ref notificationGuid));
	}

	public void VolumeStepDown()
	{
		Marshal.ThrowExceptionForHR(audioEndPointVolume.VolumeStepDown(ref notificationGuid));
	}

	internal AudioEndpointVolume(IAudioEndpointVolume realEndpointVolume)
	{
		audioEndPointVolume = realEndpointVolume;
		Channels = new AudioEndpointVolumeChannels(audioEndPointVolume);
		StepInformation = new AudioEndpointVolumeStepInformation(audioEndPointVolume);
		Marshal.ThrowExceptionForHR(audioEndPointVolume.QueryHardwareSupport(out var pdwHardwareSupportMask));
		HardwareSupport = (EEndpointHardwareSupport)pdwHardwareSupportMask;
		VolumeRange = new AudioEndpointVolumeVolumeRange(audioEndPointVolume);
		callBack = new AudioEndpointVolumeCallback(this);
		Marshal.ThrowExceptionForHR(audioEndPointVolume.RegisterControlChangeNotify(callBack));
	}

	internal void FireNotification(AudioVolumeNotificationData notificationData)
	{
		this.OnVolumeNotification?.Invoke(notificationData);
	}

	public void Dispose()
	{
		if (callBack != null)
		{
			Marshal.ThrowExceptionForHR(audioEndPointVolume.UnregisterControlChangeNotify(callBack));
			callBack = null;
		}
		Marshal.ReleaseComObject(audioEndPointVolume);
		GC.SuppressFinalize(this);
	}

	~AudioEndpointVolume()
	{
		Dispose();
	}
}
