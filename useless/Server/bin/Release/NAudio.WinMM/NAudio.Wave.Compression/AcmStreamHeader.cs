using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression;

internal class AcmStreamHeader : IDisposable
{
	private AcmStreamHeaderStruct streamHeader;

	private GCHandle hSourceBuffer;

	private GCHandle hDestBuffer;

	private IntPtr streamHandle;

	private bool firstTime;

	private bool disposed;

	public byte[] SourceBuffer { get; private set; }

	public byte[] DestBuffer { get; private set; }

	public AcmStreamHeader(IntPtr streamHandle, int sourceBufferLength, int destBufferLength)
	{
		streamHeader = new AcmStreamHeaderStruct();
		SourceBuffer = new byte[sourceBufferLength];
		hSourceBuffer = GCHandle.Alloc(SourceBuffer, GCHandleType.Pinned);
		DestBuffer = new byte[destBufferLength];
		hDestBuffer = GCHandle.Alloc(DestBuffer, GCHandleType.Pinned);
		this.streamHandle = streamHandle;
		firstTime = true;
	}

	private void Prepare()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		streamHeader.cbStruct = Marshal.SizeOf(streamHeader);
		streamHeader.sourceBufferLength = SourceBuffer.Length;
		streamHeader.sourceBufferPointer = hSourceBuffer.AddrOfPinnedObject();
		streamHeader.destBufferLength = DestBuffer.Length;
		streamHeader.destBufferPointer = hDestBuffer.AddrOfPinnedObject();
		MmException.Try(AcmInterop.acmStreamPrepareHeader(streamHandle, streamHeader, 0), "acmStreamPrepareHeader");
	}

	private void Unprepare()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		streamHeader.sourceBufferLength = SourceBuffer.Length;
		streamHeader.sourceBufferPointer = hSourceBuffer.AddrOfPinnedObject();
		streamHeader.destBufferLength = DestBuffer.Length;
		streamHeader.destBufferPointer = hDestBuffer.AddrOfPinnedObject();
		MmResult val = AcmInterop.acmStreamUnprepareHeader(streamHandle, streamHeader, 0);
		if ((int)val != 0)
		{
			throw new MmException(val, "acmStreamUnprepareHeader");
		}
	}

	public void Reposition()
	{
		firstTime = true;
	}

	public int Convert(int bytesToConvert, out int sourceBytesConverted)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Prepare();
		try
		{
			streamHeader.sourceBufferLength = bytesToConvert;
			streamHeader.sourceBufferLengthUsed = bytesToConvert;
			AcmStreamConvertFlags streamConvertFlags = (firstTime ? (AcmStreamConvertFlags.BlockAlign | AcmStreamConvertFlags.Start) : AcmStreamConvertFlags.BlockAlign);
			MmException.Try(AcmInterop.acmStreamConvert(streamHandle, streamHeader, streamConvertFlags), "acmStreamConvert");
			firstTime = false;
			sourceBytesConverted = streamHeader.sourceBufferLengthUsed;
		}
		finally
		{
			Unprepare();
		}
		return streamHeader.destBufferLengthUsed;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			SourceBuffer = null;
			DestBuffer = null;
			hSourceBuffer.Free();
			hDestBuffer.Free();
		}
		disposed = true;
	}

	~AcmStreamHeader()
	{
		Dispose(disposing: false);
	}
}
