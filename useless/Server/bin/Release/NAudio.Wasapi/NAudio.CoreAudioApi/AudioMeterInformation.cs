using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class AudioMeterInformation
{
	private readonly IAudioMeterInformation audioMeterInformation;

	public AudioMeterInformationChannels PeakValues { get; }

	public EEndpointHardwareSupport HardwareSupport { get; }

	public float MasterPeakValue
	{
		get
		{
			Marshal.ThrowExceptionForHR(audioMeterInformation.GetPeakValue(out var pfPeak));
			return pfPeak;
		}
	}

	internal AudioMeterInformation(IAudioMeterInformation realInterface)
	{
		audioMeterInformation = realInterface;
		Marshal.ThrowExceptionForHR(audioMeterInformation.QueryHardwareSupport(out var pdwHardwareSupportMask));
		HardwareSupport = (EEndpointHardwareSupport)pdwHardwareSupportMask;
		PeakValues = new AudioMeterInformationChannels(audioMeterInformation);
	}
}
