using System.Text;

namespace Vestris.ResourceLib;

public class MenuExTemplateItemCommand : MenuExTemplateItem
{
	public bool IsSeparator
	{
		get
		{
			if (_header.dwType != 2048)
			{
				if ((_header.bResInfo == ushort.MaxValue || _header.bResInfo == 0) && _header.dwMenuId == 0)
				{
					return _menuString == null;
				}
				return false;
			}
			return true;
		}
	}

	public override string ToString(int indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (IsSeparator)
		{
			stringBuilder.AppendLine($"{new string(' ', indent)}MENUITEM SEPARATOR");
		}
		else
		{
			stringBuilder.AppendLine(string.Format("{0}MENUITEM \"{1}\", {2}", new string(' ', indent), (_menuString == null) ? string.Empty : _menuString.Replace("\t", "\\t"), _header.dwMenuId));
		}
		return stringBuilder.ToString();
	}
}
