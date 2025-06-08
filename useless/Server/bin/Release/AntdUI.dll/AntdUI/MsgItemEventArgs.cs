using System;
using AntdUI.Chat;

namespace AntdUI;

public class MsgItemEventArgs : EventArgs
{
	public MsgItem Item { get; private set; }

	public MsgItemEventArgs(MsgItem item)
	{
		Item = item;
	}
}
