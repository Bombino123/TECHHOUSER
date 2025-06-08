using System;
using System.Diagnostics;
using System.IO;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			if ((string)array[0] == "Save")
			{
				string text = Path.GetTempFileName() + ".txt";
				File.WriteAllText(text, (string)array[1]);
				Process.Start(text);
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Notepad", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
