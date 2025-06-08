using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoWavesReverb : IDmoEffector<DmoWavesReverb.Params>, IDisposable
{
	public struct Params
	{
		public const float InGainMin = -96f;

		public const float InGainMax = 0f;

		public const float InGainDefault = 0f;

		public const float ReverbMixMin = -96f;

		public const float ReverbMixMax = 0f;

		public const float ReverbMixDefault = 0f;

		public const float ReverbTimeMin = 0.001f;

		public const float ReverbTimeMax = 3000f;

		public const float ReverbTimeDefault = 1000f;

		public const float HighFreqRtRatioMin = 0.001f;

		public const float HighFreqRtRatioMax = 0.999f;

		public const float HighFreqRtRatioDefault = 0.001f;

		private readonly IDirectSoundFXWavesReverb fxWavesReverb;

		public float InGain
		{
			get
			{
				return GetAllParameters().InGain;
			}
			set
			{
				DsFxWavesReverb allParameters = GetAllParameters();
				allParameters.InGain = Math.Max(Math.Min(0f, value), -96f);
				SetAllParameters(allParameters);
			}
		}

		public float ReverbMix
		{
			get
			{
				return GetAllParameters().ReverbMix;
			}
			set
			{
				DsFxWavesReverb allParameters = GetAllParameters();
				allParameters.ReverbMix = Math.Max(Math.Min(0f, value), -96f);
				SetAllParameters(allParameters);
			}
		}

		public float ReverbTime
		{
			get
			{
				return GetAllParameters().ReverbTime;
			}
			set
			{
				DsFxWavesReverb allParameters = GetAllParameters();
				allParameters.ReverbTime = Math.Max(Math.Min(3000f, value), 0.001f);
				SetAllParameters(allParameters);
			}
		}

		public float HighFreqRtRatio
		{
			get
			{
				return GetAllParameters().HighFreqRtRatio;
			}
			set
			{
				DsFxWavesReverb allParameters = GetAllParameters();
				allParameters.HighFreqRtRatio = Math.Max(Math.Min(0.999f, value), 0.001f);
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXWavesReverb dsFxObject)
		{
			fxWavesReverb = dsFxObject;
		}

		private void SetAllParameters(DsFxWavesReverb param)
		{
			Marshal.ThrowExceptionForHR(fxWavesReverb.SetAllParameters(ref param));
		}

		private DsFxWavesReverb GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxWavesReverb.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoWavesReverb()
	{
		Guid guidWavesReverb = new Guid("87FC0268-9A55-4360-95AA-004A1D9DE26C");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidWavesReverb));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXWavesReverb)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
