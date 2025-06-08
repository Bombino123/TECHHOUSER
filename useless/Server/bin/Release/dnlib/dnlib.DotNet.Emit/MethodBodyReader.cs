using System.Collections.Generic;
using System.IO;
using dnlib.IO;

namespace dnlib.DotNet.Emit;

public sealed class MethodBodyReader : MethodBodyReaderBase
{
	private readonly IInstructionOperandResolver opResolver;

	private bool hasReadHeader;

	private byte headerSize;

	private ushort flags;

	private ushort maxStack;

	private uint codeSize;

	private uint localVarSigTok;

	private uint startOfHeader;

	private uint totalBodySize;

	private DataReader? exceptionsReader;

	private readonly GenericParamContext gpContext;

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader reader, MethodDef method)
	{
		return CreateCilBody(opResolver, reader, null, method.Parameters, default(GenericParamContext));
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader reader, MethodDef method, GenericParamContext gpContext)
	{
		return CreateCilBody(opResolver, reader, null, method.Parameters, gpContext);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader reader, IList<Parameter> parameters)
	{
		return CreateCilBody(opResolver, reader, null, parameters, default(GenericParamContext));
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader reader, IList<Parameter> parameters, GenericParamContext gpContext)
	{
		return CreateCilBody(opResolver, reader, null, parameters, gpContext);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader reader, IList<Parameter> parameters, GenericParamContext gpContext, ModuleContext context)
	{
		return CreateCilBody(opResolver, reader, null, parameters, gpContext, context);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, byte[] code, byte[] exceptions, IList<Parameter> parameters)
	{
		return CreateCilBody(opResolver, ByteArrayDataReaderFactory.CreateReader(code), (exceptions == null) ? null : new DataReader?(ByteArrayDataReaderFactory.CreateReader(exceptions)), parameters, default(GenericParamContext));
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, byte[] code, byte[] exceptions, IList<Parameter> parameters, GenericParamContext gpContext)
	{
		return CreateCilBody(opResolver, ByteArrayDataReaderFactory.CreateReader(code), (exceptions == null) ? null : new DataReader?(ByteArrayDataReaderFactory.CreateReader(exceptions)), parameters, gpContext);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters)
	{
		return CreateCilBody(opResolver, codeReader, ehReader, parameters, default(GenericParamContext));
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters, GenericParamContext gpContext)
	{
		return CreateCilBody(opResolver, codeReader, ehReader, parameters, gpContext, null);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters, GenericParamContext gpContext, ModuleContext context)
	{
		MethodBodyReader methodBodyReader = new MethodBodyReader(opResolver, codeReader, ehReader, parameters, gpContext, context);
		if (!methodBodyReader.Read())
		{
			return new CilBody();
		}
		return methodBodyReader.CreateCilBody();
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, byte[] code, byte[] exceptions, IList<Parameter> parameters, ushort flags, ushort maxStack, uint codeSize, uint localVarSigTok)
	{
		return CreateCilBody(opResolver, code, exceptions, parameters, flags, maxStack, codeSize, localVarSigTok, default(GenericParamContext));
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, byte[] code, byte[] exceptions, IList<Parameter> parameters, ushort flags, ushort maxStack, uint codeSize, uint localVarSigTok, GenericParamContext gpContext)
	{
		return CreateCilBody(opResolver, code, exceptions, parameters, flags, maxStack, codeSize, localVarSigTok, gpContext, null);
	}

	public static CilBody CreateCilBody(IInstructionOperandResolver opResolver, byte[] code, byte[] exceptions, IList<Parameter> parameters, ushort flags, ushort maxStack, uint codeSize, uint localVarSigTok, GenericParamContext gpContext, ModuleContext context)
	{
		DataReader codeReader = ByteArrayDataReaderFactory.CreateReader(code);
		DataReader? ehReader = ((exceptions == null) ? null : new DataReader?(ByteArrayDataReaderFactory.CreateReader(exceptions)));
		MethodBodyReader methodBodyReader = new MethodBodyReader(opResolver, codeReader, ehReader, parameters, gpContext, context);
		methodBodyReader.SetHeader(flags, maxStack, codeSize, localVarSigTok);
		if (!methodBodyReader.Read())
		{
			return new CilBody();
		}
		return methodBodyReader.CreateCilBody();
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader reader, MethodDef method)
		: this(opResolver, reader, null, method.Parameters, default(GenericParamContext))
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader reader, MethodDef method, GenericParamContext gpContext)
		: this(opResolver, reader, null, method.Parameters, gpContext)
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader reader, IList<Parameter> parameters)
		: this(opResolver, reader, null, parameters, default(GenericParamContext))
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader reader, IList<Parameter> parameters, GenericParamContext gpContext)
		: this(opResolver, reader, null, parameters, gpContext)
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters)
		: this(opResolver, codeReader, ehReader, parameters, default(GenericParamContext))
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters, GenericParamContext gpContext)
		: this(opResolver, codeReader, ehReader, parameters, gpContext, null)
	{
	}

	public MethodBodyReader(IInstructionOperandResolver opResolver, DataReader codeReader, DataReader? ehReader, IList<Parameter> parameters, GenericParamContext gpContext, ModuleContext context)
		: base(codeReader, parameters, context)
	{
		this.opResolver = opResolver;
		exceptionsReader = ehReader;
		this.gpContext = gpContext;
		startOfHeader = uint.MaxValue;
	}

	private void SetHeader(ushort flags, ushort maxStack, uint codeSize, uint localVarSigTok)
	{
		hasReadHeader = true;
		this.flags = flags;
		this.maxStack = maxStack;
		this.codeSize = codeSize;
		this.localVarSigTok = localVarSigTok;
	}

	public bool Read()
	{
		try
		{
			if (!ReadHeader())
			{
				return false;
			}
			SetLocals(ReadLocals());
			ReadInstructions();
			ReadExceptionHandlers(out totalBodySize);
			return true;
		}
		catch (InvalidMethodException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
	}

	private bool ReadHeader()
	{
		if (hasReadHeader)
		{
			return true;
		}
		hasReadHeader = true;
		startOfHeader = reader.Position;
		byte b = reader.ReadByte();
		switch (b & 7)
		{
		case 2:
		case 6:
			flags = 2;
			maxStack = 8;
			codeSize = (uint)(b >> 2);
			localVarSigTok = 0u;
			headerSize = 1;
			break;
		case 3:
			flags = (ushort)((reader.ReadByte() << 8) | b);
			headerSize = (byte)(flags >> 12);
			maxStack = reader.ReadUInt16();
			codeSize = reader.ReadUInt32();
			localVarSigTok = reader.ReadUInt32();
			reader.Position = reader.Position - 12 + (uint)(headerSize * 4);
			if (headerSize < 3)
			{
				flags &= 65527;
			}
			headerSize *= 4;
			break;
		default:
			return false;
		}
		if ((ulong)((long)reader.Position + (long)codeSize) > (ulong)reader.Length)
		{
			return false;
		}
		return true;
	}

	private IList<TypeSig> ReadLocals()
	{
		if (!(opResolver.ResolveToken(localVarSigTok, gpContext) is StandAloneSig { LocalSig: var localSig }))
		{
			return null;
		}
		return localSig?.Locals;
	}

	private void ReadInstructions()
	{
		ReadInstructionsNumBytes(codeSize);
	}

	protected override IField ReadInlineField(Instruction instr)
	{
		return opResolver.ResolveToken(reader.ReadUInt32(), gpContext) as IField;
	}

	protected override IMethod ReadInlineMethod(Instruction instr)
	{
		return opResolver.ResolveToken(reader.ReadUInt32(), gpContext) as IMethod;
	}

	protected override MethodSig ReadInlineSig(Instruction instr)
	{
		if (!(opResolver.ResolveToken(reader.ReadUInt32(), gpContext) is StandAloneSig { MethodSig: var methodSig } standAloneSig))
		{
			return null;
		}
		if (methodSig != null)
		{
			methodSig.OriginalToken = standAloneSig.MDToken.Raw;
		}
		return methodSig;
	}

	protected override string ReadInlineString(Instruction instr)
	{
		return opResolver.ReadUserString(reader.ReadUInt32()) ?? string.Empty;
	}

	protected override ITokenOperand ReadInlineTok(Instruction instr)
	{
		return opResolver.ResolveToken(reader.ReadUInt32(), gpContext) as ITokenOperand;
	}

	protected override ITypeDefOrRef ReadInlineType(Instruction instr)
	{
		return opResolver.ResolveToken(reader.ReadUInt32(), gpContext) as ITypeDefOrRef;
	}

	private void ReadExceptionHandlers(out uint totalBodySize)
	{
		if ((flags & 8) == 0)
		{
			totalBodySize = ((startOfHeader != uint.MaxValue) ? (reader.Position - startOfHeader) : 0u);
			return;
		}
		DataReader? dataReader = exceptionsReader;
		bool flag;
		DataReader ehReader;
		if (dataReader.HasValue)
		{
			flag = false;
			ehReader = exceptionsReader.Value;
		}
		else
		{
			flag = true;
			ehReader = reader;
			ehReader.Position = (ehReader.Position + 3) & 0xFFFFFFFCu;
		}
		byte b = ehReader.ReadByte();
		if ((b & 0x3F) != 1)
		{
			totalBodySize = ((startOfHeader != uint.MaxValue) ? (reader.Position - startOfHeader) : 0u);
			return;
		}
		if ((b & 0x40u) != 0)
		{
			ReadFatExceptionHandlers(ref ehReader);
		}
		else
		{
			ReadSmallExceptionHandlers(ref ehReader);
		}
		if (flag)
		{
			totalBodySize = ((startOfHeader != uint.MaxValue) ? (ehReader.Position - startOfHeader) : 0u);
		}
		else
		{
			totalBodySize = 0u;
		}
	}

	private void ReadFatExceptionHandlers(ref DataReader ehReader)
	{
		ehReader.Position--;
		int num = (int)((ehReader.ReadUInt32() >> 8) / 24);
		for (int i = 0; i < num; i++)
		{
			ExceptionHandler exceptionHandler = new ExceptionHandler((ExceptionHandlerType)ehReader.ReadUInt32());
			uint num2 = ehReader.ReadUInt32();
			exceptionHandler.TryStart = GetInstruction(num2);
			exceptionHandler.TryEnd = GetInstruction(num2 + ehReader.ReadUInt32());
			num2 = ehReader.ReadUInt32();
			exceptionHandler.HandlerStart = GetInstruction(num2);
			exceptionHandler.HandlerEnd = GetInstruction(num2 + ehReader.ReadUInt32());
			if (exceptionHandler.IsCatch)
			{
				exceptionHandler.CatchType = opResolver.ResolveToken(ehReader.ReadUInt32(), gpContext) as ITypeDefOrRef;
			}
			else if (exceptionHandler.IsFilter)
			{
				exceptionHandler.FilterStart = GetInstruction(ehReader.ReadUInt32());
			}
			else
			{
				ehReader.ReadUInt32();
			}
			Add(exceptionHandler);
		}
	}

	private void ReadSmallExceptionHandlers(ref DataReader ehReader)
	{
		int num = (int)((uint)ehReader.ReadByte() / 12u);
		ehReader.Position += 2u;
		for (int i = 0; i < num; i++)
		{
			ExceptionHandler exceptionHandler = new ExceptionHandler((ExceptionHandlerType)ehReader.ReadUInt16());
			uint num2 = ehReader.ReadUInt16();
			exceptionHandler.TryStart = GetInstruction(num2);
			exceptionHandler.TryEnd = GetInstruction(num2 + ehReader.ReadByte());
			num2 = ehReader.ReadUInt16();
			exceptionHandler.HandlerStart = GetInstruction(num2);
			exceptionHandler.HandlerEnd = GetInstruction(num2 + ehReader.ReadByte());
			if (exceptionHandler.IsCatch)
			{
				exceptionHandler.CatchType = opResolver.ResolveToken(ehReader.ReadUInt32(), gpContext) as ITypeDefOrRef;
			}
			else if (exceptionHandler.IsFilter)
			{
				exceptionHandler.FilterStart = GetInstruction(ehReader.ReadUInt32());
			}
			else
			{
				ehReader.ReadUInt32();
			}
			Add(exceptionHandler);
		}
	}

	public CilBody CreateCilBody()
	{
		CilBody result = new CilBody(flags == 2 || (flags & 0x10) != 0, instructions, exceptionHandlers, locals)
		{
			HeaderSize = headerSize,
			MaxStack = maxStack,
			LocalVarSigTok = localVarSigTok,
			MetadataBodySize = totalBodySize
		};
		instructions = null;
		exceptionHandlers = null;
		locals = null;
		return result;
	}
}
