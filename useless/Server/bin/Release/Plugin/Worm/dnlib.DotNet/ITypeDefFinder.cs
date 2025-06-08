using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ITypeDefFinder
{
	TypeDef Find(string fullName, bool isReflectionName);

	TypeDef Find(TypeRef typeRef);
}
