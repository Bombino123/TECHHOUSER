using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace NAudio.Dmo;

public struct DmoMediaType
{
	private Guid majortype;

	private Guid subtype;

	private bool bFixedSizeSamples;

	private bool bTemporalCompression;

	private int lSampleSize;

	private Guid formattype;

	private IntPtr pUnk;

	private int cbFormat;

	private IntPtr pbFormat;

	public Guid MajorType => majortype;

	public string MajorTypeName => MediaTypes.GetMediaTypeName(majortype);

	public Guid SubType => subtype;

	public string SubTypeName
	{
		get
		{
			if (majortype == MediaTypes.MEDIATYPE_Audio)
			{
				return AudioMediaSubtypes.GetAudioSubtypeName(subtype);
			}
			return subtype.ToString();
		}
	}

	public bool FixedSizeSamples => bFixedSizeSamples;

	public int SampleSize => lSampleSize;

	public Guid FormatType => formattype;

	public string FormatTypeName
	{
		get
		{
			if (formattype == DmoMediaTypeGuids.FORMAT_None)
			{
				return "None";
			}
			if (formattype == Guid.Empty)
			{
				return "Null";
			}
			if (formattype == DmoMediaTypeGuids.FORMAT_WaveFormatEx)
			{
				return "WaveFormatEx";
			}
			return FormatType.ToString();
		}
	}

	public WaveFormat GetWaveFormat()
	{
		if (formattype == DmoMediaTypeGuids.FORMAT_WaveFormatEx)
		{
			return WaveFormat.MarshalFromPtr(pbFormat);
		}
		throw new InvalidOperationException("Not a WaveFormat type");
	}

	public void SetWaveFormat(WaveFormat waveFormat)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		majortype = MediaTypes.MEDIATYPE_Audio;
		WaveFormatExtensible val = (WaveFormatExtensible)(object)((waveFormat is WaveFormatExtensible) ? waveFormat : null);
		if (val != null)
		{
			subtype = val.SubFormat;
		}
		else
		{
			WaveFormatEncoding encoding = waveFormat.Encoding;
			if ((int)encoding != 1)
			{
				if ((int)encoding != 3)
				{
					if ((int)encoding != 85)
					{
						throw new ArgumentException($"Not a supported encoding {waveFormat.Encoding}");
					}
					subtype = AudioMediaSubtypes.WMMEDIASUBTYPE_MP3;
				}
				else
				{
					subtype = AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT;
				}
			}
			else
			{
				subtype = AudioMediaSubtypes.MEDIASUBTYPE_PCM;
			}
		}
		bFixedSizeSamples = SubType == AudioMediaSubtypes.MEDIASUBTYPE_PCM || SubType == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT;
		formattype = DmoMediaTypeGuids.FORMAT_WaveFormatEx;
		if (cbFormat < Marshal.SizeOf<WaveFormat>(waveFormat))
		{
			throw new InvalidOperationException("Not enough memory assigned for a WaveFormat structure");
		}
		Marshal.StructureToPtr<WaveFormat>(waveFormat, pbFormat, fDeleteOld: false);
	}
}
