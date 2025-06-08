using System;
using System.Runtime.InteropServices;

namespace NAudio.Dmo;

public class MediaObjectInPlace : IDisposable
{
	private IMediaObjectInPlace mediaObjectInPlace;

	internal MediaObjectInPlace(IMediaObjectInPlace mediaObjectInPlace)
	{
		this.mediaObjectInPlace = mediaObjectInPlace;
	}

	public DmoInPlaceProcessReturn Process(int size, int offset, byte[] data, long timeStart, DmoInPlaceProcessFlags inPlaceFlag)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(size);
		Marshal.Copy(data, offset, intPtr, size);
		int num = mediaObjectInPlace.Process(size, intPtr, timeStart, inPlaceFlag);
		Marshal.ThrowExceptionForHR(num);
		Marshal.Copy(intPtr, data, offset, size);
		Marshal.FreeHGlobal(intPtr);
		return (DmoInPlaceProcessReturn)num;
	}

	public MediaObjectInPlace Clone()
	{
		Marshal.ThrowExceptionForHR(this.mediaObjectInPlace.Clone(out var mediaObjectInPlace));
		return new MediaObjectInPlace(mediaObjectInPlace);
	}

	public long GetLatency()
	{
		Marshal.ThrowExceptionForHR(mediaObjectInPlace.GetLatency(out var latencyTime));
		return latencyTime;
	}

	public MediaObject GetMediaObject()
	{
		return new MediaObject((IMediaObject)mediaObjectInPlace);
	}

	public void Dispose()
	{
		if (mediaObjectInPlace != null)
		{
			Marshal.ReleaseComObject(mediaObjectInPlace);
			mediaObjectInPlace = null;
		}
	}
}
