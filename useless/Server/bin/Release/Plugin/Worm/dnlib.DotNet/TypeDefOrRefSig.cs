using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class TypeDefOrRefSig : LeafSig
{
	private readonly ITypeDefOrRef typeDefOrRef;

	public ITypeDefOrRef TypeDefOrRef => typeDefOrRef;

	public bool IsTypeRef => TypeRef != null;

	public bool IsTypeDef => TypeDef != null;

	public bool IsTypeSpec => TypeSpec != null;

	public TypeRef TypeRef => typeDefOrRef as TypeRef;

	public TypeDef TypeDef => typeDefOrRef as TypeDef;

	public TypeSpec TypeSpec => typeDefOrRef as TypeSpec;

	protected TypeDefOrRefSig(ITypeDefOrRef typeDefOrRef)
	{
		this.typeDefOrRef = typeDefOrRef;
	}
}
