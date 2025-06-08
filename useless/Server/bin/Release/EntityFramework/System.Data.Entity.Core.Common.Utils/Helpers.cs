#define TRACE
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Common.Utils;

internal static class Helpers
{
	internal static void FormatTraceLine(string format, params object[] args)
	{
		Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
	}

	internal static void StringTrace(string arg)
	{
		Trace.Write(arg);
	}

	internal static void StringTraceLine(string arg)
	{
		Trace.WriteLine(arg);
	}

	internal static bool IsSetEqual<Type>(IEnumerable<Type> list1, IEnumerable<Type> list2, IEqualityComparer<Type> comparer)
	{
		Set<Type> set = new Set<Type>(list1, comparer);
		Set<Type> equals = new Set<Type>(list2, comparer);
		return set.SetEquals(equals);
	}

	internal static IEnumerable<SuperType> AsSuperTypeList<SubType, SuperType>(IEnumerable<SubType> values) where SubType : SuperType
	{
		foreach (SubType value in values)
		{
			yield return (SuperType)(object)value;
		}
	}

	internal static TElement[] Prepend<TElement>(TElement[] args, TElement arg)
	{
		TElement[] array = new TElement[args.Length + 1];
		array[0] = arg;
		for (int i = 0; i < args.Length; i++)
		{
			array[i + 1] = args[i];
		}
		return array;
	}

	internal static TNode BuildBalancedTreeInPlace<TNode>(IList<TNode> nodes, Func<TNode, TNode, TNode> combinator)
	{
		if (nodes.Count == 1)
		{
			return nodes[0];
		}
		if (nodes.Count == 2)
		{
			return combinator(nodes[0], nodes[1]);
		}
		for (int num = nodes.Count; num != 1; num /= 2)
		{
			bool flag = (num & 1) == 1;
			if (flag)
			{
				num--;
			}
			int num2 = 0;
			for (int i = 0; i < num; i += 2)
			{
				nodes[num2++] = combinator(nodes[i], nodes[i + 1]);
			}
			if (flag)
			{
				int index = num2 - 1;
				nodes[index] = combinator(nodes[index], nodes[num]);
			}
		}
		return nodes[0];
	}

	internal static IEnumerable<TNode> GetLeafNodes<TNode>(TNode root, Func<TNode, bool> isLeaf, Func<TNode, IEnumerable<TNode>> getImmediateSubNodes)
	{
		Stack<TNode> nodes = new Stack<TNode>();
		nodes.Push(root);
		while (nodes.Count > 0)
		{
			TNode val = nodes.Pop();
			if (isLeaf(val))
			{
				yield return val;
				continue;
			}
			List<TNode> list = new List<TNode>(getImmediateSubNodes(val));
			for (int num = list.Count - 1; num > -1; num--)
			{
				nodes.Push(list[num]);
			}
		}
	}
}
