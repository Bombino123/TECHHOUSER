using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class KsJackDescription
{
	private readonly IKsJackDescription ksJackDescriptionInterface;

	public uint Count
	{
		get
		{
			ksJackDescriptionInterface.GetJackCount(out var jacks);
			return jacks;
		}
	}

	public string this[uint index]
	{
		get
		{
			ksJackDescriptionInterface.GetJackDescription(index, out var description);
			return description;
		}
	}

	internal KsJackDescription(IKsJackDescription ksJackDescription)
	{
		ksJackDescriptionInterface = ksJackDescription;
	}
}
