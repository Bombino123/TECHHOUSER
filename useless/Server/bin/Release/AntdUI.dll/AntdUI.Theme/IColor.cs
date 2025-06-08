using System;
using System.Drawing;

namespace AntdUI.Theme;

public class IColor
{
	public Color Primary => Colour.Primary.Get();

	public Color PrimaryColor => Colour.PrimaryColor.Get();

	public Color PrimaryHover => Colour.PrimaryHover.Get();

	public Color PrimaryActive => Colour.PrimaryActive.Get();

	public Color PrimaryBg => Colour.PrimaryBg.Get();

	public Color PrimaryBgHover => Colour.PrimaryBgHover.Get();

	public Color PrimaryBorder => Colour.PrimaryBorder.Get();

	public Color PrimaryBorderHover => Colour.PrimaryBorderHover.Get();

	public Color Success => Colour.Success.Get();

	public Color SuccessColor => Colour.SuccessColor.Get();

	public Color SuccessBg => Colour.SuccessBg.Get();

	public Color SuccessBorder => Colour.SuccessBorder.Get();

	public Color SuccessHover => Colour.SuccessHover.Get();

	public Color SuccessActive => Colour.SuccessActive.Get();

	public Color Warning => Colour.Warning.Get();

	public Color WarningColor => Colour.WarningColor.Get();

	public Color WarningBg => Colour.WarningBg.Get();

	public Color WarningBorder => Colour.WarningBorder.Get();

	public Color WarningHover => Colour.WarningHover.Get();

	public Color WarningActive => Colour.WarningActive.Get();

	public Color Error => Colour.Error.Get();

	public Color ErrorColor => Colour.ErrorColor.Get();

	public Color ErrorBg => Colour.ErrorBg.Get();

	public Color ErrorBorder => Colour.ErrorBorder.Get();

	public Color ErrorHover => Colour.ErrorHover.Get();

	public Color ErrorActive => Colour.ErrorActive.Get();

	public Color Info => Colour.Info.Get();

	public Color InfoColor => Colour.InfoColor.Get();

	public Color InfoBg => Colour.InfoBg.Get();

	public Color InfoBorder => Colour.InfoBorder.Get();

	public Color InfoHover => Colour.InfoHover.Get();

	public Color InfoActive => Colour.InfoActive.Get();

	public Color DefaultBg => Colour.DefaultBg.Get();

	public Color DefaultColor => Colour.DefaultColor.Get();

	public Color DefaultBorder => Colour.DefaultBorder.Get();

	public Color TagDefaultBg => Colour.TagDefaultBg.Get();

	public Color TagDefaultColor => Colour.TagDefaultColor.Get();

	public Color TextBase => Colour.TextBase.Get();

	public Color Text => Colour.Text.Get();

	public Color TextSecondary => Colour.TextSecondary.Get();

	public Color TextTertiary => Colour.TextTertiary.Get();

	public Color TextQuaternary => Colour.TextQuaternary.Get();

	public Color BgBase => Colour.BgBase.Get();

	public Color BgContainer => Colour.BgContainer.Get();

	public Color BgElevated => Colour.BgElevated.Get();

	public Color BgLayout => Colour.BgLayout.Get();

	public Color Fill => Colour.Fill.Get();

	public Color FillSecondary => Colour.FillSecondary.Get();

	public Color FillTertiary => Colour.FillTertiary.Get();

	public Color FillQuaternary => Colour.FillQuaternary.Get();

	public Color BorderColor => Colour.BorderColor.Get();

	public Color BorderSecondary => Colour.BorderSecondary.Get();

	public Color BorderColorDisable => Colour.BorderColorDisable.Get();

	public Color Split => Colour.Split.Get();

	public Color HoverBg => Colour.HoverBg.Get();

	public Color HoverColor => Colour.HoverColor.Get();

	public Color SliderHandleColorDisabled => Colour.SliderHandleColorDisabled.Get();

	[Obsolete("use Style.SetPrimary")]
	public void SetPrimary(Color primary)
	{
		Style.SetPrimary(primary);
	}

	[Obsolete("use Style.SetSuccess")]
	public void SetSuccess(Color success)
	{
		Style.SetSuccess(success);
	}

	[Obsolete("use Style.SetWarning")]
	public void SetWarning(Color warning)
	{
		Style.SetWarning(warning);
	}

	[Obsolete("use Style.SetError")]
	public void SetError(Color error)
	{
		Style.SetError(error);
	}

	[Obsolete("use Style.SetInfo")]
	public void SetInfo(Color info)
	{
		Style.SetInfo(info);
	}
}
