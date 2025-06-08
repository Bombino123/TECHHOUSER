namespace dnlib.DotNet;

internal sealed class CAAssemblyRefFinder : IAssemblyRefFinder
{
	private readonly ModuleDef module;

	public CAAssemblyRefFinder(ModuleDef module)
	{
		this.module = module;
	}

	public AssemblyRef FindAssemblyRef(TypeRef nonNestedTypeRef)
	{
		AssemblyDef assembly = module.Assembly;
		if (assembly != null)
		{
			if (assembly.Find(nonNestedTypeRef) is TypeDefMD typeDefMD && typeDefMD.ReaderModule == module)
			{
				return module.UpdateRowId(new AssemblyRefUser(assembly));
			}
		}
		else if (module.Find(nonNestedTypeRef) != null)
		{
			return AssemblyRef.CurrentAssembly;
		}
		AssemblyDef assemblyDef = module.Context.AssemblyResolver.Resolve(module.CorLibTypes.AssemblyRef, module);
		if (assemblyDef != null && assemblyDef.Find(nonNestedTypeRef) != null)
		{
			return module.CorLibTypes.AssemblyRef;
		}
		if (assembly != null)
		{
			return module.UpdateRowId(new AssemblyRefUser(assembly));
		}
		return AssemblyRef.CurrentAssembly;
	}
}
