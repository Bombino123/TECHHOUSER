using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

[Description("Splitter 分隔面板")]
[ToolboxItem(true)]
public class Splitter : SplitContainer
{
	private bool moving;

	[Description("滑块大小")]
	[Category("行为")]
	[DefaultValue(20)]
	public int SplitterSize { get; set; } = 20;


	public Splitter()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((Control)this).SetStyle((ControlStyles)139282, true);
		((SplitContainer)this).SplitterMoving += new SplitterCancelEventHandler(Splitter_SplitterMoving);
		((SplitContainer)this).SplitterMoved += new SplitterEventHandler(Splitter_SplitterMoved);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!((SplitContainer)this).Panel1Collapsed && !((SplitContainer)this).Panel2Collapsed)
		{
			Canvas canvas = e.Graphics.High();
			Rectangle splitterRectangle = ((SplitContainer)this).SplitterRectangle;
			if (moving)
			{
				canvas.Fill(Style.Db.PrimaryBg, splitterRectangle);
			}
			else
			{
				canvas.Fill(Style.Db.FillTertiary, splitterRectangle);
			}
			int num = (int)((float)SplitterSize * Config.Dpi);
			if ((int)((SplitContainer)this).Orientation == 0)
			{
				canvas.Fill(Style.Db.Fill, new Rectangle(splitterRectangle.X + (splitterRectangle.Width - num) / 2, splitterRectangle.Y, num, splitterRectangle.Height));
			}
			else
			{
				canvas.Fill(Style.Db.Fill, new Rectangle(splitterRectangle.X, splitterRectangle.Y + (splitterRectangle.Height - num) / 2, splitterRectangle.Width, num));
			}
		}
	}

	private void Splitter_SplitterMoving(object? sender, SplitterCancelEventArgs e)
	{
		if (!((CancelEventArgs)(object)e).Cancel)
		{
			moving = true;
			((Control)this).Invalidate();
		}
	}

	private void Splitter_SplitterMoved(object? sender, SplitterEventArgs e)
	{
		moving = false;
		((Control)this).Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		((SplitContainer)this).SplitterMoving -= new SplitterCancelEventHandler(Splitter_SplitterMoving);
		((SplitContainer)this).SplitterMoved -= new SplitterEventHandler(Splitter_SplitterMoved);
		((ContainerControl)this).Dispose(disposing);
	}
}
