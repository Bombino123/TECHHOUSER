using System.Collections.Generic;
using System.Globalization;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal abstract class Tile<T_Query> where T_Query : ITileQuery
{
	private readonly T_Query m_query;

	private readonly TileOpKind m_opKind;

	public T_Query Query => m_query;

	public abstract string Description { get; }

	public abstract Tile<T_Query> Arg1 { get; }

	public abstract Tile<T_Query> Arg2 { get; }

	public TileOpKind OpKind => m_opKind;

	protected Tile(TileOpKind opKind, T_Query query)
	{
		m_opKind = opKind;
		m_query = query;
	}

	public IEnumerable<T_Query> GetNamedQueries()
	{
		return GetNamedQueries(this);
	}

	private static IEnumerable<T_Query> GetNamedQueries(Tile<T_Query> rewriting)
	{
		if (rewriting == null)
		{
			yield break;
		}
		if (rewriting.OpKind == TileOpKind.Named)
		{
			yield return ((TileNamed<T_Query>)rewriting).NamedQuery;
			yield break;
		}
		foreach (T_Query namedQuery in GetNamedQueries(rewriting.Arg1))
		{
			yield return namedQuery;
		}
		foreach (T_Query namedQuery2 in GetNamedQueries(rewriting.Arg2))
		{
			yield return namedQuery2;
		}
	}

	public override string ToString()
	{
		if (Description != null)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: [{1}]", new object[2] { Description, Query });
		}
		return string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[1] { Query });
	}

	internal abstract Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile);
}
