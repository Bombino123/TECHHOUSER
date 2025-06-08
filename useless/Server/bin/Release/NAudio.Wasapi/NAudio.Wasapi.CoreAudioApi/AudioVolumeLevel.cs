using System;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.Wasapi.CoreAudioApi;

public class AudioVolumeLevel
{
	private readonly IAudioVolumeLevel audioVolumeLevelInterface;

	public uint ChannelCount
	{
		get
		{
			audioVolumeLevelInterface.GetChannelCount(out var channels);
			return channels;
		}
	}

	internal AudioVolumeLevel(IAudioVolumeLevel audioVolumeLevel)
	{
		audioVolumeLevelInterface = audioVolumeLevel;
	}

	public void GetLevelRange(uint channel, out float minLevelDb, out float maxLevelDb, out float stepping)
	{
		audioVolumeLevelInterface.GetLevelRange(channel, out minLevelDb, out maxLevelDb, out stepping);
	}

	public float GetLevel(uint channel)
	{
		audioVolumeLevelInterface.GetLevel(channel, out var levelDb);
		return levelDb;
	}

	public void SetLevel(uint channel, float value)
	{
		Guid eventGuidContext = Guid.Empty;
		audioVolumeLevelInterface.SetLevel(channel, value, ref eventGuidContext);
	}

	public void SetLevelUniform(float value)
	{
		Guid eventGuidContext = Guid.Empty;
		audioVolumeLevelInterface.SetLevelUniform(value, ref eventGuidContext);
	}

	public void SetLevelAllChannel(float[] values, uint channels)
	{
		Guid eventGuidContext = Guid.Empty;
		audioVolumeLevelInterface.SetLevelAllChannel(values, channels, ref eventGuidContext);
	}
}
