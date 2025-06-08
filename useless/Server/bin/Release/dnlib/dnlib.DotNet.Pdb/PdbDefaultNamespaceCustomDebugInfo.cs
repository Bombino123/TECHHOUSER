using System;

namespace dnlib.DotNet.Pdb;

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
