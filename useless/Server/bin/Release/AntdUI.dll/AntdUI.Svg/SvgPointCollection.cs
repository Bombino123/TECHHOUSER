using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AntdUI.Svg;

public class SvgPointCollection : List<SvgUnit>
{
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < base.Count; i += 2)
		{
			if (i + 1 < base.Count)
			{
				if (i > 1)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(base[i].Value.ToString(CultureInfo.InvariantCulture));
				stringBuilder.Append(",");
				stringBuilder.Append(base[i + 1].Value.ToString(CultureInfo.InvariantCulture));
			}
		}
		return stringBuilder.ToString();
	}
}
