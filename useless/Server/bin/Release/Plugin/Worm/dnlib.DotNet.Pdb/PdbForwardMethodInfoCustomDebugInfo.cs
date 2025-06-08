using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbForwardMethodInfoCustomDebugInfo : PdbCustomDebugInfo
{
	private IMethodDefOrRef method;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.ForwardMethodInfo;

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

	public PdbForwardMethodInfoCustomDebugInfo()
	{
	}

	public PdbForwardMethodInfoCustomDebugInfo(IMethodDefOrRef method)
	{
		this.method = method;
	}
}
