using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class DbiFunction : SymbolMethod
{
	internal int token;

	internal PdbReader reader;

	private List<SymbolSequencePoint> lines;

	private const string asyncMethodInfoAttributeName = "asyncMethodInfo";

	private volatile SymbolAsyncStepInfo[] asyncStepInfos;

	public override int Token => token;

	public string Name { get; private set; }

	public PdbAddress Address { get; private set; }

	public DbiScope Root { get; private set; }

	public List<SymbolSequencePoint> Lines
	{
		get
		{
			return lines;
		}
		set
		{
			lines = value;
		}
	}

	public override SymbolScope RootScope => Root;

	public override IList<SymbolSequencePoint> SequencePoints
	{
		get
		{
			List<SymbolSequencePoint> list = lines;
			if (list == null)
			{
				return Array2.Empty<SymbolSequencePoint>();
			}
			return list;
		}
	}

	public int AsyncKickoffMethod
	{
		get
		{
			byte[] symAttribute = Root.GetSymAttribute("asyncMethodInfo");
			if (symAttribute == null || symAttribute.Length < 4)
			{
				return 0;
			}
			return BitConverter.ToInt32(symAttribute, 0);
		}
	}

	public uint? AsyncCatchHandlerILOffset
	{
		get
		{
			byte[] symAttribute = Root.GetSymAttribute("asyncMethodInfo");
			if (symAttribute == null || symAttribute.Length < 8)
			{
				return null;
			}
			uint num = BitConverter.ToUInt32(symAttribute, 4);
			if (num != uint.MaxValue)
			{
				return num;
			}
			return null;
		}
	}

	public IList<SymbolAsyncStepInfo> AsyncStepInfos
	{
		get
		{
			if (asyncStepInfos == null)
			{
				asyncStepInfos = CreateSymbolAsyncStepInfos();
			}
			return asyncStepInfos;
		}
	}

	public void Read(ref DataReader reader, uint recEnd)
	{
		reader.Position += 4u;
		uint scopeEnd = reader.ReadUInt32();
		reader.Position += 4u;
		uint length = reader.ReadUInt32();
		reader.Position += 8u;
		token = reader.ReadInt32();
		Address = PdbAddress.ReadAddress(ref reader);
		reader.Position += 3u;
		Name = PdbReader.ReadCString(ref reader);
		reader.Position = recEnd;
		Root = new DbiScope(this, null, "", Address.Offset, length);
		Root.Read(default(RecursionCounter), ref reader, scopeEnd);
		FixOffsets(default(RecursionCounter), Root);
	}

	private void FixOffsets(RecursionCounter counter, DbiScope scope)
	{
		if (counter.Increment())
		{
			scope.startOffset -= (int)Address.Offset;
			scope.endOffset -= (int)Address.Offset;
			IList<SymbolScope> children = scope.Children;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				FixOffsets(counter, (DbiScope)children[i]);
			}
			counter.Decrement();
		}
	}

	private SymbolAsyncStepInfo[] CreateSymbolAsyncStepInfos()
	{
		byte[] symAttribute = Root.GetSymAttribute("asyncMethodInfo");
		if (symAttribute == null || symAttribute.Length < 12)
		{
			return Array2.Empty<SymbolAsyncStepInfo>();
		}
		int num = 8;
		int num2 = BitConverter.ToInt32(symAttribute, num);
		num += 4;
		if (num + (long)num2 * 12L > symAttribute.Length)
		{
			return Array2.Empty<SymbolAsyncStepInfo>();
		}
		if (num2 == 0)
		{
			return Array2.Empty<SymbolAsyncStepInfo>();
		}
		SymbolAsyncStepInfo[] array = new SymbolAsyncStepInfo[num2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new SymbolAsyncStepInfo(BitConverter.ToUInt32(symAttribute, num), BitConverter.ToUInt32(symAttribute, num + 8), BitConverter.ToUInt32(symAttribute, num + 4));
			num += 12;
		}
		return array;
	}

	public override void GetCustomDebugInfos(MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result)
	{
		reader.GetCustomDebugInfos(this, method, body, result);
	}
}
