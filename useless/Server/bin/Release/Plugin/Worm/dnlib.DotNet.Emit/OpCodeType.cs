using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public enum OpCodeType : byte
{
	Annotation,
	Macro,
	Nternal,
	Objmodel,
	Prefix,
	Primitive,
	Experimental
}
