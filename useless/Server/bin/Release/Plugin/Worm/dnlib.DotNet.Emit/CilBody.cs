using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public sealed class CilBody : MethodBody
{
	private bool keepOldMaxStack;

	private bool initLocals;

	private byte headerSize;

	private ushort maxStack;

	private uint localVarSigTok;

	private readonly IList<Instruction> instructions;

	private readonly IList<ExceptionHandler> exceptionHandlers;

	private readonly LocalList localList;

	public const byte SMALL_HEADER_SIZE = 1;

	private PdbMethod pdbMethod;

	public bool KeepOldMaxStack
	{
		get
		{
			return keepOldMaxStack;
		}
		set
		{
			keepOldMaxStack = value;
		}
	}

	public bool InitLocals
	{
		get
		{
			return initLocals;
		}
		set
		{
			initLocals = value;
		}
	}

	public byte HeaderSize
	{
		get
		{
			return headerSize;
		}
		set
		{
			headerSize = value;
		}
	}

	public bool IsSmallHeader => headerSize == 1;

	public bool IsBigHeader => headerSize != 1;

	public ushort MaxStack
	{
		get
		{
			return maxStack;
		}
		set
		{
			maxStack = value;
		}
	}

	public uint LocalVarSigTok
	{
		get
		{
			return localVarSigTok;
		}
		set
		{
			localVarSigTok = value;
		}
	}

	public bool HasInstructions => instructions.Count > 0;

	public IList<Instruction> Instructions => instructions;

	public bool HasExceptionHandlers => exceptionHandlers.Count > 0;

	public IList<ExceptionHandler> ExceptionHandlers => exceptionHandlers;

	public bool HasVariables => localList.Count > 0;

	public LocalList Variables => localList;

	public PdbMethod PdbMethod
	{
		get
		{
			return pdbMethod;
		}
		set
		{
			pdbMethod = value;
		}
	}

	public bool HasPdbMethod => PdbMethod != null;

	internal uint MetadataBodySize { get; set; }

	public CilBody()
	{
		initLocals = true;
		instructions = new List<Instruction>();
		exceptionHandlers = new List<ExceptionHandler>();
		localList = new LocalList();
	}

	public CilBody(bool initLocals, IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers, IList<Local> locals)
	{
		this.initLocals = initLocals;
		this.instructions = instructions;
		this.exceptionHandlers = exceptionHandlers;
		localList = new LocalList(locals);
	}

	public void SimplifyMacros(IList<Parameter> parameters)
	{
		instructions.SimplifyMacros(localList, parameters);
	}

	public void OptimizeMacros()
	{
		instructions.OptimizeMacros();
	}

	public void SimplifyBranches()
	{
		instructions.SimplifyBranches();
	}

	public void OptimizeBranches()
	{
		instructions.OptimizeBranches();
	}

	public uint UpdateInstructionOffsets()
	{
		return instructions.UpdateInstructionOffsets();
	}
}
