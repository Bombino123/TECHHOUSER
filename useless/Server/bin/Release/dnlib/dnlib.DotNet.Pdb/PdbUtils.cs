namespace dnlib.DotNet.Pdb;

internal static class PdbUtils
{
	public static bool IsEndInclusive(PdbFileKind pdbFileKind, Compiler compiler)
	{
		if (pdbFileKind == PdbFileKind.WindowsPDB)
		{
			return compiler == Compiler.VisualBasic;
		}
		return false;
	}
}
