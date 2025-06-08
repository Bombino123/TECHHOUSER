using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoParamEq : IDmoEffector<DmoParamEq.Params>, IDisposable
{
	public struct Params
	{
		public const float CenterMin = 80f;

		public const float CenterMax = 16000f;

		public const float CenterDefault = 8000f;

		public const float BandWidthMin = 1f;

		public const float BandWidthMax = 36f;

		public const float BandWidthDefault = 12f;

		public const float GainMin = -15f;

		public const float GainMax = 15f;

		public const float GainDefault = 0f;

		private readonly IDirectSoundFxParamEq fxParamEq;

		public float Center
		{
			get
			{
				return GetAllParameters().Center;
			}
			set
			{
				DsFxParamEq allParameters = GetAllParameters();
				allParameters.Center = Math.Max(Math.Min(16000f, value), 80f);
				SetAllParameters(allParameters);
			}
		}

		public float BandWidth
		{
			get
			{
				return GetAllParameters().BandWidth;
			}
			set
			{
				DsFxParamEq allParameters = GetAllParameters();
				allParameters.BandWidth = Math.Max(Math.Min(36f, value), 1f);
				SetAllParameters(allParameters);
			}
		}

		public float Gain
		{
			get
			{
				return GetAllParameters().Gain;
			}
			set
			{
				DsFxParamEq allParameters = GetAllParameters();
				allParameters.Gain = Math.Max(Math.Min(15f, value), -15f);
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFxParamEq dsFxObject)
		{
			fxParamEq = dsFxObject;
		}

		private void SetAllParameters(DsFxParamEq param)
		{
			Marshal.ThrowExceptionForHR(fxParamEq.SetAllParameters(ref param));
		}

		private DsFxParamEq GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxParamEq.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoParamEq()
	{
		Guid guidParamEq = new Guid("120CED89-3BF4-4173-A132-3CB406CF3231");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidParamEq));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFxParamEq)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
