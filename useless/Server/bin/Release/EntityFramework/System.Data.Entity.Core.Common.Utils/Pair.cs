using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal class Pair<TFirst, TSecond> : InternalBase
{
	internal class PairComparer : IEqualityComparer<Pair<TFirst, TSecond>>
	{
		internal static readonly PairComparer Instance = new PairComparer();

		private static readonly EqualityComparer<TFirst> _firstComparer = EqualityComparer<TFirst>.Default;

		private static readonly EqualityComparer<TSecond> _secondComparer = EqualityComparer<TSecond>.Default;

		private PairComparer()
		{
		}

		public bool Equals(Pair<TFirst, TSecond> x, Pair<TFirst, TSecond> y)
		{
			if (_firstComparer.Equals(x.First, y.First))
			{
				return _secondComparer.Equals(x.Second, y.Second);
			}
			return false;
		}

		public int GetHashCode(Pair<TFirst, TSecond> source)
		{
			return source.GetHashCode();
		}
	}

	private readonly TFirst first;

	private readonly TSecond second;

	internal TFirst First => first;

	internal TSecond Second => second;

	internal Pair(TFirst first, TSecond second)
	{
		this.first = first;
		this.second = second;
	}

	public override int GetHashCode()
	{
		return (first.GetHashCode() << 5) ^ second.GetHashCode();
	}

	public bool Equals(Pair<TFirst, TSecond> other)
	{
		if (first.Equals(other.first))
		{
			return second.Equals(other.second);
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is Pair<TFirst, TSecond> other2)
		{
			return Equals(other2);
		}
		return false;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("<");
		builder.Append(first);
		builder.Append(", " + second);
		builder.Append(">");
	}
}
