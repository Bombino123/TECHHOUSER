using System.Collections.Generic;

namespace dnlib.DotNet;

internal struct TypeHelper
{
	private RecursionCounter recursionCounter;

	internal static bool ContainsGenericParameter(StandAloneSig ss)
	{
		if (ss != null)
		{
			return ContainsGenericParameter(ss.Signature);
		}
		return false;
	}

	internal static bool ContainsGenericParameter(InterfaceImpl ii)
	{
		if (ii != null)
		{
			return ContainsGenericParameter(ii.Interface);
		}
		return false;
	}

	internal static bool ContainsGenericParameter(GenericParamConstraint gpc)
	{
		if (gpc != null)
		{
			return ContainsGenericParameter(gpc.Constraint);
		}
		return false;
	}

	internal static bool ContainsGenericParameter(MethodSpec ms)
	{
		if (ms != null)
		{
			return ContainsGenericParameter(ms.GenericInstMethodSig);
		}
		return false;
	}

	internal static bool ContainsGenericParameter(MemberRef mr)
	{
		if (mr == null)
		{
			return false;
		}
		if (ContainsGenericParameter(mr.Signature))
		{
			return true;
		}
		IMemberRefParent @class = mr.Class;
		if (@class is ITypeDefOrRef typeDefOrRef)
		{
			return typeDefOrRef.ContainsGenericParameter;
		}
		if (@class is MethodDef methodDef)
		{
			return ContainsGenericParameter(methodDef.Signature);
		}
		return false;
	}

	public static bool ContainsGenericParameter(CallingConventionSig callConv)
	{
		if (callConv is FieldSig fieldSig)
		{
			return ContainsGenericParameter(fieldSig);
		}
		if (callConv is MethodBaseSig methodSig)
		{
			return ContainsGenericParameter(methodSig);
		}
		if (callConv is LocalSig localSig)
		{
			return ContainsGenericParameter(localSig);
		}
		if (callConv is GenericInstMethodSig gim)
		{
			return ContainsGenericParameter(gim);
		}
		return false;
	}

	public static bool ContainsGenericParameter(FieldSig fieldSig)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(fieldSig);
	}

	public static bool ContainsGenericParameter(MethodBaseSig methodSig)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(methodSig);
	}

	public static bool ContainsGenericParameter(LocalSig localSig)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(localSig);
	}

	public static bool ContainsGenericParameter(GenericInstMethodSig gim)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(gim);
	}

	public static bool ContainsGenericParameter(IType type)
	{
		if (type is TypeDef type2)
		{
			return ContainsGenericParameter(type2);
		}
		if (type is TypeRef type3)
		{
			return ContainsGenericParameter(type3);
		}
		if (type is TypeSpec type4)
		{
			return ContainsGenericParameter(type4);
		}
		if (type is TypeSig type5)
		{
			return ContainsGenericParameter(type5);
		}
		if (type is ExportedType type6)
		{
			return ContainsGenericParameter(type6);
		}
		return false;
	}

	public static bool ContainsGenericParameter(TypeDef type)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(type);
	}

	public static bool ContainsGenericParameter(TypeRef type)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(type);
	}

	public static bool ContainsGenericParameter(TypeSpec type)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(type);
	}

	public static bool ContainsGenericParameter(TypeSig type)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(type);
	}

	public static bool ContainsGenericParameter(ExportedType type)
	{
		return default(TypeHelper).ContainsGenericParameterInternal(type);
	}

	private bool ContainsGenericParameterInternal(TypeDef type)
	{
		return false;
	}

	private bool ContainsGenericParameterInternal(TypeRef type)
	{
		return false;
	}

	private bool ContainsGenericParameterInternal(TypeSpec type)
	{
		if (type == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ContainsGenericParameterInternal(type.TypeSig);
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameterInternal(ITypeDefOrRef tdr)
	{
		if (tdr == null)
		{
			return false;
		}
		return ContainsGenericParameterInternal(tdr as TypeSpec);
	}

	private bool ContainsGenericParameterInternal(TypeSig type)
	{
		if (type == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		switch (type.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = ContainsGenericParameterInternal((type as TypeDefOrRefSig).TypeDefOrRef);
			break;
		case ElementType.Var:
		case ElementType.MVar:
			result = true;
			break;
		case ElementType.FnPtr:
			result = ContainsGenericParameterInternal((type as FnPtrSig).Signature);
			break;
		case ElementType.GenericInst:
		{
			GenericInstSig genericInstSig = (GenericInstSig)type;
			result = ContainsGenericParameterInternal(genericInstSig.GenericType) || ContainsGenericParameter(genericInstSig.GenericArguments);
			break;
		}
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Array:
		case ElementType.ValueArray:
		case ElementType.SZArray:
		case ElementType.Module:
		case ElementType.Pinned:
			result = ContainsGenericParameterInternal((type as NonLeafSig).Next);
			break;
		case ElementType.CModReqd:
		case ElementType.CModOpt:
			result = ContainsGenericParameterInternal((type as ModifierSig).Modifier) || ContainsGenericParameterInternal((type as NonLeafSig).Next);
			break;
		default:
			result = false;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameterInternal(ExportedType type)
	{
		return false;
	}

	private bool ContainsGenericParameterInternal(CallingConventionSig callConv)
	{
		if (callConv is FieldSig fs)
		{
			return ContainsGenericParameterInternal(fs);
		}
		if (callConv is MethodBaseSig mbs)
		{
			return ContainsGenericParameterInternal(mbs);
		}
		if (callConv is LocalSig ls)
		{
			return ContainsGenericParameterInternal(ls);
		}
		if (callConv is GenericInstMethodSig gim)
		{
			return ContainsGenericParameterInternal(gim);
		}
		return false;
	}

	private bool ContainsGenericParameterInternal(FieldSig fs)
	{
		if (fs == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ContainsGenericParameterInternal(fs.Type);
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameterInternal(MethodBaseSig mbs)
	{
		if (mbs == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ContainsGenericParameterInternal(mbs.RetType) || ContainsGenericParameter(mbs.Params) || ContainsGenericParameter(mbs.ParamsAfterSentinel);
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameterInternal(LocalSig ls)
	{
		if (ls == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ContainsGenericParameter(ls.Locals);
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameterInternal(GenericInstMethodSig gim)
	{
		if (gim == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ContainsGenericParameter(gim.GenericArguments);
		recursionCounter.Decrement();
		return result;
	}

	private bool ContainsGenericParameter(IList<TypeSig> types)
	{
		if (types == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = false;
		int count = types.Count;
		for (int i = 0; i < count; i++)
		{
			if (ContainsGenericParameter(types[i]))
			{
				result = true;
				break;
			}
		}
		recursionCounter.Decrement();
		return result;
	}
}
