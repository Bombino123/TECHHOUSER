using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet;

public class MemberFinder
{
	private enum ObjectType
	{
		Unknown,
		EventDef,
		FieldDef,
		GenericParam,
		MemberRef,
		MethodDef,
		MethodSpec,
		PropertyDef,
		TypeDef,
		TypeRef,
		TypeSig,
		TypeSpec,
		ExportedType
	}

	public readonly Dictionary<CustomAttribute, bool> CustomAttributes = new Dictionary<CustomAttribute, bool>();

	public readonly Dictionary<EventDef, bool> EventDefs = new Dictionary<EventDef, bool>();

	public readonly Dictionary<FieldDef, bool> FieldDefs = new Dictionary<FieldDef, bool>();

	public readonly Dictionary<GenericParam, bool> GenericParams = new Dictionary<GenericParam, bool>();

	public readonly Dictionary<MemberRef, bool> MemberRefs = new Dictionary<MemberRef, bool>();

	public readonly Dictionary<MethodDef, bool> MethodDefs = new Dictionary<MethodDef, bool>();

	public readonly Dictionary<MethodSpec, bool> MethodSpecs = new Dictionary<MethodSpec, bool>();

	public readonly Dictionary<PropertyDef, bool> PropertyDefs = new Dictionary<PropertyDef, bool>();

	public readonly Dictionary<TypeDef, bool> TypeDefs = new Dictionary<TypeDef, bool>();

	public readonly Dictionary<TypeRef, bool> TypeRefs = new Dictionary<TypeRef, bool>();

	public readonly Dictionary<TypeSig, bool> TypeSigs = new Dictionary<TypeSig, bool>();

	public readonly Dictionary<TypeSpec, bool> TypeSpecs = new Dictionary<TypeSpec, bool>();

	public readonly Dictionary<ExportedType, bool> ExportedTypes = new Dictionary<ExportedType, bool>();

	private Stack<object> objectStack;

	private ModuleDef validModule;

	private readonly Dictionary<Type, ObjectType> toObjectType = new Dictionary<Type, ObjectType>();

	public MemberFinder FindAll(ModuleDef module)
	{
		validModule = module;
		objectStack = new Stack<object>(4096);
		Add(module);
		ProcessAll();
		objectStack = null;
		return this;
	}

	private void Push(object mr)
	{
		if (mr != null)
		{
			objectStack.Push(mr);
		}
	}

	private void ProcessAll()
	{
		while (objectStack.Count > 0)
		{
			object obj = objectStack.Pop();
			switch (GetObjectType(obj))
			{
			case ObjectType.EventDef:
				Add((EventDef)obj);
				break;
			case ObjectType.FieldDef:
				Add((FieldDef)obj);
				break;
			case ObjectType.GenericParam:
				Add((GenericParam)obj);
				break;
			case ObjectType.MemberRef:
				Add((MemberRef)obj);
				break;
			case ObjectType.MethodDef:
				Add((MethodDef)obj);
				break;
			case ObjectType.MethodSpec:
				Add((MethodSpec)obj);
				break;
			case ObjectType.PropertyDef:
				Add((PropertyDef)obj);
				break;
			case ObjectType.TypeDef:
				Add((TypeDef)obj);
				break;
			case ObjectType.TypeRef:
				Add((TypeRef)obj);
				break;
			case ObjectType.TypeSig:
				Add((TypeSig)obj);
				break;
			case ObjectType.TypeSpec:
				Add((TypeSpec)obj);
				break;
			case ObjectType.ExportedType:
				Add((ExportedType)obj);
				break;
			default:
				throw new InvalidOperationException($"Unknown type: {obj.GetType()}");
			case ObjectType.Unknown:
				break;
			}
		}
	}

	private ObjectType GetObjectType(object o)
	{
		if (o == null)
		{
			return ObjectType.Unknown;
		}
		Type type = o.GetType();
		if (toObjectType.TryGetValue(type, out var value))
		{
			return value;
		}
		value = GetObjectType2(o);
		toObjectType[type] = value;
		return value;
	}

	private static ObjectType GetObjectType2(object o)
	{
		if (o is EventDef)
		{
			return ObjectType.EventDef;
		}
		if (o is FieldDef)
		{
			return ObjectType.FieldDef;
		}
		if (o is GenericParam)
		{
			return ObjectType.GenericParam;
		}
		if (o is MemberRef)
		{
			return ObjectType.MemberRef;
		}
		if (o is MethodDef)
		{
			return ObjectType.MethodDef;
		}
		if (o is MethodSpec)
		{
			return ObjectType.MethodSpec;
		}
		if (o is PropertyDef)
		{
			return ObjectType.PropertyDef;
		}
		if (o is TypeDef)
		{
			return ObjectType.TypeDef;
		}
		if (o is TypeRef)
		{
			return ObjectType.TypeRef;
		}
		if (o is TypeSig)
		{
			return ObjectType.TypeSig;
		}
		if (o is TypeSpec)
		{
			return ObjectType.TypeSpec;
		}
		if (o is ExportedType)
		{
			return ObjectType.ExportedType;
		}
		return ObjectType.Unknown;
	}

	private void Add(ModuleDef mod)
	{
		Push(mod.ManagedEntryPoint);
		Add(mod.CustomAttributes);
		Add(mod.Types);
		Add(mod.ExportedTypes);
		if (mod.IsManifestModule)
		{
			Add(mod.Assembly);
		}
		Add(mod.VTableFixups);
		Add(mod.Resources);
	}

	private void Add(VTableFixups fixups)
	{
		if (fixups == null)
		{
			return;
		}
		foreach (VTable fixup in fixups)
		{
			foreach (IMethod item in fixup)
			{
				Push(item);
			}
		}
	}

	private void Add(ResourceCollection resources)
	{
		foreach (Resource resource in resources)
		{
			Add(resource.CustomAttributes);
		}
	}

	private void Add(AssemblyDef asm)
	{
		if (asm != null)
		{
			Add(asm.DeclSecurities);
			Add(asm.CustomAttributes);
		}
	}

	private void Add(CallingConventionSig sig)
	{
		if (sig != null)
		{
			if (sig is FieldSig sig2)
			{
				Add(sig2);
			}
			else if (sig is MethodBaseSig sig3)
			{
				Add(sig3);
			}
			else if (sig is LocalSig sig4)
			{
				Add(sig4);
			}
			else if (sig is GenericInstMethodSig sig5)
			{
				Add(sig5);
			}
		}
	}

	private void Add(FieldSig sig)
	{
		if (sig != null)
		{
			Add(sig.Type);
		}
	}

	private void Add(MethodBaseSig sig)
	{
		if (sig != null)
		{
			Add(sig.RetType);
			Add(sig.Params);
			Add(sig.ParamsAfterSentinel);
		}
	}

	private void Add(LocalSig sig)
	{
		if (sig != null)
		{
			Add(sig.Locals);
		}
	}

	private void Add(GenericInstMethodSig sig)
	{
		if (sig != null)
		{
			Add(sig.GenericArguments);
		}
	}

	private void Add(IEnumerable<CustomAttribute> cas)
	{
		if (cas == null)
		{
			return;
		}
		foreach (CustomAttribute ca in cas)
		{
			Add(ca);
		}
	}

	private void Add(CustomAttribute ca)
	{
		if (ca != null && !CustomAttributes.ContainsKey(ca))
		{
			CustomAttributes[ca] = true;
			Push(ca.Constructor);
			Add(ca.ConstructorArguments);
			Add(ca.NamedArguments);
		}
	}

	private void Add(IEnumerable<CAArgument> args)
	{
		if (args == null)
		{
			return;
		}
		foreach (CAArgument arg in args)
		{
			Add(arg);
		}
	}

	private void Add(CAArgument arg)
	{
		Add(arg.Type);
		if (arg.Value is TypeSig ts)
		{
			Add(ts);
		}
		else if (arg.Value is IList<CAArgument> args)
		{
			Add(args);
		}
		else if (arg.Value is CAArgument arg2)
		{
			Add(arg2);
		}
	}

	private void Add(IEnumerable<CANamedArgument> args)
	{
		if (args == null)
		{
			return;
		}
		foreach (CANamedArgument arg in args)
		{
			Add(arg);
		}
	}

	private void Add(CANamedArgument arg)
	{
		if (arg != null)
		{
			Add(arg.Type);
			Add(arg.Argument);
		}
	}

	private void Add(IEnumerable<DeclSecurity> decls)
	{
		if (decls == null)
		{
			return;
		}
		foreach (DeclSecurity decl in decls)
		{
			Add(decl);
		}
	}

	private void Add(DeclSecurity decl)
	{
		if (decl != null)
		{
			Add(decl.SecurityAttributes);
			Add(decl.CustomAttributes);
		}
	}

	private void Add(IEnumerable<SecurityAttribute> secAttrs)
	{
		if (secAttrs == null)
		{
			return;
		}
		foreach (SecurityAttribute secAttr in secAttrs)
		{
			Add(secAttr);
		}
	}

	private void Add(SecurityAttribute secAttr)
	{
		if (secAttr != null)
		{
			Add(secAttr.AttributeType);
			Add(secAttr.NamedArguments);
		}
	}

	private void Add(ITypeDefOrRef tdr)
	{
		if (tdr is TypeDef td)
		{
			Add(td);
		}
		else if (tdr is TypeRef tr)
		{
			Add(tr);
		}
		else if (tdr is TypeSpec ts)
		{
			Add(ts);
		}
	}

	private void Add(IEnumerable<EventDef> eds)
	{
		if (eds == null)
		{
			return;
		}
		foreach (EventDef ed in eds)
		{
			Add(ed);
		}
	}

	private void Add(EventDef ed)
	{
		if (ed != null && !EventDefs.ContainsKey(ed) && (ed.DeclaringType == null || ed.DeclaringType.Module == validModule))
		{
			EventDefs[ed] = true;
			Push(ed.EventType);
			Add(ed.CustomAttributes);
			Add(ed.AddMethod);
			Add(ed.InvokeMethod);
			Add(ed.RemoveMethod);
			Add(ed.OtherMethods);
			Add(ed.DeclaringType);
		}
	}

	private void Add(IEnumerable<FieldDef> fds)
	{
		if (fds == null)
		{
			return;
		}
		foreach (FieldDef fd in fds)
		{
			Add(fd);
		}
	}

	private void Add(FieldDef fd)
	{
		if (fd != null && !FieldDefs.ContainsKey(fd) && (fd.DeclaringType == null || fd.DeclaringType.Module == validModule))
		{
			FieldDefs[fd] = true;
			Add(fd.CustomAttributes);
			Add(fd.Signature);
			Add(fd.DeclaringType);
			Add(fd.MarshalType);
		}
	}

	private void Add(IEnumerable<GenericParam> gps)
	{
		if (gps == null)
		{
			return;
		}
		foreach (GenericParam gp in gps)
		{
			Add(gp);
		}
	}

	private void Add(GenericParam gp)
	{
		if (gp != null && !GenericParams.ContainsKey(gp))
		{
			GenericParams[gp] = true;
			Push(gp.Owner);
			Push(gp.Kind);
			Add(gp.GenericParamConstraints);
			Add(gp.CustomAttributes);
		}
	}

	private void Add(IEnumerable<GenericParamConstraint> gpcs)
	{
		if (gpcs == null)
		{
			return;
		}
		foreach (GenericParamConstraint gpc in gpcs)
		{
			Add(gpc);
		}
	}

	private void Add(GenericParamConstraint gpc)
	{
		if (gpc != null)
		{
			Add(gpc.Owner);
			Push(gpc.Constraint);
			Add(gpc.CustomAttributes);
		}
	}

	private void Add(MemberRef mr)
	{
		if (mr != null && !MemberRefs.ContainsKey(mr) && mr.Module == validModule)
		{
			MemberRefs[mr] = true;
			Push(mr.Class);
			Add(mr.Signature);
			Add(mr.CustomAttributes);
		}
	}

	private void Add(IEnumerable<MethodDef> methods)
	{
		if (methods == null)
		{
			return;
		}
		foreach (MethodDef method in methods)
		{
			Add(method);
		}
	}

	private void Add(MethodDef md)
	{
		if (md != null && !MethodDefs.ContainsKey(md) && (md.DeclaringType == null || md.DeclaringType.Module == validModule))
		{
			MethodDefs[md] = true;
			Add(md.Signature);
			Add(md.ParamDefs);
			Add(md.GenericParameters);
			Add(md.DeclSecurities);
			Add(md.MethodBody);
			Add(md.CustomAttributes);
			Add(md.Overrides);
			Add(md.DeclaringType);
		}
	}

	private void Add(MethodBody mb)
	{
		if (mb is CilBody cb)
		{
			Add(cb);
		}
	}

	private void Add(CilBody cb)
	{
		if (cb != null)
		{
			Add(cb.Instructions);
			Add(cb.ExceptionHandlers);
			Add(cb.Variables);
		}
	}

	private void Add(IEnumerable<Instruction> instrs)
	{
		if (instrs == null)
		{
			return;
		}
		foreach (Instruction instr in instrs)
		{
			if (instr == null)
			{
				continue;
			}
			switch (instr.OpCode.OperandType)
			{
			case OperandType.InlineField:
			case OperandType.InlineMethod:
			case OperandType.InlineTok:
			case OperandType.InlineType:
				Push(instr.Operand);
				break;
			case OperandType.InlineSig:
				Add(instr.Operand as CallingConventionSig);
				break;
			case OperandType.InlineVar:
			case OperandType.ShortInlineVar:
				if (instr.Operand is Local local)
				{
					Add(local);
				}
				else if (instr.Operand is Parameter param)
				{
					Add(param);
				}
				break;
			}
		}
	}

	private void Add(IEnumerable<ExceptionHandler> ehs)
	{
		if (ehs == null)
		{
			return;
		}
		foreach (ExceptionHandler eh in ehs)
		{
			Push(eh.CatchType);
		}
	}

	private void Add(IEnumerable<Local> locals)
	{
		if (locals == null)
		{
			return;
		}
		foreach (Local local in locals)
		{
			Add(local);
		}
	}

	private void Add(Local local)
	{
		if (local != null)
		{
			Add(local.Type);
		}
	}

	private void Add(IEnumerable<Parameter> ps)
	{
		if (ps == null)
		{
			return;
		}
		foreach (Parameter p in ps)
		{
			Add(p);
		}
	}

	private void Add(Parameter param)
	{
		if (param != null)
		{
			Add(param.Type);
			Add(param.Method);
		}
	}

	private void Add(IEnumerable<ParamDef> pds)
	{
		if (pds == null)
		{
			return;
		}
		foreach (ParamDef pd in pds)
		{
			Add(pd);
		}
	}

	private void Add(ParamDef pd)
	{
		if (pd != null)
		{
			Add(pd.DeclaringMethod);
			Add(pd.CustomAttributes);
			Add(pd.MarshalType);
		}
	}

	private void Add(MarshalType mt)
	{
		if (mt != null)
		{
			switch (mt.NativeType)
			{
			case NativeType.SafeArray:
				Add(((SafeArrayMarshalType)mt).UserDefinedSubType);
				break;
			case NativeType.CustomMarshaler:
				Add(((CustomMarshalType)mt).CustomMarshaler);
				break;
			}
		}
	}

	private void Add(IEnumerable<MethodOverride> mos)
	{
		if (mos == null)
		{
			return;
		}
		foreach (MethodOverride mo in mos)
		{
			Add(mo);
		}
	}

	private void Add(MethodOverride mo)
	{
		Push(mo.MethodBody);
		Push(mo.MethodDeclaration);
	}

	private void Add(MethodSpec ms)
	{
		if (ms != null && !MethodSpecs.ContainsKey(ms) && (ms.Method == null || ms.Method.DeclaringType == null || ms.Method.DeclaringType.Module == validModule))
		{
			MethodSpecs[ms] = true;
			Push(ms.Method);
			Add(ms.Instantiation);
			Add(ms.CustomAttributes);
		}
	}

	private void Add(IEnumerable<PropertyDef> pds)
	{
		if (pds == null)
		{
			return;
		}
		foreach (PropertyDef pd in pds)
		{
			Add(pd);
		}
	}

	private void Add(PropertyDef pd)
	{
		if (pd != null && !PropertyDefs.ContainsKey(pd) && (pd.DeclaringType == null || pd.DeclaringType.Module == validModule))
		{
			PropertyDefs[pd] = true;
			Add(pd.Type);
			Add(pd.CustomAttributes);
			Add(pd.GetMethods);
			Add(pd.SetMethods);
			Add(pd.OtherMethods);
			Add(pd.DeclaringType);
		}
	}

	private void Add(IEnumerable<TypeDef> tds)
	{
		if (tds == null)
		{
			return;
		}
		foreach (TypeDef td in tds)
		{
			Add(td);
		}
	}

	private void Add(TypeDef td)
	{
		if (td != null && !TypeDefs.ContainsKey(td) && td.Module == validModule)
		{
			TypeDefs[td] = true;
			Push(td.BaseType);
			Add(td.Fields);
			Add(td.Methods);
			Add(td.GenericParameters);
			Add(td.Interfaces);
			Add(td.DeclSecurities);
			Add(td.DeclaringType);
			Add(td.Events);
			Add(td.Properties);
			Add(td.NestedTypes);
			Add(td.CustomAttributes);
		}
	}

	private void Add(IEnumerable<InterfaceImpl> iis)
	{
		if (iis == null)
		{
			return;
		}
		foreach (InterfaceImpl ii in iis)
		{
			Add(ii);
		}
	}

	private void Add(InterfaceImpl ii)
	{
		if (ii != null)
		{
			Push(ii.Interface);
			Add(ii.CustomAttributes);
		}
	}

	private void Add(TypeRef tr)
	{
		if (tr != null && !TypeRefs.ContainsKey(tr) && tr.Module == validModule)
		{
			TypeRefs[tr] = true;
			Push(tr.ResolutionScope);
			Add(tr.CustomAttributes);
		}
	}

	private void Add(IEnumerable<TypeSig> tss)
	{
		if (tss == null)
		{
			return;
		}
		foreach (TypeSig item in tss)
		{
			Add(item);
		}
	}

	private void Add(TypeSig ts)
	{
		if (ts == null || TypeSigs.ContainsKey(ts) || ts.Module != validModule)
		{
			return;
		}
		TypeSigs[ts] = true;
		while (ts != null)
		{
			switch (ts.ElementType)
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
			{
				TypeDefOrRefSig typeDefOrRefSig = (TypeDefOrRefSig)ts;
				Push(typeDefOrRefSig.TypeDefOrRef);
				break;
			}
			case ElementType.FnPtr:
			{
				FnPtrSig fnPtrSig = (FnPtrSig)ts;
				Add(fnPtrSig.Signature);
				break;
			}
			case ElementType.GenericInst:
			{
				GenericInstSig genericInstSig = (GenericInstSig)ts;
				Add(genericInstSig.GenericType);
				Add(genericInstSig.GenericArguments);
				break;
			}
			case ElementType.CModReqd:
			case ElementType.CModOpt:
			{
				ModifierSig modifierSig = (ModifierSig)ts;
				Push(modifierSig.Modifier);
				break;
			}
			}
			ts = ts.Next;
		}
	}

	private void Add(TypeSpec ts)
	{
		if (ts != null && !TypeSpecs.ContainsKey(ts) && ts.Module == validModule)
		{
			TypeSpecs[ts] = true;
			Add(ts.TypeSig);
			Add(ts.CustomAttributes);
		}
	}

	private void Add(IEnumerable<ExportedType> ets)
	{
		if (ets == null)
		{
			return;
		}
		foreach (ExportedType et in ets)
		{
			Add(et);
		}
	}

	private void Add(ExportedType et)
	{
		if (et != null && !ExportedTypes.ContainsKey(et) && et.Module == validModule)
		{
			ExportedTypes[et] = true;
			Add(et.CustomAttributes);
			Push(et.Implementation);
		}
	}
}
