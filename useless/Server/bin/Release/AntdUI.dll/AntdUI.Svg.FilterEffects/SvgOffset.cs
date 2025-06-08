using System;
using System.Drawing;

namespace AntdUI.Svg.FilterEffects;

public class SvgOffset : SvgFilterPrimitive
{
	public override string ClassName => "feOffset";

	[SvgAttribute("dx")]
	public SvgUnit Dx { get; set; }

	[SvgAttribute("dy")]
	public SvgUnit Dy { get; set; }

	public override void Process(ImageBuffer buffer)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		Bitmap val = buffer[base.Input];
		if (val != null)
		{
			Bitmap val2 = new Bitmap(((Image)val).Width, ((Image)val).Height);
			PointF[] array = new PointF[1]
			{
				new PointF(Dx.ToDeviceValue(null, UnitRenderingType.Horizontal, null), Dy.ToDeviceValue(null, UnitRenderingType.Vertical, null))
			};
			buffer.Transform.TransformVectors(array);
			Graphics val3 = Graphics.FromImage((Image)(object)val2);
			try
			{
				val3.DrawImage((Image)(object)val, new Rectangle((int)array[0].X, (int)array[0].Y, ((Image)val).Width, ((Image)val).Height), 0, 0, ((Image)val).Width, ((Image)val).Height, (GraphicsUnit)2);
				val3.Flush();
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			buffer[base.Result] = val2;
		}
	}
}
