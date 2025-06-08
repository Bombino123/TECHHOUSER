using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.DotNet.Pdb.WindowsPdb;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class PdbReader : SymbolReader
{
	private MsfStream[] streams;

	private Dictionary<string, uint> names;

	private Dictionary<uint, string> strings;

	private List<DbiModule> modules;

	private ModuleDef module;

	private const int STREAM_ROOT = 0;

	private const int STREAM_NAMES = 1;

	private const int STREAM_TPI = 2;

	private const int STREAM_DBI = 3;

	private const ushort STREAM_INVALID_INDEX = ushort.MaxValue;

	private Dictionary<string, DbiDocument> documents;

	private Dictionary<int, DbiFunction> functions;

	private byte[] sourcelinkData;

	private byte[] srcsrvData;

	private uint entryPt;

	private readonly Guid expectedGuid;

	private readonly uint expectedAge;

	private volatile SymbolDocument[] documentsResult;

	public override PdbFileKind PdbFileKind => PdbFileKind.WindowsPDB;

	private uint Age { get; set; }

	private Guid Guid { get; set; }

	internal bool MatchesModule
	{
		get
		{
			if (expectedGuid == Guid)
			{
				return expectedAge == Age;
			}
			return false;
		}
	}

	public override IList<SymbolDocument> Documents
	{
		get
		{
			if (documentsResult == null)
			{
				SymbolDocument[] array = new SymbolDocument[documents.Count];
				int num = 0;
				foreach (KeyValuePair<string, DbiDocument> document in documents)
				{
					array[num++] = document.Value;
				}
				documentsResult = array;
			}
			return documentsResult;
		}
	}

	public override int UserEntryPoint => (int)entryPt;

	public PdbReader(Guid expectedGuid, uint expectedAge)
	{
		this.expectedGuid = expectedGuid;
		this.expectedAge = expectedAge;
	}

	public override void Initialize(ModuleDef module)
	{
		this.module = module;
	}

	public void Read(DataReader reader)
	{
		try
		{
			ReadInternal(ref reader);
		}
		catch (Exception ex)
		{
			if (ex is PdbException)
			{
				throw;
			}
			throw new PdbException(ex);
		}
		finally
		{
			streams = null;
			names = null;
			strings = null;
			modules = null;
		}
	}

	private static uint RoundUpDiv(uint value, uint divisor)
	{
		return (value + divisor - 1) / divisor;
	}

	private void ReadInternal(ref DataReader reader)
	{
		if (reader.ReadString(30, Encoding.ASCII) != "Microsoft C/C++ MSF 7.00\r\n\u001aDS\0")
		{
			throw new PdbException("Invalid signature");
		}
		reader.Position += 2u;
		uint num = reader.ReadUInt32();
		reader.ReadUInt32();
		uint num2 = reader.ReadUInt32();
		uint num3 = reader.ReadUInt32();
		reader.ReadUInt32();
		uint num4 = RoundUpDiv(num3, num);
		uint num5 = RoundUpDiv(num4 * 4, num);
		if (num2 * num != reader.Length)
		{
			throw new PdbException("File size mismatch");
		}
		DataReader[] array = new DataReader[num2];
		uint num6 = 0u;
		for (uint num7 = 0u; num7 < num2; num7++)
		{
			array[num7] = reader.Slice(num6, num);
			num6 += num;
		}
		DataReader[] array2 = new DataReader[num4];
		int num8 = 0;
		for (int i = 0; i < num5; i++)
		{
			if (num8 >= num4)
			{
				break;
			}
			DataReader dataReader = array[reader.ReadUInt32()];
			dataReader.Position = 0u;
			while (dataReader.Position < dataReader.Length && num8 < num4)
			{
				array2[num8] = array[dataReader.ReadUInt32()];
				num8++;
			}
		}
		ReadRootDirectory(new MsfStream(array2, num3), array, num);
		ReadNames();
		if (!MatchesModule)
		{
			return;
		}
		ReadStringTable();
		ushort? num9 = ReadModules();
		documents = new Dictionary<string, DbiDocument>(StringComparer.OrdinalIgnoreCase);
		foreach (DbiModule module in modules)
		{
			if (IsValidStreamIndex(module.StreamId))
			{
				module.LoadFunctions(this, ref streams[module.StreamId].Content);
			}
		}
		if (IsValidStreamIndex(num9 ?? ushort.MaxValue))
		{
			ApplyRidMap(ref streams[num9.Value].Content);
		}
		functions = new Dictionary<int, DbiFunction>();
		foreach (DbiModule module2 in modules)
		{
			foreach (DbiFunction function in module2.Functions)
			{
				function.reader = this;
				functions.Add(function.Token, function);
			}
		}
		sourcelinkData = TryGetRawFileData("sourcelink");
		srcsrvData = TryGetRawFileData("srcsrv");
	}

	private byte[] TryGetRawFileData(string name)
	{
		if (!names.TryGetValue(name, out var value))
		{
			return null;
		}
		if (value > 65535 || !IsValidStreamIndex((ushort)value))
		{
			return null;
		}
		return streams[value].Content.ToArray();
	}

	private bool IsValidStreamIndex(ushort index)
	{
		if (index != ushort.MaxValue)
		{
			return index < streams.Length;
		}
		return false;
	}

	private void ReadRootDirectory(MsfStream stream, DataReader[] pages, uint pageSize)
	{
		uint num = stream.Content.ReadUInt32();
		uint[] array = new uint[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = stream.Content.ReadUInt32();
		}
		streams = new MsfStream[num];
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] == uint.MaxValue)
			{
				streams[j] = null;
				continue;
			}
			DataReader[] array2 = new DataReader[RoundUpDiv(array[j], pageSize)];
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = pages[stream.Content.ReadUInt32()];
			}
			streams[j] = new MsfStream(array2, array[j]);
		}
	}

	private void ReadNames()
	{
		ref DataReader content = ref streams[1].Content;
		content.Position = 8u;
		Age = content.ReadUInt32();
		Guid = content.ReadGuid();
		uint num = content.ReadUInt32();
		DataReader reader = content.Slice(content.Position, num);
		content.Position += num;
		content.ReadUInt32();
		uint val = content.ReadUInt32();
		BitArray bitArray = new BitArray(content.ReadBytes(content.ReadInt32() * 4));
		if (content.ReadUInt32() != 0)
		{
			throw new NotSupportedException();
		}
		names = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		val = Math.Min(val, (uint)bitArray.Count);
		for (int i = 0; i < val; i++)
		{
			if (bitArray[i])
			{
				uint position = content.ReadUInt32();
				uint value = content.ReadUInt32();
				reader.Position = position;
				string key = ReadCString(ref reader);
				names[key] = value;
			}
		}
	}

	private void ReadStringTable()
	{
		if (!names.TryGetValue("/names", out var value))
		{
			throw new PdbException("String table not found");
		}
		ref DataReader content = ref streams[value].Content;
		content.Position = 8u;
		uint num = content.ReadUInt32();
		DataReader reader = content.Slice(content.Position, num);
		content.Position += num;
		uint num2 = content.ReadUInt32();
		strings = new Dictionary<uint, string>((int)num2);
		for (uint num3 = 0u; num3 < num2; num3++)
		{
			uint num4 = content.ReadUInt32();
			if (num4 != 0)
			{
				reader.Position = num4;
				strings[num4] = ReadCString(ref reader);
			}
		}
	}

	private static uint ReadSizeField(ref DataReader reader)
	{
		int num = reader.ReadInt32();
		if (num > 0)
		{
			return (uint)num;
		}
		return 0u;
	}

	private ushort? ReadModules()
	{
		ref DataReader content = ref streams[3].Content;
		modules = new List<DbiModule>();
		if (content.Length == 0)
		{
			return null;
		}
		content.Position = 20u;
		ushort num = content.ReadUInt16();
		content.Position += 2u;
		uint num2 = ReadSizeField(ref content);
		uint num3 = 0u;
		num3 += ReadSizeField(ref content);
		num3 += ReadSizeField(ref content);
		num3 += ReadSizeField(ref content);
		num3 += ReadSizeField(ref content);
		content.ReadUInt32();
		uint num4 = ReadSizeField(ref content);
		num3 += ReadSizeField(ref content);
		content.Position += 8u;
		DataReader reader = content.Slice(content.Position, num2);
		while (reader.Position < reader.Length)
		{
			DbiModule dbiModule = new DbiModule();
			dbiModule.Read(ref reader);
			modules.Add(dbiModule);
		}
		if (IsValidStreamIndex(num))
		{
			ReadGlobalSymbols(ref streams[num].Content);
		}
		if (num4 != 0)
		{
			content.Position += num2;
			content.Position += num3;
			content.Position += 12u;
			return content.ReadUInt16();
		}
		return null;
	}

	internal DbiDocument GetDocument(uint nameId)
	{
		string text = strings[nameId];
		if (!documents.TryGetValue(text, out var value))
		{
			value = new DbiDocument(text);
			if (names.TryGetValue("/src/files/" + text, out var value2))
			{
				value.Read(ref streams[value2].Content);
			}
			documents.Add(text, value);
		}
		return value;
	}

	private void ReadGlobalSymbols(ref DataReader reader)
	{
		reader.Position = 0u;
		while (reader.Position < reader.Length)
		{
			ushort num = reader.ReadUInt16();
			uint position = reader.Position + num;
			if (reader.ReadUInt16() == 4366)
			{
				reader.Position += 4u;
				uint num2 = reader.ReadUInt32();
				reader.Position += 2u;
				if (ReadCString(ref reader) == "COM+_Entry_Point")
				{
					entryPt = num2;
					break;
				}
			}
			reader.Position = position;
		}
	}

	private void ApplyRidMap(ref DataReader reader)
	{
		reader.Position = 0u;
		uint[] array = new uint[reader.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = reader.ReadUInt32();
		}
		foreach (DbiModule module in modules)
		{
			foreach (DbiFunction function in module.Functions)
			{
				uint num = (uint)function.Token & 0xFFFFFFu;
				num = array[num];
				function.token = (int)((function.Token & 0xFF000000u) | num);
			}
		}
		if (entryPt != 0)
		{
			uint num2 = entryPt & 0xFFFFFFu;
			num2 = array[num2];
			entryPt = (entryPt & 0xFF000000u) | num2;
		}
	}

	internal static string ReadCString(ref DataReader reader)
	{
		return reader.TryReadZeroTerminatedUtf8String() ?? string.Empty;
	}

	public override SymbolMethod GetMethod(MethodDef method, int version)
	{
		if (version != 1)
		{
			return null;
		}
		if (functions.TryGetValue(method.MDToken.ToInt32(), out var value))
		{
			return value;
		}
		return null;
	}

	internal void GetCustomDebugInfos(DbiFunction symMethod, MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result)
	{
		PdbAsyncMethodCustomDebugInfo pdbAsyncMethodCustomDebugInfo = PseudoCustomDebugInfoFactory.TryCreateAsyncMethod(method.Module, method, body, symMethod.AsyncKickoffMethod, symMethod.AsyncStepInfos, symMethod.AsyncCatchHandlerILOffset);
		if (pdbAsyncMethodCustomDebugInfo != null)
		{
			result.Add(pdbAsyncMethodCustomDebugInfo);
		}
		byte[] symAttribute = symMethod.Root.GetSymAttribute("MD2");
		if (symAttribute != null)
		{
			PdbCustomDebugInfoReader.Read(method, body, result, symAttribute);
		}
	}

	public override void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result)
	{
		if (token == 1)
		{
			GetCustomDebugInfos_ModuleDef(result);
		}
	}

	private void GetCustomDebugInfos_ModuleDef(IList<PdbCustomDebugInfo> result)
	{
		if (sourcelinkData != null)
		{
			result.Add(new PdbSourceLinkCustomDebugInfo(sourcelinkData));
		}
		if (srcsrvData != null)
		{
			result.Add(new PdbSourceServerCustomDebugInfo(srcsrvData));
		}
	}
}
