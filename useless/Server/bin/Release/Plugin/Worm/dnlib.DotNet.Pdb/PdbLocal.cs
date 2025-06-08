using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbLocal : IHasCustomDebugInformation
{
	private readonly IList<PdbCustomDebugInfo> customDebugInfos = new List<PdbCustomDebugInfo>();

	public Local Local { get; set; }

	public string Name { get; set; }

	public PdbLocalAttributes Attributes { get; set; }

	public int Index => Local.Index;

	public bool IsDebuggerHidden
	{
		get
		{
			return (Attributes & PdbLocalAttributes.DebuggerHidden) != 0;
		}
		set
		{
			if (value)
			{
				Attributes |= PdbLocalAttributes.DebuggerHidden;
			}
			else
			{
				Attributes &= ~PdbLocalAttributes.DebuggerHidden;
			}
		}
	}

	public int HasCustomDebugInformationTag => 24;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos => customDebugInfos;

	public PdbLocal()
	{
	}

	public PdbLocal(Local local, string name, PdbLocalAttributes attributes)
	{
		Local = local;
		Name = name;
		Attributes = attributes;
	}
}
