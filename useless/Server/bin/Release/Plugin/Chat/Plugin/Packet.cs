using System;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		try
		{
			object[] objects = LEB128.Read(data);
			if ((string)objects[0] == "Message")
			{
				((Control)Plugin.chat).Invoke((Delegate)(MethodInvoker)delegate
				{
					RichTextBox richTextBox = Plugin.chat.richTextBox1;
					((Control)richTextBox).Text = ((Control)richTextBox).Text + (string)objects[1];
					((TextBoxBase)Plugin.chat.richTextBox1).SelectionStart = ((Control)Plugin.chat.richTextBox1).Text.Length;
					((TextBoxBase)Plugin.chat.richTextBox1).ScrollToCaret();
				});
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}
}
