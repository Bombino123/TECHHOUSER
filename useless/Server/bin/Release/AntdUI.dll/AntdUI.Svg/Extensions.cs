using System;
using System.Collections.Generic;

namespace AntdUI.Svg;

public static class Extensions
{
	public static IEnumerable<SvgElement> Descendants<T>(this IEnumerable<T> source) where T : SvgElement
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendants(source, self: false);
	}

	private static IEnumerable<SvgElement> GetDescendants<T>(IEnumerable<T> source, bool self) where T : SvgElement
	{
		Stack<int> positons = new Stack<int>();
		foreach (T start in source)
		{
			if (start == null)
			{
				continue;
			}
			if (self)
			{
				yield return start;
			}
			positons.Push(0);
			SvgElement currParent = start;
			while (positons.Count > 0)
			{
				int currPos = positons.Pop();
				if (currPos < currParent.Children.Count)
				{
					yield return currParent.Children[currPos];
					currParent = currParent.Children[currPos];
					positons.Push(currPos + 1);
					positons.Push(0);
				}
				else
				{
					currParent = currParent.Parent;
				}
			}
		}
	}
}
