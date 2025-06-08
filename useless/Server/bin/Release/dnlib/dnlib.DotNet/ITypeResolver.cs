namespace dnlib.DotNet;

public interface ITypeResolver
{
	TypeDef Resolve(TypeRef typeRef, ModuleDef sourceModule);
}
