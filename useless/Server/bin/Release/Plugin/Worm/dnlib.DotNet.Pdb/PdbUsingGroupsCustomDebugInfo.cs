using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbUsingGroupsCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<ushort> usingCounts;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.UsingGroups;

	public override Guid Guid => Guid.Empty;

	public IList<ushort> UsingCounts => usingCounts;

	public PdbUsingGroupsCustomDebugInfo()
	{
		usingCounts = new List<ushort>();
	}

	public PdbUsingGroupsCustomDebugInfo(int capacity)
	{
		usingCounts = new List<ushort>(capacity);
	}
}
