using System.Collections.Generic;
using System.IO;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.WindowsPdb;

internal sealed class PdbCustomDebugInfoWriterContext
{
	public ILogger Logger;

	public readonly MemoryStream MemoryStream;

	public readonly DataWriter Writer;

	public readonly Dictionary<Instruction, uint> InstructionToOffsetDict;

	public PdbCustomDebugInfoWriterContext()
	{
		MemoryStream = new MemoryStream();
		Writer = new DataWriter(MemoryStream);
		InstructionToOffsetDict = new Dictionary<Instruction, uint>();
	}
}
