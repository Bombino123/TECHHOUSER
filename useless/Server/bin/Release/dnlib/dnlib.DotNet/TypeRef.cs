using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

public abstract class TypeRef : ITypeDefOrRef, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMemberRefParent, IFullName, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter, ITokenOperand, IMemberRef, IHasCustomDebugInformation, IResolutionScope
{
	protected uint rid;

	protected ModuleDef module;

	private readonly Lock theLock = Lock.Create();

	protected IResolutionScope resolutionScope;

	protected bool resolutionScope_isInitialized;

	protected UTF8String name;

	protected UTF8String @namespace;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.TypeRef, rid);

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

	public int TypeDefOrRefTag => 1;

	public int HasCustomAttributeTag => 2;

	public int MemberRefParentTag => 1;

	public int ResolutionScopeTag => 3;

	int IGenericParameterProvider.NumberOfGenericParameters => 0;

	string IType.TypeName => Name;

	public string ReflectionName => FullNameFactory.Name(this, isReflection: true);

	string IType.Namespace => Namespace;

	public string ReflectionNamespace => FullNameFactory.Namespace(this, isReflection: true);

	public string FullName => FullNameFactory.FullName(this, isReflection: false);

	public string ReflectionFullName => FullNameFactory.FullName(this, isReflection: true);

	public string AssemblyQualifiedName => FullNameFactory.AssemblyQualifiedName(this);

	public IAssembly DefinitionAssembly => FullNameFactory.DefinitionAssembly(this);

	public IScope Scope => FullNameFactory.Scope(this);

	public ITypeDefOrRef ScopeType => this;

	public bool ContainsGenericParameter => false;

	public ModuleDef Module => module;

	public IResolutionScope ResolutionScope
	{
		get
		{
			if (!resolutionScope_isInitialized)
			{
				InitializeResolutionScope();
			}
			return resolutionScope;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				resolutionScope = value;
				resolutionScope_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
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

	public UTF8String Namespace
	{
		get
		{
			return @namespace;
		}
		set
		{
			@namespace = value;
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

	public int HasCustomDebugInformationTag => 2;

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

	public bool IsNested => DeclaringType != null;

	public bool IsValueType => Resolve()?.IsValueType ?? false;

	public bool IsPrimitive => this.IsPrimitive();

	public TypeRef DeclaringType => ResolutionScope as TypeRef;

	ITypeDefOrRef IMemberRef.DeclaringType => DeclaringType;

	bool IIsTypeOrMethod.IsType => true;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => true;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	private void InitializeResolutionScope()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!resolutionScope_isInitialized)
			{
				resolutionScope = GetResolutionScope_NoLock();
				resolutionScope_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual IResolutionScope GetResolutionScope_NoLock()
	{
		return null;
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
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
		return module.Context.Resolver.Resolve(this, sourceModule ?? module);
	}

	public TypeDef ResolveThrow()
	{
		return ResolveThrow(null);
	}

	public TypeDef ResolveThrow(ModuleDef sourceModule)
	{
		TypeDef typeDef = Resolve(sourceModule);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException($"Could not resolve type: {this} ({DefinitionAssembly})");
	}

	internal static TypeRef GetNonNestedTypeRef(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			return null;
		}
		for (int i = 0; i < 1000; i++)
		{
			if (!(typeRef.ResolutionScope is TypeRef typeRef2))
			{
				return typeRef;
			}
			typeRef = typeRef2;
		}
		return null;
	}

	public override string ToString()
	{
		return FullName;
	}
}
