namespace dnlib.DotNet.Pdb;

public abstract class PdbImport
{
	public abstract PdbImportDefinitionKind Kind { get; }

	internal abstract void PreventNewClasses();
}
