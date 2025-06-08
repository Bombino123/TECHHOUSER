using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public abstract class PdbImport
{
	public abstract PdbImportDefinitionKind Kind { get; }

	internal abstract void PreventNewClasses();
}
