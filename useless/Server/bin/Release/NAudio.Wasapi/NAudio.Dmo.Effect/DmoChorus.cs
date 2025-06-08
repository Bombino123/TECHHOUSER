using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoChorus : IDmoEffector<DmoChorus.Params>, IDisposable
{
	public struct Params
	{
		public const float WetDryMixMin = 0f;

		public const float WetDryMixMax = 100f;

		public const float WetDrtMixDefault = 50f;

		public const float DepthMin = 0f;

		public const float DepthMax = 100f;

		public const float DepthDefault = 10f;

		public const float FeedBackMin = -99f;

		public const float FeedBackMax = 99f;

		public const float FeedBaclDefault = 25f;

		public const float FrequencyMin = 0f;

		public const float FrequencyMax = 10f;

		public const float FrequencyDefault = 1.1f;

		public const ChorusWaveForm WaveFormDefault = ChorusWaveForm.Sin;

		public const float DelayMin = 0f;

		public const float DelayMax = 20f;

		public const float DelayDefault = 16f;

		public const ChorusPhase PhaseDefault = ChorusPhase.Pos90;

		private readonly IDirectSoundFXChorus fxChorus;

		public float WetDryMix
		{
			get
			{
				return GetAllParameters().WetDryMix;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				allParameters.WetDryMix = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float Depth
		{
			get
			{
				return GetAllParameters().Depth;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				allParameters.Depth = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float FeedBack
		{
			get
			{
				return GetAllParameters().FeedBack;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				allParameters.FeedBack = Math.Max(Math.Min(99f, value), -99f);
				SetAllParameters(allParameters);
			}
		}

		public float Frequency
		{
			get
			{
				return GetAllParameters().Frequency;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				allParameters.Frequency = Math.Max(Math.Min(10f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public ChorusWaveForm WaveForm
		{
			get
			{
				return GetAllParameters().WaveForm;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(ChorusWaveForm), value))
				{
					allParameters.WaveForm = value;
				}
				SetAllParameters(allParameters);
			}
		}

		public float Delay
		{
			get
			{
				return GetAllParameters().Delay;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				allParameters.Delay = Math.Max(Math.Min(20f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public ChorusPhase Phase
		{
			get
			{
				return GetAllParameters().Phase;
			}
			set
			{
				DsFxChorus allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(ChorusPhase), value))
				{
					allParameters.Phase = value;
				}
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXChorus dsFxObject)
		{
			fxChorus = dsFxObject;
		}

		private void SetAllParameters(DsFxChorus param)
		{
			Marshal.ThrowExceptionForHR(fxChorus.SetAllParameters(ref param));
		}

		private DsFxChorus GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxChorus.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoChorus()
	{
		Guid guidChorus = new Guid("EFE6629C-81F7-4281-BD91-C9D604A95AF6");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidChorus));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXChorus)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
