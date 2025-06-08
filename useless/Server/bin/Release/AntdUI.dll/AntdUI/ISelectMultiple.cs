using System.Collections.Generic;

namespace AntdUI;

internal abstract class ISelectMultiple : ILayeredFormOpacityDown
{
	public virtual void SetValues(object[] value)
	{
	}

	public virtual void SetValues(List<object> value)
	{
	}

	public virtual void ClearValues()
	{
	}

	public virtual void TextChange(string val)
	{
	}
}
