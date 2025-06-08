namespace dnlib.DotNet;

public interface IAssemblyResolver
{
	AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule);
}
