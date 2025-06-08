namespace dnlib.DotNet;

public sealed class ValueTypeSig : ClassOrValueTypeSig
{
	public override ElementType ElementType => ElementType.ValueType;

	public ValueTypeSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
