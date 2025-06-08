using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AntdUI.Core;

public class CanvasGDI : Canvas, IDisposable
{
	private Graphics g;

	public float DpiX => g.DpiX;

	public float DpiY => g.DpiY;

	public CompositingMode CompositingMode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return g.CompositingMode;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			g.CompositingMode = value;
		}
	}

	public CanvasGDI(Graphics gdi)
	{
		g = gdi;
	}

	public Size MeasureString(string? text, Font font)
	{
		return g.MeasureString(text, font).Size();
	}

	public Size MeasureString(string? text, Font font, int width)
	{
		return g.MeasureString(text, font, width).Size();
	}

	public Size MeasureString(string? text, Font font, int width, StringFormat? format)
	{
		return g.MeasureString(text, font, width, format).Size();
	}

	public void String(string? text, Font font, Color color, Rectangle rect, StringFormat? format = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		CorrectionTextRendering.CORE(font, text, ref rect);
		SolidBrush val = new SolidBrush(color);
		try
		{
			String(text, font, (Brush)(object)val, rect, format);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void String(string? text, Font font, Brush brush, Rectangle rect, StringFormat? format = null)
	{
		CorrectionTextRendering.CORE(font, text, ref rect);
		g.DrawString(text, font, brush, (RectangleF)rect, format);
	}

	public void String(string? text, Font font, Color color, int x, int y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			String(text, font, (Brush)(object)val, x, y);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void String(string? text, Font font, Brush brush, int x, int y)
	{
		g.DrawString(text, font, brush, (float)x, (float)y);
	}

	public void String(string? text, Font font, Color color, Point point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			String(text, font, (Brush)(object)val, point);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void String(string? text, Font font, Brush brush, Point point)
	{
		g.DrawString(text, font, brush, (PointF)point);
	}

	public void String(string? text, Font font, Color color, float x, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			String(text, font, (Brush)(object)val, x, y);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void String(string? text, Font font, Brush brush, float x, float y)
	{
		g.DrawString(text, font, brush, x, y);
	}

	public void Image(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes? imageAttr)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			lock (image)
			{
				g.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr, (DrawImageAbort)null);
			}
		}
		catch
		{
		}
	}

	public void Image(Image image, float x, float y, float w, float h)
	{
		try
		{
			lock (image)
			{
				g.DrawImage(image, x, y, w, h);
			}
		}
		catch
		{
		}
	}

	public void Image(Image image, int srcX, int srcY, int srcWidth, int srcHeight)
	{
		try
		{
			lock (image)
			{
				g.DrawImage(image, srcX, srcY, srcWidth, srcHeight);
			}
		}
		catch
		{
		}
	}

	public void Image(Bitmap image, Rectangle rect)
	{
		try
		{
			lock (image)
			{
				g.DrawImage((Image)(object)image, rect);
			}
		}
		catch
		{
		}
	}

	public void Icon(Icon icon, Rectangle rect)
	{
		try
		{
			lock (icon)
			{
				g.DrawIcon(icon, rect);
			}
		}
		catch
		{
		}
	}

	public void Image(Image image, Rectangle rect)
	{
		try
		{
			lock (image)
			{
				g.DrawImage(image, rect);
			}
		}
		catch
		{
		}
	}

	public void Image(Image image, RectangleF rect)
	{
		try
		{
			lock (image)
			{
				g.DrawImage(image, rect);
			}
		}
		catch
		{
		}
	}

	public void Image(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			lock (image)
			{
				g.DrawImage(image, destRect, srcRect, srcUnit);
			}
		}
		catch
		{
		}
	}

	public void Image(Bitmap bmp, Rectangle rect, float opacity)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		try
		{
			lock (bmp)
			{
				if (opacity >= 1f)
				{
					g.DrawImage((Image)(object)bmp, rect);
					return;
				}
				ImageAttributes val = new ImageAttributes();
				try
				{
					ColorMatrix val2 = new ColorMatrix
					{
						Matrix33 = opacity
					};
					val.SetColorMatrix(val2, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.DrawImage((Image)(object)bmp, rect, 0, 0, ((Image)bmp).Width, ((Image)bmp).Height, (GraphicsUnit)2, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		catch
		{
		}
	}

	public void Image(Image bmp, Rectangle rect, float opacity)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		try
		{
			lock (bmp)
			{
				if (opacity >= 1f)
				{
					g.DrawImage(bmp, rect);
					return;
				}
				ImageAttributes val = new ImageAttributes();
				try
				{
					ColorMatrix val2 = new ColorMatrix
					{
						Matrix33 = opacity
					};
					val.SetColorMatrix(val2, (ColorMatrixFlag)0, (ColorAdjustType)1);
					g.DrawImage(bmp, rect, 0, 0, bmp.Width, bmp.Height, (GraphicsUnit)2, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		catch
		{
		}
	}

	public void Image(RectangleF rect, Image image, TFit fit)
	{
		switch (fit)
		{
		case TFit.Fill:
			g.DrawImage(image, rect);
			break;
		case TFit.None:
			g.DrawImage(image, new RectangleF(rect.X + (rect.Width - (float)image.Width) / 2f, rect.Y + (rect.Height - (float)image.Height) / 2f, image.Width, image.Height));
			break;
		case TFit.Contain:
			PaintImgContain(this, image, rect);
			break;
		case TFit.Cover:
			PaintImgCover(this, image, rect);
			break;
		}
	}

	public void Image(RectangleF rect, Image image, TFit fit, float radius, bool round)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		try
		{
			if (round || radius > 0f)
			{
				Bitmap val = new Bitmap((int)rect.Width, (int)rect.Height);
				try
				{
					using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
					{
						PaintImg(canvas, new RectangleF(0f, 0f, rect.Width, rect.Height), image, fit);
					}
					TextureBrush val2 = new TextureBrush((Image)(object)val, (WrapMode)4);
					try
					{
						val2.TranslateTransform(rect.X, rect.Y);
						if (round)
						{
							g.FillEllipse((Brush)(object)val2, rect);
							return;
						}
						GraphicsPath val3 = rect.RoundPath(radius);
						try
						{
							g.FillPath((Brush)(object)val2, val3);
							return;
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			PaintImg(this, rect, image, fit);
		}
		catch
		{
		}
	}

	public void Image(RectangleF rect, Image image, TFit fit, float radius, TShape shape)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		try
		{
			if (shape == TShape.Circle || shape == TShape.Round || radius > 0f)
			{
				Bitmap val = new Bitmap((int)rect.Width, (int)rect.Height);
				try
				{
					using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
					{
						PaintImg(canvas, new RectangleF(0f, 0f, rect.Width, rect.Height), image, fit);
					}
					TextureBrush val2 = new TextureBrush((Image)(object)val, (WrapMode)4);
					try
					{
						val2.TranslateTransform(rect.X, rect.Y);
						if (shape == TShape.Circle)
						{
							g.FillEllipse((Brush)(object)val2, rect);
							return;
						}
						GraphicsPath val3 = rect.RoundPath(radius);
						try
						{
							g.FillPath((Brush)(object)val2, val3);
							return;
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			PaintImg(this, rect, image, fit);
		}
		catch
		{
		}
	}

	private static void PaintImg(Canvas g, RectangleF rect, Image image, TFit fit)
	{
		switch (fit)
		{
		case TFit.Fill:
			g.Image(image, rect);
			break;
		case TFit.None:
			g.Image(image, new RectangleF(rect.X + (rect.Width - (float)image.Width) / 2f, rect.Y + (rect.Height - (float)image.Height) / 2f, image.Width, image.Height));
			break;
		case TFit.Contain:
			PaintImgContain(g, image, rect);
			break;
		case TFit.Cover:
			PaintImgCover(g, image, rect);
			break;
		}
	}

	private static void PaintImgCover(Canvas g, Image image, RectangleF rect)
	{
		float num = image.Width;
		float num2 = image.Height;
		if (num == num2)
		{
			if (rect.Width == rect.Height)
			{
				g.Image(image, rect);
			}
			else if (rect.Width > rect.Height)
			{
				g.Image(image, new RectangleF(0f, (rect.Height - rect.Width) / 2f, rect.Width, rect.Width));
			}
			else
			{
				g.Image(image, new RectangleF((rect.Width - rect.Height) / 2f, 0f, rect.Height, rect.Height));
			}
			return;
		}
		float width = rect.Width;
		float height = rect.Height;
		float num3;
		float num4;
		if (num * height > num2 * width)
		{
			num3 = height;
			num4 = num * height / num2;
		}
		else
		{
			num4 = width;
			num3 = width * num2 / num;
		}
		g.Image(image, new RectangleF(rect.X + (width - num4) / 2f, rect.Y + (height - num3) / 2f, num4, num3), new RectangleF(0f, 0f, num, num2), (GraphicsUnit)2);
	}

	private static void PaintImgContain(Canvas g, Image image, RectangleF rect)
	{
		float num = image.Width;
		float num2 = image.Height;
		if (num == num2)
		{
			if (rect.Width == rect.Height)
			{
				g.Image(image, rect);
			}
			else if (rect.Width > rect.Height)
			{
				g.Image(image, new RectangleF((rect.Width - rect.Height) / 2f, 0f, rect.Height, rect.Height));
			}
			else
			{
				g.Image(image, new RectangleF(0f, (rect.Height - rect.Width) / 2f, rect.Width, rect.Width));
			}
			return;
		}
		float width = rect.Width;
		float height = rect.Height;
		float num3;
		float num4;
		if (num * height > num2 * width)
		{
			num3 = width;
			num4 = width * num2 / num;
		}
		else
		{
			num4 = height;
			num3 = num * height / num2;
		}
		g.Image(image, new RectangleF(rect.X + (width - num3) / 2f, rect.Y + (height - num4) / 2f, num3, num4), new RectangleF(0f, 0f, num, num2), (GraphicsUnit)2);
	}

	public void Fill(Brush brush, GraphicsPath path)
	{
		try
		{
			g.FillPath(brush, path);
		}
		catch
		{
		}
	}

	public void Fill(Brush brush, Rectangle rect)
	{
		g.FillRectangle(brush, rect);
	}

	public void Fill(Brush brush, int x, int y, int w, int h)
	{
		g.FillRectangle(brush, x, y, w, h);
	}

	public void Fill(Brush brush, RectangleF rect)
	{
		g.FillRectangle(brush, rect);
	}

	public void Fill(Color color, GraphicsPath path)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			Fill((Brush)(object)val, path);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Fill(Color color, Rectangle rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			Fill((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Fill(Color color, RectangleF rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			Fill((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void FillEllipse(Brush brush, Rectangle rect)
	{
		g.FillEllipse(brush, rect);
	}

	public void FillEllipse(Brush brush, RectangleF rect)
	{
		g.FillEllipse(brush, rect);
	}

	public void FillEllipse(Color color, Rectangle rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			FillEllipse((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void FillEllipse(Color color, RectangleF rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			FillEllipse((Brush)(object)val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void FillPolygon(Brush brush, Point[] points)
	{
		g.FillPolygon(brush, points);
	}

	public void FillPolygon(Brush brush, PointF[] points)
	{
		g.FillPolygon(brush, points);
	}

	public void FillPolygon(Color color, PointF[] points)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(color);
		try
		{
			FillPolygon((Brush)(object)val, points);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
	{
		g.FillPie(brush, rect, startAngle, sweepAngle);
	}

	public void FillPie(Brush brush, float x, float y, float w, float h, float startAngle, float sweepAngle)
	{
		g.FillPie(brush, x, y, w, h, startAngle, sweepAngle);
	}

	public void Draw(Pen pen, GraphicsPath path)
	{
		g.DrawPath(pen, path);
	}

	public void Draw(Pen pen, Rectangle rect)
	{
		g.DrawRectangle(pen, rect);
	}

	public void Draw(Brush brush, float width, GraphicsPath path)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Pen val = new Pen(brush, width);
		try
		{
			Draw(val, path);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Draw(Color color, float width, GraphicsPath path)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Pen val = new Pen(color, width);
		try
		{
			Draw(val, path);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Draw(Color color, float width, Rectangle rect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Pen val = new Pen(color, width);
		try
		{
			Draw(val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Draw(Color color, float width, DashStyle dashStyle, GraphicsPath path)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Pen val = new Pen(color, width);
		try
		{
			val.DashStyle = dashStyle;
			Draw(val, path);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawEllipse(Pen pen, Rectangle rect)
	{
		g.DrawEllipse(pen, rect);
	}

	public void DrawEllipse(Pen pen, RectangleF rect)
	{
		g.DrawEllipse(pen, rect);
	}

	public void DrawEllipse(Color color, float width, RectangleF rect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Pen val = new Pen(color, width);
		try
		{
			g.DrawEllipse(val, rect);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawPolygon(Pen pen, Point[] points)
	{
		g.DrawPolygon(pen, points);
	}

	public void DrawPolygon(Pen pen, PointF[] points)
	{
		g.DrawPolygon(pen, points);
	}

	public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
	{
		try
		{
			g.DrawArc(pen, rect, startAngle, sweepAngle);
		}
		catch
		{
		}
	}

	public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
	{
		try
		{
			g.DrawArc(pen, rect, startAngle, sweepAngle);
		}
		catch
		{
		}
	}

	public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
	{
		g.DrawPie(pen, rect, startAngle, sweepAngle);
	}

	public void DrawLine(Pen pen, Point pt1, Point pt2)
	{
		g.DrawLine(pen, pt1, pt2);
	}

	public void DrawLine(Pen pen, PointF pt1, PointF pt2)
	{
		g.DrawLine(pen, pt1, pt2);
	}

	public void DrawLine(Pen pen, float x, float y, float x2, float y2)
	{
		g.DrawLine(pen, x, y, x2, y2);
	}

	public void DrawLines(Pen pen, Point[] points)
	{
		g.DrawLines(pen, points);
	}

	public void DrawLines(Pen pen, PointF[] points)
	{
		g.DrawLines(pen, points);
	}

	public void DrawLines(Color color, float width, PointF[] points)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Pen val = new Pen(color, width);
		try
		{
			DrawLines(val, points);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public GraphicsState Save()
	{
		return g.Save();
	}

	public void Restore(GraphicsState state)
	{
		g.Restore(state);
	}

	public void SetClip(Rectangle rect)
	{
		g.SetClip(rect);
	}

	public void SetClip(RectangleF rect)
	{
		g.SetClip(rect);
	}

	public void SetClip(GraphicsPath path)
	{
		g.SetClip(path);
	}

	public void ResetClip()
	{
		g.ResetClip();
	}

	public void ResetTransform()
	{
		g.ResetTransform();
	}

	public void TranslateTransform(float dx, float dy)
	{
		g.TranslateTransform(dx, dy);
	}

	public void RotateTransform(float angle)
	{
		g.RotateTransform(angle);
	}

	public void Dispose()
	{
		g.Dispose();
	}
}
