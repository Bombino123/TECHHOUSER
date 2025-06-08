using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.PE;
using dnlib.Threading;
using dnlib.W32Resources;

namespace dnlib.DotNet;

internal readonly struct ModuleLoader
{
	private readonly ModuleDef module;

	private readonly ICancellationToken cancellationToken;

	private readonly Dictionary<object, bool> seen;

	private readonly Stack<object> stack;

	private ModuleLoader(ModuleDef module, ICancellationToken cancellationToken)
	{
		this.module = module;
		this.cancellationToken = cancellationToken;
		seen = new Dictionary<object, bool>(16384);
		stack = new Stack<object>(16384);
	}

	public static void LoadAll(ModuleDef module, ICancellationToken cancellationToken)
	{
		new ModuleLoader(module, cancellationToken).Load();
	}

	private void Add(UTF8String a)
	{
	}

	private void Add(Guid? a)
	{
	}

	private void Add(ushort a)
	{
	}

	private void Add(AssemblyHashAlgorithm a)
	{
	}

	private void Add(Version a)
	{
	}

	private void Add(AssemblyAttributes a)
	{
	}

	private void Add(PublicKeyBase a)
	{
	}

	private void Add(RVA a)
	{
	}

	private void Add(IManagedEntryPoint a)
	{
	}

	private void Add(string a)
	{
	}

	private void Add(WinMDStatus a)
	{
	}

	private void Add(TypeAttributes a)
	{
	}

	private void Add(FieldAttributes a)
	{
	}

	private void Add(uint? a)
	{
	}

	private void Add(byte[] a)
	{
	}

	private void Add(MethodImplAttributes a)
	{
	}

	private void Add(MethodAttributes a)
	{
	}

	private void Add(MethodSemanticsAttributes a)
	{
	}

	private void Add(ParamAttributes a)
	{
	}

	private void Add(ElementType a)
	{
	}

	private void Add(SecurityAction a)
	{
	}

	private void Add(EventAttributes a)
	{
	}

	private void Add(PropertyAttributes a)
	{
	}

	private void Add(PInvokeAttributes a)
	{
	}

	private void Add(FileAttributes a)
	{
	}

	private void Add(ManifestResourceAttributes a)
	{
	}

	private void Add(GenericParamAttributes a)
	{
	}

	private void Add(NativeType a)
	{
	}

	private void Load()
	{
		LoadAllTables();
		Load(module);
		Process();
	}

	private void Process()
	{
		while (stack.Count != 0)
		{
			if (cancellationToken != null)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			object o = stack.Pop();
			LoadObj(o);
		}
	}

	private void LoadAllTables()
	{
		ITokenResolver tokenResolver = module;
		if (tokenResolver == null)
		{
			return;
		}
		Table table = Table.Module;
		while ((int)table <= 44)
		{
			uint num = 1u;
			while (true)
			{
				IMDTokenProvider iMDTokenProvider = tokenResolver.ResolveToken(new MDToken(table, num).Raw, default(GenericParamContext));
				if (iMDTokenProvider == null)
				{
					break;
				}
				Add(iMDTokenProvider);
				Process();
				num++;
			}
			table++;
		}
	}

	private void LoadObj(object o)
	{
		if (o is TypeSig ts)
		{
			Load(ts);
		}
		else if (o is IMDTokenProvider mdt)
		{
			Load(mdt);
		}
		else if (o is CustomAttribute obj)
		{
			Load(obj);
		}
		else if (o is SecurityAttribute obj2)
		{
			Load(obj2);
		}
		else if (o is CANamedArgument obj3)
		{
			Load(obj3);
		}
		else if (o is Parameter obj4)
		{
			Load(obj4);
		}
		else if (o is PdbMethod obj5)
		{
			Load(obj5);
		}
		else if (o is ResourceDirectory obj6)
		{
			Load(obj6);
		}
		else if (o is ResourceData obj7)
		{
			Load(obj7);
		}
	}

	private void Load(TypeSig ts)
	{
		if (ts != null)
		{
			Add(ts.Next);
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
				Add(((TypeDefOrRefSig)ts).TypeDefOrRef);
				break;
			case ElementType.Var:
			case ElementType.MVar:
			{
				GenericSig genericSig = (GenericSig)ts;
				Add(genericSig.OwnerType);
				Add(genericSig.OwnerMethod);
				break;
			}
			case ElementType.GenericInst:
			{
				GenericInstSig genericInstSig = (GenericInstSig)ts;
				Add(genericInstSig.GenericType);
				Add(genericInstSig.GenericArguments);
				break;
			}
			case ElementType.FnPtr:
			{
				FnPtrSig fnPtrSig = (FnPtrSig)ts;
				Add(fnPtrSig.Signature);
				break;
			}
			case ElementType.CModReqd:
			case ElementType.CModOpt:
			{
				ModifierSig modifierSig = (ModifierSig)ts;
				Add(modifierSig.Modifier);
				break;
			}
			case ElementType.End:
			case ElementType.Ptr:
			case ElementType.ByRef:
			case ElementType.Array:
			case ElementType.ValueArray:
			case ElementType.R:
			case ElementType.SZArray:
			case ElementType.Internal:
			case (ElementType)34:
			case (ElementType)35:
			case (ElementType)36:
			case (ElementType)37:
			case (ElementType)38:
			case (ElementType)39:
			case (ElementType)40:
			case (ElementType)41:
			case (ElementType)42:
			case (ElementType)43:
			case (ElementType)44:
			case (ElementType)45:
			case (ElementType)46:
			case (ElementType)47:
			case (ElementType)48:
			case (ElementType)49:
			case (ElementType)50:
			case (ElementType)51:
			case (ElementType)52:
			case (ElementType)53:
			case (ElementType)54:
			case (ElementType)55:
			case (ElementType)56:
			case (ElementType)57:
			case (ElementType)58:
			case (ElementType)59:
			case (ElementType)60:
			case (ElementType)61:
			case (ElementType)62:
			case ElementType.Module:
			case (ElementType)64:
			case ElementType.Sentinel:
			case (ElementType)66:
			case (ElementType)67:
			case (ElementType)68:
			case ElementType.Pinned:
				break;
			}
		}
	}

	private void Load(IMDTokenProvider mdt)
	{
		if (mdt == null)
		{
			return;
		}
		switch (mdt.MDToken.Table)
		{
		case Table.Module:
			Load((ModuleDef)mdt);
			break;
		case Table.TypeRef:
			Load((TypeRef)mdt);
			break;
		case Table.TypeDef:
			Load((TypeDef)mdt);
			break;
		case Table.Field:
			Load((FieldDef)mdt);
			break;
		case Table.Method:
			Load((MethodDef)mdt);
			break;
		case Table.Param:
			Load((ParamDef)mdt);
			break;
		case Table.InterfaceImpl:
			Load((InterfaceImpl)mdt);
			break;
		case Table.MemberRef:
			Load((MemberRef)mdt);
			break;
		case Table.Constant:
			Load((Constant)mdt);
			break;
		case Table.DeclSecurity:
			Load((DeclSecurity)mdt);
			break;
		case Table.ClassLayout:
			Load((ClassLayout)mdt);
			break;
		case Table.StandAloneSig:
			Load((StandAloneSig)mdt);
			break;
		case Table.Event:
			Load((EventDef)mdt);
			break;
		case Table.Property:
			Load((PropertyDef)mdt);
			break;
		case Table.ModuleRef:
			Load((ModuleRef)mdt);
			break;
		case Table.TypeSpec:
			Load((TypeSpec)mdt);
			break;
		case Table.ImplMap:
			Load((ImplMap)mdt);
			break;
		case Table.Assembly:
			Load((AssemblyDef)mdt);
			break;
		case Table.AssemblyRef:
			Load((AssemblyRef)mdt);
			break;
		case Table.File:
			Load((FileDef)mdt);
			break;
		case Table.ExportedType:
			Load((ExportedType)mdt);
			break;
		case Table.GenericParam:
			Load((GenericParam)mdt);
			break;
		case Table.MethodSpec:
			Load((MethodSpec)mdt);
			break;
		case Table.GenericParamConstraint:
			Load((GenericParamConstraint)mdt);
			break;
		case Table.ManifestResource:
			if (mdt is Resource obj)
			{
				Load(obj);
			}
			else if (mdt is ManifestResource obj2)
			{
				Load(obj2);
			}
			break;
		case Table.FieldPtr:
		case Table.MethodPtr:
		case Table.ParamPtr:
		case Table.CustomAttribute:
		case Table.FieldMarshal:
		case Table.FieldLayout:
		case Table.EventMap:
		case Table.EventPtr:
		case Table.PropertyMap:
		case Table.PropertyPtr:
		case Table.MethodSemantics:
		case Table.MethodImpl:
		case Table.FieldRVA:
		case Table.ENCLog:
		case Table.ENCMap:
		case Table.AssemblyProcessor:
		case Table.AssemblyOS:
		case Table.AssemblyRefProcessor:
		case Table.AssemblyRefOS:
		case Table.NestedClass:
		case (Table)45:
		case (Table)46:
		case (Table)47:
		case Table.Document:
		case Table.MethodDebugInformation:
		case Table.LocalScope:
		case Table.LocalVariable:
		case Table.LocalConstant:
		case Table.ImportScope:
		case Table.StateMachineMethod:
		case Table.CustomDebugInformation:
			break;
		}
	}

	private void Load(ModuleDef obj)
	{
		if (obj != null && obj == module)
		{
			Add(obj.Generation);
			Add(obj.Name);
			Add(obj.Mvid);
			Add(obj.EncId);
			Add(obj.EncBaseId);
			Add(obj.CustomAttributes);
			Add(obj.Assembly);
			Add(obj.Types);
			Add(obj.ExportedTypes);
			Add(obj.NativeEntryPoint);
			Add(obj.ManagedEntryPoint);
			Add(obj.Resources);
			Add(obj.VTableFixups);
			Add(obj.Location);
			Add(obj.Win32Resources);
			Add(obj.RuntimeVersion);
			Add(obj.WinMDStatus);
			Add(obj.RuntimeVersionWinMD);
			Add(obj.WinMDVersion);
			Add(obj.PdbState);
		}
	}

	private void Load(TypeRef obj)
	{
		if (obj != null)
		{
			Add(obj.ResolutionScope);
			Add(obj.Name);
			Add(obj.Namespace);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(TypeDef obj)
	{
		if (obj != null)
		{
			Add(obj.Module2);
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.Namespace);
			Add(obj.BaseType);
			Add(obj.Fields);
			Add(obj.Methods);
			Add(obj.GenericParameters);
			Add(obj.Interfaces);
			Add(obj.DeclSecurities);
			Add(obj.ClassLayout);
			Add(obj.DeclaringType);
			Add(obj.DeclaringType2);
			Add(obj.NestedTypes);
			Add(obj.Events);
			Add(obj.Properties);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(FieldDef obj)
	{
		if (obj != null)
		{
			Add(obj.CustomAttributes);
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.Signature);
			Add(obj.FieldOffset);
			Add(obj.MarshalType);
			Add(obj.RVA);
			Add(obj.InitialValue);
			Add(obj.ImplMap);
			Add(obj.Constant);
			Add(obj.DeclaringType);
		}
	}

	private void Load(MethodDef obj)
	{
		if (obj != null)
		{
			Add(obj.RVA);
			Add(obj.ImplAttributes);
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.Signature);
			Add(obj.ParamDefs);
			Add(obj.GenericParameters);
			Add(obj.DeclSecurities);
			Add(obj.ImplMap);
			Add(obj.MethodBody);
			Add(obj.CustomAttributes);
			Add(obj.Overrides);
			Add(obj.DeclaringType);
			Add(obj.Parameters);
			Add(obj.SemanticsAttributes);
		}
	}

	private void Load(ParamDef obj)
	{
		if (obj != null)
		{
			Add(obj.DeclaringMethod);
			Add(obj.Attributes);
			Add(obj.Sequence);
			Add(obj.Name);
			Add(obj.MarshalType);
			Add(obj.Constant);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(InterfaceImpl obj)
	{
		if (obj != null)
		{
			Add(obj.Interface);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(MemberRef obj)
	{
		if (obj != null)
		{
			Add(obj.Class);
			Add(obj.Name);
			Add(obj.Signature);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(Constant obj)
	{
		if (obj != null)
		{
			Add(obj.Type);
			_ = obj.Value;
		}
	}

	private void Load(DeclSecurity obj)
	{
		if (obj != null)
		{
			Add(obj.Action);
			Add(obj.SecurityAttributes);
			Add(obj.CustomAttributes);
			obj.GetBlob();
		}
	}

	private void Load(ClassLayout obj)
	{
		if (obj != null)
		{
			Add(obj.PackingSize);
			Add(obj.ClassSize);
		}
	}

	private void Load(StandAloneSig obj)
	{
		if (obj != null)
		{
			Add(obj.Signature);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(EventDef obj)
	{
		if (obj != null)
		{
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.EventType);
			Add(obj.CustomAttributes);
			Add(obj.AddMethod);
			Add(obj.InvokeMethod);
			Add(obj.RemoveMethod);
			Add(obj.OtherMethods);
			Add(obj.DeclaringType);
		}
	}

	private void Load(PropertyDef obj)
	{
		if (obj != null)
		{
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.Type);
			Add(obj.Constant);
			Add(obj.CustomAttributes);
			Add(obj.GetMethods);
			Add(obj.SetMethods);
			Add(obj.OtherMethods);
			Add(obj.DeclaringType);
		}
	}

	private void Load(ModuleRef obj)
	{
		if (obj != null)
		{
			Add(obj.Name);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(TypeSpec obj)
	{
		if (obj != null)
		{
			Add(obj.TypeSig);
			Add(obj.ExtraData);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(ImplMap obj)
	{
		if (obj != null)
		{
			Add(obj.Attributes);
			Add(obj.Name);
			Add(obj.Module);
		}
	}

	private void Load(AssemblyDef obj)
	{
		if (obj != null && obj.ManifestModule == module)
		{
			Add(obj.HashAlgorithm);
			Add(obj.Version);
			Add(obj.Attributes);
			Add(obj.PublicKey);
			Add(obj.Name);
			Add(obj.Culture);
			Add(obj.DeclSecurities);
			Add(obj.Modules);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(AssemblyRef obj)
	{
		if (obj != null)
		{
			Add(obj.Version);
			Add(obj.Attributes);
			Add(obj.PublicKeyOrToken);
			Add(obj.Name);
			Add(obj.Culture);
			Add(obj.Hash);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(FileDef obj)
	{
		if (obj != null)
		{
			Add(obj.Flags);
			Add(obj.Name);
			Add(obj.HashValue);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(ExportedType obj)
	{
		if (obj != null)
		{
			Add(obj.CustomAttributes);
			Add(obj.Attributes);
			Add(obj.TypeDefId);
			Add(obj.TypeName);
			Add(obj.TypeNamespace);
			Add(obj.Implementation);
		}
	}

	private void Load(Resource obj)
	{
		if (obj != null)
		{
			Add(obj.Offset);
			Add(obj.Name);
			Add(obj.Attributes);
			Add(obj.CustomAttributes);
			switch (obj.ResourceType)
			{
			case ResourceType.AssemblyLinked:
			{
				AssemblyLinkedResource assemblyLinkedResource = (AssemblyLinkedResource)obj;
				Add(assemblyLinkedResource.Assembly);
				break;
			}
			case ResourceType.Linked:
			{
				LinkedResource linkedResource = (LinkedResource)obj;
				Add(linkedResource.File);
				Add(linkedResource.Hash);
				break;
			}
			case ResourceType.Embedded:
				break;
			}
		}
	}

	private void Load(ManifestResource obj)
	{
		if (obj != null)
		{
			Add(obj.Offset);
			Add(obj.Flags);
			Add(obj.Name);
			Add(obj.Implementation);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(GenericParam obj)
	{
		if (obj != null)
		{
			Add(obj.Owner);
			Add(obj.Number);
			Add(obj.Flags);
			Add(obj.Name);
			Add(obj.Kind);
			Add(obj.GenericParamConstraints);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(MethodSpec obj)
	{
		if (obj != null)
		{
			Add(obj.Method);
			Add(obj.Instantiation);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(GenericParamConstraint obj)
	{
		if (obj != null)
		{
			Add(obj.Owner);
			Add(obj.Constraint);
			Add(obj.CustomAttributes);
		}
	}

	private void Load(CANamedArgument obj)
	{
		if (obj != null)
		{
			Add(obj.Type);
			Add(obj.Name);
			Load(obj.Argument);
		}
	}

	private void Load(Parameter obj)
	{
		if (obj != null)
		{
			Add(obj.Type);
		}
	}

	private void Load(SecurityAttribute obj)
	{
		if (obj != null)
		{
			Add(obj.AttributeType);
			Add(obj.NamedArguments);
		}
	}

	private void Load(CustomAttribute obj)
	{
		if (obj != null)
		{
			Add(obj.Constructor);
			Add(obj.RawData);
			Add(obj.ConstructorArguments);
			Add(obj.NamedArguments);
		}
	}

	private void Load(MethodOverride obj)
	{
		Add(obj.MethodBody);
		Add(obj.MethodDeclaration);
	}

	private void AddCAValue(object obj)
	{
		if (obj is CAArgument)
		{
			Load((CAArgument)obj);
		}
		else if (obj is IList<CAArgument> list)
		{
			Add(list);
		}
		else if (obj is IMDTokenProvider o)
		{
			Add(o);
		}
	}

	private void Load(CAArgument obj)
	{
		Add(obj.Type);
		AddCAValue(obj.Value);
	}

	private void Load(PdbMethod obj)
	{
	}

	private void Load(ResourceDirectory obj)
	{
		if (obj != null)
		{
			Add(obj.Directories);
			Add(obj.Data);
		}
	}

	private void Load(ResourceData obj)
	{
	}

	private void AddToStack<T>(T t) where T : class
	{
		if (t != null && !seen.ContainsKey(t))
		{
			seen[t] = true;
			stack.Push(t);
		}
	}

	private void Add(CustomAttribute obj)
	{
		AddToStack(obj);
	}

	private void Add(SecurityAttribute obj)
	{
		AddToStack(obj);
	}

	private void Add(CANamedArgument obj)
	{
		AddToStack(obj);
	}

	private void Add(Parameter obj)
	{
		AddToStack(obj);
	}

	private void Add(IMDTokenProvider o)
	{
		AddToStack(o);
	}

	private void Add(PdbMethod pdbMethod)
	{
	}

	private void Add(TypeSig ts)
	{
		AddToStack(ts);
	}

	private void Add(ResourceDirectory rd)
	{
		AddToStack(rd);
	}

	private void Add(ResourceData rd)
	{
		AddToStack(rd);
	}

	private void Add<T>(IList<T> list) where T : IMDTokenProvider
	{
		if (list == null)
		{
			return;
		}
		foreach (T item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<TypeSig> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (TypeSig item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<CustomAttribute> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (CustomAttribute item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<SecurityAttribute> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (SecurityAttribute item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<MethodOverride> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (MethodOverride item in list)
		{
			Load(item);
		}
	}

	private void Add(IList<CAArgument> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (CAArgument item in list)
		{
			Load(item);
		}
	}

	private void Add(IList<CANamedArgument> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (CANamedArgument item in list)
		{
			Add(item);
		}
	}

	private void Add(ParameterList list)
	{
		if (list == null)
		{
			return;
		}
		foreach (Parameter item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<Instruction> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (Instruction item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<ExceptionHandler> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ExceptionHandler item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<Local> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (Local item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<ResourceDirectory> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ResourceDirectory item in list)
		{
			Add(item);
		}
	}

	private void Add(IList<ResourceData> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ResourceData item in list)
		{
			Add(item);
		}
	}

	private void Add(VTableFixups vtf)
	{
		if (vtf == null)
		{
			return;
		}
		foreach (VTable item in vtf)
		{
			foreach (IMethod item2 in item)
			{
				Add(item2);
			}
		}
	}

	private void Add(Win32Resources vtf)
	{
		if (vtf != null)
		{
			Add(vtf.Root);
		}
	}

	private void Add(CallingConventionSig sig)
	{
		if (sig is MethodBaseSig msig)
		{
			Add(msig);
		}
		else if (sig is FieldSig fsig)
		{
			Add(fsig);
		}
		else if (sig is LocalSig lsig)
		{
			Add(lsig);
		}
		else if (sig is GenericInstMethodSig gsig)
		{
			Add(gsig);
		}
	}

	private void Add(MethodBaseSig msig)
	{
		if (msig != null)
		{
			Add(msig.ExtraData);
			Add(msig.RetType);
			Add(msig.Params);
			Add(msig.ParamsAfterSentinel);
		}
	}

	private void Add(FieldSig fsig)
	{
		if (fsig != null)
		{
			Add(fsig.ExtraData);
			Add(fsig.Type);
		}
	}

	private void Add(LocalSig lsig)
	{
		if (lsig != null)
		{
			Add(lsig.ExtraData);
			Add(lsig.Locals);
		}
	}

	private void Add(GenericInstMethodSig gsig)
	{
		if (gsig != null)
		{
			Add(gsig.ExtraData);
			Add(gsig.GenericArguments);
		}
	}

	private void Add(MarshalType mt)
	{
		if (mt != null)
		{
			Add(mt.NativeType);
		}
	}

	private void Add(MethodBody mb)
	{
		if (mb is CilBody body)
		{
			Add(body);
		}
		else if (mb is NativeMethodBody body2)
		{
			Add(body2);
		}
	}

	private void Add(NativeMethodBody body)
	{
		if (body != null)
		{
			Add(body.RVA);
		}
	}

	private void Add(CilBody body)
	{
		if (body != null)
		{
			Add(body.Instructions);
			Add(body.ExceptionHandlers);
			Add(body.Variables);
			Add(body.PdbMethod);
		}
	}

	private void Add(Instruction instr)
	{
		if (instr != null)
		{
			if (instr.Operand is IMDTokenProvider o)
			{
				Add(o);
			}
			else if (instr.Operand is Parameter obj)
			{
				Add(obj);
			}
			else if (instr.Operand is Local local)
			{
				Add(local);
			}
			else if (instr.Operand is CallingConventionSig sig)
			{
				Add(sig);
			}
		}
	}

	private void Add(ExceptionHandler eh)
	{
		if (eh != null)
		{
			Add(eh.CatchType);
		}
	}

	private void Add(Local local)
	{
		if (local != null)
		{
			Add(local.Type);
		}
	}

	private void Add(PdbState state)
	{
		if (state != null)
		{
			Add(state.UserEntryPoint);
		}
	}
}
