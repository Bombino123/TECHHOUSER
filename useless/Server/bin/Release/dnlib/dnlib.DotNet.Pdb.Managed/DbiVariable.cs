using System;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class DbiVariable : SymbolVariable
{
	private string name;

	private PdbLocalAttributes attributes;

	private int index;

	public override string Name => name;

	public override PdbLocalAttributes Attributes => attributes;

	public override int Index => index;

	public override PdbCustomDebugInfo[] CustomDebugInfos => Array2.Empty<PdbCustomDebugInfo>();

	public bool Read(ref DataReader reader)
	{
		index = reader.ReadInt32();
		reader.Position += 10u;
		ushort num = reader.ReadUInt16();
		attributes = GetAttributes(num);
		name = PdbReader.ReadCString(ref reader);
		return (num & 1) == 0;
	}

	private static PdbLocalAttributes GetAttributes(uint flags)
	{
		PdbLocalAttributes pdbLocalAttributes = PdbLocalAttributes.None;
		if ((flags & 4u) != 0)
		{
			pdbLocalAttributes |= PdbLocalAttributes.DebuggerHidden;
		}
		return pdbLocalAttributes;
	}
}
