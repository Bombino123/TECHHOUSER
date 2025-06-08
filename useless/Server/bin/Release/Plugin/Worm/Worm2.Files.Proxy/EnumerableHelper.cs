using System;
using System.Collections.Generic;
using System.Linq;

namespace Worm2.Files.Proxy;

public static class EnumerableHelper
{
	private static readonly Random R;

	static EnumerableHelper()
	{
		R = new Random();
	}

	public static TE Random<TE>(IEnumerable<TE> input)
	{
		TE[] array = (input as TE[]) ?? input.ToArray();
		return array.ElementAt(R.Next(array.Length));
	}
}
