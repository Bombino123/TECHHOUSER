using System.Drawing;
using System.Drawing.Text;

namespace AntdUI;

public class Config
{
	private static TMode mode = TMode.Light;

	private static bool dpione = true;

	private static float _dpi = 1f;

	private static float? _dpi_custom;

	public const string NullText = "龍Qq";

	public static TMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			EventHub.Dispatch(EventType.THEME, value);
		}
	}

	public static bool IsLight
	{
		get
		{
			return mode == TMode.Light;
		}
		set
		{
			Mode = ((!value) ? TMode.Dark : TMode.Light);
			EventHub.Dispatch(EventType.THEME, value);
		}
	}

	public static bool IsDark
	{
		get
		{
			return mode == TMode.Dark;
		}
		set
		{
			Mode = (value ? TMode.Dark : TMode.Light);
			EventHub.Dispatch(EventType.THEME, value);
		}
	}

	public static bool Animation { get; set; } = true;


	public static bool TouchEnabled { get; set; } = true;


	public static int TouchThreshold { get; set; } = 10;


	public static bool TouchClickEnabled { get; set; }

	public static bool ShadowEnabled { get; set; } = true;


	public static bool ShowInWindow { get; set; }

	public static bool ShowInWindowByMessage { get; set; }

	public static bool ShowInWindowByNotification { get; set; }

	public static int NoticeWindowOffsetXY { get; set; }

	public static TextRenderingHint? TextRenderingHint { get; set; }

	public static Font? Font { get; set; }

	public static bool ScrollBarHide { get; set; }

	public static int ScrollMinSizeY { get; set; } = 30;


	public static float Dpi
	{
		get
		{
			if (dpione)
			{
				Helper.GDI((Canvas g) => g.DpiX);
			}
			if (_dpi_custom.HasValue)
			{
				return _dpi_custom.Value;
			}
			return _dpi;
		}
	}

	public static void SetDpi(float? dpi)
	{
		if (_dpi_custom != dpi)
		{
			_dpi_custom = dpi;
			if (dpi.HasValue)
			{
				EventHub.Dispatch(EventType.DPI, dpi.Value);
			}
			else
			{
				EventHub.Dispatch(EventType.DPI, _dpi);
			}
		}
	}

	internal static void SetDpi(float dpi)
	{
		dpione = false;
		if (_dpi != dpi)
		{
			_dpi = dpi;
			if (!_dpi_custom.HasValue)
			{
				EventHub.Dispatch(EventType.DPI, dpi);
			}
		}
	}

	internal static void SetDpi(Graphics g)
	{
		SetDpi(g.DpiX / 96f);
	}

	public static void SetCorrectionTextRendering(params string[] families)
	{
		for (int i = 0; i < families.Length; i++)
		{
			CorrectionTextRendering.Set(families[i]);
		}
	}
}
