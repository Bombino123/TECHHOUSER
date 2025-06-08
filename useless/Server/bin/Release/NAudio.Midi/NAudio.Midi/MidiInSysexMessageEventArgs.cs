using System;

namespace NAudio.Midi;

public class MidiInSysexMessageEventArgs : EventArgs
{
	public byte[] SysexBytes { get; private set; }

	public int Timestamp { get; private set; }

	public MidiInSysexMessageEventArgs(byte[] sysexBytes, int timestamp)
	{
		SysexBytes = sysexBytes;
		Timestamp = timestamp;
	}
}
