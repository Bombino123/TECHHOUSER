using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Entity.Hierarchy;

[Serializable]
[DataContract]
public class HierarchyId : IComparable
{
	private readonly string _hierarchyId;

	private readonly int[][] _nodes;

	public const string PathSeparator = "/";

	private const string InvalidHierarchyIdExceptionMessage = "The input string '{0}' is not a valid string representation of a HierarchyId node.";

	private const string GetReparentedValueOldRootExceptionMessage = "HierarchyId.GetReparentedValue failed because 'oldRoot' was not an ancestor node of 'this'.  'oldRoot' was '{0}', and 'this' was '{1}'.";

	private const string GetDescendantMostBeChildExceptionMessage = "HierarchyId.GetDescendant failed because '{0}' must be a child of 'this'.  '{0}' was '{1}' and 'this' was '{2}'.";

	private const string GetDescendantChild1MustLessThanChild2ExceptionMessage = "HierarchyId.GetDescendant failed because 'child1' must be less than 'child2'.  'child1' was '{0}' and 'child2' was '{1}'.";

	public HierarchyId()
	{
	}

	public HierarchyId(string hierarchyId)
	{
		_hierarchyId = hierarchyId;
		if (hierarchyId == null)
		{
			return;
		}
		string[] array = hierarchyId.Split(new char[1] { '/' });
		if (!string.IsNullOrEmpty(array[0]) || !string.IsNullOrEmpty(array[^1]))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The input string '{0}' is not a valid string representation of a HierarchyId node.", new object[1] { hierarchyId }), "hierarchyId");
		}
		int num = array.Length - 2;
		int[][] array2 = new int[num][];
		for (int i = 0; i < num; i++)
		{
			string[] array3 = array[i + 1].Split(new char[1] { '.' });
			int[] array4 = new int[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				if (!int.TryParse(array3[j], out var result))
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The input string '{0}' is not a valid string representation of a HierarchyId node.", new object[1] { hierarchyId }), "hierarchyId");
				}
				array4[j] = result;
			}
			array2[i] = array4;
		}
		_nodes = array2;
	}

	public HierarchyId GetAncestor(int n)
	{
		if (_nodes == null || GetLevel() < n)
		{
			return new HierarchyId(null);
		}
		if (GetLevel() == n)
		{
			return new HierarchyId("/");
		}
		return new HierarchyId("/" + string.Join("/", _nodes.Take(GetLevel() - n).Select(IntArrayToStirng)) + "/");
	}

	public HierarchyId GetDescendant(HierarchyId child1, HierarchyId child2)
	{
		if (_nodes == null)
		{
			return new HierarchyId(null);
		}
		if (child1 != null && (child1.GetLevel() != GetLevel() + 1 || !child1.IsDescendantOf(this)))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "HierarchyId.GetDescendant failed because '{0}' must be a child of 'this'.  '{0}' was '{1}' and 'this' was '{2}'.", new object[3]
			{
				"child1",
				child1,
				ToString()
			}), "child1");
		}
		if (child2 != null && (child2.GetLevel() != GetLevel() + 1 || !child2.IsDescendantOf(this)))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "HierarchyId.GetDescendant failed because '{0}' must be a child of 'this'.  '{0}' was '{1}' and 'this' was '{2}'.", new object[3]
			{
				"child2",
				child1,
				ToString()
			}), "child2");
		}
		if (child1 == null && child2 == null)
		{
			return new HierarchyId(ToString() + 1 + "/");
		}
		if (child1 == null)
		{
			HierarchyId hierarchyId = new HierarchyId(child2.ToString());
			hierarchyId._nodes.Last()[^1]--;
			return new HierarchyId("/" + string.Join("/", hierarchyId._nodes.Select(IntArrayToStirng)) + "/");
		}
		if (child2 == null)
		{
			HierarchyId hierarchyId2 = new HierarchyId(child1.ToString());
			hierarchyId2._nodes.Last()[^1]++;
			return new HierarchyId("/" + string.Join("/", hierarchyId2._nodes.Select(IntArrayToStirng)) + "/");
		}
		int[] array = child1._nodes.Last();
		int[] array2 = child2._nodes.Last();
		if (CompareIntArrays(array, array2) >= 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "HierarchyId.GetDescendant failed because 'child1' must be less than 'child2'.  'child1' was '{0}' and 'child2' was '{1}'.", new object[2] { child1, child2 }), "child1");
		}
		int i;
		for (i = 0; i < array.Length && array[i] >= array2[i]; i++)
		{
		}
		array = array.Take(i + 1).ToArray();
		if (array[i] + 1 < array2[i])
		{
			array[i]++;
		}
		else
		{
			array = array.Concat(new int[1] { 1 }).ToArray();
		}
		return new HierarchyId("/" + string.Join("/", _nodes.Select(IntArrayToStirng)) + "/" + IntArrayToStirng(array) + "/");
	}

	public short GetLevel()
	{
		if (_nodes == null)
		{
			return 0;
		}
		return (short)_nodes.Length;
	}

	public static HierarchyId GetRoot()
	{
		return DbHierarchyServices.GetRoot();
	}

	public bool IsDescendantOf(HierarchyId parent)
	{
		if (parent == null)
		{
			return true;
		}
		if (_nodes == null || parent.GetLevel() > GetLevel())
		{
			return false;
		}
		for (int i = 0; i < parent.GetLevel(); i++)
		{
			if (CompareIntArrays(_nodes[i], parent._nodes[i]) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public HierarchyId GetReparentedValue(HierarchyId oldRoot, HierarchyId newRoot)
	{
		if (oldRoot == null || newRoot == null)
		{
			return new HierarchyId(null);
		}
		if (!IsDescendantOf(oldRoot))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "HierarchyId.GetReparentedValue failed because 'oldRoot' was not an ancestor node of 'this'.  'oldRoot' was '{0}', and 'this' was '{1}'.", new object[2]
			{
				oldRoot,
				ToString()
			}), "oldRoot");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("/");
		int[][] nodes = newRoot._nodes;
		foreach (int[] array in nodes)
		{
			stringBuilder.Append(IntArrayToStirng(array));
			stringBuilder.Append("/");
		}
		foreach (int[] item in _nodes.Skip(oldRoot.GetLevel()))
		{
			stringBuilder.Append(IntArrayToStirng(item));
			stringBuilder.Append("/");
		}
		return new HierarchyId(stringBuilder.ToString());
	}

	public static HierarchyId Parse(string input)
	{
		return new HierarchyId(input);
	}

	private static string IntArrayToStirng(IEnumerable<int> array)
	{
		return string.Join(".", array);
	}

	private static int CompareIntArrays(int[] array1, int[] array2)
	{
		int num = Math.Min(array1.Length, array2.Length);
		for (int i = 0; i < num; i++)
		{
			int num2 = array1[i];
			int num3 = array2[i];
			if (num2 < num3)
			{
				return -1;
			}
			if (num2 > num3)
			{
				return 1;
			}
		}
		if (array1.Length > num)
		{
			return 1;
		}
		if (array2.Length > num)
		{
			return -1;
		}
		return 0;
	}

	public static int Compare(HierarchyId hid1, HierarchyId hid2)
	{
		int[][] array = hid1?._nodes;
		int[][] array2 = hid2?._nodes;
		if (array == null && array2 == null)
		{
			return 0;
		}
		if (array == null)
		{
			return -1;
		}
		if (array2 == null)
		{
			return 1;
		}
		int num = Math.Min(array.Length, array2.Length);
		for (int i = 0; i < num; i++)
		{
			int[] array3 = array[i];
			int[] array4 = array2[i];
			int num2 = CompareIntArrays(array3, array4);
			if (num2 != 0)
			{
				return num2;
			}
		}
		if (hid1._nodes.Length > num)
		{
			return 1;
		}
		if (hid2._nodes.Length > num)
		{
			return -1;
		}
		return 0;
	}

	public static bool operator <(HierarchyId hid1, HierarchyId hid2)
	{
		return Compare(hid1, hid2) < 0;
	}

	public static bool operator >(HierarchyId hid1, HierarchyId hid2)
	{
		return hid2 < hid1;
	}

	public static bool operator <=(HierarchyId hid1, HierarchyId hid2)
	{
		return Compare(hid1, hid2) <= 0;
	}

	public static bool operator >=(HierarchyId hid1, HierarchyId hid2)
	{
		return hid2 <= hid1;
	}

	public static bool operator ==(HierarchyId hid1, HierarchyId hid2)
	{
		return Compare(hid1, hid2) == 0;
	}

	public static bool operator !=(HierarchyId hid1, HierarchyId hid2)
	{
		return !(hid1 == hid2);
	}

	protected bool Equals(HierarchyId other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		if (_hierarchyId == null)
		{
			return 0;
		}
		return _hierarchyId.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals((HierarchyId)obj);
	}

	public override string ToString()
	{
		return _hierarchyId;
	}

	public int CompareTo(object obj)
	{
		HierarchyId hierarchyId = obj as HierarchyId;
		if (hierarchyId != null)
		{
			return Compare(this, hierarchyId);
		}
		return -1;
	}
}
