using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AntdUI;

public interface Canvas : IDisposable
{
	float DpiX { get; }

	float DpiY { get; }

	CompositingMode CompositingMode { get; set; }

	Size MeasureString(string? text, Font font);

	Size MeasureString(string? text, Font font, int width);

	Size MeasureString(string? text, Font font, int width, StringFormat? format);

	void String(string? text, Font font, Brush brush, Rectangle rect, StringFormat? format = null);

	void String(string? text, Font font, Color color, Rectangle rect, StringFormat? format = null);

	void String(string? text, Font font, Color color, int x, int y);

	void String(string? text, Font font, Brush brush, int x, int y);

	void String(string? text, Font font, Color color, Point point);

	void String(string? text, Font font, Brush brush, Point point);

	void String(string? text, Font font, Color color, float x, float y);

	void String(string? text, Font font, Brush brush, float x, float y);

	void Image(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes? imageAttr);

	void Image(Image image, float x, float y, float w, float h);

	void Image(Image image, int srcX, int srcY, int srcWidth, int srcHeight);

	void Image(Bitmap image, Rectangle rect);

	void Icon(Icon icon, Rectangle rect);

	void Image(Image image, Rectangle rect);

	void Image(Image image, RectangleF rect);

	void Image(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit);

	void Image(Bitmap bmp, Rectangle rect, float opacity);

	void Image(Image bmp, Rectangle rect, float opacity);

	void Image(RectangleF rect, Image image, TFit fit, float radius, bool round);

	void Image(RectangleF rect, Image image, TFit fit, float radius, TShape shape);

	void Image(RectangleF rect, Image image, TFit fit);

	void Fill(Brush brush, GraphicsPath path);

	void Fill(Brush brush, Rectangle rect);

	void Fill(Brush brush, int x, int y, int w, int h);

	void Fill(Brush brush, RectangleF rect);

	void Fill(Color color, GraphicsPath path);

	void Fill(Color color, Rectangle rect);

	void Fill(Color color, RectangleF rect);

	void FillEllipse(Brush brush, Rectangle rect);

	void FillEllipse(Brush brush, RectangleF rect);

	void FillEllipse(Color color, Rectangle rect);

	void FillEllipse(Color color, RectangleF rect);

	void FillPolygon(Brush brush, Point[] points);

	void FillPolygon(Brush brush, PointF[] points);

	void FillPolygon(Color color, PointF[] points);

	void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle);

	void FillPie(Brush brush, float x, float y, float w, float h, float startAngle, float sweepAngle);

	void Draw(Pen pen, GraphicsPath path);

	void Draw(Pen pen, Rectangle rect);

	void Draw(Color color, float width, GraphicsPath path);

	void Draw(Brush brush, float width, GraphicsPath path);

	void Draw(Color color, float width, Rectangle rect);

	void Draw(Color color, float width, DashStyle dashStyle, GraphicsPath path);

	void DrawEllipse(Pen pen, Rectangle rect);

	void DrawEllipse(Pen pen, RectangleF rect);

	void DrawEllipse(Color color, float width, RectangleF rect);

	void DrawPolygon(Pen pen, Point[] points);

	void DrawPolygon(Pen pen, PointF[] points);

	void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle);

	void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle);

	void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle);

	void DrawLine(Pen pen, Point points, Point points2);

	void DrawLine(Pen pen, PointF points, PointF points2);

	void DrawLine(Pen pen, float x, float y, float x2, float y2);

	void DrawLines(Color color, float width, PointF[] points);

	void DrawLines(Pen pen, Point[] points);

	void DrawLines(Pen pen, PointF[] points);

	GraphicsState Save();

	void Restore(GraphicsState state);

	void SetClip(Rectangle rect);

	void SetClip(RectangleF rect);

	void SetClip(GraphicsPath path);

	void ResetClip();

	void ResetTransform();

	void TranslateTransform(float dx, float dy);

	void RotateTransform(float angle);
}
