using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbDefaultNamespaceCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.DefaultNamespace;

	public override Guid Guid => CustomDebugInfoGuids.DefaultNamespace;

	public string Namespace { get; set; }

	public PdbDefaultNamespaceCustomDebugInfo()
	{
	}

	public PdbDefaultNamespaceCustomDebugInfo(string defaultNamespace)
	{
		Namespace = defaultNamespace;
	}
}
