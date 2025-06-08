using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class RewritingPass<T_Tile> where T_Tile : class
{
	private readonly T_Tile m_toFill;

	private readonly T_Tile m_toAvoid;

	private readonly List<T_Tile> m_views;

	private readonly RewritingProcessor<T_Tile> m_qp;

	private readonly Dictionary<T_Tile, TileOpKind> m_usedViews = new Dictionary<T_Tile, TileOpKind>();

	private IEnumerable<T_Tile> AvailableViews => m_views.Where((T_Tile view) => !m_usedViews.ContainsKey(view));

	public RewritingPass(T_Tile toFill, T_Tile toAvoid, List<T_Tile> views, RewritingProcessor<T_Tile> qp)
	{
		m_toFill = toFill;
		m_toAvoid = toAvoid;
		m_views = views;
		m_qp = qp;
	}

	public static bool RewriteQuery(T_Tile toFill, T_Tile toAvoid, out T_Tile rewriting, List<T_Tile> views, RewritingProcessor<T_Tile> qp)
	{
		if (new RewritingPass<T_Tile>(toFill, toAvoid, views, qp).RewriteQuery(out rewriting))
		{
			RewritingSimplifier<T_Tile>.TrySimplifyUnionRewriting(ref rewriting, toFill, toAvoid, qp);
			return true;
		}
		return false;
	}

	private static bool RewriteQueryInternal(T_Tile toFill, T_Tile toAvoid, out T_Tile rewriting, List<T_Tile> views, RewritingProcessor<T_Tile> qp)
	{
		return new RewritingPass<T_Tile>(toFill, toAvoid, views, qp).RewriteQuery(out rewriting);
	}

	private bool RewriteQuery(out T_Tile rewriting)
	{
		rewriting = m_toFill;
		if (!FindRewritingByIncludedAndDisjoint(out var rewritingSoFar) && !FindContributingView(out rewritingSoFar))
		{
			return false;
		}
		bool flag = !m_qp.IsDisjointFrom(rewritingSoFar, m_toAvoid);
		if (flag)
		{
			foreach (T_Tile availableView in AvailableViews)
			{
				if (TryJoin(availableView, ref rewritingSoFar))
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			foreach (T_Tile availableView2 in AvailableViews)
			{
				if (TryAntiSemiJoin(availableView2, ref rewritingSoFar))
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			return false;
		}
		RewritingSimplifier<T_Tile>.TrySimplifyJoinRewriting(ref rewritingSoFar, m_toAvoid, m_usedViews, m_qp);
		T_Tile val = m_qp.AntiSemiJoin(m_toFill, rewritingSoFar);
		if (!m_qp.IsEmpty(val))
		{
			if (!RewriteQueryInternal(val, m_toAvoid, out var rewriting2, m_views, m_qp))
			{
				rewriting = rewriting2;
				return false;
			}
			rewritingSoFar = ((!m_qp.IsContainedIn(rewritingSoFar, rewriting2)) ? m_qp.Union(rewritingSoFar, rewriting2) : rewriting2);
		}
		rewriting = rewritingSoFar;
		return true;
	}

	private bool TryJoin(T_Tile view, ref T_Tile rewriting)
	{
		T_Tile val = m_qp.Join(rewriting, view);
		if (!m_qp.IsEmpty(val))
		{
			m_usedViews[view] = TileOpKind.Join;
			rewriting = val;
			return m_qp.IsDisjointFrom(rewriting, m_toAvoid);
		}
		return false;
	}

	private bool TryAntiSemiJoin(T_Tile view, ref T_Tile rewriting)
	{
		T_Tile val = m_qp.AntiSemiJoin(rewriting, view);
		if (!m_qp.IsEmpty(val))
		{
			m_usedViews[view] = TileOpKind.AntiSemiJoin;
			rewriting = val;
			return m_qp.IsDisjointFrom(rewriting, m_toAvoid);
		}
		return false;
	}

	private bool FindRewritingByIncludedAndDisjoint(out T_Tile rewritingSoFar)
	{
		rewritingSoFar = null;
		foreach (T_Tile availableView in AvailableViews)
		{
			if (!m_qp.IsContainedIn(m_toFill, availableView))
			{
				continue;
			}
			if (rewritingSoFar == null)
			{
				rewritingSoFar = availableView;
				m_usedViews[availableView] = TileOpKind.Join;
			}
			else
			{
				T_Tile val = m_qp.Join(rewritingSoFar, availableView);
				if (m_qp.IsContainedIn(rewritingSoFar, val))
				{
					continue;
				}
				rewritingSoFar = val;
				m_usedViews[availableView] = TileOpKind.Join;
			}
			if (m_qp.IsContainedIn(rewritingSoFar, m_toFill))
			{
				return true;
			}
		}
		if (rewritingSoFar != null)
		{
			foreach (T_Tile availableView2 in AvailableViews)
			{
				if (m_qp.IsDisjointFrom(m_toFill, availableView2) && !m_qp.IsDisjointFrom(rewritingSoFar, availableView2))
				{
					rewritingSoFar = m_qp.AntiSemiJoin(rewritingSoFar, availableView2);
					m_usedViews[availableView2] = TileOpKind.AntiSemiJoin;
					if (m_qp.IsContainedIn(rewritingSoFar, m_toFill))
					{
						return true;
					}
				}
			}
		}
		return rewritingSoFar != null;
	}

	private bool FindContributingView(out T_Tile rewriting)
	{
		foreach (T_Tile availableView in AvailableViews)
		{
			if (!m_qp.IsDisjointFrom(availableView, m_toFill))
			{
				rewriting = availableView;
				m_usedViews[availableView] = TileOpKind.Join;
				return true;
			}
		}
		rewriting = null;
		return false;
	}
}
