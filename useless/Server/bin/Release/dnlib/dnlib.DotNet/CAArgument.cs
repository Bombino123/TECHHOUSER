using System;
using System.Collections.Generic;

namespace dnlib.DotNet;

public struct CAArgument : ICloneable
{
	private TypeSig type;

	private object value;

	public TypeSig Type
	{
		readonly get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public object Value
	{
		readonly get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public CAArgument(TypeSig type)
	{
		this.type = type;
		value = null;
	}

	public CAArgument(TypeSig type, object value)
	{
		this.type = type;
		this.value = value;
	}

	readonly object ICloneable.Clone()
	{
		return Clone();
	}

	public readonly CAArgument Clone()
	{
		object obj = value;
		if (obj is CAArgument cAArgument)
		{
			obj = cAArgument.Clone();
		}
		else if (obj is IList<CAArgument> list)
		{
			List<CAArgument> list2 = new List<CAArgument>(list.Count);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				list2.Add(list[i].Clone());
			}
			obj = list2;
		}
		return new CAArgument(type, obj);
	}

	public override readonly string ToString()
	{
		return string.Format("{0} ({1})", value ?? "null", type);
	}
}
