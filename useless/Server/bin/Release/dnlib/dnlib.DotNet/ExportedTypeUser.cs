namespace dnlib.DotNet;

public class ExportedTypeUser : ExportedType
{
	public ExportedTypeUser(ModuleDef module)
	{
		base.module = module;
	}

	public ExportedTypeUser(ModuleDef module, uint typeDefId, UTF8String typeNamespace, UTF8String typeName, TypeAttributes flags, IImplementation implementation)
	{
		base.module = module;
		base.typeDefId = typeDefId;
		base.typeName = typeName;
		base.typeNamespace = typeNamespace;
		attributes = (int)flags;
		base.implementation = implementation;
		implementation_isInitialized = true;
	}
}
