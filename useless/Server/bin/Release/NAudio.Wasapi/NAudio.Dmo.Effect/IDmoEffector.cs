using System;

namespace NAudio.Dmo.Effect;

public interface IDmoEffector<out TParameters> : IDisposable
{
	MediaObject MediaObject { get; }

	MediaObjectInPlace MediaObjectInPlace { get; }

	TParameters EffectParams { get; }
}
