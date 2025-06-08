using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoEcho : IDmoEffector<DmoEcho.Params>, IDisposable
{
	public struct Params
	{
		public const float WetDryMixMin = 0f;

		public const float WetDryMixMax = 100f;

		public const float WetDeyMixDefault = 50f;

		public const float FeedBackMin = 0f;

		public const float FeedBackMax = 100f;

		public const float FeedBackDefault = 50f;

		public const float LeftDelayMin = 1f;

		public const float LeftDelayMax = 2000f;

		public const float LeftDelayDefault = 500f;

		public const float RightDelayMin = 1f;

		public const float RightDelayMax = 2000f;

		public const float RightDelayDefault = 500f;

		public const EchoPanDelay PanDelayDefault = EchoPanDelay.Off;

		private readonly IDirectSoundFXEcho fxEcho;

		public float WetDryMix
		{
			get
			{
				return GetAllParameters().WetDryMix;
			}
			set
			{
				DsFxEcho allParameters = GetAllParameters();
				allParameters.WetDryMix = Math.Max(Math.Min(100f, value), 0f);
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
				DsFxEcho allParameters = GetAllParameters();
				allParameters.FeedBack = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float LeftDelay
		{
			get
			{
				return GetAllParameters().LeftDelay;
			}
			set
			{
				DsFxEcho allParameters = GetAllParameters();
				allParameters.LeftDelay = Math.Max(Math.Min(2000f, value), 1f);
				SetAllParameters(allParameters);
			}
		}

		public float RightDelay
		{
			get
			{
				return GetAllParameters().RightDelay;
			}
			set
			{
				DsFxEcho allParameters = GetAllParameters();
				allParameters.RightDelay = Math.Max(Math.Min(2000f, value), 1f);
				SetAllParameters(allParameters);
			}
		}

		public EchoPanDelay PanDelay
		{
			get
			{
				return GetAllParameters().PanDelay;
			}
			set
			{
				DsFxEcho allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(EchoPanDelay), value))
				{
					allParameters.PanDelay = value;
				}
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXEcho dsFxObject)
		{
			fxEcho = dsFxObject;
		}

		private void SetAllParameters(DsFxEcho param)
		{
			Marshal.ThrowExceptionForHR(fxEcho.SetAllParameters(ref param));
		}

		private DsFxEcho GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxEcho.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoEcho()
	{
		Guid guidEcho = new Guid("EF3E932C-D40B-4F51-8CCF-3F98F1B29D5D");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidEcho));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXEcho)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
