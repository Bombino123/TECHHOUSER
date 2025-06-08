using System.Globalization;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class TileBinaryOperator<T_Query> : Tile<T_Query> where T_Query : ITileQuery
{
	private readonly Tile<T_Query> m_arg1;

	private readonly Tile<T_Query> m_arg2;

	public override Tile<T_Query> Arg1 => m_arg1;

	public override Tile<T_Query> Arg2 => m_arg2;

	public override string Description
	{
		get
		{
			string format = null;
			switch (base.OpKind)
			{
			case TileOpKind.Join:
				format = "({0} & {1})";
				break;
			case TileOpKind.AntiSemiJoin:
				format = "({0} - {1})";
				break;
			case TileOpKind.Union:
				format = "({0} | {1})";
				break;
			}
			return string.Format(CultureInfo.InvariantCulture, format, new object[2] { Arg1.Description, Arg2.Description });
		}
	}

	public TileBinaryOperator(Tile<T_Query> arg1, Tile<T_Query> arg2, TileOpKind opKind, T_Query query)
		: base(opKind, query)
	{
		m_arg1 = arg1;
		m_arg2 = arg2;
	}

	internal override Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile)
	{
		Tile<T_Query> tile = Arg1.Replace(oldTile, newTile);
		Tile<T_Query> tile2 = Arg2.Replace(oldTile, newTile);
		if (tile != Arg1 || tile2 != Arg2)
		{
			return new TileBinaryOperator<T_Query>(tile, tile2, base.OpKind, base.Query);
		}
		return this;
	}
}
