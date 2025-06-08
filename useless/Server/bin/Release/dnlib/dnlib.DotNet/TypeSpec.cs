using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

public abstract class TypeSpec : ITypeDefOrRef, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMemberRefParent, IFullName, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter, ITokenOperand, IMemberRef, IHasCustomDebugInformation
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected TypeSig typeSig;

	protected byte[] extraData;

	protected bool typeSigAndExtraData_isInitialized;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.TypeSpec, rid);

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

	public int TypeDefOrRefTag => 2;

	public int HasCustomAttributeTag => 13;

	public int MemberRefParentTag => 4;

	int IGenericParameterProvider.NumberOfGenericParameters => ((IGenericParameterProvider)TypeSig)?.NumberOfGenericParameters ?? 0;

	UTF8String IFullName.Name
	{
		get
		{
			ITypeDefOrRef scopeType = ScopeType;
			if (scopeType != null)
			{
				return scopeType.Name;
			}
			return UTF8String.Empty;
		}
		set
		{
			ITypeDefOrRef scopeType = ScopeType;
			if (scopeType != null)
			{
				scopeType.Name = value;
			}
		}
	}

	ITypeDefOrRef IMemberRef.DeclaringType
	{
		get
		{
			TypeSig typeSig = TypeSig.RemovePinnedAndModifiers();
			if (typeSig is GenericInstSig genericInstSig)
			{
				typeSig = genericInstSig.GenericType;
			}
			if (typeSig is TypeDefOrRefSig typeDefOrRefSig)
			{
				if (typeDefOrRefSig.IsTypeDef || typeDefOrRefSig.IsTypeRef)
				{
					return typeDefOrRefSig.TypeDefOrRef.DeclaringType;
				}
				return null;
			}
			return null;
		}
	}

	bool IIsTypeOrMethod.IsType => true;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => true;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public bool IsValueType => TypeSig?.IsValueType ?? false;

	public bool IsPrimitive => TypeSig?.IsPrimitive ?? false;

	public string TypeName => FullNameFactory.Name(this, isReflection: false);

	public string ReflectionName => FullNameFactory.Name(this, isReflection: true);

	string IType.Namespace => FullNameFactory.Namespace(this, isReflection: false);

	public string ReflectionNamespace => FullNameFactory.Namespace(this, isReflection: true);

	public string FullName => FullNameFactory.FullName(this, isReflection: false);

	public string ReflectionFullName => FullNameFactory.FullName(this, isReflection: true);

	public string AssemblyQualifiedName => FullNameFactory.AssemblyQualifiedName(this);

	public IAssembly DefinitionAssembly => FullNameFactory.DefinitionAssembly(this);

	public IScope Scope => FullNameFactory.Scope(this);

	public ITypeDefOrRef ScopeType => FullNameFactory.ScopeType(this);

	public bool ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	public ModuleDef Module => FullNameFactory.OwnerModule(this);

	public TypeSig TypeSig
	{
		get
		{
			if (!typeSigAndExtraData_isInitialized)
			{
				InitializeTypeSigAndExtraData();
			}
			return typeSig;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				typeSig = value;
				if (!typeSigAndExtraData_isInitialized)
				{
					GetTypeSigAndExtraData_NoLock(out extraData);
				}
				typeSigAndExtraData_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public byte[] ExtraData
	{
		get
		{
			if (!typeSigAndExtraData_isInitialized)
			{
				InitializeTypeSigAndExtraData();
			}
			return extraData;
		}
		set
		{
			if (!typeSigAndExtraData_isInitialized)
			{
				InitializeTypeSigAndExtraData();
			}
			extraData = value;
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

	public int HasCustomDebugInformationTag => 13;

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

	private void InitializeTypeSigAndExtraData()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!typeSigAndExtraData_isInitialized)
			{
				typeSig = GetTypeSigAndExtraData_NoLock(out extraData);
				typeSigAndExtraData_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual TypeSig GetTypeSigAndExtraData_NoLock(out byte[] extraData)
	{
		extraData = null;
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

	public override string ToString()
	{
		return FullName;
	}
}
