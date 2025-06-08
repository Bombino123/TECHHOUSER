using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PdbTupleElementNamesCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<PdbTupleElementNames> names;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.TupleElementNames;

	public override Guid Guid => Guid.Empty;

	public IList<PdbTupleElementNames> Names => names;

	public PdbTupleElementNamesCustomDebugInfo()
	{
		names = new List<PdbTupleElementNames>();
	}

	public PdbTupleElementNamesCustomDebugInfo(int capacity)
	{
		names = new List<PdbTupleElementNames>(capacity);
	}
}
