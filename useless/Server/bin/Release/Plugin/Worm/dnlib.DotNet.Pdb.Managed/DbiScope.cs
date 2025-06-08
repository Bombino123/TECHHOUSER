using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class DbiScope : SymbolScope
{
	private readonly struct ConstantInfo
	{
		public readonly string Name;

		public readonly uint SignatureToken;

		public readonly object Value;

		public ConstantInfo(string name, uint signatureToken, object value)
		{
			Name = name;
			SignatureToken = signatureToken;
			Value = value;
		}
	}

	internal readonly struct OemInfo
	{
		public readonly string Name;

		public readonly byte[] Data;

		public OemInfo(string name, byte[] data)
		{
			Name = name;
			Data = data;
		}

		public override string ToString()
		{
			return $"{Name} = ({Data.Length} bytes)";
		}
	}

	private readonly SymbolMethod method;

	private readonly SymbolScope parent;

	internal int startOffset;

	internal int endOffset;

	private readonly List<SymbolScope> childrenList;

	private readonly List<SymbolVariable> localsList;

	private readonly List<SymbolNamespace> namespacesList;

	private List<OemInfo> oemInfos;

	private List<ConstantInfo> constants;

	private static readonly byte[] dotNetOemGuid = new byte[16]
	{
		201, 63, 234, 198, 179, 89, 214, 73, 188, 37,
		9, 2, 187, 171, 180, 96
	};

	public override SymbolMethod Method => method;

	public override SymbolScope Parent => parent;

	public override int StartOffset => startOffset;

	public override int EndOffset => endOffset;

	public override IList<SymbolScope> Children => childrenList;

	public override IList<SymbolVariable> Locals => localsList;

	public override IList<SymbolNamespace> Namespaces => namespacesList;

	public override IList<PdbCustomDebugInfo> CustomDebugInfos => Array2.Empty<PdbCustomDebugInfo>();

	public override PdbImportScope ImportScope => null;

	public string Name { get; private set; }

	public DbiScope(SymbolMethod method, SymbolScope parent, string name, uint offset, uint length)
	{
		this.method = method;
		this.parent = parent;
		Name = name;
		startOffset = (int)offset;
		endOffset = (int)(offset + length);
		childrenList = new List<SymbolScope>();
		localsList = new List<SymbolVariable>();
		namespacesList = new List<SymbolNamespace>();
	}

	public void Read(RecursionCounter counter, ref DataReader reader, uint scopeEnd)
	{
		if (!counter.Increment())
		{
			throw new PdbException("Scopes too deep");
		}
		while (reader.Position < scopeEnd)
		{
			ushort num = reader.ReadUInt16();
			uint num2 = reader.Position + num;
			SymbolType symbolType = (SymbolType)reader.ReadUInt16();
			DbiScope dbiScope = null;
			uint? num3 = null;
			switch (symbolType)
			{
			case SymbolType.S_BLOCK32:
			{
				reader.Position += 4u;
				num3 = reader.ReadUInt32();
				uint length = reader.ReadUInt32();
				PdbAddress pdbAddress = PdbAddress.ReadAddress(ref reader);
				string name = PdbReader.ReadCString(ref reader);
				dbiScope = new DbiScope(method, this, name, pdbAddress.Offset, length);
				break;
			}
			case SymbolType.S_UNAMESPACE:
				namespacesList.Add(new DbiNamespace(PdbReader.ReadCString(ref reader)));
				break;
			case SymbolType.S_MANSLOT:
			{
				DbiVariable dbiVariable = new DbiVariable();
				if (dbiVariable.Read(ref reader))
				{
					localsList.Add(dbiVariable);
				}
				break;
			}
			case SymbolType.S_OEM:
			{
				if ((ulong)((long)reader.Position + 20L) > (ulong)num2 || !ReadAndCompareBytes(ref reader, num2, dotNetOemGuid))
				{
					break;
				}
				reader.Position += 4u;
				string name = ReadUnicodeString(ref reader, num2);
				if (name != null)
				{
					byte[] data = reader.ReadBytes((int)(num2 - reader.Position));
					if (oemInfos == null)
					{
						oemInfos = new List<OemInfo>(1);
					}
					oemInfos.Add(new OemInfo(name, data));
				}
				break;
			}
			case SymbolType.S_MANCONSTANT:
			{
				uint signatureToken = reader.ReadUInt32();
				if (NumericReader.TryReadNumeric(ref reader, num2, out var value))
				{
					string name = PdbReader.ReadCString(ref reader);
					if (constants == null)
					{
						constants = new List<ConstantInfo>();
					}
					constants.Add(new ConstantInfo(name, signatureToken, value));
				}
				break;
			}
			}
			reader.Position = num2;
			if (dbiScope != null)
			{
				dbiScope.Read(counter, ref reader, num3.Value);
				childrenList.Add(dbiScope);
				dbiScope = null;
			}
		}
		counter.Decrement();
		if (reader.Position != scopeEnd)
		{
			Debugger.Break();
		}
	}

	private static string ReadUnicodeString(ref DataReader reader, uint end)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			if ((ulong)((long)reader.Position + 2L) > (ulong)end)
			{
				return null;
			}
			char c = reader.ReadChar();
			if (c == '\0')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private static bool ReadAndCompareBytes(ref DataReader reader, uint end, byte[] bytes)
	{
		if ((ulong)((long)reader.Position + (long)(uint)bytes.Length) > (ulong)end)
		{
			return false;
		}
		for (int i = 0; i < bytes.Length; i++)
		{
			if (reader.ReadByte() != bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	public override IList<PdbConstant> GetConstants(ModuleDef module, GenericParamContext gpContext)
	{
		if (constants == null)
		{
			return Array2.Empty<PdbConstant>();
		}
		PdbConstant[] array = new PdbConstant[constants.Count];
		for (int i = 0; i < array.Length; i++)
		{
			ConstantInfo constantInfo = constants[i];
			array[i] = new PdbConstant(type: ((!(module.ResolveToken(constantInfo.SignatureToken, gpContext) is StandAloneSig standAloneSig)) ? null : (standAloneSig.Signature as FieldSig))?.Type, name: constantInfo.Name, value: constantInfo.Value);
		}
		return array;
	}

	internal byte[] GetSymAttribute(string name)
	{
		if (oemInfos == null)
		{
			return null;
		}
		foreach (OemInfo oemInfo in oemInfos)
		{
			if (oemInfo.Name == name)
			{
				return oemInfo.Data;
			}
		}
		return null;
	}
}
