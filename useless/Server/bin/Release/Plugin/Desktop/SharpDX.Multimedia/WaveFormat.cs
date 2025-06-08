using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.Multimedia;

public class WaveFormat
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct __Native
	{
		public __PcmNative pcmWaveFormat;

		public short extraSize;

		internal void __MarshalFree()
		{
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct __PcmNative
	{
		public WaveFormatEncoding waveFormatTag;

		public short channels;

		public int sampleRate;

		public int averageBytesPerSecond;

		public short blockAlign;

		public short bitsPerSample;

		internal void __MarshalFree()
		{
		}
	}

	protected WaveFormatEncoding waveFormatTag;

	protected short channels;

	protected int sampleRate;

	protected int averageBytesPerSecond;

	protected short blockAlign;

	protected short bitsPerSample;

	protected short extraSize;

	public WaveFormatEncoding Encoding => waveFormatTag;

	public int Channels => channels;

	public int SampleRate => sampleRate;

	public int AverageBytesPerSecond => averageBytesPerSecond;

	public int BlockAlign => blockAlign;

	public int BitsPerSample => bitsPerSample;

	public int ExtraSize => extraSize;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		waveFormatTag = @ref.pcmWaveFormat.waveFormatTag;
		channels = @ref.pcmWaveFormat.channels;
		sampleRate = @ref.pcmWaveFormat.sampleRate;
		averageBytesPerSecond = @ref.pcmWaveFormat.averageBytesPerSecond;
		blockAlign = @ref.pcmWaveFormat.blockAlign;
		bitsPerSample = @ref.pcmWaveFormat.bitsPerSample;
		extraSize = @ref.extraSize;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.pcmWaveFormat.waveFormatTag = waveFormatTag;
		@ref.pcmWaveFormat.channels = channels;
		@ref.pcmWaveFormat.sampleRate = sampleRate;
		@ref.pcmWaveFormat.averageBytesPerSecond = averageBytesPerSecond;
		@ref.pcmWaveFormat.blockAlign = blockAlign;
		@ref.pcmWaveFormat.bitsPerSample = bitsPerSample;
		@ref.extraSize = extraSize;
	}

	internal void __MarshalFree(ref __PcmNative @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __PcmNative @ref)
	{
		waveFormatTag = @ref.waveFormatTag;
		channels = @ref.channels;
		sampleRate = @ref.sampleRate;
		averageBytesPerSecond = @ref.averageBytesPerSecond;
		blockAlign = @ref.blockAlign;
		bitsPerSample = @ref.bitsPerSample;
		extraSize = 0;
	}

	internal void __MarshalTo(ref __PcmNative @ref)
	{
		@ref.waveFormatTag = waveFormatTag;
		@ref.channels = channels;
		@ref.sampleRate = sampleRate;
		@ref.averageBytesPerSecond = averageBytesPerSecond;
		@ref.blockAlign = blockAlign;
		@ref.bitsPerSample = bitsPerSample;
	}

	public WaveFormat()
		: this(44100, 16, 2)
	{
	}

	public WaveFormat(int sampleRate, int channels)
		: this(sampleRate, 16, channels)
	{
	}

	public int ConvertLatencyToByteSize(int milliseconds)
	{
		int num = (int)((double)AverageBytesPerSecond / 1000.0 * (double)milliseconds);
		if (num % BlockAlign != 0)
		{
			num = num + BlockAlign - num % BlockAlign;
		}
		return num;
	}

	public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels, int averageBytesPerSecond, int blockAlign, int bitsPerSample)
	{
		return new WaveFormat
		{
			waveFormatTag = tag,
			channels = (short)channels,
			sampleRate = sampleRate,
			averageBytesPerSecond = averageBytesPerSecond,
			blockAlign = (short)blockAlign,
			bitsPerSample = (short)bitsPerSample,
			extraSize = 0
		};
	}

	public static WaveFormat CreateALawFormat(int sampleRate, int channels)
	{
		return CreateCustomFormat(WaveFormatEncoding.Alaw, sampleRate, channels, sampleRate * channels, 1, 8);
	}

	public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
	{
		return CreateCustomFormat(WaveFormatEncoding.Mulaw, sampleRate, channels, sampleRate * channels, 1, 8);
	}

	public WaveFormat(int rate, int bits, int channels)
	{
		if (channels < 1)
		{
			throw new ArgumentOutOfRangeException("channels", "Channels must be 1 or greater");
		}
		waveFormatTag = ((bits < 32) ? WaveFormatEncoding.Pcm : WaveFormatEncoding.IeeeFloat);
		this.channels = (short)channels;
		sampleRate = rate;
		bitsPerSample = (short)bits;
		extraSize = 0;
		blockAlign = (short)(channels * (bits / 8));
		averageBytesPerSecond = sampleRate * blockAlign;
	}

	public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels)
	{
		WaveFormat waveFormat = new WaveFormat
		{
			waveFormatTag = WaveFormatEncoding.IeeeFloat,
			channels = (short)channels,
			bitsPerSample = 32,
			sampleRate = sampleRate,
			blockAlign = (short)(4 * channels)
		};
		waveFormat.averageBytesPerSecond = sampleRate * waveFormat.blockAlign;
		waveFormat.extraSize = 0;
		return waveFormat;
	}

	public unsafe static WaveFormat MarshalFrom(byte[] rawdata)
	{
		fixed (byte* ptr = rawdata)
		{
			void* ptr2 = ptr;
			return MarshalFrom((IntPtr)ptr2);
		}
	}

	public unsafe static WaveFormat MarshalFrom(IntPtr pointer)
	{
		if (pointer == IntPtr.Zero)
		{
			return null;
		}
		__PcmNative @ref = *(__PcmNative*)(void*)pointer;
		WaveFormatEncoding waveFormatEncoding = @ref.waveFormatTag;
		if (@ref.channels <= 2 && (waveFormatEncoding == WaveFormatEncoding.Pcm || waveFormatEncoding == WaveFormatEncoding.IeeeFloat || waveFormatEncoding == WaveFormatEncoding.Wmaudio2 || waveFormatEncoding == WaveFormatEncoding.Wmaudio3))
		{
			WaveFormat waveFormat = new WaveFormat();
			waveFormat.__MarshalFrom(ref @ref);
			return waveFormat;
		}
		switch (waveFormatEncoding)
		{
		case WaveFormatEncoding.Extensible:
		{
			WaveFormatExtensible waveFormatExtensible = new WaveFormatExtensible();
			waveFormatExtensible.__MarshalFrom(ref *(WaveFormatExtensible.__Native*)(void*)pointer);
			return waveFormatExtensible;
		}
		case WaveFormatEncoding.Adpcm:
		{
			WaveFormatAdpcm waveFormatAdpcm = new WaveFormatAdpcm();
			waveFormatAdpcm.__MarshalFrom(ref *(WaveFormatAdpcm.__Native*)(void*)pointer);
			return waveFormatAdpcm;
		}
		default:
			throw new InvalidOperationException($"Unsupported WaveFormat [{waveFormatEncoding}]");
		}
	}

	protected unsafe virtual IntPtr MarshalToPtr()
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Utilities.SizeOf<__Native>());
		__MarshalTo(ref *(__Native*)(void*)intPtr);
		return intPtr;
	}

	public static IntPtr MarshalToPtr(WaveFormat format)
	{
		return format?.MarshalToPtr() ?? IntPtr.Zero;
	}

	public WaveFormat(BinaryReader br)
	{
		int num = br.ReadInt32();
		if (num < 16)
		{
			throw new SharpDXException("Invalid WaveFormat Structure");
		}
		waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
		channels = br.ReadInt16();
		sampleRate = br.ReadInt32();
		averageBytesPerSecond = br.ReadInt32();
		blockAlign = br.ReadInt16();
		bitsPerSample = br.ReadInt16();
		extraSize = 0;
		if (num > 16)
		{
			extraSize = br.ReadInt16();
			if (extraSize > num - 18)
			{
				extraSize = (short)(num - 18);
			}
		}
	}

	public override string ToString()
	{
		WaveFormatEncoding waveFormatEncoding = waveFormatTag;
		if (waveFormatEncoding == WaveFormatEncoding.Extensible || waveFormatEncoding == WaveFormatEncoding.Pcm)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} bit PCM: {1}kHz {2} channels", new object[3]
			{
				bitsPerSample,
				sampleRate / 1000,
				channels
			});
		}
		return waveFormatTag.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is WaveFormat))
		{
			return false;
		}
		WaveFormat waveFormat = (WaveFormat)obj;
		if (waveFormatTag == waveFormat.waveFormatTag && channels == waveFormat.channels && sampleRate == waveFormat.sampleRate && averageBytesPerSecond == waveFormat.averageBytesPerSecond && blockAlign == waveFormat.blockAlign)
		{
			return bitsPerSample == waveFormat.bitsPerSample;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)waveFormatTag ^ (int)channels ^ sampleRate ^ averageBytesPerSecond ^ blockAlign ^ bitsPerSample;
	}
}
