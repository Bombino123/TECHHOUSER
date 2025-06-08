using System.Windows.Forms;

namespace AntdUI.Chat;

public class ChatItemCollection : iCollection<IChatItem>
{
	public ChatItemCollection(ChatList it)
	{
		BindData(it);
	}

	internal ChatItemCollection BindData(ChatList it)
	{
		ChatList it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.ChangeList();
			}
			((Control)it2).Invalidate();
		};
		return this;
	}
}
