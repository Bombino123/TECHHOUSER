using System;
using dnlib.DotNet.MD;

namespace dnlib.DotNet;

public abstract class TypeSig : IType, IFullName, IOwnerModule, ICodedToken, IMDTokenProvider, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter
{
	private uint rid;

	public abstract TypeSig Next { get; }

	public abstract ElementType ElementType { get; }

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

	bool IIsTypeOrMethod.IsMethod => false;

	bool IIsTypeOrMethod.IsType => true;

	int IGenericParameterProvider.NumberOfGenericParameters
	{
		get
		{
			if (this.RemovePinnedAndModifiers() is GenericInstSig genericInstSig)
			{
				return genericInstSig.GenericArguments.Count;
			}
			return 0;
		}
	}

	public bool IsValueType
	{
		get
		{
			TypeSig typeSig = this.RemovePinnedAndModifiers();
			if (typeSig == null)
			{
				return false;
			}
			if (typeSig.ElementType == ElementType.GenericInst)
			{
				typeSig = ((GenericInstSig)typeSig).GenericType;
				if (typeSig == null)
				{
					return false;
				}
			}
			return typeSig.ElementType.IsValueType();
		}
	}

	public bool IsPrimitive => ElementType.IsPrimitive();

	public string TypeName => FullNameFactory.Name(this, isReflection: false);

	UTF8String IFullName.Name
	{
		get
		{
			return new UTF8String(FullNameFactory.Name(this, isReflection: false));
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public string ReflectionName => FullNameFactory.Name(this, isReflection: true);

	public string Namespace => FullNameFactory.Namespace(this, isReflection: false);

	public string ReflectionNamespace => FullNameFactory.Namespace(this, isReflection: true);

	public string FullName => FullNameFactory.FullName(this, isReflection: false);

	public string ReflectionFullName => FullNameFactory.FullName(this, isReflection: true);

	public string AssemblyQualifiedName => FullNameFactory.AssemblyQualifiedName(this);

	public IAssembly DefinitionAssembly => FullNameFactory.DefinitionAssembly(this);

	public IScope Scope => FullNameFactory.Scope(this);

	public ITypeDefOrRef ScopeType => FullNameFactory.ScopeType(this);

	public ModuleDef Module => FullNameFactory.OwnerModule(this);

	public bool IsTypeDefOrRef => this is TypeDefOrRefSig;

	public bool IsCorLibType => this is CorLibTypeSig;

	public bool IsClassSig => this is ClassSig;

	public bool IsValueTypeSig => this is ValueTypeSig;

	public bool IsGenericParameter => this is GenericSig;

	public bool IsGenericTypeParameter => this is GenericVar;

	public bool IsGenericMethodParameter => this is GenericMVar;

	public bool IsSentinel => this is SentinelSig;

	public bool IsFunctionPointer => this is FnPtrSig;

	public bool IsGenericInstanceType => this is GenericInstSig;

	public bool IsPointer => this is PtrSig;

	public bool IsByRef => this is ByRefSig;

	public bool IsSingleOrMultiDimensionalArray => this is ArraySigBase;

	public bool IsArray => this is ArraySig;

	public bool IsSZArray => this is SZArraySig;

	public bool IsModifier => this is ModifierSig;

	public bool IsRequiredModifier => this is CModReqdSig;

	public bool IsOptionalModifier => this is CModOptSig;

	public bool IsPinned => this is PinnedSig;

	public bool IsValueArray => this is ValueArraySig;

	public bool IsModuleSig => this is ModuleSig;

	public bool ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	public override string ToString()
	{
		return FullName;
	}
}
