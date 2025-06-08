using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using NAudio.MediaFoundation;
using NAudio.Utils;

namespace NAudio.Wave;

public class MediaFoundationEncoder : IDisposable
{
	private readonly MediaType outputMediaType;

	private bool disposed;

	public int DefaultReadBufferSize { get; set; }

	public static int[] GetEncodeBitrates(Guid audioSubtype, int sampleRate, int channels)
	{
		return (from br in (from mt in GetOutputMediaTypes(audioSubtype)
				where mt.SampleRate == sampleRate && mt.ChannelCount == channels
				select mt.AverageBytesPerSecond * 8).Distinct()
			orderby br
			select br).ToArray();
	}

	public static MediaType[] GetOutputMediaTypes(Guid audioSubtype)
	{
		MediaFoundationApi.Startup();
		IMFCollection ppAvailableTypes;
		try
		{
			MediaFoundationInterop.MFTranscodeGetAudioOutputAvailableTypes(audioSubtype, _MFT_ENUM_FLAG.MFT_ENUM_FLAG_ALL, null, out ppAvailableTypes);
		}
		catch (COMException ex)
		{
			if (HResult.GetHResult(ex) == -1072875819)
			{
				return new MediaType[0];
			}
			throw;
		}
		ppAvailableTypes.GetElementCount(out var pcElements);
		List<MediaType> list = new List<MediaType>(pcElements);
		for (int i = 0; i < pcElements; i++)
		{
			ppAvailableTypes.GetElement(i, out var ppUnkElement);
			IMFMediaType mediaType = (IMFMediaType)ppUnkElement;
			list.Add(new MediaType(mediaType));
		}
		Marshal.ReleaseComObject(ppAvailableTypes);
		return list.ToArray();
	}

	public static void EncodeToWma(IWaveProvider inputProvider, string outputFile, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_WMAudioV8, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable WMA encoders available"));
		mediaFoundationEncoder.Encode(outputFile, inputProvider);
	}

	public static void EncodeToWma(IWaveProvider inputProvider, Stream outputStream, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_WMAudioV8, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable WMA encoders available"));
		mediaFoundationEncoder.Encode(outputStream, inputProvider, TranscodeContainerTypes.MFTranscodeContainerType_ASF);
	}

	public static void EncodeToMp3(IWaveProvider inputProvider, string outputFile, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable MP3 encoders available"));
		mediaFoundationEncoder.Encode(outputFile, inputProvider);
	}

	public static void EncodeToMp3(IWaveProvider inputProvider, Stream outputStream, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable MP3 encoders available"));
		mediaFoundationEncoder.Encode(outputStream, inputProvider, TranscodeContainerTypes.MFTranscodeContainerType_MP3);
	}

	public static void EncodeToAac(IWaveProvider inputProvider, string outputFile, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_AAC, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable AAC encoders available"));
		mediaFoundationEncoder.Encode(outputFile, inputProvider);
	}

	public static void EncodeToAac(IWaveProvider inputProvider, Stream outputStream, int desiredBitRate = 192000)
	{
		using MediaFoundationEncoder mediaFoundationEncoder = new MediaFoundationEncoder(SelectMediaType(AudioSubtypes.MFAudioFormat_AAC, inputProvider.WaveFormat, desiredBitRate) ?? throw new InvalidOperationException("No suitable AAC encoders available"));
		mediaFoundationEncoder.Encode(outputStream, inputProvider, TranscodeContainerTypes.MFTranscodeContainerType_MPEG4);
	}

	public static MediaType SelectMediaType(Guid audioSubtype, WaveFormat inputFormat, int desiredBitRate)
	{
		MediaFoundationApi.Startup();
		return (from mt in GetOutputMediaTypes(audioSubtype)
			where mt.SampleRate == inputFormat.SampleRate && mt.ChannelCount == inputFormat.Channels
			select new
			{
				MediaType = mt,
				Delta = Math.Abs(desiredBitRate - mt.AverageBytesPerSecond * 8)
			} into mt
			orderby mt.Delta
			select mt.MediaType).FirstOrDefault();
	}

	public MediaFoundationEncoder(MediaType outputMediaType)
	{
		if (outputMediaType == null)
		{
			throw new ArgumentNullException("outputMediaType");
		}
		this.outputMediaType = outputMediaType;
	}

	public void Encode(string outputFile, IWaveProvider inputProvider)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)inputProvider.WaveFormat.Encoding != 1 && (int)inputProvider.WaveFormat.Encoding != 3)
		{
			throw new ArgumentException("Encode input format must be PCM or IEEE float");
		}
		MediaType mediaType = new MediaType(inputProvider.WaveFormat);
		IMFSinkWriter iMFSinkWriter = CreateSinkWriter(outputFile);
		try
		{
			iMFSinkWriter.AddStream(outputMediaType.MediaFoundationObject, out var pdwStreamIndex);
			iMFSinkWriter.SetInputMediaType(pdwStreamIndex, mediaType.MediaFoundationObject, null);
			PerformEncode(iMFSinkWriter, pdwStreamIndex, inputProvider);
		}
		finally
		{
			if (iMFSinkWriter != null)
			{
				Marshal.ReleaseComObject(iMFSinkWriter);
			}
			if (mediaType.MediaFoundationObject != null)
			{
				Marshal.ReleaseComObject(mediaType.MediaFoundationObject);
			}
		}
	}

	public void Encode(Stream outputStream, IWaveProvider inputProvider, Guid transcodeContainerType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)inputProvider.WaveFormat.Encoding != 1 && (int)inputProvider.WaveFormat.Encoding != 3)
		{
			throw new ArgumentException("Encode input format must be PCM or IEEE float");
		}
		MediaType mediaType = new MediaType(inputProvider.WaveFormat);
		IMFSinkWriter iMFSinkWriter = CreateSinkWriter(new ComStream(outputStream), transcodeContainerType);
		try
		{
			iMFSinkWriter.AddStream(outputMediaType.MediaFoundationObject, out var pdwStreamIndex);
			iMFSinkWriter.SetInputMediaType(pdwStreamIndex, mediaType.MediaFoundationObject, null);
			PerformEncode(iMFSinkWriter, pdwStreamIndex, inputProvider);
		}
		finally
		{
			if (iMFSinkWriter != null)
			{
				Marshal.ReleaseComObject(iMFSinkWriter);
			}
			if (mediaType.MediaFoundationObject != null)
			{
				Marshal.ReleaseComObject(mediaType.MediaFoundationObject);
			}
		}
	}

	private static IMFSinkWriter CreateSinkWriter(string outputFile)
	{
		IMFAttributes iMFAttributes = MediaFoundationApi.CreateAttributes(1);
		iMFAttributes.SetUINT32(MediaFoundationAttributes.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, 1);
		try
		{
			MediaFoundationInterop.MFCreateSinkWriterFromURL(outputFile, null, iMFAttributes, out var ppSinkWriter);
			return ppSinkWriter;
		}
		catch (COMException ex)
		{
			if (HResult.GetHResult(ex) == -1072875819)
			{
				throw new ArgumentException("Was not able to create a sink writer for this file extension");
			}
			throw;
		}
		finally
		{
			Marshal.ReleaseComObject(iMFAttributes);
		}
	}

	private static IMFSinkWriter CreateSinkWriter(IStream outputStream, Guid TranscodeContainerType)
	{
		IMFAttributes iMFAttributes = MediaFoundationApi.CreateAttributes(1);
		iMFAttributes.SetGUID(MediaFoundationAttributes.MF_TRANSCODE_CONTAINERTYPE, TranscodeContainerType);
		try
		{
			MediaFoundationInterop.MFCreateMFByteStreamOnStream(outputStream, out var ppByteStream);
			MediaFoundationInterop.MFCreateSinkWriterFromURL(null, ppByteStream, iMFAttributes, out var ppSinkWriter);
			return ppSinkWriter;
		}
		finally
		{
			Marshal.ReleaseComObject(iMFAttributes);
		}
	}

	private void PerformEncode(IMFSinkWriter writer, int streamIndex, IWaveProvider inputProvider)
	{
		if (DefaultReadBufferSize == 0)
		{
			DefaultReadBufferSize = inputProvider.WaveFormat.AverageBytesPerSecond * 4;
		}
		byte[] managedBuffer = new byte[DefaultReadBufferSize];
		writer.BeginWriting();
		long num = 0L;
		long num2;
		do
		{
			num2 = ConvertOneBuffer(writer, streamIndex, inputProvider, num, managedBuffer);
			num += num2;
		}
		while (num2 > 0);
		writer.DoFinalize();
	}

	private static long BytesToNsPosition(int bytes, WaveFormat waveFormat)
	{
		return 10000000L * (long)bytes / waveFormat.AverageBytesPerSecond;
	}

	private long ConvertOneBuffer(IMFSinkWriter writer, int streamIndex, IWaveProvider inputProvider, long position, byte[] managedBuffer)
	{
		long num = 0L;
		IMFMediaBuffer iMFMediaBuffer = MediaFoundationApi.CreateMemoryBuffer(managedBuffer.Length);
		iMFMediaBuffer.GetMaxLength(out var pcbMaxLength);
		IMFSample iMFSample = MediaFoundationApi.CreateSample();
		iMFSample.AddBuffer(iMFMediaBuffer);
		int num2 = inputProvider.Read(managedBuffer, 0, pcbMaxLength);
		if (num2 > 0)
		{
			iMFMediaBuffer.Lock(out var ppbBuffer, out pcbMaxLength, out var _);
			num = BytesToNsPosition(num2, inputProvider.WaveFormat);
			Marshal.Copy(managedBuffer, 0, ppbBuffer, num2);
			iMFMediaBuffer.SetCurrentLength(num2);
			iMFMediaBuffer.Unlock();
			iMFSample.SetSampleTime(position);
			iMFSample.SetSampleDuration(num);
			writer.WriteSample(streamIndex, iMFSample);
		}
		Marshal.ReleaseComObject(iMFSample);
		Marshal.ReleaseComObject(iMFMediaBuffer);
		return num;
	}

	protected void Dispose(bool disposing)
	{
		Marshal.ReleaseComObject(outputMediaType.MediaFoundationObject);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			Dispose(disposing: true);
		}
		GC.SuppressFinalize(this);
	}

	~MediaFoundationEncoder()
	{
		Dispose(disposing: false);
	}
}
