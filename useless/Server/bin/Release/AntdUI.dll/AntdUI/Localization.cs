using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

public static class Localization
{
	private static string? currentLanguage;

	public static ILocalization? Provider { get; set; }

	public static string? DefaultLanguage { get; set; }

	public static string CurrentLanguage
	{
		get
		{
			if (currentLanguage == null)
			{
				currentLanguage = Thread.CurrentThread.CurrentUICulture.Name;
			}
			return currentLanguage;
		}
	}

	public static void SetLanguage(this string lang)
	{
		CultureInfo cultureInfo = new CultureInfo(lang);
		currentLanguage = cultureInfo.Name;
		Thread.CurrentThread.CurrentUICulture = cultureInfo;
		EventHub.Dispatch(EventType.LANG, cultureInfo);
	}

	public static string Get(string id, string def)
	{
		return Provider?.GetLocalizedString(id) ?? def;
	}

	public static string? GetLangI(this Control control, string? id, string? def)
	{
		if (id == null)
		{
			return def;
		}
		if (DefaultLanguage == CurrentLanguage)
		{
			return def;
		}
		return Provider?.GetLocalizedString(id.Replace("{id}", control.Name)) ?? def;
	}

	public static string GetLangIN(this Control control, string? id, string def)
	{
		if (id == null)
		{
			return def;
		}
		if (DefaultLanguage == CurrentLanguage)
		{
			return def;
		}
		return Provider?.GetLocalizedString(id.Replace("{id}", control.Name)) ?? def;
	}

	public static string? GetLangI(string? id, string? def)
	{
		if (id == null)
		{
			return def;
		}
		if (DefaultLanguage == CurrentLanguage)
		{
			return def;
		}
		return Provider?.GetLocalizedString(id) ?? def;
	}

	public static string? GetLangI(string? id, string? def, params string?[][] dir)
	{
		if (id == null)
		{
			return def;
		}
		if (DefaultLanguage == CurrentLanguage)
		{
			return def;
		}
		if (dir.Length != 0)
		{
			foreach (string?[] obj in dir)
			{
				string text = obj[0];
				string text2 = obj[1];
				if (text != null && text2 != null)
				{
					id = id.Replace(text, text2);
				}
			}
		}
		return Provider?.GetLocalizedString(id) ?? def;
	}

	public static string GetLangIN(string? id, string def, params string?[][] dir)
	{
		if (id == null)
		{
			return def;
		}
		if (DefaultLanguage == CurrentLanguage)
		{
			return def;
		}
		if (dir.Length != 0)
		{
			foreach (string?[] obj in dir)
			{
				string text = obj[0];
				string text2 = obj[1];
				if (text != null && text2 != null)
				{
					id = id.Replace(text, text2);
				}
			}
		}
		return Provider?.GetLocalizedString(id) ?? def;
	}

	public static void LoadLanguage<T>(this Form form)
	{
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(T));
		componentResourceManager.ApplyResources(form, "$this");
		Loading((Control)(object)form, componentResourceManager);
		if (form is BaseForm { AutoHandDpi: not false } baseForm)
		{
			baseForm.AutoDpi((Control)(object)baseForm);
		}
	}

	private static void Loading(Control control, ComponentResourceManager resources)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		MenuStrip val = (MenuStrip)(object)((control is MenuStrip) ? control : null);
		if (val != null)
		{
			resources.ApplyResources(control, control.Name);
			if (((ArrangedElementCollection)((ToolStrip)val).Items).Count > 0)
			{
				foreach (ToolStripMenuItem item in (ArrangedElementCollection)((ToolStrip)val).Items)
				{
					Loading(item, resources);
				}
			}
		}
		foreach (Control item2 in (ArrangedElementCollection)control.Controls)
		{
			Control val2 = item2;
			resources.ApplyResources(val2, val2.Name);
			Loading(val2, resources);
		}
	}

	private static void Loading(ToolStripMenuItem item, ComponentResourceManager resources)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (item == null)
		{
			return;
		}
		resources.ApplyResources(item, ((ToolStripItem)item).Name);
		if (((ArrangedElementCollection)((ToolStripDropDownItem)item).DropDownItems).Count <= 0)
		{
			return;
		}
		foreach (ToolStripMenuItem item2 in (ArrangedElementCollection)((ToolStripDropDownItem)item).DropDownItems)
		{
			Loading(item2, resources);
		}
	}
}
