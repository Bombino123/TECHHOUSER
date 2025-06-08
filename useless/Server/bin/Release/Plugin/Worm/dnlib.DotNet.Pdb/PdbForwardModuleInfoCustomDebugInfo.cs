using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbForwardModuleInfoCustomDebugInfo : PdbCustomDebugInfo
{
	private IMethodDefOrRef method;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.ForwardModuleInfo;

	public override Guid Guid => Guid.Empty;

	public IMethodDefOrRef Method
	{
		get
		{
			return method;
		}
		set
		{
			method = value;
		}
	}

	public PdbForwardModuleInfoCustomDebugInfo()
	{
	}

	public PdbForwardModuleInfoCustomDebugInfo(IMethodDefOrRef method)
	{
		this.method = method;
	}
}
