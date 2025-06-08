using System.Windows.Forms;
using AntdUI.Chat;

namespace AntdUI;

public class ChatItemEventArgs : VMEventArgs<IChatItem>
{
	public ChatItemEventArgs(IChatItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
