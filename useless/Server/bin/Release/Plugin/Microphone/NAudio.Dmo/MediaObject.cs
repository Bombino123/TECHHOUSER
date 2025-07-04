using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NAudio.Utils;
using NAudio.Wave;

namespace NAudio.Dmo;

public class MediaObject : IDisposable
{
	private IMediaObject mediaObject;

	private int inputStreams;

	private int outputStreams;

	public int InputStreamCount => inputStreams;

	public int OutputStreamCount => outputStreams;

	internal MediaObject(IMediaObject mediaObject)
	{
		this.mediaObject = mediaObject;
		mediaObject.GetStreamCount(out inputStreams, out outputStreams);
	}

	public DmoMediaType? GetInputType(int inputStream, int inputTypeIndex)
	{
		try
		{
			if (mediaObject.GetInputType(inputStream, inputTypeIndex, out var mediaType) == 0)
			{
				DmoInterop.MoFreeMediaType(ref mediaType);
				return mediaType;
			}
		}
		catch (COMException exception)
		{
			if (exception.GetHResult() != -2147220986)
			{
				throw;
			}
		}
		return null;
	}

	public DmoMediaType? GetOutputType(int outputStream, int outputTypeIndex)
	{
		try
		{
			if (mediaObject.GetOutputType(outputStream, outputTypeIndex, out var mediaType) == 0)
			{
				DmoInterop.MoFreeMediaType(ref mediaType);
				return mediaType;
			}
		}
		catch (COMException exception)
		{
			if (exception.GetHResult() != -2147220986)
			{
				throw;
			}
		}
		return null;
	}

	public DmoMediaType GetOutputCurrentType(int outputStreamIndex)
	{
		DmoMediaType mediaType;
		int outputCurrentType = mediaObject.GetOutputCurrentType(outputStreamIndex, out mediaType);
		switch (outputCurrentType)
		{
		case 0:
			DmoInterop.MoFreeMediaType(ref mediaType);
			return mediaType;
		case -2147220989:
			throw new InvalidOperationException("Media type was not set.");
		default:
			throw Marshal.GetExceptionForHR(outputCurrentType);
		}
	}

	public IEnumerable<DmoMediaType> GetInputTypes(int inputStreamIndex)
	{
		int typeIndex = 0;
		while (true)
		{
			DmoMediaType? inputType;
			DmoMediaType? mediaType = (inputType = GetInputType(inputStreamIndex, typeIndex));
			inputType = inputType;
			if (inputType.HasValue)
			{
				yield return mediaType.Value;
				typeIndex++;
				continue;
			}
			break;
		}
	}

	public IEnumerable<DmoMediaType> GetOutputTypes(int outputStreamIndex)
	{
		int typeIndex = 0;
		while (true)
		{
			DmoMediaType? outputType;
			DmoMediaType? mediaType = (outputType = GetOutputType(outputStreamIndex, typeIndex));
			outputType = outputType;
			if (outputType.HasValue)
			{
				yield return mediaType.Value;
				typeIndex++;
				continue;
			}
			break;
		}
	}

	public bool SupportsInputType(int inputStreamIndex, DmoMediaType mediaType)
	{
		return SetInputType(inputStreamIndex, mediaType, DmoSetTypeFlags.DMO_SET_TYPEF_TEST_ONLY);
	}

	private bool SetInputType(int inputStreamIndex, DmoMediaType mediaType, DmoSetTypeFlags flags)
	{
		switch (mediaObject.SetInputType(inputStreamIndex, ref mediaType, flags))
		{
		case -2147220991:
			throw new ArgumentException("Invalid stream index");
		default:
			_ = -2147220987;
			return false;
		case 0:
			return true;
		}
	}

	public void SetInputType(int inputStreamIndex, DmoMediaType mediaType)
	{
		if (!SetInputType(inputStreamIndex, mediaType, DmoSetTypeFlags.None))
		{
			throw new ArgumentException("Media Type not supported");
		}
	}

	public void SetInputWaveFormat(int inputStreamIndex, WaveFormat waveFormat)
	{
		DmoMediaType mediaType = CreateDmoMediaTypeForWaveFormat(waveFormat);
		bool num = SetInputType(inputStreamIndex, mediaType, DmoSetTypeFlags.None);
		DmoInterop.MoFreeMediaType(ref mediaType);
		if (!num)
		{
			throw new ArgumentException("Media Type not supported");
		}
	}

	public bool SupportsInputWaveFormat(int inputStreamIndex, WaveFormat waveFormat)
	{
		DmoMediaType mediaType = CreateDmoMediaTypeForWaveFormat(waveFormat);
		bool result = SetInputType(inputStreamIndex, mediaType, DmoSetTypeFlags.DMO_SET_TYPEF_TEST_ONLY);
		DmoInterop.MoFreeMediaType(ref mediaType);
		return result;
	}

	private DmoMediaType CreateDmoMediaTypeForWaveFormat(WaveFormat waveFormat)
	{
		DmoMediaType mediaType = default(DmoMediaType);
		int formatBlockBytes = Marshal.SizeOf((object)waveFormat);
		DmoInterop.MoInitMediaType(ref mediaType, formatBlockBytes);
		mediaType.SetWaveFormat(waveFormat);
		return mediaType;
	}

	public bool SupportsOutputType(int outputStreamIndex, DmoMediaType mediaType)
	{
		return SetOutputType(outputStreamIndex, mediaType, DmoSetTypeFlags.DMO_SET_TYPEF_TEST_ONLY);
	}

	public bool SupportsOutputWaveFormat(int outputStreamIndex, WaveFormat waveFormat)
	{
		DmoMediaType mediaType = CreateDmoMediaTypeForWaveFormat(waveFormat);
		bool result = SetOutputType(outputStreamIndex, mediaType, DmoSetTypeFlags.DMO_SET_TYPEF_TEST_ONLY);
		DmoInterop.MoFreeMediaType(ref mediaType);
		return result;
	}

	private bool SetOutputType(int outputStreamIndex, DmoMediaType mediaType, DmoSetTypeFlags flags)
	{
		int num = mediaObject.SetOutputType(outputStreamIndex, ref mediaType, flags);
		return num switch
		{
			-2147220987 => false, 
			0 => true, 
			_ => throw Marshal.GetExceptionForHR(num), 
		};
	}

	public void SetOutputType(int outputStreamIndex, DmoMediaType mediaType)
	{
		if (!SetOutputType(outputStreamIndex, mediaType, DmoSetTypeFlags.None))
		{
			throw new ArgumentException("Media Type not supported");
		}
	}

	public void SetOutputWaveFormat(int outputStreamIndex, WaveFormat waveFormat)
	{
		DmoMediaType mediaType = CreateDmoMediaTypeForWaveFormat(waveFormat);
		bool num = SetOutputType(outputStreamIndex, mediaType, DmoSetTypeFlags.None);
		DmoInterop.MoFreeMediaType(ref mediaType);
		if (!num)
		{
			throw new ArgumentException("Media Type not supported");
		}
	}

	public MediaObjectSizeInfo GetInputSizeInfo(int inputStreamIndex)
	{
		Marshal.ThrowExceptionForHR(mediaObject.GetInputSizeInfo(inputStreamIndex, out var size, out var maxLookahead, out var alignment));
		return new MediaObjectSizeInfo(size, maxLookahead, alignment);
	}

	public MediaObjectSizeInfo GetOutputSizeInfo(int outputStreamIndex)
	{
		Marshal.ThrowExceptionForHR(mediaObject.GetOutputSizeInfo(outputStreamIndex, out var size, out var alignment));
		return new MediaObjectSizeInfo(size, 0, alignment);
	}

	public void ProcessInput(int inputStreamIndex, IMediaBuffer mediaBuffer, DmoInputDataBufferFlags flags, long timestamp, long duration)
	{
		Marshal.ThrowExceptionForHR(mediaObject.ProcessInput(inputStreamIndex, mediaBuffer, flags, timestamp, duration));
	}

	public void ProcessOutput(DmoProcessOutputFlags flags, int outputBufferCount, DmoOutputDataBuffer[] outputBuffers)
	{
		Marshal.ThrowExceptionForHR(mediaObject.ProcessOutput(flags, outputBufferCount, outputBuffers, out var _));
	}

	public void AllocateStreamingResources()
	{
		Marshal.ThrowExceptionForHR(mediaObject.AllocateStreamingResources());
	}

	public void FreeStreamingResources()
	{
		Marshal.ThrowExceptionForHR(mediaObject.FreeStreamingResources());
	}

	public long GetInputMaxLatency(int inputStreamIndex)
	{
		Marshal.ThrowExceptionForHR(mediaObject.GetInputMaxLatency(inputStreamIndex, out var referenceTimeMaxLatency));
		return referenceTimeMaxLatency;
	}

	public void Flush()
	{
		Marshal.ThrowExceptionForHR(mediaObject.Flush());
	}

	public void Discontinuity(int inputStreamIndex)
	{
		Marshal.ThrowExceptionForHR(mediaObject.Discontinuity(inputStreamIndex));
	}

	public bool IsAcceptingData(int inputStreamIndex)
	{
		Marshal.ThrowExceptionForHR(mediaObject.GetInputStatus(inputStreamIndex, out var flags));
		return (flags & DmoInputStatusFlags.DMO_INPUT_STATUSF_ACCEPT_DATA) == DmoInputStatusFlags.DMO_INPUT_STATUSF_ACCEPT_DATA;
	}

	public void Dispose()
	{
		if (mediaObject != null)
		{
			Marshal.ReleaseComObject(mediaObject);
			mediaObject = null;
		}
	}
}
