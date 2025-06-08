using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class ClassOrValueTypeSig : TypeDefOrRefSig
{
	protected ClassOrValueTypeSig(ITypeDefOrRef typeDefOrRef)
		: base(typeDefOrRef)
	{
	}
}
