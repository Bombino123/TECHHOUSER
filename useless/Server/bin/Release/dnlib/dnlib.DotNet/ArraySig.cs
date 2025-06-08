using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class ArraySig : ArraySigBase
{
	private uint rank;

	private readonly IList<uint> sizes;

	private readonly IList<int> lowerBounds;

	public override ElementType ElementType => ElementType.Array;

	public override uint Rank
	{
		get
		{
			return rank;
		}
		set
		{
			rank = value;
		}
	}

	public IList<uint> Sizes => sizes;

	public IList<int> LowerBounds => lowerBounds;

	public ArraySig(TypeSig arrayType)
		: base(arrayType)
	{
		sizes = new List<uint>();
		lowerBounds = new List<int>();
	}

	public ArraySig(TypeSig arrayType, uint rank)
		: base(arrayType)
	{
		this.rank = rank;
		sizes = new List<uint>();
		lowerBounds = new List<int>();
	}

	public ArraySig(TypeSig arrayType, int rank)
		: this(arrayType, (uint)rank)
	{
	}

	public ArraySig(TypeSig arrayType, uint rank, IEnumerable<uint> sizes, IEnumerable<int> lowerBounds)
		: base(arrayType)
	{
		this.rank = rank;
		this.sizes = new List<uint>(sizes);
		this.lowerBounds = new List<int>(lowerBounds);
	}

	public ArraySig(TypeSig arrayType, int rank, IEnumerable<uint> sizes, IEnumerable<int> lowerBounds)
		: this(arrayType, (uint)rank, sizes, lowerBounds)
	{
	}

	internal ArraySig(TypeSig arrayType, uint rank, IList<uint> sizes, IList<int> lowerBounds)
		: base(arrayType)
	{
		this.rank = rank;
		this.sizes = sizes;
		this.lowerBounds = lowerBounds;
	}

	public override IList<uint> GetSizes()
	{
		return sizes;
	}

	public override IList<int> GetLowerBounds()
	{
		return lowerBounds;
	}
}
