using System;
using System.Runtime.InteropServices;
using NAudio.Dmo;
using NAudio.MediaFoundation;

namespace NAudio.Wave;

public class MediaFoundationResampler : MediaFoundationTransform
{
	private int resamplerQuality;

	private static readonly Guid ResamplerClsid = new Guid("f447b69e-1884-4a7e-8055-346f74d6edb3");

	private static readonly Guid IMFTransformIid = new Guid("bf94c121-5b05-4e6f-8000-ba598961414d");

	private IMFActivate activate;

	public int ResamplerQuality
	{
		get
		{
			return resamplerQuality;
		}
		set
		{
			if (value < 1 || value > 60)
			{
				throw new ArgumentOutOfRangeException("Resampler Quality must be between 1 and 60");
			}
			resamplerQuality = value;
		}
	}

	private static bool IsPcmOrIeeeFloat(WaveFormat waveFormat)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		WaveFormatExtensible val = (WaveFormatExtensible)(object)((waveFormat is WaveFormatExtensible) ? waveFormat : null);
		if ((int)waveFormat.Encoding != 1 && (int)waveFormat.Encoding != 3)
		{
			if (val != null)
			{
				if (!(val.SubFormat == AudioSubtypes.MFAudioFormat_PCM))
				{
					return val.SubFormat == AudioSubtypes.MFAudioFormat_Float;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public MediaFoundationResampler(IWaveProvider sourceProvider, WaveFormat outputFormat)
		: base(sourceProvider, outputFormat)
	{
		if (!IsPcmOrIeeeFloat(sourceProvider.WaveFormat))
		{
			throw new ArgumentException("Input must be PCM or IEEE float", "sourceProvider");
		}
		if (!IsPcmOrIeeeFloat(outputFormat))
		{
			throw new ArgumentException("Output must be PCM or IEEE float", "outputFormat");
		}
		MediaFoundationApi.Startup();
		ResamplerQuality = 60;
		object comObject = CreateResamplerComObject();
		FreeComObject(comObject);
	}

	private void FreeComObject(object comObject)
	{
		if (activate != null)
		{
			activate.ShutdownObject();
		}
		Marshal.ReleaseComObject(comObject);
	}

	private object CreateResamplerComObject()
	{
		return new ResamplerMediaComObject();
	}

	private object CreateResamplerComObjectUsingActivator()
	{
		foreach (IMFActivate item in MediaFoundationApi.EnumerateTransforms(MediaFoundationTransformCategories.AudioEffect))
		{
			item.GetGUID(MediaFoundationAttributes.MFT_TRANSFORM_CLSID_Attribute, out var pguidValue);
			if (pguidValue.Equals(ResamplerClsid))
			{
				item.ActivateObject(IMFTransformIid, out var ppv);
				activate = item;
				return ppv;
			}
		}
		return null;
	}

	public MediaFoundationResampler(IWaveProvider sourceProvider, int outputSampleRate)
		: this(sourceProvider, CreateOutputFormat(sourceProvider.WaveFormat, outputSampleRate))
	{
	}

	protected override IMFTransform CreateTransform()
	{
		object obj = CreateResamplerComObject();
		IMFTransform obj2 = (IMFTransform)obj;
		IMFMediaType iMFMediaType = MediaFoundationApi.CreateMediaTypeFromWaveFormat(sourceProvider.WaveFormat);
		obj2.SetInputType(0, iMFMediaType, _MFT_SET_TYPE_FLAGS.None);
		Marshal.ReleaseComObject(iMFMediaType);
		IMFMediaType iMFMediaType2 = MediaFoundationApi.CreateMediaTypeFromWaveFormat(outputWaveFormat);
		obj2.SetOutputType(0, iMFMediaType2, _MFT_SET_TYPE_FLAGS.None);
		Marshal.ReleaseComObject(iMFMediaType2);
		((IWMResamplerProps)obj).SetHalfFilterLength(ResamplerQuality);
		return obj2;
	}

	private static WaveFormat CreateOutputFormat(WaveFormat inputFormat, int outputSampleRate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if ((int)inputFormat.Encoding == 1)
		{
			return new WaveFormat(outputSampleRate, inputFormat.BitsPerSample, inputFormat.Channels);
		}
		if ((int)inputFormat.Encoding == 3)
		{
			return WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, inputFormat.Channels);
		}
		throw new ArgumentException("Can only resample PCM or IEEE float");
	}

	protected override void Dispose(bool disposing)
	{
		if (activate != null)
		{
			activate.ShutdownObject();
			activate = null;
		}
		base.Dispose(disposing);
	}
}
