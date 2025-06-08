using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoI3DL2Reverb : IDmoEffector<DmoI3DL2Reverb.Params>, IDisposable
{
	public struct Params
	{
		public const int RoomMin = -10000;

		public const int RoomMax = 0;

		public const int RoomDefault = -1000;

		public const int RoomHfMin = -10000;

		public const int RoomHfMax = 0;

		public const int RoomHfDefault = -100;

		public const float RoomRollOffFactorMin = 0f;

		public const float RoomRollOffFactorMax = 10f;

		public const float RoomRollOffFactorDefault = 0f;

		public const float DecayTimeMin = 0.1f;

		public const float DecayTimeMax = 20f;

		public const float DecayTimeDefault = 1.49f;

		public const float DecayHfRatioMin = 0.1f;

		public const float DecayHfRatioMax = 2f;

		public const float DecayHfRatioDefault = 0.83f;

		public const int ReflectionsMin = -10000;

		public const int ReflectionsMax = 1000;

		public const int ReflectionsDefault = -2602;

		public const float ReflectionsDelayMin = 0f;

		public const float ReflectionsDelayMax = 0.3f;

		public const float ReflectionsDelayDefault = 0.007f;

		public const int ReverbMin = -10000;

		public const int ReverbMax = 2000;

		public const int ReverbDefault = 200;

		public const float ReverbDelayMin = 0f;

		public const float ReverbDelayMax = 0.1f;

		public const float ReverbDelayDefault = 0.011f;

		public const float DiffusionMin = 0f;

		public const float DiffusionMax = 100f;

		public const float DiffusionDefault = 100f;

		public const float DensityMin = 0f;

		public const float DensityMax = 100f;

		public const float DensityDefault = 100f;

		public const float HfReferenceMin = 20f;

		public const float HfReferenceMax = 20000f;

		public const float HfReferenceDefault = 5000f;

		public const int QualityMin = 0;

		public const int QualityMax = 3;

		public const int QualityDefault = 2;

		private readonly IDirectSoundFXI3DL2Reverb fxI3Dl2Reverb;

		public int Room
		{
			get
			{
				return GetAllParameters().Room;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.Room = Math.Max(Math.Min(0, value), -10000);
				SetAllParameters(allParameters);
			}
		}

		public int RoomHf
		{
			get
			{
				return GetAllParameters().RoomHf;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.RoomHf = Math.Max(Math.Min(0, value), -10000);
				SetAllParameters(allParameters);
			}
		}

		public float RoomRollOffFactor
		{
			get
			{
				return GetAllParameters().RoomRollOffFactor;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.RoomRollOffFactor = Math.Max(Math.Min(10f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float DecayTime
		{
			get
			{
				return GetAllParameters().DecayTime;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.DecayTime = Math.Max(Math.Min(20f, value), 0.1f);
				SetAllParameters(allParameters);
			}
		}

		public float DecayHfRatio
		{
			get
			{
				return GetAllParameters().DecayHfRatio;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.DecayHfRatio = Math.Max(Math.Min(2f, value), 0.1f);
				SetAllParameters(allParameters);
			}
		}

		public int Reflections
		{
			get
			{
				return GetAllParameters().Reflections;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.Reflections = Math.Max(Math.Min(1000, value), -10000);
				SetAllParameters(allParameters);
			}
		}

		public float ReflectionsDelay
		{
			get
			{
				return GetAllParameters().ReflectionsDelay;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.ReflectionsDelay = Math.Max(Math.Min(0.3f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public int Reverb
		{
			get
			{
				return GetAllParameters().Reverb;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.Reverb = Math.Max(Math.Min(2000, value), -10000);
				SetAllParameters(allParameters);
			}
		}

		public float ReverbDelay
		{
			get
			{
				return GetAllParameters().ReverbDelay;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.ReverbDelay = Math.Max(Math.Min(0.1f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float Diffusion
		{
			get
			{
				return GetAllParameters().Diffusion;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.Diffusion = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float Density
		{
			get
			{
				return GetAllParameters().Density;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.Density = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float HfReference
		{
			get
			{
				return GetAllParameters().HfReference;
			}
			set
			{
				DsFxI3Dl2Reverb allParameters = GetAllParameters();
				allParameters.HfReference = Math.Max(Math.Min(20000f, value), 20f);
				SetAllParameters(allParameters);
			}
		}

		public int Quality
		{
			get
			{
				Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.GetQuality(out var quality));
				return quality;
			}
			set
			{
				Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.SetQuality(value));
			}
		}

		internal Params(IDirectSoundFXI3DL2Reverb dsFxObject)
		{
			fxI3Dl2Reverb = dsFxObject;
		}

		public void SetPreset(I3DL2EnvironmentPreset preset)
		{
			Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.SetPreset((uint)preset));
		}

		public I3DL2EnvironmentPreset GetPreset()
		{
			Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.GetPreset(out var preset));
			return (I3DL2EnvironmentPreset)preset;
		}

		private void SetAllParameters(DsFxI3Dl2Reverb param)
		{
			Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.SetAllParameters(ref param));
		}

		private DsFxI3Dl2Reverb GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxI3Dl2Reverb.GetAllParameters(out var param));
			return param;
		}
	}

	private readonly MediaObject mediaObject;

	private readonly MediaObjectInPlace mediaObjectInPlace;

	private readonly Params effectParams;

	public MediaObject MediaObject => mediaObject;

	public MediaObjectInPlace MediaObjectInPlace => mediaObjectInPlace;

	public Params EffectParams => effectParams;

	public DmoI3DL2Reverb()
	{
		Guid guidi3Dl2Reverb = new Guid("EF985E71-D5C7-42D4-BA4D-2D073E2E96F4");
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, guidi3Dl2Reverb));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			mediaObject = new MediaObject((IMediaObject)obj);
			mediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			effectParams = new Params((IDirectSoundFXI3DL2Reverb)obj);
		}
	}

	public void Dispose()
	{
		mediaObjectInPlace?.Dispose();
		mediaObject?.Dispose();
	}
}
