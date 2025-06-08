using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace AntdUI.Svg;

public sealed class SvgRenderer : ISvgRenderer, IDisposable, IGraphicsProvider
{
	private readonly Graphics _innerGraphics;

	private readonly bool _disposable;

	private readonly Image _image;

	private readonly Stack<ISvgBoundable> _boundables = new Stack<ISvgBoundable>();

	public float DpiY => _innerGraphics.DpiY;

	public SmoothingMode SmoothingMode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _innerGraphics.SmoothingMode;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_innerGraphics.SmoothingMode = value;
		}
	}

	public Matrix Transform
	{
		get
		{
			return _innerGraphics.Transform;
		}
		set
		{
			_innerGraphics.Transform = value;
		}
	}

	public void SetBoundable(ISvgBoundable boundable)
	{
		_boundables.Push(boundable);
	}

	public ISvgBoundable GetBoundable()
	{
		return _boundables.Peek();
	}

	public ISvgBoundable PopBoundable()
	{
		return _boundables.Pop();
	}

	private SvgRenderer(Graphics graphics, bool disposable = true)
	{
		_innerGraphics = graphics;
		_disposable = disposable;
	}

	private SvgRenderer(Graphics graphics, Image image)
		: this(graphics)
	{
		_image = image;
	}

	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_innerGraphics.DrawImage(image, destRect, srcRect, graphicsUnit);
	}

	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit, float opacity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		ColorMatrix val = new ColorMatrix
		{
			Matrix33 = opacity
		};
		ImageAttributes val2 = new ImageAttributes();
		val2.SetColorMatrix(val, (ColorMatrixFlag)0, (ColorAdjustType)1);
		PointF[] array = new PointF[3]
		{
			destRect.Location,
			new PointF(destRect.X + destRect.Width, destRect.Y),
			new PointF(destRect.X, destRect.Y + destRect.Height)
		};
		_innerGraphics.DrawImage(image, array, srcRect, graphicsUnit, val2);
	}

	public void DrawImageUnscaled(Image image, Point location)
	{
		_innerGraphics.DrawImageUnscaled(image, location);
	}

	public void DrawPath(Pen pen, GraphicsPath path)
	{
		_innerGraphics.DrawPath(pen, path);
	}

	public void FillPath(Brush brush, GraphicsPath path)
	{
		_innerGraphics.FillPath(brush, path);
	}

	public Region GetClip()
	{
		return _innerGraphics.Clip;
	}

	public void RotateTransform(float fAngle, MatrixOrder order = 1)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_innerGraphics.RotateTransform(fAngle, order);
	}

	public void ScaleTransform(float sx, float sy, MatrixOrder order = 1)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_innerGraphics.ScaleTransform(sx, sy, order);
	}

	public void SetClip(Region region, CombineMode combineMode = 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_innerGraphics.SetClip(region, combineMode);
	}

	public void TranslateTransform(float dx, float dy, MatrixOrder order = 1)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_innerGraphics.TranslateTransform(dx, dy, order);
	}

	public void Dispose()
	{
		if (_disposable)
		{
			_innerGraphics.Dispose();
		}
		if (_image != null)
		{
			_image.Dispose();
		}
	}

	Graphics IGraphicsProvider.GetGraphics()
	{
		return _innerGraphics;
	}

	private static Graphics CreateGraphics(Image image)
	{
		Graphics obj = Graphics.FromImage(image);
		obj.PixelOffsetMode = (PixelOffsetMode)4;
		obj.CompositingQuality = (CompositingQuality)2;
		obj.TextRenderingHint = (TextRenderingHint)4;
		obj.TextContrast = 1;
		return obj;
	}

	public static ISvgRenderer FromImage(Image image)
	{
		return new SvgRenderer(CreateGraphics(image));
	}

	public static ISvgRenderer FromGraphics(Graphics graphics)
	{
		return new SvgRenderer(graphics, disposable: false);
	}

	public static ISvgRenderer FromNull()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Bitmap image = new Bitmap(1, 1);
		return new SvgRenderer(CreateGraphics((Image)(object)image), (Image)(object)image);
	}
}
