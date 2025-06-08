using System;
using System.Runtime.InteropServices;

namespace NAudio.Midi;

public class MidiIn : IDisposable
{
	private IntPtr hMidiIn = IntPtr.Zero;

	private bool disposeIsRunning;

	private bool disposed;

	private MidiInterop.MidiInCallback callback;

	private IntPtr[] SysexBufferHeaders = new IntPtr[0];

	public static int NumberOfDevices => MidiInterop.midiInGetNumDevs();

	public event EventHandler<MidiInMessageEventArgs> MessageReceived;

	public event EventHandler<MidiInMessageEventArgs> ErrorReceived;

	public event EventHandler<MidiInSysexMessageEventArgs> SysexMessageReceived;

	public MidiIn(int deviceNo)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		callback = Callback;
		MmException.Try(MidiInterop.midiInOpen(out hMidiIn, (IntPtr)deviceNo, callback, IntPtr.Zero, 196608), "midiInOpen");
	}

	public void Close()
	{
		Dispose();
	}

	public void Dispose()
	{
		GC.KeepAlive(callback);
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MmException.Try(MidiInterop.midiInStart(hMidiIn), "midiInStart");
	}

	public void Stop()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MmException.Try(MidiInterop.midiInStop(hMidiIn), "midiInStop");
	}

	public void Reset()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MmException.Try(MidiInterop.midiInReset(hMidiIn), "midiInReset");
	}

	public void CreateSysexBuffers(int bufferSize, int numberOfBuffers)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		SysexBufferHeaders = new IntPtr[numberOfBuffers];
		int cb = Marshal.SizeOf(typeof(MidiInterop.MIDIHDR));
		for (int i = 0; i < numberOfBuffers; i++)
		{
			MidiInterop.MIDIHDR structure = default(MidiInterop.MIDIHDR);
			structure.dwBufferLength = bufferSize;
			structure.dwBytesRecorded = 0;
			structure.lpData = Marshal.AllocHGlobal(bufferSize);
			structure.dwFlags = 0;
			IntPtr intPtr = Marshal.AllocHGlobal(cb);
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
			MmException.Try(MidiInterop.midiInPrepareHeader(hMidiIn, intPtr, Marshal.SizeOf(typeof(MidiInterop.MIDIHDR))), "midiInPrepareHeader");
			MmException.Try(MidiInterop.midiInAddBuffer(hMidiIn, intPtr, Marshal.SizeOf(typeof(MidiInterop.MIDIHDR))), "midiInAddBuffer");
			SysexBufferHeaders[i] = intPtr;
		}
	}

	private void Callback(IntPtr midiInHandle, MidiInterop.MidiInMessage message, IntPtr userData, IntPtr messageParameter1, IntPtr messageParameter2)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		switch (message)
		{
		case MidiInterop.MidiInMessage.Data:
			if (this.MessageReceived != null)
			{
				this.MessageReceived(this, new MidiInMessageEventArgs(messageParameter1.ToInt32(), messageParameter2.ToInt32()));
			}
			break;
		case MidiInterop.MidiInMessage.Error:
			if (this.ErrorReceived != null)
			{
				this.ErrorReceived(this, new MidiInMessageEventArgs(messageParameter1.ToInt32(), messageParameter2.ToInt32()));
			}
			break;
		case MidiInterop.MidiInMessage.LongData:
			if (this.SysexMessageReceived != null)
			{
				MidiInterop.MIDIHDR mIDIHDR = (MidiInterop.MIDIHDR)Marshal.PtrToStructure(messageParameter1, typeof(MidiInterop.MIDIHDR));
				byte[] array = new byte[mIDIHDR.dwBytesRecorded];
				Marshal.Copy(mIDIHDR.lpData, array, 0, mIDIHDR.dwBytesRecorded);
				if (array.Length != 0)
				{
					this.SysexMessageReceived(this, new MidiInSysexMessageEventArgs(array, messageParameter2.ToInt32()));
				}
				if (!disposeIsRunning)
				{
					MidiInterop.midiInAddBuffer(hMidiIn, messageParameter1, Marshal.SizeOf(typeof(MidiInterop.MIDIHDR)));
				}
			}
			break;
		case MidiInterop.MidiInMessage.Open:
		case MidiInterop.MidiInMessage.Close:
		case MidiInterop.MidiInMessage.LongError:
		case (MidiInterop.MidiInMessage)967:
		case (MidiInterop.MidiInMessage)968:
		case (MidiInterop.MidiInMessage)969:
		case (MidiInterop.MidiInMessage)970:
		case (MidiInterop.MidiInMessage)971:
		case MidiInterop.MidiInMessage.MoreData:
			break;
		}
	}

	public static MidiInCapabilities DeviceInfo(int midiInDeviceNumber)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		MidiInCapabilities capabilities = default(MidiInCapabilities);
		int size = Marshal.SizeOf(capabilities);
		MmException.Try(MidiInterop.midiInGetDevCaps((IntPtr)midiInDeviceNumber, out capabilities, size), "midiInGetDevCaps");
		return capabilities;
	}

	protected virtual void Dispose(bool disposing)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!disposed)
		{
			disposeIsRunning = true;
			if (SysexBufferHeaders.Length != 0)
			{
				MmException.Try(MidiInterop.midiInReset(hMidiIn), "midiInReset");
				IntPtr[] sysexBufferHeaders = SysexBufferHeaders;
				foreach (IntPtr intPtr in sysexBufferHeaders)
				{
					MidiInterop.MIDIHDR obj = (MidiInterop.MIDIHDR)Marshal.PtrToStructure(intPtr, typeof(MidiInterop.MIDIHDR));
					MmException.Try(MidiInterop.midiInUnprepareHeader(hMidiIn, intPtr, Marshal.SizeOf(typeof(MidiInterop.MIDIHDR))), "midiInPrepareHeader");
					Marshal.FreeHGlobal(obj.lpData);
					Marshal.FreeHGlobal(intPtr);
				}
				SysexBufferHeaders = new IntPtr[0];
			}
			MidiInterop.midiInClose(hMidiIn);
		}
		disposed = true;
		disposeIsRunning = false;
	}

	~MidiIn()
	{
		Dispose(disposing: false);
	}
}
