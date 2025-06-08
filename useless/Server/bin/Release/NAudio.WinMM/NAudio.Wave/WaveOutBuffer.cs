using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave;

public class WaveOutBuffer : IDisposable
{
	private readonly WaveHeader header;

	private readonly int bufferSize;

	private readonly byte[] buffer;

	private readonly IWaveProvider waveStream;

	private readonly object waveOutLock;

	private GCHandle hBuffer;

	private IntPtr hWaveOut;

	private GCHandle hHeader;

	private GCHandle hThis;

	public bool InQueue => (header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;

	public int BufferSize => bufferSize;

	public WaveOutBuffer(IntPtr hWaveOut, int bufferSize, IWaveProvider bufferFillStream, object waveOutLock)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		this.bufferSize = bufferSize;
		buffer = new byte[bufferSize];
		hBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		this.hWaveOut = hWaveOut;
		waveStream = bufferFillStream;
		this.waveOutLock = waveOutLock;
		header = new WaveHeader();
		hHeader = GCHandle.Alloc(header, GCHandleType.Pinned);
		header.dataBuffer = hBuffer.AddrOfPinnedObject();
		header.bufferLength = bufferSize;
		header.loops = 1;
		hThis = GCHandle.Alloc(this);
		header.userData = (IntPtr)hThis;
		lock (waveOutLock)
		{
			MmException.Try(WaveInterop.waveOutPrepareHeader(hWaveOut, header, Marshal.SizeOf(header)), "waveOutPrepareHeader");
		}
	}

	~WaveOutBuffer()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	protected void Dispose(bool disposing)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (hHeader.IsAllocated)
		{
			hHeader.Free();
		}
		if (hBuffer.IsAllocated)
		{
			hBuffer.Free();
		}
		if (hThis.IsAllocated)
		{
			hThis.Free();
		}
		if (hWaveOut != IntPtr.Zero)
		{
			lock (waveOutLock)
			{
				WaveInterop.waveOutUnprepareHeader(hWaveOut, header, Marshal.SizeOf(header));
			}
			hWaveOut = IntPtr.Zero;
		}
	}

	public bool OnDone()
	{
		int num;
		lock (waveStream)
		{
			num = waveStream.Read(buffer, 0, buffer.Length);
		}
		if (num == 0)
		{
			return false;
		}
		for (int i = num; i < buffer.Length; i++)
		{
			buffer[i] = 0;
		}
		WriteToWaveOut();
		return true;
	}

	private void WriteToWaveOut()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		MmResult val;
		lock (waveOutLock)
		{
			val = WaveInterop.waveOutWrite(hWaveOut, header, Marshal.SizeOf(header));
		}
		if ((int)val != 0)
		{
			throw new MmException(val, "waveOutWrite");
		}
		GC.KeepAlive(this);
	}
}
