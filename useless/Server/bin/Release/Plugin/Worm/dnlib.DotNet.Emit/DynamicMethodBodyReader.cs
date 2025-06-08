using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public class DynamicMethodBodyReader : MethodBodyReaderBase, ISignatureReaderHelper
{
	private class ReflectionFieldInfo
	{
		private FieldInfo fieldInfo;

		private readonly string fieldName1;

		private readonly string fieldName2;

		public ReflectionFieldInfo(string fieldName)
		{
			fieldName1 = fieldName;
		}

		public ReflectionFieldInfo(string fieldName1, string fieldName2)
		{
			this.fieldName1 = fieldName1;
			this.fieldName2 = fieldName2;
		}

		public object Read(object instance)
		{
			if ((object)fieldInfo == null)
			{
				InitializeField(instance.GetType());
			}
			if ((object)fieldInfo == null)
			{
				throw new Exception("Couldn't find field '" + fieldName1 + "' or '" + fieldName2 + "'");
			}
			return fieldInfo.GetValue(instance);
		}

		public bool Exists(object instance)
		{
			InitializeField(instance.GetType());
			return (object)fieldInfo != null;
		}

		private void InitializeField(Type type)
		{
			if ((object)fieldInfo == null)
			{
				fieldInfo = type.GetField(fieldName1, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)fieldInfo == null && fieldName2 != null)
				{
					fieldInfo = type.GetField(fieldName2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
			}
		}
	}

	private class ExceptionInfo
	{
		public int[] CatchAddr;

		public Type[] CatchClass;

		public int[] CatchEndAddr;

		public int CurrentCatch;

		public int[] Type;

		public int StartAddr;

		public int EndAddr;

		public int EndFinally;
	}

	private static readonly ReflectionFieldInfo rtdmOwnerFieldInfo = new ReflectionFieldInfo("m_owner");

	private static readonly ReflectionFieldInfo dmResolverFieldInfo = new ReflectionFieldInfo("m_resolver", "_resolver");

	private static readonly ReflectionFieldInfo rslvCodeFieldInfo = new ReflectionFieldInfo("m_code");

	private static readonly ReflectionFieldInfo rslvDynamicScopeFieldInfo = new ReflectionFieldInfo("m_scope");

	private static readonly ReflectionFieldInfo rslvMethodFieldInfo = new ReflectionFieldInfo("m_method");

	private static readonly ReflectionFieldInfo rslvLocalsFieldInfo = new ReflectionFieldInfo("m_localSignature");

	private static readonly ReflectionFieldInfo rslvMaxStackFieldInfo = new ReflectionFieldInfo("m_stackSize");

	private static readonly ReflectionFieldInfo rslvExceptionsFieldInfo = new ReflectionFieldInfo("m_exceptions");

	private static readonly ReflectionFieldInfo rslvExceptionHeaderFieldInfo = new ReflectionFieldInfo("m_exceptionHeader");

	private static readonly ReflectionFieldInfo scopeTokensFieldInfo = new ReflectionFieldInfo("m_tokens");

	private static readonly ReflectionFieldInfo gfiFieldHandleFieldInfo = new ReflectionFieldInfo("m_field", "m_fieldHandle");

	private static readonly ReflectionFieldInfo gfiContextFieldInfo = new ReflectionFieldInfo("m_context");

	private static readonly ReflectionFieldInfo gmiMethodHandleFieldInfo = new ReflectionFieldInfo("m_method", "m_methodHandle");

	private static readonly ReflectionFieldInfo gmiContextFieldInfo = new ReflectionFieldInfo("m_context");

	private static readonly ReflectionFieldInfo ehCatchAddrFieldInfo = new ReflectionFieldInfo("m_catchAddr");

	private static readonly ReflectionFieldInfo ehCatchClassFieldInfo = new ReflectionFieldInfo("m_catchClass");

	private static readonly ReflectionFieldInfo ehCatchEndAddrFieldInfo = new ReflectionFieldInfo("m_catchEndAddr");

	private static readonly ReflectionFieldInfo ehCurrentCatchFieldInfo = new ReflectionFieldInfo("m_currentCatch");

	private static readonly ReflectionFieldInfo ehTypeFieldInfo = new ReflectionFieldInfo("m_type");

	private static readonly ReflectionFieldInfo ehStartAddrFieldInfo = new ReflectionFieldInfo("m_startAddr");

	private static readonly ReflectionFieldInfo ehEndAddrFieldInfo = new ReflectionFieldInfo("m_endAddr");

	private static readonly ReflectionFieldInfo ehEndFinallyFieldInfo = new ReflectionFieldInfo("m_endFinally");

	private static readonly ReflectionFieldInfo vamMethodFieldInfo = new ReflectionFieldInfo("m_method");

	private static readonly ReflectionFieldInfo vamDynamicMethodFieldInfo = new ReflectionFieldInfo("m_dynamicMethod");

	private static readonly ReflectionFieldInfo dmDynamicILInfoFieldInfo = new ReflectionFieldInfo("m_DynamicILInfo", "_dynamicILInfo");

	private static readonly ReflectionFieldInfo dynILInfoMaxStackFieldInfo = new ReflectionFieldInfo("m_maxStackSize");

	private readonly ModuleDef module;

	private readonly Importer importer;

	private readonly GenericParamContext gpContext;

	private readonly MethodDef method;

	private readonly int codeSize;

	private readonly int maxStack;

	private readonly bool initLocals;

	private readonly List<object> tokens;

	private readonly IList<object> ehInfos;

	private readonly byte[] ehHeader;

	private readonly string methodName;

	private readonly DynamicMethodBodyReaderOptions options;

	public DynamicMethodBodyReader(ModuleDef module, object obj)
		: this(module, obj, default(GenericParamContext))
	{
	}

	public DynamicMethodBodyReader(ModuleDef module, object obj, GenericParamContext gpContext)
		: this(module, obj, new Importer(module, ImporterOptions.TryToUseDefs, gpContext), DynamicMethodBodyReaderOptions.None)
	{
	}

	public DynamicMethodBodyReader(ModuleDef module, object obj, Importer importer)
		: this(module, obj, importer, DynamicMethodBodyReaderOptions.None)
	{
	}

	public DynamicMethodBodyReader(ModuleDef module, object obj, Importer importer, DynamicMethodBodyReaderOptions options)
		: base(module.Context)
	{
		this.module = module;
		this.importer = importer;
		this.options = options;
		gpContext = importer.gpContext;
		methodName = null;
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj is Delegate @delegate)
		{
			obj = @delegate.Method;
			if (obj == null)
			{
				throw new Exception("Delegate.Method is null");
			}
		}
		if (obj.GetType().ToString() == "System.Reflection.Emit.DynamicMethod+RTDynamicMethod")
		{
			obj = rtdmOwnerFieldInfo.Read(obj) as DynamicMethod;
			if (obj == null)
			{
				throw new Exception("RTDynamicMethod.m_owner is null or invalid");
			}
		}
		if (obj is DynamicMethod dynamicMethod)
		{
			methodName = dynamicMethod.Name;
			obj = dmResolverFieldInfo.Read(obj) ?? dmDynamicILInfoFieldInfo.Read(obj);
			if (obj == null)
			{
				throw new Exception("No resolver found");
			}
		}
		string text = obj.GetType().ToString();
		bool flag = text == "System.Reflection.Emit.DynamicILInfo";
		if (text != "System.Reflection.Emit.DynamicResolver" && !flag)
		{
			throw new Exception("Couldn't find DynamicResolver or DynamicILInfo");
		}
		if (!(rslvCodeFieldInfo.Read(obj) is byte[] array))
		{
			throw new Exception("No code");
		}
		codeSize = array.Length;
		if (!(rslvMethodFieldInfo.Read(obj) is DynamicMethod dynamicMethod2))
		{
			throw new Exception("No method");
		}
		initLocals = dynamicMethod2.InitLocals;
		maxStack = (flag ? ((int)dynILInfoMaxStackFieldInfo.Read(obj)) : ((int)rslvMaxStackFieldInfo.Read(obj)));
		object obj2 = rslvDynamicScopeFieldInfo.Read(obj);
		if (obj2 == null)
		{
			throw new Exception("No scope");
		}
		if (!(scopeTokensFieldInfo.Read(obj2) is IList list))
		{
			throw new Exception("No tokens");
		}
		tokens = new List<object>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			tokens.Add(list[i]);
		}
		if (flag)
		{
			ehHeader = rslvExceptionsFieldInfo.Read(obj) as byte[];
		}
		else
		{
			ehInfos = (IList<object>)rslvExceptionsFieldInfo.Read(obj);
			ehHeader = rslvExceptionHeaderFieldInfo.Read(obj) as byte[];
		}
		UpdateLocals(rslvLocalsFieldInfo.Read(obj) as byte[]);
		reader = ByteArrayDataReaderFactory.CreateReader(array);
		method = CreateMethodDef(dynamicMethod2);
		parameters = method.Parameters;
	}

	private static List<ExceptionInfo> CreateExceptionInfos(IList<object> ehInfos)
	{
		if (ehInfos == null)
		{
			return new List<ExceptionInfo>();
		}
		List<ExceptionInfo> list = new List<ExceptionInfo>(ehInfos.Count);
		int count = ehInfos.Count;
		for (int i = 0; i < count; i++)
		{
			object instance = ehInfos[i];
			ExceptionInfo item = new ExceptionInfo
			{
				CatchAddr = (int[])ehCatchAddrFieldInfo.Read(instance),
				CatchClass = (Type[])ehCatchClassFieldInfo.Read(instance),
				CatchEndAddr = (int[])ehCatchEndAddrFieldInfo.Read(instance),
				CurrentCatch = (int)ehCurrentCatchFieldInfo.Read(instance),
				Type = (int[])ehTypeFieldInfo.Read(instance),
				StartAddr = (int)ehStartAddrFieldInfo.Read(instance),
				EndAddr = (int)ehEndAddrFieldInfo.Read(instance),
				EndFinally = (int)ehEndFinallyFieldInfo.Read(instance)
			};
			list.Add(item);
		}
		return list;
	}

	private void UpdateLocals(byte[] localsSig)
	{
		if (localsSig != null && localsSig.Length != 0 && SignatureReader.ReadSig(this, module.CorLibTypes, localsSig, gpContext) is LocalSig { Locals: var list })
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				locals.Add(new Local(list[i]));
			}
		}
	}

	private MethodDef CreateMethodDef(MethodBase delMethod)
	{
		MethodDefUser methodDefUser = new MethodDefUser();
		TypeSig returnType = GetReturnType(delMethod);
		List<TypeSig> list = GetParameters(delMethod);
		if (true)
		{
			methodDefUser.Signature = MethodSig.CreateStatic(returnType, list.ToArray());
		}
		else
		{
			methodDefUser.Signature = MethodSig.CreateInstance(returnType, list.ToArray());
		}
		methodDefUser.Parameters.UpdateParameterTypes();
		methodDefUser.ImplAttributes = MethodImplAttributes.IL;
		methodDefUser.Attributes = MethodAttributes.PrivateScope;
		if (true)
		{
			methodDefUser.Attributes |= MethodAttributes.Static;
		}
		return module.UpdateRowId(methodDefUser);
	}

	private TypeSig GetReturnType(MethodBase mb)
	{
		if (mb is MethodInfo methodInfo)
		{
			return importer.ImportAsTypeSig(methodInfo.ReturnType);
		}
		return module.CorLibTypes.Void;
	}

	private List<TypeSig> GetParameters(MethodBase delMethod)
	{
		List<TypeSig> list = new List<TypeSig>();
		ParameterInfo[] array = delMethod.GetParameters();
		foreach (ParameterInfo parameterInfo in array)
		{
			list.Add(importer.ImportAsTypeSig(parameterInfo.ParameterType));
		}
		return list;
	}

	public bool Read()
	{
		ReadInstructionsNumBytes((uint)codeSize);
		CreateExceptionHandlers();
		return true;
	}

	private void CreateExceptionHandlers()
	{
		if (ehHeader != null)
		{
			if (ehHeader.Length < 4)
			{
				return;
			}
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(ehHeader));
			if ((binaryReader.ReadByte() & 0x40) == 0)
			{
				int num = (ushort)((binaryReader.ReadByte() - 2) / 12);
				binaryReader.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					if (binaryReader.BaseStream.Position + 12 > binaryReader.BaseStream.Length)
					{
						break;
					}
					ExceptionHandler exceptionHandler = new ExceptionHandler();
					exceptionHandler.HandlerType = (ExceptionHandlerType)binaryReader.ReadUInt16();
					int num2 = binaryReader.ReadUInt16();
					exceptionHandler.TryStart = GetInstructionThrow((uint)num2);
					exceptionHandler.TryEnd = GetInstruction((uint)(binaryReader.ReadByte() + num2));
					num2 = binaryReader.ReadUInt16();
					exceptionHandler.HandlerStart = GetInstructionThrow((uint)num2);
					exceptionHandler.HandlerEnd = GetInstruction((uint)(binaryReader.ReadByte() + num2));
					if (exceptionHandler.IsCatch)
					{
						exceptionHandler.CatchType = ReadToken(binaryReader.ReadUInt32()) as ITypeDefOrRef;
					}
					else if (exceptionHandler.IsFilter)
					{
						exceptionHandler.FilterStart = GetInstruction(binaryReader.ReadUInt32());
					}
					else
					{
						binaryReader.ReadUInt32();
					}
					exceptionHandlers.Add(exceptionHandler);
				}
				return;
			}
			binaryReader.BaseStream.Position--;
			int num3 = (ushort)(((binaryReader.ReadUInt32() >> 8) - 4) / 24);
			for (int j = 0; j < num3; j++)
			{
				if (binaryReader.BaseStream.Position + 24 > binaryReader.BaseStream.Length)
				{
					break;
				}
				ExceptionHandler exceptionHandler2 = new ExceptionHandler();
				exceptionHandler2.HandlerType = (ExceptionHandlerType)binaryReader.ReadUInt32();
				uint num4 = binaryReader.ReadUInt32();
				exceptionHandler2.TryStart = GetInstructionThrow(num4);
				exceptionHandler2.TryEnd = GetInstruction(binaryReader.ReadUInt32() + num4);
				num4 = binaryReader.ReadUInt32();
				exceptionHandler2.HandlerStart = GetInstructionThrow(num4);
				exceptionHandler2.HandlerEnd = GetInstruction(binaryReader.ReadUInt32() + num4);
				if (exceptionHandler2.IsCatch)
				{
					exceptionHandler2.CatchType = ReadToken(binaryReader.ReadUInt32()) as ITypeDefOrRef;
				}
				else if (exceptionHandler2.IsFilter)
				{
					exceptionHandler2.FilterStart = GetInstruction(binaryReader.ReadUInt32());
				}
				else
				{
					binaryReader.ReadUInt32();
				}
				exceptionHandlers.Add(exceptionHandler2);
			}
		}
		else
		{
			if (ehInfos == null)
			{
				return;
			}
			foreach (ExceptionInfo item in CreateExceptionInfos(ehInfos))
			{
				Instruction instructionThrow = GetInstructionThrow((uint)item.StartAddr);
				Instruction instruction = GetInstruction((uint)item.EndAddr);
				Instruction instruction2 = ((item.EndFinally < 0) ? null : GetInstruction((uint)item.EndFinally));
				for (int k = 0; k < item.CurrentCatch; k++)
				{
					ExceptionHandler exceptionHandler3 = new ExceptionHandler();
					exceptionHandler3.HandlerType = (ExceptionHandlerType)item.Type[k];
					exceptionHandler3.TryStart = instructionThrow;
					exceptionHandler3.TryEnd = (exceptionHandler3.IsFinally ? instruction2 : instruction);
					exceptionHandler3.FilterStart = null;
					exceptionHandler3.HandlerStart = GetInstructionThrow((uint)item.CatchAddr[k]);
					exceptionHandler3.HandlerEnd = GetInstruction((uint)item.CatchEndAddr[k]);
					exceptionHandler3.CatchType = importer.Import(item.CatchClass[k]);
					exceptionHandlers.Add(exceptionHandler3);
				}
			}
		}
	}

	public MethodDef GetMethod()
	{
		CilBody cilBody = new CilBody(initLocals, instructions, exceptionHandlers, locals);
		cilBody.MaxStack = (ushort)Math.Min(maxStack, 65535);
		instructions = null;
		exceptionHandlers = null;
		locals = null;
		method.Body = cilBody;
		method.Name = methodName;
		return method;
	}

	protected override IField ReadInlineField(Instruction instr)
	{
		return ReadToken(reader.ReadUInt32()) as IField;
	}

	protected override IMethod ReadInlineMethod(Instruction instr)
	{
		return ReadToken(reader.ReadUInt32()) as IMethod;
	}

	protected override MethodSig ReadInlineSig(Instruction instr)
	{
		return ReadToken(reader.ReadUInt32()) as MethodSig;
	}

	protected override string ReadInlineString(Instruction instr)
	{
		return (ReadToken(reader.ReadUInt32()) as string) ?? string.Empty;
	}

	protected override ITokenOperand ReadInlineTok(Instruction instr)
	{
		return ReadToken(reader.ReadUInt32()) as ITokenOperand;
	}

	protected override ITypeDefOrRef ReadInlineType(Instruction instr)
	{
		return ReadToken(reader.ReadUInt32()) as ITypeDefOrRef;
	}

	private object ReadToken(uint token)
	{
		uint num = token & 0xFFFFFFu;
		switch (token >> 24)
		{
		case 2u:
			return ImportType(num);
		case 4u:
			return ImportField(num);
		case 6u:
		case 10u:
			return ImportMethod(num);
		case 17u:
			return ImportSignature(num);
		case 112u:
			return Resolve(num) as string;
		default:
			return null;
		}
	}

	private IMethod ImportMethod(uint rid)
	{
		object obj = Resolve(rid);
		if (obj == null)
		{
			return null;
		}
		if (obj is RuntimeMethodHandle)
		{
			if ((options & DynamicMethodBodyReaderOptions.UnknownDeclaringType) != 0)
			{
				return importer.Import(MethodBase.GetMethodFromHandle((RuntimeMethodHandle)obj, default(RuntimeTypeHandle)));
			}
			return importer.Import(MethodBase.GetMethodFromHandle((RuntimeMethodHandle)obj));
		}
		if (obj.GetType().ToString() == "System.Reflection.Emit.GenericMethodInfo")
		{
			RuntimeTypeHandle declaringType = (RuntimeTypeHandle)gmiContextFieldInfo.Read(obj);
			MethodBase methodFromHandle = MethodBase.GetMethodFromHandle((RuntimeMethodHandle)gmiMethodHandleFieldInfo.Read(obj), declaringType);
			return importer.Import(methodFromHandle);
		}
		if (obj.GetType().ToString() == "System.Reflection.Emit.VarArgMethod")
		{
			MethodInfo varArgMethod = GetVarArgMethod(obj);
			if (!(varArgMethod is DynamicMethod))
			{
				return importer.Import(varArgMethod);
			}
			obj = varArgMethod;
		}
		if (obj is DynamicMethod)
		{
			throw new Exception("DynamicMethod calls another DynamicMethod");
		}
		return null;
	}

	private MethodInfo GetVarArgMethod(object obj)
	{
		if (vamDynamicMethodFieldInfo.Exists(obj))
		{
			MethodInfo methodInfo = vamMethodFieldInfo.Read(obj) as MethodInfo;
			return (vamDynamicMethodFieldInfo.Read(obj) as DynamicMethod) ?? methodInfo;
		}
		return vamMethodFieldInfo.Read(obj) as MethodInfo;
	}

	private IField ImportField(uint rid)
	{
		object obj = Resolve(rid);
		if (obj == null)
		{
			return null;
		}
		if (obj is RuntimeFieldHandle)
		{
			if ((options & DynamicMethodBodyReaderOptions.UnknownDeclaringType) != 0)
			{
				return importer.Import(FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)obj, default(RuntimeTypeHandle)));
			}
			return importer.Import(FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)obj));
		}
		if (obj.GetType().ToString() == "System.Reflection.Emit.GenericFieldInfo")
		{
			RuntimeTypeHandle declaringType = (RuntimeTypeHandle)gfiContextFieldInfo.Read(obj);
			FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)gfiFieldHandleFieldInfo.Read(obj), declaringType);
			return importer.Import(fieldFromHandle);
		}
		return null;
	}

	private ITypeDefOrRef ImportType(uint rid)
	{
		object obj = Resolve(rid);
		if (obj is RuntimeTypeHandle)
		{
			return importer.Import(Type.GetTypeFromHandle((RuntimeTypeHandle)obj));
		}
		return null;
	}

	private CallingConventionSig ImportSignature(uint rid)
	{
		if (!(Resolve(rid) is byte[] signature))
		{
			return null;
		}
		return SignatureReader.ReadSig(this, module.CorLibTypes, signature, gpContext);
	}

	private object Resolve(uint index)
	{
		if (index >= (uint)tokens.Count)
		{
			return null;
		}
		return tokens[(int)index];
	}

	public override void RestoreMethod(MethodDef method)
	{
		base.RestoreMethod(method);
		CilBody body = method.Body;
		body.InitLocals = initLocals;
		body.MaxStack = (ushort)Math.Min(maxStack, 65535);
	}

	ITypeDefOrRef ISignatureReaderHelper.ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.TypeDefOrRef.Decode(codedToken, out uint token))
		{
			return null;
		}
		Table table = MDToken.ToTable(token);
		if (table - 1 <= Table.TypeRef || table == Table.TypeSpec)
		{
			return module.ResolveToken(token) as ITypeDefOrRef;
		}
		return null;
	}

	TypeSig ISignatureReaderHelper.ConvertRTInternalAddress(IntPtr address)
	{
		return importer.ImportAsTypeSig(MethodTableToTypeConverter.Convert(address));
	}
}
