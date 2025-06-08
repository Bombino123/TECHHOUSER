using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void GetBot()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		List<object> list = new List<object> { "BotSpeaker", "List" };
		foreach (InstalledVoice installedVoice in new SpeechSynthesizer().GetInstalledVoices())
		{
			string[] obj = new string[7]
			{
				installedVoice.VoiceInfo.Name,
				" | ",
				installedVoice.VoiceInfo.Culture?.ToString(),
				" ",
				null,
				null,
				null
			};
			VoiceGender gender = installedVoice.VoiceInfo.Gender;
			obj[4] = ((object)(VoiceGender)(ref gender)).ToString();
			obj[5] = " Age: ";
			VoiceAge age = installedVoice.VoiceInfo.Age;
			obj[6] = ((object)(VoiceAge)(ref age)).ToString();
			list.Add(string.Concat(obj));
		}
		Client.Send(LEB128.Write(list.ToArray()));
	}

	public static void Speak(int Rate, int Volume, string bot, string text)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (Client.itsConnect)
		{
			SpeechSynthesizer val = new SpeechSynthesizer
			{
				Rate = Convert.ToInt32(Rate),
				Volume = Convert.ToInt32(Volume)
			};
			val.SelectVoice(bot);
			val.Speak(text);
		}
	}

	public static void Read(byte[] data)
	{
		try
		{
			object[] objects = LEB128.Read(data);
			if ((string)objects[0] == "Speak")
			{
				new Thread((ThreadStart)delegate
				{
					Speak((int)objects[1], (int)objects[2], (string)objects[3], (string)objects[4]);
				}).Start();
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
