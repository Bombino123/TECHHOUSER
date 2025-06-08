using System;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolVariableImpl : SymbolVariable
{
	private readonly ISymUnmanagedVariable variable;

	public override int Index
	{
		get
		{
			variable.GetAddressField1(out var pRetVal);
			return (int)pRetVal;
		}
	}

	public override PdbLocalAttributes Attributes
	{
		get
		{
			variable.GetAttributes(out var pRetVal);
			if ((pRetVal & (true ? 1u : 0u)) != 0)
			{
				return PdbLocalAttributes.DebuggerHidden;
			}
			return PdbLocalAttributes.None;
		}
	}

	public override string Name
	{
		get
		{
			variable.GetName(0u, out var pcchName, null);
			char[] array = new char[pcchName];
			variable.GetName((uint)array.Length, out pcchName, array);
			if (array.Length == 0)
			{
				return string.Empty;
			}
			return new string(array, 0, array.Length - 1);
		}
	}

	public override PdbCustomDebugInfo[] CustomDebugInfos => Array2.Empty<PdbCustomDebugInfo>();

	public SymbolVariableImpl(ISymUnmanagedVariable variable)
	{
		this.variable = variable;
	}
}
