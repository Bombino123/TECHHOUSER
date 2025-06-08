using System;

namespace NAudio.Wave.Asio;

internal class AsioSampleConvertor
{
	public delegate void SampleConvertor(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples);

	public static SampleConvertor SelectSampleConvertor(WaveFormat waveFormat, AsioSampleType asioType)
	{
		SampleConvertor result = null;
		bool flag = waveFormat.Channels == 2;
		switch (asioType)
		{
		case AsioSampleType.Int32LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				result = (flag ? new SampleConvertor(ConvertorShortToInt2Channels) : new SampleConvertor(ConvertorShortToIntGeneric));
				break;
			case 32:
				result = (flag ? new SampleConvertor(ConvertorFloatToInt2Channels) : new SampleConvertor(ConvertorFloatToIntGeneric));
				break;
			}
			break;
		case AsioSampleType.Int16LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				result = (flag ? new SampleConvertor(ConvertorShortToShort2Channels) : new SampleConvertor(ConvertorShortToShortGeneric));
				break;
			case 32:
				result = (flag ? new SampleConvertor(ConvertorFloatToShort2Channels) : new SampleConvertor(ConvertorFloatToShortGeneric));
				break;
			}
			break;
		case AsioSampleType.Int24LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				throw new ArgumentException("Not a supported conversion");
			case 32:
				result = ConverterFloatTo24LSBGeneric;
				break;
			}
			break;
		case AsioSampleType.Float32LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				throw new ArgumentException("Not a supported conversion");
			case 32:
				result = ConverterFloatToFloatGeneric;
				break;
			}
			break;
		default:
			throw new ArgumentException($"ASIO Buffer Type {Enum.GetName(typeof(AsioSampleType), asioType)} is not yet supported.");
		}
		return result;
	}

	public unsafe static void ConvertorShortToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		short* ptr = (short*)(void*)inputInterleavedBuffer;
		short* ptr2 = (short*)(void*)asioOutputBuffers[0];
		short* ptr3 = (short*)(void*)asioOutputBuffers[1];
		ptr2++;
		ptr3++;
		for (int i = 0; i < nbSamples; i++)
		{
			*ptr2 = *ptr;
			*ptr3 = ptr[1];
			ptr += 2;
			ptr2 += 2;
			ptr3 += 2;
		}
	}

	public unsafe static void ConvertorShortToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		short* ptr = (short*)(void*)inputInterleavedBuffer;
		short*[] array = new short*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (short*)(void*)asioOutputBuffers[i];
			array[i]++;
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*array[k] = *(ptr++);
				array[k] += 2;
			}
		}
	}

	public unsafe static void ConvertorFloatToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		int* ptr2 = (int*)(void*)asioOutputBuffers[0];
		int* ptr3 = (int*)(void*)asioOutputBuffers[1];
		for (int i = 0; i < nbSamples; i++)
		{
			*(ptr2++) = clampToInt(*ptr);
			*(ptr3++) = clampToInt(ptr[1]);
			ptr += 2;
		}
	}

	public unsafe static void ConvertorFloatToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		int*[] array = new int*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (int*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*(array[k]++) = clampToInt(*(ptr++));
			}
		}
	}

	public unsafe static void ConvertorShortToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		short* ptr = (short*)(void*)inputInterleavedBuffer;
		short* ptr2 = (short*)(void*)asioOutputBuffers[0];
		short* ptr3 = (short*)(void*)asioOutputBuffers[1];
		for (int i = 0; i < nbSamples; i++)
		{
			*(ptr2++) = *ptr;
			*(ptr3++) = ptr[1];
			ptr += 2;
		}
	}

	public unsafe static void ConvertorShortToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		short* ptr = (short*)(void*)inputInterleavedBuffer;
		short*[] array = new short*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (short*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*(array[k]++) = *(ptr++);
			}
		}
	}

	public unsafe static void ConvertorFloatToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		short* ptr2 = (short*)(void*)asioOutputBuffers[0];
		short* ptr3 = (short*)(void*)asioOutputBuffers[1];
		for (int i = 0; i < nbSamples; i++)
		{
			*(ptr2++) = clampToShort(*ptr);
			*(ptr3++) = clampToShort(ptr[1]);
			ptr += 2;
		}
	}

	public unsafe static void ConvertorFloatToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		short*[] array = new short*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (short*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*(array[k]++) = clampToShort(*(ptr++));
			}
		}
	}

	public unsafe static void ConverterFloatTo24LSBGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		byte*[] array = new byte*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (byte*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				int num = clampTo24Bit(*(ptr++));
				*(array[k]++) = (byte)num;
				*(array[k]++) = (byte)(num >> 8);
				*(array[k]++) = (byte)(num >> 16);
			}
		}
	}

	public unsafe static void ConverterFloatToFloatGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		float* ptr = (float*)(void*)inputInterleavedBuffer;
		float*[] array = new float*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (float*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*(array[k]++) = *(ptr++);
			}
		}
	}

	private static int clampTo24Bit(double sampleValue)
	{
		sampleValue = ((sampleValue < -1.0) ? (-1.0) : ((sampleValue > 1.0) ? 1.0 : sampleValue));
		return (int)(sampleValue * 8388607.0);
	}

	private static int clampToInt(double sampleValue)
	{
		sampleValue = ((sampleValue < -1.0) ? (-1.0) : ((sampleValue > 1.0) ? 1.0 : sampleValue));
		return (int)(sampleValue * 2147483647.0);
	}

	private static short clampToShort(double sampleValue)
	{
		sampleValue = ((sampleValue < -1.0) ? (-1.0) : ((sampleValue > 1.0) ? 1.0 : sampleValue));
		return (short)(sampleValue * 32767.0);
	}
}
