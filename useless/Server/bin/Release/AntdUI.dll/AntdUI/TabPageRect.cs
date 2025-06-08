using System.Drawing;

namespace AntdUI;

public class TabPageRect
{
	public Rectangle Rect;

	public Rectangle Rect_Line;

	public Rectangle Rect_Text;

	public Rectangle Rect_Ico;

	public Rectangle Rect_Close;

	internal ITaskOpacity? hover_close;

	public TabPageRect()
	{
	}

	public TabPageRect(Rectangle rect, Rectangle rect_line)
	{
		Rect = (Rect_Text = rect);
		Rect_Line = rect_line;
	}

	public TabPageRect(Rectangle rect_it, Rectangle rect_line, Size size, int ico_size, int gap, int gapI)
	{
		Rect = rect_it;
		Rect_Line = rect_line;
		Rect_Text = new Rectangle(rect_it.X + ico_size + gapI, rect_it.Y, size.Width + gap, rect_it.Height);
		Rect_Ico = new Rectangle(rect_it.X + gapI, rect_it.Y + (rect_it.Height - ico_size) / 2, ico_size, ico_size);
	}

	public TabPageRect(Rectangle rect)
	{
		Rect = (Rect_Text = rect);
	}

	public TabPageRect(Rectangle rect_it, Size size, int ico_size, int gap, int gapI)
	{
		Rect = rect_it;
		Rect_Text = new Rectangle(rect_it.X + ico_size + gap, rect_it.Y + gapI, size.Width + gap, rect_it.Height - gap);
		Rect_Ico = new Rectangle(rect_it.X + gap, rect_it.Y + (rect_it.Height - ico_size) / 2, ico_size, ico_size);
	}

	public TabPageRect(Rectangle rect_it, Size size, int ico_size, int close_size, int gap, int gapI)
	{
		Rect = rect_it;
		Rect_Text = new Rectangle(rect_it.X + ico_size + gap, rect_it.Y + gapI, size.Width + gap, rect_it.Height - gap);
		Rect_Ico = new Rectangle(rect_it.X + gap, rect_it.Y + (rect_it.Height - ico_size) / 2, ico_size, ico_size);
		Rect_Close = new Rectangle(rect_it.Right - gap - close_size, rect_it.Y + (rect_it.Height - close_size) / 2, close_size, close_size);
	}

	public TabPageRect(Rectangle rect_it, bool test, Size size, int close_size, int gap, int gapI)
	{
		Rect = rect_it;
		int y = rect_it.Y + (rect_it.Height - close_size) / 2;
		Rect_Text = new Rectangle(rect_it.X, rect_it.Y + gapI, size.Width + gap, rect_it.Height - gap);
		Rect_Close = new Rectangle(rect_it.Right - gap - close_size, y, close_size, close_size);
	}
}
