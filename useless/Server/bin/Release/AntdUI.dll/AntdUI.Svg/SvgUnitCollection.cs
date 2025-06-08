using System.Collections.Generic;
using System.Linq;

namespace AntdUI.Svg;

public class SvgUnitCollection : List<SvgUnit>
{
	public override string ToString()
	{
		return string.Join(" ", this.Select((SvgUnit u) => u.ToString()).ToArray());
	}

	public static bool IsNullOrEmpty(SvgUnitCollection collection)
	{
		if (collection != null && collection.Count >= 1)
		{
			if (collection.Count == 1)
			{
				if (!(collection[0] == SvgUnit.Empty))
				{
					return collection[0] == SvgUnit.None;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
