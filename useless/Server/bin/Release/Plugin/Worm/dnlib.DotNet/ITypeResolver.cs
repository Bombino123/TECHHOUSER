using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ITypeResolver
{
	TypeDef Resolve(TypeRef typeRef, ModuleDef sourceModule);
}
