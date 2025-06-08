using System.Drawing;

namespace AntdUI;

public class DateBadge
{
	public string Date { get; set; }

	public string? Content { get; set; }

	public Color? Fill { get; set; }

	public bool Round { get; set; }

	public int Radius { get; set; } = 6;


	public TAlignFrom Align { get; set; } = TAlignFrom.TR;


	public float Size { get; set; } = 0.6f;


	public int OffsetX { get; set; } = 2;


	public int OffsetY { get; set; } = 2;


	public DateBadge(string date)
	{
		Date = date;
	}

	public DateBadge(string date, Color fill)
	{
		Date = date;
		Fill = fill;
	}

	public DateBadge(string date, string content)
	{
		Date = date;
		Content = content;
	}

	public DateBadge(string date, int count)
	{
		Round = true;
		Date = date;
		if (count > 0)
		{
			if (count == 999)
			{
				Content = "999";
			}
			else if (count > 1000)
			{
				Content = (count / 1000).ToString().Substring(0, 1) + "K+";
			}
			else if (count > 99)
			{
				Content = "99+";
			}
			else
			{
				Content = count.ToString();
			}
		}
	}

	public DateBadge(string date, int count, Color fill)
		: this(date, count)
	{
		Fill = fill;
	}

	public DateBadge(string date, string content, Color fill)
		: this(date, content)
	{
		Fill = fill;
	}
}
