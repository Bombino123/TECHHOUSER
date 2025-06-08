using System;
using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class SZArraySig : ArraySigBase
{
	public override ElementType ElementType => ElementType.SZArray;

	public override uint Rank
	{
		get
		{
			return 1u;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public SZArraySig(TypeSig nextSig)
		: base(nextSig)
	{
	}

	public override IList<uint> GetSizes()
	{
		return Array2.Empty<uint>();
	}

	public override IList<int> GetLowerBounds()
	{
		return Array2.Empty<int>();
	}
}
