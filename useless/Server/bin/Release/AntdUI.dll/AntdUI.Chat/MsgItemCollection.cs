using System.Windows.Forms;

namespace AntdUI.Chat;

public class MsgItemCollection : iCollection<MsgItem>
{
	public MsgItemCollection(MsgList it)
	{
		BindData(it);
	}

	internal MsgItemCollection BindData(MsgList it)
	{
		MsgList it2 = it;
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
