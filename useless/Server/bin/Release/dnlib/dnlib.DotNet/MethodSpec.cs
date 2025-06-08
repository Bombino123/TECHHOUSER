using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class MethodSpec : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule, IContainsGenericParameter
{
	protected uint rid;

	protected IMethodDefOrRef method;

	protected CallingConventionSig instantiation;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.MethodSpec, rid);

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

	public int HasCustomAttributeTag => 21;

	public IMethodDefOrRef Method
	{
		get
		{
			return method;
		}
		set
		{
			method = value;
		}
	}

	public CallingConventionSig Instantiation
	{
		get
		{
			return instantiation;
		}
		set
		{
			instantiation = value;
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

	public int HasCustomDebugInformationTag => 21;

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

	MethodSig IMethod.MethodSig
	{
		get
		{
			return method?.MethodSig;
		}
		set
		{
			IMethodDefOrRef methodDefOrRef = method;
			if (methodDefOrRef != null)
			{
				methodDefOrRef.MethodSig = value;
			}
		}
	}

	public UTF8String Name
	{
		get
		{
			IMethodDefOrRef methodDefOrRef = method;
			if (methodDefOrRef != null)
			{
				return methodDefOrRef.Name;
			}
			return UTF8String.Empty;
		}
		set
		{
			IMethodDefOrRef methodDefOrRef = method;
			if (methodDefOrRef != null)
			{
				methodDefOrRef.Name = value;
			}
		}
	}

	public ITypeDefOrRef DeclaringType => method?.DeclaringType;

	public GenericInstMethodSig GenericInstMethodSig
	{
		get
		{
			return instantiation as GenericInstMethodSig;
		}
		set
		{
			instantiation = value;
		}
	}

	int IGenericParameterProvider.NumberOfGenericParameters => GenericInstMethodSig?.GenericArguments.Count ?? 0;

	public ModuleDef Module => method?.Module;

	public string FullName
	{
		get
		{
			IList<TypeSig> methodGenArgs = GenericInstMethodSig?.GenericArguments;
			IMethodDefOrRef methodDefOrRef = method;
			if (methodDefOrRef is MethodDef methodDef)
			{
				return FullNameFactory.MethodFullName(methodDef.DeclaringType?.FullName, methodDef.Name, methodDef.MethodSig, null, methodGenArgs);
			}
			if (methodDefOrRef is MemberRef { MethodSig: { } methodSig } memberRef)
			{
				IList<TypeSig> typeGenArgs = ((memberRef.Class as TypeSpec)?.TypeSig as GenericInstSig)?.GenericArguments;
				return FullNameFactory.MethodFullName(memberRef.GetDeclaringTypeFullName(), memberRef.Name, methodSig, typeGenArgs, methodGenArgs);
			}
			return string.Empty;
		}
	}

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => true;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => true;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	bool IContainsGenericParameter.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

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
