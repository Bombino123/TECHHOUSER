using System;
using System.Drawing;

namespace AntdUI;

public class CellLink : ICell
{
	internal bool textLine;

	private string? _text;

	internal StringFormat stringFormat = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private ContentAlignment textAlign = (ContentAlignment)32;

	private bool enabled = true;

	internal ITask? ThreadHover;

	internal ITask? ThreadImageHover;

	internal bool _mouseDown;

	internal int AnimationHoverValue;

	internal bool AnimationHover;

	internal bool _mouseHover;

	internal bool AnimationImageHover;

	internal float AnimationImageHoverValue;

	internal ITask? ThreadClick;

	internal bool AnimationClick;

	internal float AnimationClickValue;

	public string Id { get; set; }

	public string? Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (!(_text == value))
			{
				_text = value;
				if (_text == null)
				{
					textLine = false;
				}
				else
				{
					textLine = _text.Contains(Environment.NewLine);
				}
				OnPropertyChanged(layout: true);
			}
		}
	}

	public ContentAlignment TextAlign
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return textAlign;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (textAlign != value)
			{
				textAlign = value;
				textAlign.SetAlignment(ref stringFormat);
				OnPropertyChanged();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled == value)
			{
				enabled = value;
			}
			enabled = value;
			OnPropertyChanged();
		}
	}

	public string? Tooltip { get; set; }

	internal bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown != value)
			{
				_mouseDown = value;
				OnPropertyChanged();
			}
		}
	}

	internal virtual bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover != value)
			{
				_mouseHover = value;
				if (Enabled)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	public CellLink(string id, string? text)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Id = id;
		_text = text;
	}

	public override string? ToString()
	{
		return _text;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		Table.PaintLink(g, font, base.Rect, this, enable);
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		Size size = g.MeasureString(Text ?? "龍Qq", font);
		return new Size(size.Width, size.Height);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		base.Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, rect.Width, size.Height);
	}

	internal virtual void Click()
	{
	}
}
