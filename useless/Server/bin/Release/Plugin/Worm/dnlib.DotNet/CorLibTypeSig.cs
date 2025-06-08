using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class CorLibTypeSig : TypeDefOrRefSig
{
	private readonly ElementType elementType;

	public override ElementType ElementType => elementType;

	public CorLibTypeSig(ITypeDefOrRef corType, ElementType elementType)
		: base(corType)
	{
		if (!(corType is TypeRef) && !(corType is TypeDef))
		{
			throw new ArgumentException("corType must be a TypeDef or a TypeRef. null and TypeSpec are invalid inputs.");
		}
		this.elementType = elementType;
	}
}
