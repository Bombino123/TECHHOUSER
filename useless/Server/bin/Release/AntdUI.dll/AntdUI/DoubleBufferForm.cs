using System.Windows.Forms;

namespace AntdUI;

internal class DoubleBufferForm : Form
{
	public DoubleBufferForm(Form form, Control control)
		: this()
	{
		((Control)this).Tag = form;
		control.Dock = (DockStyle)5;
		((Control)this).Controls.Add(control);
	}

	public DoubleBufferForm()
	{
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		((Form)this).ShowInTaskbar = false;
	}
}
