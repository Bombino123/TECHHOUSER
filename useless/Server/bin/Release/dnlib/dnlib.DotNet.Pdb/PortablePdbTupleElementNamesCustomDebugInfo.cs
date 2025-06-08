using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PortablePdbTupleElementNamesCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<string> names;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.TupleElementNames_PortablePdb;

	public override Guid Guid => CustomDebugInfoGuids.TupleElementNames;

	public IList<string> Names => names;

	public PortablePdbTupleElementNamesCustomDebugInfo()
	{
		names = new List<string>();
	}

	public PortablePdbTupleElementNamesCustomDebugInfo(int capacity)
	{
		names = new List<string>(capacity);
	}
}
