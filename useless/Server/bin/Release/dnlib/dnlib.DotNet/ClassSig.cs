namespace dnlib.DotNet;

public sealed class ClassSig : ClassOrValueTypeSig
{
	public override ElementType ElementType => ElementType.Class;

	public ClassSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
