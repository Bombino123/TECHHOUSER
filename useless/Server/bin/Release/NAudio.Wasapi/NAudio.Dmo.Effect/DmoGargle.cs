using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoGargle : IDmoEffector<DmoGargle.Params>, IDisposable
{
	public struct Params
	{
		public const uint RateHzMin = 1u;

		public const uint RateHzMax = 1000u;

		public const uint RateHzDefault = 20u;

		public const GargleWaveShape WaveShapeDefault = GargleWaveShape.Triangle;

		private readonly IDirectSoundFXGargle fxGargle;

		public uint RateHz
		{
			get
			{
				return GetAllParameters().RateHz;
			}
			set
			{
				DsFxGargle allParameters = GetAllParameters();
				allParameters.RateHz = Math.Max(Math.Min(1000u, value), 1u);
				SetAllParameters(allParameters);
			}
		}

		public GargleWaveShape WaveShape
		{
			get
			{
				return GetAllParameters().WaveShape;
			}
			set
			{
				DsFxGargle allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(GargleWaveShape), value))
				{
					allParameters.WaveShape = value;
				}
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXGargle dsFxObject)
		{
			fxGargle = dsFxObject;
		}

		private void SetAllParameters(DsFxGargle param)
		{
			Marshal.ThrowExceptionForHR(fxGargle.SetAllParameters(ref param));
		}

		private DsFxGargle GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxGargle.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoGargle()
	{
		Guid guidGargle = new Guid("DAFD8210-5711-4B91-9FE3-F75B7AE279BF");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidGargle));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXGargle)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
