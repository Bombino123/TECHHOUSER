using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression;

public class AcmStream : IDisposable
{
	private IntPtr streamHandle;

	private IntPtr driverHandle;

	private AcmStreamHeader streamHeader;

	private readonly WaveFormat sourceFormat;

	public byte[] SourceBuffer => streamHeader.SourceBuffer;

	public byte[] DestBuffer => streamHeader.DestBuffer;

	public AcmStream(WaveFormat sourceFormat, WaveFormat destFormat)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			streamHandle = IntPtr.Zero;
			this.sourceFormat = sourceFormat;
			int num = Math.Max(65536, sourceFormat.AverageBytesPerSecond);
			num -= num % sourceFormat.BlockAlign;
			IntPtr intPtr = WaveFormat.MarshalToPtr(sourceFormat);
			IntPtr intPtr2 = WaveFormat.MarshalToPtr(destFormat);
			try
			{
				MmException.Try(AcmInterop.acmStreamOpen2(out streamHandle, IntPtr.Zero, intPtr, intPtr2, null, IntPtr.Zero, IntPtr.Zero, AcmStreamOpenFlags.NonRealTime), "acmStreamOpen");
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
				Marshal.FreeHGlobal(intPtr2);
			}
			int destBufferLength = SourceToDest(num);
			streamHeader = new AcmStreamHeader(streamHandle, num, destBufferLength);
			driverHandle = IntPtr.Zero;
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public AcmStream(IntPtr driverId, WaveFormat sourceFormat, WaveFilter waveFilter)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Max(16384, sourceFormat.AverageBytesPerSecond);
		this.sourceFormat = sourceFormat;
		num -= num % sourceFormat.BlockAlign;
		MmException.Try(AcmInterop.acmDriverOpen(out driverHandle, driverId, 0), "acmDriverOpen");
		IntPtr intPtr = WaveFormat.MarshalToPtr(sourceFormat);
		try
		{
			MmException.Try(AcmInterop.acmStreamOpen2(out streamHandle, driverHandle, intPtr, intPtr, waveFilter, IntPtr.Zero, IntPtr.Zero, AcmStreamOpenFlags.NonRealTime), "acmStreamOpen");
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
		streamHeader = new AcmStreamHeader(streamHandle, num, SourceToDest(num));
	}

	public int SourceToDest(int source)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (source == 0)
		{
			return 0;
		}
		MmException.Try(AcmInterop.acmStreamSize(streamHandle, source, out var outputBufferSize, AcmStreamSizeFlags.Source), "acmStreamSize");
		return outputBufferSize;
	}

	public int DestToSource(int dest)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (dest == 0)
		{
			return 0;
		}
		MmException.Try(AcmInterop.acmStreamSize(streamHandle, dest, out var outputBufferSize, AcmStreamSizeFlags.Destination), "acmStreamSize");
		return outputBufferSize;
	}

	public static WaveFormat SuggestPcmFormat(WaveFormat compressedFormat)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		WaveFormat val = new WaveFormat(compressedFormat.SampleRate, 16, compressedFormat.Channels);
		IntPtr intPtr = WaveFormat.MarshalToPtr(val);
		IntPtr intPtr2 = WaveFormat.MarshalToPtr(compressedFormat);
		try
		{
			MmResult val2 = AcmInterop.acmFormatSuggest2(IntPtr.Zero, intPtr2, intPtr, Marshal.SizeOf<WaveFormat>(val), AcmFormatSuggestFlags.FormatTag);
			val = WaveFormat.MarshalFromPtr(intPtr);
			MmException.Try(val2, "acmFormatSuggest");
			return val;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
			Marshal.FreeHGlobal(intPtr2);
		}
	}

	public void Reposition()
	{
		streamHeader.Reposition();
	}

	public int Convert(int bytesToConvert, out int sourceBytesConverted)
	{
		if (bytesToConvert % sourceFormat.BlockAlign != 0)
		{
			bytesToConvert -= bytesToConvert % sourceFormat.BlockAlign;
		}
		return streamHeader.Convert(bytesToConvert, out sourceBytesConverted);
	}

	[Obsolete("Call the version returning sourceBytesConverted instead")]
	public int Convert(int bytesToConvert)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		int sourceBytesConverted;
		int result = Convert(bytesToConvert, out sourceBytesConverted);
		if (sourceBytesConverted != bytesToConvert)
		{
			throw new MmException((MmResult)8, "AcmStreamHeader.Convert didn't convert everything");
		}
		return result;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (disposing && streamHeader != null)
		{
			streamHeader.Dispose();
			streamHeader = null;
		}
		if (streamHandle != IntPtr.Zero)
		{
			MmResult val = AcmInterop.acmStreamClose(streamHandle, 0);
			streamHandle = IntPtr.Zero;
			if ((int)val != 0)
			{
				throw new MmException(val, "acmStreamClose");
			}
		}
		if (driverHandle != IntPtr.Zero)
		{
			AcmInterop.acmDriverClose(driverHandle, 0);
			driverHandle = IntPtr.Zero;
		}
	}

	~AcmStream()
	{
		Dispose(disposing: false);
	}
}
