using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class MemberRef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IMethodDefOrRef, ICustomAttributeType, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule, IField, IContainsGenericParameter, IHasCustomDebugInformation
{
	protected uint rid;

	protected ModuleDef module;

	protected IMemberRefParent @class;

	protected UTF8String name;

	protected CallingConventionSig signature;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.MemberRef, rid);

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

	public int HasCustomAttributeTag => 6;

	public int MethodDefOrRefTag => 1;

	public int CustomAttributeTypeTag => 3;

	public IMemberRefParent Class
	{
		get
		{
			return @class;
		}
		set
		{
			@class = value;
		}
	}

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public CallingConventionSig Signature
	{
		get
		{
			return signature;
		}
		set
		{
			signature = value;
		}
	}

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

	public int HasCustomDebugInformationTag => 6;

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

	public ITypeDefOrRef DeclaringType
	{
		get
		{
			IMemberRefParent memberRefParent = @class;
			if (memberRefParent is ITypeDefOrRef result)
			{
				return result;
			}
			if (memberRefParent is MethodDef methodDef)
			{
				return methodDef.DeclaringType;
			}
			if (memberRefParent is ModuleRef mr)
			{
				TypeRefUser globalTypeRef = GetGlobalTypeRef(mr);
				if (module != null)
				{
					return module.UpdateRowId(globalTypeRef);
				}
				return globalTypeRef;
			}
			return null;
		}
	}

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => IsMethodRef;

	bool IMemberRef.IsField => IsFieldRef;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => true;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public bool IsMethodRef => MethodSig != null;

	public bool IsFieldRef => FieldSig != null;

	public MethodSig MethodSig
	{
		get
		{
			return signature as MethodSig;
		}
		set
		{
			signature = value;
		}
	}

	public FieldSig FieldSig
	{
		get
		{
			return signature as FieldSig;
		}
		set
		{
			signature = value;
		}
	}

	public ModuleDef Module => module;

	public bool HasThis => MethodSig?.HasThis ?? false;

	public bool ExplicitThis => MethodSig?.ExplicitThis ?? false;

	public CallingConvention CallingConvention
	{
		get
		{
			MethodSig methodSig = MethodSig;
			if (methodSig != null)
			{
				return methodSig.CallingConvention & CallingConvention.Mask;
			}
			return CallingConvention.Default;
		}
	}

	public TypeSig ReturnType
	{
		get
		{
			return MethodSig?.RetType;
		}
		set
		{
			MethodSig methodSig = MethodSig;
			if (methodSig != null)
			{
				methodSig.RetType = value;
			}
		}
	}

	int IGenericParameterProvider.NumberOfGenericParameters => (int)(MethodSig?.GenParamCount ?? 0);

	public string FullName
	{
		get
		{
			IMemberRefParent memberRefParent = @class;
			IList<TypeSig> typeGenArgs = null;
			if (memberRefParent is TypeSpec && ((TypeSpec)memberRefParent).TypeSig is GenericInstSig genericInstSig)
			{
				typeGenArgs = genericInstSig.GenericArguments;
			}
			MethodSig methodSig = MethodSig;
			if (methodSig != null)
			{
				return FullNameFactory.MethodFullName(GetDeclaringTypeFullName(memberRefParent), name, methodSig, typeGenArgs);
			}
			FieldSig fieldSig = FieldSig;
			if (fieldSig != null)
			{
				return FullNameFactory.FieldFullName(GetDeclaringTypeFullName(memberRefParent), name, fieldSig, typeGenArgs);
			}
			return string.Empty;
		}
	}

	bool IContainsGenericParameter.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private TypeRefUser GetGlobalTypeRef(ModuleRef mr)
	{
		if (module == null)
		{
			return CreateDefaultGlobalTypeRef(mr);
		}
		TypeDef globalType = module.GlobalType;
		if (globalType != null && default(SigComparer).Equals(module, mr))
		{
			return new TypeRefUser(module, globalType.Namespace, globalType.Name, mr);
		}
		AssemblyDef assembly = module.Assembly;
		if (assembly == null)
		{
			return CreateDefaultGlobalTypeRef(mr);
		}
		ModuleDef moduleDef = assembly.FindModule(mr.Name);
		if (moduleDef == null)
		{
			return CreateDefaultGlobalTypeRef(mr);
		}
		globalType = moduleDef.GlobalType;
		if (globalType == null)
		{
			return CreateDefaultGlobalTypeRef(mr);
		}
		return new TypeRefUser(module, globalType.Namespace, globalType.Name, mr);
	}

	private TypeRefUser CreateDefaultGlobalTypeRef(ModuleRef mr)
	{
		TypeRefUser typeRefUser = new TypeRefUser(module, string.Empty, "<Module>", mr);
		if (module != null)
		{
			module.UpdateRowId(typeRefUser);
		}
		return typeRefUser;
	}

	public string GetDeclaringTypeFullName()
	{
		return GetDeclaringTypeFullName(@class);
	}

	private string GetDeclaringTypeFullName(IMemberRefParent parent)
	{
		if (parent == null)
		{
			return null;
		}
		if (parent is ITypeDefOrRef)
		{
			return ((ITypeDefOrRef)parent).FullName;
		}
		if (parent is ModuleRef)
		{
			return "[module:" + ((ModuleRef)parent).ToString() + "]<Module>";
		}
		if (parent is MethodDef)
		{
			return ((MethodDef)parent).DeclaringType?.FullName;
		}
		return null;
	}

	internal string GetDeclaringTypeName()
	{
		return GetDeclaringTypeName(@class);
	}

	private string GetDeclaringTypeName(IMemberRefParent parent)
	{
		if (parent == null)
		{
			return null;
		}
		if (parent is ITypeDefOrRef)
		{
			return ((ITypeDefOrRef)parent).Name;
		}
		if (parent is ModuleRef)
		{
			return "<Module>";
		}
		if (parent is MethodDef)
		{
			return ((MethodDef)parent).DeclaringType?.Name;
		}
		return null;
	}

	public IMemberForwarded Resolve()
	{
		if (module == null)
		{
			return null;
		}
		return module.Context.Resolver.Resolve(this);
	}

	public IMemberForwarded ResolveThrow()
	{
		IMemberForwarded memberForwarded = Resolve();
		if (memberForwarded != null)
		{
			return memberForwarded;
		}
		throw new MemberRefResolveException($"Could not resolve method/field: {this} ({this.GetDefinitionAssembly()})");
	}

	public FieldDef ResolveField()
	{
		return Resolve() as FieldDef;
	}

	public FieldDef ResolveFieldThrow()
	{
		FieldDef fieldDef = ResolveField();
		if (fieldDef != null)
		{
			return fieldDef;
		}
		throw new MemberRefResolveException($"Could not resolve field: {this} ({this.GetDefinitionAssembly()})");
	}

	public MethodDef ResolveMethod()
	{
		return Resolve() as MethodDef;
	}

	public MethodDef ResolveMethodThrow()
	{
		MethodDef methodDef = ResolveMethod();
		if (methodDef != null)
		{
			return methodDef;
		}
		throw new MemberRefResolveException($"Could not resolve method: {this} ({this.GetDefinitionAssembly()})");
	}

	protected static GenericParamContext GetSignatureGenericParamContext(GenericParamContext gpContext, IMemberRefParent @class)
	{
		TypeDef type = null;
		MethodDef method = gpContext.Method;
		if (@class is TypeSpec { TypeSig: GenericInstSig typeSig })
		{
			type = typeSig.GenericType.ToTypeDefOrRef().ResolveTypeDef();
		}
		return new GenericParamContext(type, method);
	}

	public override string ToString()
	{
		return FullName;
	}
}
