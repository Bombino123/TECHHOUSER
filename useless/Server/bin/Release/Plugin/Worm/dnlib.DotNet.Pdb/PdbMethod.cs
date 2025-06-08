using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbMethod
{
	public PdbScope Scope { get; set; }
}
