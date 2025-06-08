using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioEndpointVolume : IDisposable
{
	private readonly IAudioEndpointVolume audioEndPointVolume;

	private readonly AudioEndpointVolumeChannels channels;

	private readonly AudioEndpointVolumeStepInformation stepInformation;

	private readonly AudioEndpointVolumeVolumeRange volumeRange;

	private readonly EEndpointHardwareSupport hardwareSupport;

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

	public AudioEndpointVolumeVolumeRange VolumeRange => volumeRange;

	public EEndpointHardwareSupport HardwareSupport => hardwareSupport;

	public AudioEndpointVolumeStepInformation StepInformation => stepInformation;

	public AudioEndpointVolumeChannels Channels => channels;

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
		channels = new AudioEndpointVolumeChannels(audioEndPointVolume);
		stepInformation = new AudioEndpointVolumeStepInformation(audioEndPointVolume);
		Marshal.ThrowExceptionForHR(audioEndPointVolume.QueryHardwareSupport(out var pdwHardwareSupportMask));
		hardwareSupport = (EEndpointHardwareSupport)pdwHardwareSupportMask;
		volumeRange = new AudioEndpointVolumeVolumeRange(audioEndPointVolume);
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
		GC.SuppressFinalize(this);
	}

	~AudioEndpointVolume()
	{
		Dispose();
	}
}
