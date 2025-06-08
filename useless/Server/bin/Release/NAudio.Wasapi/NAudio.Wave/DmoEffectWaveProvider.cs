using System;
using NAudio.Dmo;
using NAudio.Dmo.Effect;

namespace NAudio.Wave;

public class DmoEffectWaveProvider<TDmoEffector, TEffectorParam> : IWaveProvider, IDisposable where TDmoEffector : IDmoEffector<TEffectorParam>, new()
{
	private readonly IWaveProvider inputProvider;

	private readonly IDmoEffector<TEffectorParam> effector;

	public WaveFormat WaveFormat => inputProvider.WaveFormat;

	public TEffectorParam EffectParams => effector.EffectParams;

	public DmoEffectWaveProvider(IWaveProvider inputProvider)
	{
		this.inputProvider = inputProvider;
		effector = new TDmoEffector();
		MediaObject obj = effector.MediaObject ?? throw new NotSupportedException("Dmo Effector Not Supported: TDmoEffector");
		if (!obj.SupportsInputWaveFormat(0, inputProvider.WaveFormat))
		{
			throw new ArgumentException("Unsupported Input Stream format", "inputProvider");
		}
		obj.AllocateStreamingResources();
		obj.SetInputWaveFormat(0, this.inputProvider.WaveFormat);
		obj.SetOutputWaveFormat(0, this.inputProvider.WaveFormat);
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		int num = inputProvider.Read(buffer, offset, count);
		if (effector == null)
		{
			return num;
		}
		if (effector.MediaObjectInPlace.Process(num, offset, buffer, 0L, DmoInPlaceProcessFlags.Normal) == DmoInPlaceProcessReturn.HasEffectTail)
		{
			byte[] data = new byte[num];
			while (effector.MediaObjectInPlace.Process(num, 0, data, 0L, DmoInPlaceProcessFlags.Zero) == DmoInPlaceProcessReturn.HasEffectTail)
			{
			}
		}
		return num;
	}

	public void Dispose()
	{
		if (effector != null)
		{
			effector.MediaObject.FreeStreamingResources();
			effector.Dispose();
		}
	}
}
