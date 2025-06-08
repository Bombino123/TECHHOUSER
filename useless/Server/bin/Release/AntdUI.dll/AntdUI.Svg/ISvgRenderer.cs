using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public interface ISvgRenderer : IDisposable
{
	float DpiY { get; }

	SmoothingMode SmoothingMode { get; set; }

	Matrix Transform { get; set; }

	void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit);

	void DrawImageUnscaled(Image image, Point location);

	void DrawPath(Pen pen, GraphicsPath path);

	void FillPath(Brush brush, GraphicsPath path);

	ISvgBoundable GetBoundable();

	Region GetClip();

	ISvgBoundable PopBoundable();

	void RotateTransform(float fAngle, MatrixOrder order = 1);

	void ScaleTransform(float sx, float sy, MatrixOrder order = 1);

	void SetBoundable(ISvgBoundable boundable);

	void SetClip(Region region, CombineMode combineMode = 0);

	void TranslateTransform(float dx, float dy, MatrixOrder order = 1);

	void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit, float opacity);
}
