using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AntdUI.Svg.FilterEffects;

public class SvgMerge : SvgFilterPrimitive
{
	public override string ClassName => "feMerge";

	public override void Process(ImageBuffer buffer)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		List<SvgMergeNode> list = Children.OfType<SvgMergeNode>().ToList();
		Bitmap val = buffer[list.First().Input];
		Bitmap val2 = new Bitmap(((Image)val).Width, ((Image)val).Height);
		Graphics val3 = Graphics.FromImage((Image)(object)val2);
		try
		{
			foreach (SvgMergeNode item in list)
			{
				val3.DrawImage((Image)(object)buffer[item.Input], new Rectangle(0, 0, ((Image)val).Width, ((Image)val).Height), 0, 0, ((Image)val).Width, ((Image)val).Height, (GraphicsUnit)2);
			}
			val3.Flush();
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		buffer[base.Result] = val2;
	}
}
