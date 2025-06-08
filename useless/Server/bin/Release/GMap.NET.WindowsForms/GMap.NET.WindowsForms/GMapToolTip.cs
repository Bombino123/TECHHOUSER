using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms;

[Serializable]
public class GMapToolTip : ISerializable, IDisposable
{
	private GMapMarker _marker;

	public Point Offset;

	public static readonly StringFormat DefaultFormat;

	[NonSerialized]
	public readonly StringFormat Format = DefaultFormat;

	public static readonly Font DefaultFont;

	public static readonly Font TitleFont;

	[NonSerialized]
	public Font Font = DefaultFont;

	public static readonly Pen DefaultStroke;

	[NonSerialized]
	public Pen Stroke = DefaultStroke;

	public static readonly Brush DefaultFill;

	[NonSerialized]
	public Brush Fill = DefaultFill;

	public static readonly Brush DefaultForeground;

	[NonSerialized]
	public Brush Foreground = DefaultForeground;

	public Size TextPadding = new Size(20, 21);

	private bool _disposed;

	public GMapMarker Marker
	{
		get
		{
			return _marker;
		}
		internal set
		{
			_marker = value;
		}
	}

	static GMapToolTip()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		DefaultFormat = new StringFormat();
		DefaultFont = new Font(FontFamily.GenericSansSerif, 11f, (FontStyle)0, (GraphicsUnit)2);
		TitleFont = new Font(FontFamily.GenericSansSerif, 11f, (FontStyle)1, (GraphicsUnit)2);
		DefaultStroke = new Pen(Color.FromArgb(140, Color.Black));
		DefaultFill = (Brush)new SolidBrush(Color.FromArgb(222, Color.White));
		DefaultForeground = (Brush)new SolidBrush(Color.DimGray);
		DefaultStroke.Width = 1f;
		DefaultStroke.LineJoin = (LineJoin)2;
		DefaultStroke.StartCap = (LineCap)18;
		DefaultFormat.LineAlignment = (StringAlignment)0;
		DefaultFormat.Alignment = (StringAlignment)0;
	}

	public GMapToolTip(GMapMarker marker)
	{
		Marker = marker;
		Offset = new Point(14, -44);
	}

	public virtual void OnRender(Graphics g)
	{
		Size size = g.MeasureString(Marker.ToolTipText, Font).ToSize();
		RectangleF rectangleF = new Rectangle(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y - size.Height, size.Width + TextPadding.Width, size.Height + TextPadding.Height);
		RectangleF rectText = new Rectangle(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y - size.Height, size.Width + TextPadding.Width, size.Height + TextPadding.Height);
		rectangleF.Offset(Offset.X, Offset.Y);
		rectText.Offset(Offset.X + 7, Offset.Y + 7);
		g.DrawLine(Stroke, (float)Marker.ToolTipPosition.X, (float)Marker.ToolTipPosition.Y, rectangleF.X, rectangleF.Y + rectangleF.Height / 2f);
		g.FillRectangle(Fill, rectangleF);
		DrawRoundRectangle(g, Stroke, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height, 8f);
		WriteString(g, Marker.ToolTipText, rectText);
		g.Flush();
	}

	protected GMapToolTip(SerializationInfo info, StreamingContext context)
	{
		Offset = Extensions.GetStruct<Point>(info, "Offset", Point.Empty);
		TextPadding = Extensions.GetStruct<Size>(info, "TextPadding", new Size(10, 10));
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Offset", Offset);
		info.AddValue("TextPadding", TextPadding);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
		}
	}

	public void DrawRoundRectangle(Graphics g, Pen pen, float h, float v, float width, float height, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		try
		{
			val.AddLine(h + radius, v, h + width - radius * 2f, v);
			val.AddArc(h + width - radius * 2f, v, radius * 2f, radius * 2f, 270f, 90f);
			val.AddLine(h + width, v + radius, h + width, v + height - radius * 2f);
			val.AddArc(h + width - radius * 2f, v + height - radius * 2f, radius * 2f, radius * 2f, 0f, 90f);
			val.AddLine(h + width - radius * 2f, v + height, h + radius, v + height);
			val.AddArc(h, v + height - radius * 2f, radius * 2f, radius * 2f, 90f, 90f);
			val.AddLine(h, v + height - radius * 2f, h, v + radius);
			val.AddArc(h, v, radius * 2f, radius * 2f, 180f, 90f);
			val.CloseFigure();
			g.FillPath(Fill, val);
			g.DrawPath(pen, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void WriteString(Graphics g, string text, RectangleF rectText)
	{
		string[] array = text.Split(new char[1] { '\n' });
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != "")
			{
				string[] array2 = array[i].Split(new char[1] { '|' });
				if (array2.Length != 0)
				{
					Size size = g.MeasureString(array2[0], TitleFont).ToSize();
					g.DrawString($"{array2[0]}", TitleFont, Foreground, rectText, Format);
					rectText.X += size.Width + 2;
					if (array2.Length > 1)
					{
						g.DrawString(array2[1], Font, Foreground, rectText, Format);
					}
					rectText.X -= size.Width + 2;
					rectText.Y += size.Height;
				}
			}
			else
			{
				Size size2 = g.MeasureString("\n", TitleFont).ToSize();
				rectText.Y += size2.Height;
			}
		}
	}
}
