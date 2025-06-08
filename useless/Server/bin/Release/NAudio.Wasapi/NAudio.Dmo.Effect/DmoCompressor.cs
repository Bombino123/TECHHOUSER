using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoCompressor : IDmoEffector<DmoCompressor.Params>, IDisposable
{
	public struct Params
	{
		public const float GainMin = -60f;

		public const float GainMax = 60f;

		public const float GainDefault = 0f;

		public const float AttackMin = 0.01f;

		public const float AttackMax = 500f;

		public const float AttackDefault = 10f;

		public const float ReleaseMin = 50f;

		public const float ReleaseMax = 3000f;

		public const float ReleaseDefault = 200f;

		public const float ThresholdMin = -60f;

		public const float ThresholdMax = 0f;

		public const float TjresholdDefault = -20f;

		public const float RatioMin = 1f;

		public const float RatioMax = 100f;

		public const float RatioDefault = 3f;

		public const float PreDelayMin = 0f;

		public const float PreDelayMax = 4f;

		public const float PreDelayDefault = 4f;

		private readonly IDirectSoundFXCompressor fxCompressor;

		public float Gain
		{
			get
			{
				return GetAllParameters().Gain;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.Gain = Math.Max(Math.Min(60f, value), -60f);
				SetAllParameters(allParameters);
			}
		}

		public float Attack
		{
			get
			{
				return GetAllParameters().Attack;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.Attack = Math.Max(Math.Min(500f, value), 0.01f);
				SetAllParameters(allParameters);
			}
		}

		public float Release
		{
			get
			{
				return GetAllParameters().Release;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.Release = Math.Max(Math.Min(3000f, value), 50f);
				SetAllParameters(allParameters);
			}
		}

		public float Threshold
		{
			get
			{
				return GetAllParameters().Threshold;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.Threshold = Math.Max(Math.Min(0f, value), -60f);
				SetAllParameters(allParameters);
			}
		}

		public float Ratio
		{
			get
			{
				return GetAllParameters().Ratio;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.Ratio = Math.Max(Math.Min(100f, value), 1f);
				SetAllParameters(allParameters);
			}
		}

		public float PreDelay
		{
			get
			{
				return GetAllParameters().PreDelay;
			}
			set
			{
				DsFxCompressor allParameters = GetAllParameters();
				allParameters.PreDelay = Math.Max(Math.Min(4f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXCompressor dsFxObject)
		{
			fxCompressor = dsFxObject;
		}

		private void SetAllParameters(DsFxCompressor param)
		{
			Marshal.ThrowExceptionForHR(fxCompressor.SetAllParameters(ref param));
		}

		private DsFxCompressor GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxCompressor.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoCompressor()
	{
		Guid guidChorus = new Guid("EF011F79-4000-406D-87AF-BFFB3FC39D57");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidChorus));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXCompressor)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
