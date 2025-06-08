using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class ExportedType : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IImplementation, IFullName, IHasCustomDebugInformation, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected ModuleDef module;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected int attributes;

	protected uint typeDefId;

	protected UTF8String typeName;

	protected UTF8String typeNamespace;

	protected IImplementation implementation;

	protected bool implementation_isInitialized;

	private const int MAX_LOOP_ITERS = 50;

	public MDToken MDToken => new MDToken(Table.ExportedType, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public int HasCustomAttributeTag => 17;

	public int ImplementationTag => 2;

	public CustomAttributeCollection CustomAttributes
	{
		get
		{
			if (customAttributes == null)
			{
				InitializeCustomAttributes();
			}
			return customAttributes;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public int HasCustomDebugInformationTag => 17;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				InitializeCustomDebugInfos();
			}
			return customDebugInfos;
		}
	}

	public bool IsValueType => Resolve()?.IsValueType ?? false;

	public bool IsPrimitive => this.IsPrimitive();

	string IType.TypeName => TypeName;

	public UTF8String Name
	{
		get
		{
			return typeName;
		}
		set
		{
			typeName = value;
		}
	}

	public string ReflectionName => FullNameFactory.Name(this, isReflection: true);

	public string Namespace => TypeNamespace;

	public string ReflectionNamespace => FullNameFactory.Namespace(this, isReflection: true);

	public string FullName => FullNameFactory.FullName(this, isReflection: false);

	public string ReflectionFullName => FullNameFactory.FullName(this, isReflection: true);

	public string AssemblyQualifiedName => FullNameFactory.AssemblyQualifiedName(this);

	public IAssembly DefinitionAssembly => FullNameFactory.DefinitionAssembly(this);

	public IScope Scope => FullNameFactory.Scope(this);

	public ITypeDefOrRef ScopeType => FullNameFactory.ScopeType(this);

	public bool ContainsGenericParameter => false;

	public ModuleDef Module => module;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IIsTypeOrMethod.IsType => true;

	int IGenericParameterProvider.NumberOfGenericParameters => 0;

	public TypeAttributes Attributes
	{
		get
		{
			return (TypeAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public uint TypeDefId
	{
		get
		{
			return typeDefId;
		}
		set
		{
			typeDefId = value;
		}
	}

	public UTF8String TypeName
	{
		get
		{
			return typeName;
		}
		set
		{
			typeName = value;
		}
	}

	public UTF8String TypeNamespace
	{
		get
		{
			return typeNamespace;
		}
		set
		{
			typeNamespace = value;
		}
	}

	public IImplementation Implementation
	{
		get
		{
			if (!implementation_isInitialized)
			{
				InitializeImplementation();
			}
			return implementation;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				implementation = value;
				implementation_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool IsNested => DeclaringType != null;

	public ExportedType DeclaringType
	{
		get
		{
			if (!implementation_isInitialized)
			{
				InitializeImplementation();
			}
			return implementation as ExportedType;
		}
	}

	public TypeAttributes Visibility
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 7u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.VisibilityMask, value & TypeAttributes.VisibilityMask);
		}
	}

	public bool IsNotPublic => (attributes & 7) == 0;

	public bool IsPublic => (attributes & 7) == 1;

	public bool IsNestedPublic => (attributes & 7) == 2;

	public bool IsNestedPrivate => (attributes & 7) == 3;

	public bool IsNestedFamily => (attributes & 7) == 4;

	public bool IsNestedAssembly => (attributes & 7) == 5;

	public bool IsNestedFamilyAndAssembly => (attributes & 7) == 6;

	public bool IsNestedFamilyOrAssembly => (attributes & 7) == 7;

	public TypeAttributes Layout
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 0x18u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.LayoutMask, value & TypeAttributes.LayoutMask);
		}
	}

	public bool IsAutoLayout => (attributes & 0x18) == 0;

	public bool IsSequentialLayout => (attributes & 0x18) == 8;

	public bool IsExplicitLayout => (attributes & 0x18) == 16;

	public bool IsInterface
	{
		get
		{
			return (attributes & 0x20) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.ClassSemanticsMask);
		}
	}

	public bool IsClass
	{
		get
		{
			return (attributes & 0x20) == 0;
		}
		set
		{
			ModifyAttributes(!value, TypeAttributes.ClassSemanticsMask);
		}
	}

	public bool IsAbstract
	{
		get
		{
			return (attributes & 0x80) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Abstract);
		}
	}

	public bool IsSealed
	{
		get
		{
			return (attributes & 0x100) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Sealed);
		}
	}

	public bool IsSpecialName
	{
		get
		{
			return (attributes & 0x400) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.SpecialName);
		}
	}

	public bool IsImport
	{
		get
		{
			return (attributes & 0x1000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Import);
		}
	}

	public bool IsSerializable
	{
		get
		{
			return (attributes & 0x2000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Serializable);
		}
	}

	public bool IsWindowsRuntime
	{
		get
		{
			return (attributes & 0x4000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.WindowsRuntime);
		}
	}

	public TypeAttributes StringFormat
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 0x30000u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.StringFormatMask, value & TypeAttributes.StringFormatMask);
		}
	}

	public bool IsAnsiClass => (attributes & 0x30000) == 0;

	public bool IsUnicodeClass => (attributes & 0x30000) == 65536;

	public bool IsAutoClass => (attributes & 0x30000) == 131072;

	public bool IsCustomFormatClass => (attributes & 0x30000) == 196608;

	public bool IsBeforeFieldInit
	{
		get
		{
			return (attributes & 0x100000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.BeforeFieldInit);
		}
	}

	public bool IsForwarder
	{
		get
		{
			return (attributes & 0x200000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Forwarder);
		}
	}

	public bool IsRuntimeSpecialName
	{
		get
		{
			return (attributes & 0x800) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.RTSpecialName);
		}
	}

	public bool HasSecurity
	{
		get
		{
			return (attributes & 0x40000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.HasSecurity);
		}
	}

	public bool MovedToAnotherAssembly
	{
		get
		{
			ExportedType exportedType = this;
			for (int i = 0; i < 50; i++)
			{
				IImplementation implementation = exportedType.Implementation;
				if (implementation is AssemblyRef)
				{
					return exportedType.IsForwarder;
				}
				exportedType = implementation as ExportedType;
				if (exportedType == null)
				{
					break;
				}
			}
			return false;
		}
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void InitializeImplementation()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!implementation_isInitialized)
			{
				implementation = GetImplementation_NoLock();
				implementation_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual IImplementation GetImplementation_NoLock()
	{
		return null;
	}

	private void ModifyAttributes(TypeAttributes andMask, TypeAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, TypeAttributes flags)
	{
		if (set)
		{
			attributes |= (int)flags;
		}
		else
		{
			attributes &= (int)(~flags);
		}
	}

	public TypeDef Resolve()
	{
		return Resolve(null);
	}

	public TypeDef Resolve(ModuleDef sourceModule)
	{
		if (module == null)
		{
			return null;
		}
		return Resolve(sourceModule, this);
	}

	private static TypeDef Resolve(ModuleDef sourceModule, ExportedType et)
	{
		for (int i = 0; i < 50; i++)
		{
			if (et == null)
			{
				break;
			}
			if (et.module == null)
			{
				break;
			}
			AssemblyDef assemblyDef = et.module.Context.AssemblyResolver.Resolve(et.DefinitionAssembly, sourceModule ?? et.module);
			if (assemblyDef == null)
			{
				break;
			}
			TypeDef typeDef = assemblyDef.Find(et.FullName, isReflectionName: false);
			if (typeDef != null)
			{
				return typeDef;
			}
			et = FindExportedType(assemblyDef, et);
		}
		return null;
	}

	private static ExportedType FindExportedType(AssemblyDef asm, ExportedType et)
	{
		IList<ModuleDef> modules = asm.Modules;
		int count = modules.Count;
		for (int i = 0; i < count; i++)
		{
			IList<ExportedType> exportedTypes = modules[i].ExportedTypes;
			int count2 = exportedTypes.Count;
			for (int j = 0; j < count2; j++)
			{
				ExportedType exportedType = exportedTypes[j];
				if (new SigComparer(SigComparerOptions.DontCompareTypeScope).Equals(et, exportedType))
				{
					return exportedType;
				}
			}
		}
		return null;
	}

	public TypeDef ResolveThrow()
	{
		TypeDef typeDef = Resolve();
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException($"Could not resolve type: {this} ({DefinitionAssembly})");
	}

	public TypeRef ToTypeRef()
	{
		TypeRef typeRef = null;
		TypeRef typeRef2 = null;
		ModuleDef moduleDef = module;
		IImplementation implementation = this;
		for (int i = 0; i < 50; i++)
		{
			if (implementation == null)
			{
				break;
			}
			if (implementation is ExportedType exportedType)
			{
				TypeRefUser typeRefUser = moduleDef.UpdateRowId(new TypeRefUser(moduleDef, exportedType.TypeNamespace, exportedType.TypeName));
				if (typeRef == null)
				{
					typeRef = typeRefUser;
				}
				if (typeRef2 != null)
				{
					typeRef2.ResolutionScope = typeRefUser;
				}
				typeRef2 = typeRefUser;
				implementation = exportedType.Implementation;
				continue;
			}
			if (implementation is AssemblyRef resolutionScope)
			{
				typeRef2.ResolutionScope = resolutionScope;
				return typeRef;
			}
			if (!(implementation is FileDef file))
			{
				break;
			}
			typeRef2.ResolutionScope = FindModule(moduleDef, file);
			return typeRef;
		}
		return typeRef;
	}

	private static ModuleDef FindModule(ModuleDef module, FileDef file)
	{
		if (module == null || file == null)
		{
			return null;
		}
		if (UTF8String.CaseInsensitiveEquals(module.Name, file.Name))
		{
			return module;
		}
		return module.Assembly?.FindModule(file.Name);
	}

	public override string ToString()
	{
		return FullName;
	}
}
