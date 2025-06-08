using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControls.RJControls;

public class RJDropdownMenu : ContextMenuStrip
{
	private bool isMainMenu;

	private int menuItemHeight = 25;

	private Color menuItemTextColor = Color.Empty;

	private Color primaryColor = Color.Empty;

	private Bitmap menuItemHeaderSize;

	[Browsable(false)]
	public bool IsMainMenu
	{
		get
		{
			return isMainMenu;
		}
		set
		{
			isMainMenu = value;
		}
	}

	[Browsable(false)]
	public int MenuItemHeight
	{
		get
		{
			return menuItemHeight;
		}
		set
		{
			menuItemHeight = value;
		}
	}

	[Browsable(false)]
	public Color MenuItemTextColor
	{
		get
		{
			return menuItemTextColor;
		}
		set
		{
			menuItemTextColor = value;
		}
	}

	[Browsable(false)]
	public Color PrimaryColor
	{
		get
		{
			return primaryColor;
		}
		set
		{
			primaryColor = value;
		}
	}

	public RJDropdownMenu(IContainer container)
		: base(container)
	{
	}

	private void LoadMenuItemHeight()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		if (isMainMenu)
		{
			menuItemHeaderSize = new Bitmap(25, 45);
		}
		else
		{
			menuItemHeaderSize = new Bitmap(20, menuItemHeight);
		}
		foreach (ToolStripMenuItem item in (ArrangedElementCollection)((ToolStrip)this).Items)
		{
			ToolStripMenuItem val = item;
			((ToolStripItem)val).ImageScaling = (ToolStripItemImageScaling)0;
			if (((ToolStripItem)val).Image == null)
			{
				((ToolStripItem)val).Image = (Image)(object)menuItemHeaderSize;
			}
			foreach (ToolStripMenuItem item2 in (ArrangedElementCollection)((ToolStripDropDownItem)val).DropDownItems)
			{
				ToolStripMenuItem val2 = item2;
				((ToolStripItem)val2).ImageScaling = (ToolStripItemImageScaling)0;
				if (((ToolStripItem)val2).Image == null)
				{
					((ToolStripItem)val2).Image = (Image)(object)menuItemHeaderSize;
				}
				foreach (ToolStripMenuItem item3 in (ArrangedElementCollection)((ToolStripDropDownItem)val2).DropDownItems)
				{
					ToolStripMenuItem val3 = item3;
					((ToolStripItem)val3).ImageScaling = (ToolStripItemImageScaling)0;
					if (((ToolStripItem)val3).Image == null)
					{
						((ToolStripItem)val3).Image = (Image)(object)menuItemHeaderSize;
					}
					foreach (ToolStripMenuItem item4 in (ArrangedElementCollection)((ToolStripDropDownItem)val3).DropDownItems)
					{
						ToolStripMenuItem val4 = item4;
						((ToolStripItem)val4).ImageScaling = (ToolStripItemImageScaling)0;
						if (((ToolStripItem)val4).Image == null)
						{
							((ToolStripItem)val4).Image = (Image)(object)menuItemHeaderSize;
						}
					}
				}
			}
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((ToolStripDropDown)this).OnHandleCreated(e);
		if (!((Component)this).DesignMode)
		{
			((ToolStrip)this).Renderer = (ToolStripRenderer)(object)new MenuRenderer(isMainMenu, primaryColor, menuItemTextColor);
			LoadMenuItemHeight();
		}
	}
}
