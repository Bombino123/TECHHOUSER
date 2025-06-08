using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Writer;

internal sealed class SerializerMethodContext
{
	private readonly Dictionary<Instruction, uint> toOffset;

	private readonly IWriterError helper;

	private MethodDef method;

	private CilBody body;

	private uint bodySize;

	private bool dictInitd;

	public bool HasBody => body != null;

	public SerializerMethodContext(IWriterError helper)
	{
		toOffset = new Dictionary<Instruction, uint>();
		this.helper = helper;
	}

	internal void SetBody(MethodDef method)
	{
		if (this.method != method)
		{
			toOffset.Clear();
			this.method = method;
			body = method?.Body;
			dictInitd = false;
		}
	}

	public uint GetOffset(Instruction instr)
	{
		if (!dictInitd)
		{
			if (body == null)
			{
				return 0u;
			}
			InitializeDict();
		}
		if (instr == null)
		{
			return bodySize;
		}
		if (toOffset.TryGetValue(instr, out var value))
		{
			return value;
		}
		helper.Error("Couldn't find an instruction, maybe it was removed. It's still being referenced by some code or by the PDB");
		return bodySize;
	}

	public bool IsSameMethod(MethodDef method)
	{
		return this.method == method;
	}

	private void InitializeDict()
	{
		uint num = 0u;
		IList<Instruction> instructions = body.Instructions;
		for (int i = 0; i < instructions.Count; i++)
		{
			Instruction instruction = instructions[i];
			toOffset[instruction] = num;
			num += (uint)instruction.GetSize();
		}
		bodySize = num;
		dictInitd = true;
	}
}
