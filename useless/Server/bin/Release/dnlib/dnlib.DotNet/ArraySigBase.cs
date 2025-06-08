using System.Collections.Generic;

namespace dnlib.DotNet;

public abstract class ArraySigBase : NonLeafSig
{
	public bool IsMultiDimensional => ElementType == ElementType.Array;

	public bool IsSingleDimensional => ElementType == ElementType.SZArray;

	public abstract uint Rank { get; set; }

	protected ArraySigBase(TypeSig arrayType)
		: base(arrayType)
	{
	}

	public abstract IList<uint> GetSizes();

	public abstract IList<int> GetLowerBounds();
}
