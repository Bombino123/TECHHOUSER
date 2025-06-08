using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class ClassSig : ClassOrValueTypeSig
{
	public override ElementType ElementType => ElementType.Class;

	public ClassSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
