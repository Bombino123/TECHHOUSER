namespace dnlib.DotNet;

public interface ITypeDefFinder
{
	TypeDef Find(string fullName, bool isReflectionName);

	TypeDef Find(TypeRef typeRef);
}
