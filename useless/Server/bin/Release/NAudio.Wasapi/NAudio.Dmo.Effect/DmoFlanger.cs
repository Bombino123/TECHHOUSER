using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoFlanger : IDmoEffector<DmoFlanger.Params>, IDisposable
{
	public struct Params
	{
		public const float WetDryMixMin = 0f;

		public const float WetDryMixMax = 100f;

		public const float WetDrtMixDefault = 50f;

		public const float DepthMin = 0f;

		public const float DepthMax = 100f;

		public const float DepthDefault = 100f;

		public const float FeedBackMin = -99f;

		public const float FeedBackMax = 99f;

		public const float FeedBaclDefault = -50f;

		public const float FrequencyMin = 0f;

		public const float FrequencyMax = 10f;

		public const float FrequencyDefault = 0.25f;

		public const FlangerWaveForm WaveFormDefault = FlangerWaveForm.Sin;

		public const float DelayMin = 0f;

		public const float DelayMax = 4f;

		public const float DelayDefault = 2f;

		public const FlangerPhase PhaseDefault = FlangerPhase.Zero;

		private readonly IDirectSoundFXFlanger fxFlanger;

		public float WetDryMix
		{
			get
			{
				return GetAllParameters().WetDryMix;
			}
			set
			{
				DsFxFlanger allParameters = GetAllParameters();
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
				DsFxFlanger allParameters = GetAllParameters();
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
				DsFxFlanger allParameters = GetAllParameters();
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
				DsFxFlanger allParameters = GetAllParameters();
				allParameters.Frequency = Math.Max(Math.Min(10f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public FlangerWaveForm WaveForm
		{
			get
			{
				return GetAllParameters().WaveForm;
			}
			set
			{
				DsFxFlanger allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(FlangerWaveForm), value))
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
				DsFxFlanger allParameters = GetAllParameters();
				allParameters.Delay = Math.Max(Math.Min(4f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public FlangerPhase Phase
		{
			get
			{
				return GetAllParameters().Phase;
			}
			set
			{
				DsFxFlanger allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(FlangerPhase), value))
				{
					allParameters.Phase = value;
				}
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXFlanger dsFxObject)
		{
			fxFlanger = dsFxObject;
		}

		private void SetAllParameters(DsFxFlanger param)
		{
			Marshal.ThrowExceptionForHR(fxFlanger.SetAllParameters(ref param));
		}

		private DsFxFlanger GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxFlanger.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoFlanger()
	{
		Guid guidFlanger = new Guid("EFCA3D92-DFD8-4672-A603-7420894BAD98");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidFlanger));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXFlanger)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
