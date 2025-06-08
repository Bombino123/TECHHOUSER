using System;
using System.Runtime.InteropServices;
using dnlib.PE;
using dnlib.Utils;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ModuleDefUser : ModuleDef
{
	public ModuleDefUser()
		: this(null, null)
	{
	}

	public ModuleDefUser(UTF8String name)
		: this(name, Guid.NewGuid())
	{
	}

	public ModuleDefUser(UTF8String name, Guid? mvid)
		: this(name, mvid, null)
	{
	}

	public ModuleDefUser(UTF8String name, Guid? mvid, AssemblyRef corLibAssemblyRef)
	{
		base.Kind = ModuleKind.Windows;
		base.Characteristics = Characteristics.ExecutableImage | Characteristics.Bit32Machine;
		base.DllCharacteristics = DllCharacteristics.DynamicBase | DllCharacteristics.NxCompat | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware;
		base.RuntimeVersion = "v2.0.50727";
		base.Machine = Machine.I386;
		cor20HeaderFlags = 1;
		base.Cor20HeaderRuntimeVersion = 131077u;
		base.TablesHeaderVersion = (ushort)512;
		types = new LazyList<TypeDef>(this);
		exportedTypes = new LazyList<ExportedType>();
		resources = new ResourceCollection();
		corLibTypes = new CorLibTypes(this, corLibAssemblyRef);
		types = new LazyList<TypeDef>(this);
		base.name = name;
		base.mvid = mvid;
		types.Add(CreateModuleType());
		UpdateRowId(this);
	}

	private TypeDef CreateModuleType()
	{
		TypeDefUser typeDefUser = UpdateRowId(new TypeDefUser(UTF8String.Empty, "<Module>", null));
		typeDefUser.Attributes = TypeAttributes.NotPublic;
		return typeDefUser;
	}
}
