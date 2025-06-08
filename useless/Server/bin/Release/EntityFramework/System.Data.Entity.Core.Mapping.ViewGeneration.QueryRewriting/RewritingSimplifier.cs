using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class RewritingSimplifier<T_Tile> where T_Tile : class
{
	private readonly T_Tile m_originalRewriting;

	private readonly T_Tile m_toAvoid;

	private readonly RewritingProcessor<T_Tile> m_qp;

	private readonly Dictionary<T_Tile, TileOpKind> m_usedViews = new Dictionary<T_Tile, TileOpKind>();

	private RewritingSimplifier(T_Tile originalRewriting, T_Tile toAvoid, Dictionary<T_Tile, TileOpKind> usedViews, RewritingProcessor<T_Tile> qp)
	{
		m_originalRewriting = originalRewriting;
		m_toAvoid = toAvoid;
		m_qp = qp;
		m_usedViews = usedViews;
	}

	private RewritingSimplifier(T_Tile rewriting, T_Tile toFill, T_Tile toAvoid, RewritingProcessor<T_Tile> qp)
	{
		m_originalRewriting = toFill;
		m_toAvoid = toAvoid;
		m_qp = qp;
		m_usedViews = new Dictionary<T_Tile, TileOpKind>();
		GatherUnionedSubqueriesInUsedViews(rewriting);
	}

	internal static bool TrySimplifyUnionRewriting(ref T_Tile rewriting, T_Tile toFill, T_Tile toAvoid, RewritingProcessor<T_Tile> qp)
	{
		if (new RewritingSimplifier<T_Tile>(rewriting, toFill, toAvoid, qp).SimplifyRewriting(out var simplifiedRewriting))
		{
			rewriting = simplifiedRewriting;
			return true;
		}
		return false;
	}

	internal static bool TrySimplifyJoinRewriting(ref T_Tile rewriting, T_Tile toAvoid, Dictionary<T_Tile, TileOpKind> usedViews, RewritingProcessor<T_Tile> qp)
	{
		if (new RewritingSimplifier<T_Tile>(rewriting, toAvoid, usedViews, qp).SimplifyRewriting(out var simplifiedRewriting))
		{
			rewriting = simplifiedRewriting;
			return true;
		}
		return false;
	}

	private void GatherUnionedSubqueriesInUsedViews(T_Tile query)
	{
		if (query != null)
		{
			if (m_qp.GetOpKind(query) != 0)
			{
				m_usedViews[query] = TileOpKind.Union;
				return;
			}
			GatherUnionedSubqueriesInUsedViews(m_qp.GetArg1(query));
			GatherUnionedSubqueriesInUsedViews(m_qp.GetArg2(query));
		}
	}

	private bool SimplifyRewriting(out T_Tile simplifiedRewriting)
	{
		bool result = false;
		simplifiedRewriting = null;
		T_Tile simplifiedRewriting2;
		while (SimplifyRewritingOnce(out simplifiedRewriting2))
		{
			result = true;
			simplifiedRewriting = simplifiedRewriting2;
		}
		return result;
	}

	private bool SimplifyRewritingOnce(out T_Tile simplifiedRewriting)
	{
		HashSet<T_Tile> hashSet = new HashSet<T_Tile>(m_usedViews.Keys);
		foreach (T_Tile key in m_usedViews.Keys)
		{
			TileOpKind tileOpKind = m_usedViews[key];
			if ((uint)tileOpKind <= 1u)
			{
				hashSet.Remove(key);
				if (SimplifyRewritingOnce(key, hashSet, out simplifiedRewriting))
				{
					return true;
				}
				hashSet.Add(key);
			}
		}
		simplifiedRewriting = null;
		return false;
	}

	private bool SimplifyRewritingOnce(T_Tile newRewriting, HashSet<T_Tile> remainingViews, out T_Tile simplifiedRewriting)
	{
		simplifiedRewriting = null;
		if (remainingViews.Count == 0)
		{
			return false;
		}
		if (remainingViews.Count == 1)
		{
			T_Tile key = remainingViews.First();
			bool flag = false;
			if ((m_usedViews[key] != 0) ? (m_qp.IsContainedIn(m_originalRewriting, newRewriting) && m_qp.IsDisjointFrom(m_toAvoid, newRewriting)) : m_qp.IsContainedIn(m_originalRewriting, newRewriting))
			{
				simplifiedRewriting = newRewriting;
				m_usedViews.Remove(key);
				return true;
			}
			return false;
		}
		int num = remainingViews.Count / 2;
		int num2 = 0;
		T_Tile val = newRewriting;
		T_Tile val2 = newRewriting;
		HashSet<T_Tile> hashSet = new HashSet<T_Tile>();
		HashSet<T_Tile> hashSet2 = new HashSet<T_Tile>();
		foreach (T_Tile remainingView in remainingViews)
		{
			TileOpKind viewKind = m_usedViews[remainingView];
			if (num2++ < num)
			{
				hashSet.Add(remainingView);
				val = GetRewritingHalf(val, remainingView, viewKind);
			}
			else
			{
				hashSet2.Add(remainingView);
				val2 = GetRewritingHalf(val2, remainingView, viewKind);
			}
		}
		if (!SimplifyRewritingOnce(val, hashSet2, out simplifiedRewriting))
		{
			return SimplifyRewritingOnce(val2, hashSet, out simplifiedRewriting);
		}
		return true;
	}

	private T_Tile GetRewritingHalf(T_Tile halfRewriting, T_Tile remainingView, TileOpKind viewKind)
	{
		switch (viewKind)
		{
		case TileOpKind.Join:
			halfRewriting = m_qp.Join(halfRewriting, remainingView);
			break;
		case TileOpKind.AntiSemiJoin:
			halfRewriting = m_qp.AntiSemiJoin(halfRewriting, remainingView);
			break;
		case TileOpKind.Union:
			halfRewriting = m_qp.Union(halfRewriting, remainingView);
			break;
		}
		return halfRewriting;
	}
}
