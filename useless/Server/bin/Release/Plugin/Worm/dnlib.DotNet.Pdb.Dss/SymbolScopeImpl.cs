using System;
using System.Collections.Generic;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolScopeImpl : SymbolScope
{
	private readonly ISymUnmanagedScope scope;

	private readonly SymbolMethod method;

	private readonly SymbolScope parent;

	private volatile SymbolScope[] children;

	private volatile SymbolVariable[] locals;

	private volatile SymbolNamespace[] namespaces;

	public override SymbolMethod Method => method;

	public override SymbolScope Parent => parent;

	public override int StartOffset
	{
		get
		{
			scope.GetStartOffset(out var pRetVal);
			return (int)pRetVal;
		}
	}

	public override int EndOffset
	{
		get
		{
			scope.GetEndOffset(out var pRetVal);
			return (int)pRetVal;
		}
	}

	public override IList<SymbolScope> Children
	{
		get
		{
			if (children == null)
			{
				scope.GetChildren(0u, out var pcChildren, null);
				ISymUnmanagedScope[] array = new ISymUnmanagedScope[pcChildren];
				scope.GetChildren((uint)array.Length, out pcChildren, array);
				SymbolScope[] array2 = new SymbolScope[pcChildren];
				for (uint num = 0u; num < pcChildren; num++)
				{
					array2[num] = new SymbolScopeImpl(array[num], method, this);
				}
				children = array2;
			}
			return children;
		}
	}

	public override IList<SymbolVariable> Locals
	{
		get
		{
			if (locals == null)
			{
				scope.GetLocals(0u, out var pcLocals, null);
				ISymUnmanagedVariable[] array = new ISymUnmanagedVariable[pcLocals];
				scope.GetLocals((uint)array.Length, out pcLocals, array);
				SymbolVariable[] array2 = new SymbolVariable[pcLocals];
				for (uint num = 0u; num < pcLocals; num++)
				{
					array2[num] = new SymbolVariableImpl(array[num]);
				}
				locals = array2;
			}
			return locals;
		}
	}

	public override IList<SymbolNamespace> Namespaces
	{
		get
		{
			if (namespaces == null)
			{
				scope.GetNamespaces(0u, out var pcNameSpaces, null);
				ISymUnmanagedNamespace[] array = new ISymUnmanagedNamespace[pcNameSpaces];
				scope.GetNamespaces((uint)array.Length, out pcNameSpaces, array);
				SymbolNamespace[] array2 = new SymbolNamespace[pcNameSpaces];
				for (uint num = 0u; num < pcNameSpaces; num++)
				{
					array2[num] = new SymbolNamespaceImpl(array[num]);
				}
				namespaces = array2;
			}
			return namespaces;
		}
	}

	public override IList<PdbCustomDebugInfo> CustomDebugInfos => Array2.Empty<PdbCustomDebugInfo>();

	public override PdbImportScope ImportScope => null;

	public SymbolScopeImpl(ISymUnmanagedScope scope, SymbolMethod method, SymbolScope parent)
	{
		this.scope = scope;
		this.method = method;
		this.parent = parent;
	}

	public override IList<PdbConstant> GetConstants(ModuleDef module, GenericParamContext gpContext)
	{
		if (!(scope is ISymUnmanagedScope2 symUnmanagedScope))
		{
			return Array2.Empty<PdbConstant>();
		}
		symUnmanagedScope.GetConstants(0u, out var pcConstants, null);
		if (pcConstants == 0)
		{
			return Array2.Empty<PdbConstant>();
		}
		ISymUnmanagedConstant[] array = new ISymUnmanagedConstant[pcConstants];
		symUnmanagedScope.GetConstants((uint)array.Length, out pcConstants, array);
		PdbConstant[] array2 = new PdbConstant[pcConstants];
		for (uint num = 0u; num < pcConstants; num++)
		{
			ISymUnmanagedConstant symUnmanagedConstant = array[num];
			string name = GetName(symUnmanagedConstant);
			symUnmanagedConstant.GetValue(out var pValue);
			byte[] signatureBytes = GetSignatureBytes(symUnmanagedConstant);
			TypeSig type = ((signatureBytes.Length != 0) ? SignatureReader.ReadTypeSig(module, module.CorLibTypes, signatureBytes, gpContext) : null);
			array2[num] = new PdbConstant(name, type, pValue);
		}
		return array2;
	}

	private string GetName(ISymUnmanagedConstant unc)
	{
		unc.GetName(0u, out var pcchName, null);
		char[] array = new char[pcchName];
		unc.GetName((uint)array.Length, out pcchName, array);
		if (array.Length == 0)
		{
			return string.Empty;
		}
		return new string(array, 0, array.Length - 1);
	}

	private byte[] GetSignatureBytes(ISymUnmanagedConstant unc)
	{
		uint pcSig;
		int signature = unc.GetSignature(0u, out pcSig, null);
		if (pcSig == 0 || (signature < 0 && signature != -2147467259 && signature != -2147467263))
		{
			return Array2.Empty<byte>();
		}
		byte[] array = new byte[pcSig];
		if (unc.GetSignature((uint)array.Length, out pcSig, array) != 0)
		{
			return Array2.Empty<byte>();
		}
		return array;
	}
}
