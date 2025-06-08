using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AntdUI.Design;

public class ColorEditor : UITypeEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
	{
		return (UITypeEditorEditStyle)3;
	}

	public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
	{
		object? service = provider.GetService(typeof(IWindowsFormsEditorService));
		IWindowsFormsEditorService val = (IWindowsFormsEditorService)((service is IWindowsFormsEditorService) ? service : null);
		if (val != null)
		{
			FrmColorEditor frmColorEditor = new FrmColorEditor(value);
			val.DropDownControl((Control)(object)frmColorEditor);
			return frmColorEditor.Value;
		}
		return null;
	}

	public override bool GetPaintValueSupported(ITypeDescriptorContext? context)
	{
		return true;
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (e.Value is Color color)
		{
			SolidBrush val = new SolidBrush(color);
			try
			{
				e.Graphics.FillRectangle((Brush)(object)val, e.Bounds);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}
