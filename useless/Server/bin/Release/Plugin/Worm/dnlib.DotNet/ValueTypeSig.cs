using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class ValueTypeSig : ClassOrValueTypeSig
{
	public override ElementType ElementType => ElementType.ValueType;

	public ValueTypeSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
