namespace dnlib.DotNet;

public abstract class ClassOrValueTypeSig : TypeDefOrRefSig
{
	protected ClassOrValueTypeSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
