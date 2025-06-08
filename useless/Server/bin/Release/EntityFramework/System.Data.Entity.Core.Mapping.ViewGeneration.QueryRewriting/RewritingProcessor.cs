using System.Collections.Generic;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class RewritingProcessor<T_Tile> : TileProcessor<T_Tile> where T_Tile : class
{
	public const double PermuteFraction = 0.0;

	public const int MinPermutations = 0;

	public const int MaxPermutations = 0;

	private int m_numSATChecks;

	private int m_numIntersection;

	private int m_numDifference;

	private int m_numUnion;

	private int m_numErrors;

	private readonly TileProcessor<T_Tile> m_tileProcessor;

	private static Random rnd = new Random(1507);

	internal TileProcessor<T_Tile> TileProcessor => m_tileProcessor;

	public RewritingProcessor(TileProcessor<T_Tile> tileProcessor)
	{
		m_tileProcessor = tileProcessor;
	}

	public void GetStatistics(out int numSATChecks, out int numIntersection, out int numUnion, out int numDifference, out int numErrors)
	{
		numSATChecks = m_numSATChecks;
		numIntersection = m_numIntersection;
		numUnion = m_numUnion;
		numDifference = m_numDifference;
		numErrors = m_numErrors;
	}

	internal override T_Tile GetArg1(T_Tile tile)
	{
		return m_tileProcessor.GetArg1(tile);
	}

	internal override T_Tile GetArg2(T_Tile tile)
	{
		return m_tileProcessor.GetArg2(tile);
	}

	internal override TileOpKind GetOpKind(T_Tile tile)
	{
		return m_tileProcessor.GetOpKind(tile);
	}

	internal override bool IsEmpty(T_Tile a)
	{
		m_numSATChecks++;
		return m_tileProcessor.IsEmpty(a);
	}

	public bool IsDisjointFrom(T_Tile a, T_Tile b)
	{
		return m_tileProcessor.IsEmpty(Join(a, b));
	}

	internal bool IsContainedIn(T_Tile a, T_Tile b)
	{
		T_Tile tile = AntiSemiJoin(a, b);
		return IsEmpty(tile);
	}

	internal bool IsEquivalentTo(T_Tile a, T_Tile b)
	{
		bool num = IsContainedIn(a, b);
		bool flag = IsContainedIn(b, a);
		return num && flag;
	}

	internal override T_Tile Union(T_Tile a, T_Tile b)
	{
		m_numUnion++;
		return m_tileProcessor.Union(a, b);
	}

	internal override T_Tile Join(T_Tile a, T_Tile b)
	{
		if (a == null)
		{
			return b;
		}
		m_numIntersection++;
		return m_tileProcessor.Join(a, b);
	}

	internal override T_Tile AntiSemiJoin(T_Tile a, T_Tile b)
	{
		m_numDifference++;
		return m_tileProcessor.AntiSemiJoin(a, b);
	}

	public void AddError()
	{
		m_numErrors++;
	}

	public int CountOperators(T_Tile query)
	{
		int num = 0;
		if (query != null && GetOpKind(query) != TileOpKind.Named)
		{
			num++;
			num += CountOperators(GetArg1(query));
			num += CountOperators(GetArg2(query));
		}
		return num;
	}

	public int CountViews(T_Tile query)
	{
		HashSet<T_Tile> hashSet = new HashSet<T_Tile>();
		GatherViews(query, hashSet);
		return hashSet.Count;
	}

	public void GatherViews(T_Tile rewriting, HashSet<T_Tile> views)
	{
		if (rewriting != null)
		{
			if (GetOpKind(rewriting) == TileOpKind.Named)
			{
				views.Add(rewriting);
				return;
			}
			GatherViews(GetArg1(rewriting), views);
			GatherViews(GetArg2(rewriting), views);
		}
	}

	public static IEnumerable<T> AllButOne<T>(IEnumerable<T> list, int toSkipPosition)
	{
		int valuePosition = 0;
		foreach (T item in list)
		{
			if (valuePosition++ != toSkipPosition)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> Concat<T>(T value, IEnumerable<T> rest)
	{
		yield return value;
		foreach (T item in rest)
		{
			yield return item;
		}
	}

	public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list)
	{
		IEnumerable<T> rest = null;
		int valuePosition = 0;
		foreach (T value in list)
		{
			rest = AllButOne(list, valuePosition++);
			foreach (IEnumerable<T> item in Permute(rest))
			{
				yield return Concat(value, item);
			}
		}
		if (rest == null)
		{
			yield return list;
		}
	}

	public static List<T> RandomPermutation<T>(IEnumerable<T> input)
	{
		List<T> list = new List<T>(input);
		for (int i = 0; i < list.Count; i++)
		{
			int index = rnd.Next(list.Count);
			T value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
		return list;
	}

	public static IEnumerable<T> Reverse<T>(IEnumerable<T> input, HashSet<T> filter)
	{
		List<T> list = new List<T>(input);
		list.Reverse();
		foreach (T item in list)
		{
			if (filter.Contains(item))
			{
				yield return item;
			}
		}
	}

	public bool RewriteQuery(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
	{
		if (RewriteQueryOnce(toFill, toAvoid, views, out rewriting))
		{
			HashSet<T_Tile> hashSet = new HashSet<T_Tile>();
			GatherViews(rewriting, hashSet);
			int num = CountOperators(rewriting);
			int num2 = 0;
			int num3 = Math.Min(0, Math.Max(0, (int)((double)hashSet.Count * 0.0)));
			while (num2++ < num3)
			{
				IEnumerable<T_Tile> views2 = ((num2 != 1) ? RandomPermutation(hashSet) : Reverse(views, hashSet));
				RewriteQueryOnce(toFill, toAvoid, views2, out var rewriting2);
				int num4 = CountOperators(rewriting2);
				if (num4 < num)
				{
					num = num4;
					rewriting = rewriting2;
				}
				HashSet<T_Tile> hashSet2 = new HashSet<T_Tile>();
				GatherViews(rewriting2, hashSet2);
				hashSet = hashSet2;
			}
			return true;
		}
		return false;
	}

	public bool RewriteQueryOnce(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
	{
		List<T_Tile> views2 = new List<T_Tile>(views);
		return RewritingPass<T_Tile>.RewriteQuery(toFill, toAvoid, out rewriting, views2, this);
	}
}
