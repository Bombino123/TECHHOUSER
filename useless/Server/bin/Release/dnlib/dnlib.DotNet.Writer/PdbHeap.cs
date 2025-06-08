using System;
using dnlib.IO;

namespace dnlib.DotNet.Writer;

public sealed class PdbHeap : HeapBase
{
	private readonly byte[] pdbId;

	private uint entryPoint;

	private ulong referencedTypeSystemTables;

	private bool referencedTypeSystemTablesInitd;

	private int typeSystemTablesCount;

	private readonly uint[] typeSystemTableRows;

	public override string Name => "#Pdb";

	public byte[] PdbId => pdbId;

	public uint EntryPoint
	{
		get
		{
			return entryPoint;
		}
		set
		{
			entryPoint = value;
		}
	}

	public FileOffset PdbIdOffset => base.FileOffset;

	public ulong ReferencedTypeSystemTables
	{
		get
		{
			if (!referencedTypeSystemTablesInitd)
			{
				throw new InvalidOperationException("ReferencedTypeSystemTables hasn't been initialized yet");
			}
			return referencedTypeSystemTables;
		}
		set
		{
			if (isReadOnly)
			{
				throw new InvalidOperationException("Size has already been calculated, can't write a new value");
			}
			referencedTypeSystemTables = value;
			referencedTypeSystemTablesInitd = true;
			typeSystemTablesCount = 0;
			for (ulong num = value; num != 0L; num >>= 1)
			{
				if (((uint)(int)num & (true ? 1u : 0u)) != 0)
				{
					typeSystemTablesCount++;
				}
			}
		}
	}

	public uint[] TypeSystemTableRows => typeSystemTableRows;

	public PdbHeap()
	{
		pdbId = new byte[20];
		typeSystemTableRows = new uint[64];
	}

	public override uint GetRawLength()
	{
		if (!referencedTypeSystemTablesInitd)
		{
			throw new InvalidOperationException("ReferencedTypeSystemTables hasn't been initialized yet");
		}
		return (uint)(pdbId.Length + 4 + 8 + 4 * typeSystemTablesCount);
	}

	protected override void WriteToImpl(DataWriter writer)
	{
		if (!referencedTypeSystemTablesInitd)
		{
			throw new InvalidOperationException("ReferencedTypeSystemTables hasn't been initialized yet");
		}
		writer.WriteBytes(pdbId);
		writer.WriteUInt32(entryPoint);
		writer.WriteUInt64(referencedTypeSystemTables);
		ulong num = referencedTypeSystemTables;
		int num2 = 0;
		while (num2 < typeSystemTableRows.Length)
		{
			if (((uint)(int)num & (true ? 1u : 0u)) != 0)
			{
				writer.WriteUInt32(typeSystemTableRows[num2]);
			}
			num2++;
			num >>= 1;
		}
	}
}
