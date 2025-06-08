using System;

namespace NAudio.Wave.Asio;

internal class AsioSampleConvertor
{
	public delegate void SampleConvertor(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples);

	public static SampleConvertor SelectSampleConvertor(WaveFormat waveFormat, AsioSampleType asioType)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Invalid comparison between Unknown and I4
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Invalid comparison between Unknown and I4
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
				result = (((int)waveFormat.Encoding != 3) ? (flag ? new SampleConvertor(ConvertorIntToInt2Channels) : new SampleConvertor(ConvertorIntToIntGeneric)) : (flag ? new SampleConvertor(ConvertorFloatToInt2Channels) : new SampleConvertor(ConvertorFloatToIntGeneric)));
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
				result = (((int)waveFormat.Encoding != 3) ? (flag ? new SampleConvertor(ConvertorIntToShort2Channels) : new SampleConvertor(ConvertorIntToShortGeneric)) : (flag ? new SampleConvertor(ConvertorFloatToShort2Channels) : new SampleConvertor(ConvertorFloatToShortGeneric)));
				break;
			}
			break;
		case AsioSampleType.Int24LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				throw new ArgumentException("Not a supported conversion");
			case 32:
				if ((int)waveFormat.Encoding == 3)
				{
					result = ConverterFloatTo24LSBGeneric;
					break;
				}
				throw new ArgumentException("Not a supported conversion");
			}
			break;
		case AsioSampleType.Float32LSB:
			switch (waveFormat.BitsPerSample)
			{
			case 16:
				throw new ArgumentException("Not a supported conversion");
			case 32:
				result = (((int)waveFormat.Encoding != 3) ? new SampleConvertor(ConvertorIntToFloatGeneric) : new SampleConvertor(ConverterFloatToFloatGeneric));
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
			int num = i;
			short* ptr2 = array[num];
			array[num] = ptr2 + 1;
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				*array[k] = *(ptr++);
				short*[] array2 = array;
				int num = k;
				array2[num] += 2;
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
				int num = k;
				int* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = clampToInt(*(ptr++));
			}
		}
	}

	public unsafe static void ConvertorIntToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		int* ptr = (int*)(void*)inputInterleavedBuffer;
		int* ptr2 = (int*)(void*)asioOutputBuffers[0];
		int* ptr3 = (int*)(void*)asioOutputBuffers[1];
		for (int i = 0; i < nbSamples; i++)
		{
			*(ptr2++) = *ptr;
			*(ptr3++) = ptr[1];
			ptr += 2;
		}
	}

	public unsafe static void ConvertorIntToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		int* ptr = (int*)(void*)inputInterleavedBuffer;
		int*[] array = new int*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (int*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				int num = k;
				int* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = *(ptr++);
			}
		}
	}

	public unsafe static void ConvertorIntToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		int* ptr = (int*)(void*)inputInterleavedBuffer;
		short* ptr2 = (short*)(void*)asioOutputBuffers[0];
		short* ptr3 = (short*)(void*)asioOutputBuffers[1];
		for (int i = 0; i < nbSamples; i++)
		{
			*(ptr2++) = (short)(*ptr / 65536);
			*(ptr3++) = (short)(ptr[1] / 65536);
			ptr += 2;
		}
	}

	public unsafe static void ConvertorIntToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		int* ptr = (int*)(void*)inputInterleavedBuffer;
		int*[] array = new int*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (int*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				int num = k;
				int* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = (short)(*(ptr++) / 65536);
			}
		}
	}

	public unsafe static void ConvertorIntToFloatGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
	{
		int* ptr = (int*)(void*)inputInterleavedBuffer;
		float*[] array = new float*[nbChannels];
		for (int i = 0; i < nbChannels; i++)
		{
			array[i] = (float*)(void*)asioOutputBuffers[i];
		}
		for (int j = 0; j < nbSamples; j++)
		{
			for (int k = 0; k < nbChannels; k++)
			{
				int num = k;
				float* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = *(ptr++) / int.MinValue;
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
				int num = k;
				short* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = *(ptr++);
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
				int num = k;
				short* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = clampToShort(*(ptr++));
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
				int num2 = k;
				*(array[num2]++) = (byte)num;
				num2 = k;
				*(array[num2]++) = (byte)(num >> 8);
				num2 = k;
				*(array[num2]++) = (byte)(num >> 16);
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
				int num = k;
				float* ptr2 = array[num];
				array[num] = ptr2 + 1;
				*ptr2 = *(ptr++);
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
