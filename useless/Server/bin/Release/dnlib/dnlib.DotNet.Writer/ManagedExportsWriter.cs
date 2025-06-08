using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

internal sealed class ManagedExportsWriter
{
	private sealed class ExportDir : IChunk
	{
		private readonly ManagedExportsWriter owner;

		public FileOffset FileOffset => owner.ExportDirOffset;

		public RVA RVA => owner.ExportDirRVA;

		public ExportDir(ManagedExportsWriter owner)
		{
			this.owner = owner;
		}

		void IChunk.SetOffset(FileOffset offset, RVA rva)
		{
			throw new NotSupportedException();
		}

		public uint GetFileLength()
		{
			return owner.ExportDirSize;
		}

		public uint GetVirtualSize()
		{
			return GetFileLength();
		}

		public uint CalculateAlignment()
		{
			return 0u;
		}

		void IChunk.WriteTo(DataWriter writer)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class VtableFixupsChunk : IChunk
	{
		private readonly ManagedExportsWriter owner;

		private FileOffset offset;

		private RVA rva;

		internal uint length;

		public FileOffset FileOffset => offset;

		public RVA RVA => rva;

		public VtableFixupsChunk(ManagedExportsWriter owner)
		{
			this.owner = owner;
		}

		public void SetOffset(FileOffset offset, RVA rva)
		{
			this.offset = offset;
			this.rva = rva;
		}

		public uint GetFileLength()
		{
			return length;
		}

		public uint GetVirtualSize()
		{
			return GetFileLength();
		}

		public uint CalculateAlignment()
		{
			return 0u;
		}

		public void WriteTo(DataWriter writer)
		{
			owner.WriteVtableFixups(writer);
		}
	}

	private sealed class StubsChunk : IChunk
	{
		private readonly ManagedExportsWriter owner;

		private FileOffset offset;

		private RVA rva;

		internal uint length;

		public FileOffset FileOffset => offset;

		public RVA RVA => rva;

		public StubsChunk(ManagedExportsWriter owner)
		{
			this.owner = owner;
		}

		public void SetOffset(FileOffset offset, RVA rva)
		{
			this.offset = offset;
			this.rva = rva;
		}

		public uint GetFileLength()
		{
			return length;
		}

		public uint GetVirtualSize()
		{
			return GetFileLength();
		}

		public uint CalculateAlignment()
		{
			return 0u;
		}

		public void WriteTo(DataWriter writer)
		{
			owner.WriteStubs(writer);
		}
	}

	private sealed class SdataChunk : IChunk
	{
		private readonly ManagedExportsWriter owner;

		private FileOffset offset;

		private RVA rva;

		internal uint length;

		public FileOffset FileOffset => offset;

		public RVA RVA => rva;

		public SdataChunk(ManagedExportsWriter owner)
		{
			this.owner = owner;
		}

		public void SetOffset(FileOffset offset, RVA rva)
		{
			this.offset = offset;
			this.rva = rva;
		}

		public uint GetFileLength()
		{
			return length;
		}

		public uint GetVirtualSize()
		{
			return GetFileLength();
		}

		public uint CalculateAlignment()
		{
			return 0u;
		}

		public void WriteTo(DataWriter writer)
		{
			owner.WriteSdata(writer);
		}
	}

	private sealed class MethodInfo
	{
		public readonly MethodDef Method;

		public readonly uint StubChunkOffset;

		public int FunctionIndex;

		public uint ManagedVtblOffset;

		public uint NameOffset;

		public int NameIndex;

		public byte[] NameBytes;

		public MethodInfo(MethodDef method, uint stubChunkOffset)
		{
			Method = method;
			StubChunkOffset = stubChunkOffset;
		}
	}

	private sealed class VTableInfo
	{
		public readonly VTableFlags Flags;

		public readonly List<MethodInfo> Methods;

		public uint SdataChunkOffset { get; set; }

		public VTableInfo(VTableFlags flags)
		{
			Flags = flags;
			Methods = new List<MethodInfo>();
		}
	}

	private struct NamesBlob
	{
		private readonly struct NameInfo
		{
			public readonly uint Offset;

			public readonly byte[] Bytes;

			public NameInfo(uint offset, byte[] bytes)
			{
				Offset = offset;
				Bytes = bytes;
			}
		}

		private readonly Dictionary<string, NameInfo> nameOffsets;

		private readonly List<byte[]> names;

		private readonly List<uint> methodNameOffsets;

		private uint currentOffset;

		private int methodNamesCount;

		private bool methodNamesIsFrozen;

		public int MethodNamesCount => methodNamesCount;

		public NamesBlob(bool dummy)
		{
			nameOffsets = new Dictionary<string, NameInfo>(StringComparer.Ordinal);
			names = new List<byte[]>();
			methodNameOffsets = new List<uint>();
			currentOffset = 0u;
			methodNamesCount = 0;
			methodNamesIsFrozen = false;
		}

		public uint GetMethodNameOffset(string name, out byte[] bytes)
		{
			if (methodNamesIsFrozen)
			{
				throw new InvalidOperationException();
			}
			methodNamesCount++;
			uint offset = GetOffset(name, out bytes);
			methodNameOffsets.Add(offset);
			return offset;
		}

		public uint GetOtherNameOffset(string name)
		{
			methodNamesIsFrozen = true;
			byte[] bytes;
			return GetOffset(name, out bytes);
		}

		private uint GetOffset(string name, out byte[] bytes)
		{
			if (nameOffsets.TryGetValue(name, out var value))
			{
				bytes = value.Bytes;
				return value.Offset;
			}
			bytes = GetNameASCIIZ(name);
			names.Add(bytes);
			uint num = currentOffset;
			nameOffsets.Add(name, new NameInfo(num, bytes));
			currentOffset += (uint)bytes.Length;
			return num;
		}

		private static byte[] GetNameASCIIZ(string name)
		{
			byte[] array = new byte[Encoding.UTF8.GetByteCount(name) + 1];
			Encoding.UTF8.GetBytes(name, 0, name.Length, array, 0);
			if (array[^1] != 0)
			{
				throw new ModuleWriterException();
			}
			return array;
		}

		public void Write(DataWriter writer)
		{
			foreach (byte[] name in names)
			{
				writer.WriteBytes(name);
			}
		}

		public uint[] GetMethodNameOffsets()
		{
			return methodNameOffsets.ToArray();
		}
	}

	private struct SdataBytesInfo
	{
		public byte[] Data;

		public uint namesBlobStreamOffset;

		public uint moduleNameOffset;

		public uint exportDirModuleNameStreamOffset;

		public uint exportDirAddressOfFunctionsStreamOffset;

		public uint addressOfFunctionsStreamOffset;

		public uint addressOfNamesStreamOffset;

		public uint addressOfNameOrdinalsStreamOffset;

		public uint[] MethodNameOffsets;
	}

	private const uint DEFAULT_VTBL_FIXUPS_ALIGNMENT = 4u;

	private const uint DEFAULT_SDATA_ALIGNMENT = 8u;

	private const StubType stubType = StubType.Export;

	private readonly string moduleName;

	private readonly Machine machine;

	private readonly RelocDirectory relocDirectory;

	private readonly Metadata metadata;

	private readonly PEHeaders peHeaders;

	private readonly Action<string, object[]> logError;

	private readonly VtableFixupsChunk vtableFixups;

	private readonly StubsChunk stubsChunk;

	private readonly SdataChunk sdataChunk;

	private readonly ExportDir exportDir;

	private readonly List<VTableInfo> vtables;

	private readonly List<MethodInfo> allMethodInfos;

	private readonly List<MethodInfo> sortedOrdinalMethodInfos;

	private readonly List<MethodInfo> sortedNameMethodInfos;

	private readonly CpuArch cpuArch;

	private uint exportDirOffset;

	private SdataBytesInfo sdataBytesInfo;

	private bool Is64Bit => machine.Is64Bit();

	private FileOffset ExportDirOffset => sdataChunk.FileOffset + exportDirOffset;

	private RVA ExportDirRVA => sdataChunk.RVA + exportDirOffset;

	private uint ExportDirSize => 40u;

	internal bool HasExports => vtables.Count != 0;

	public ManagedExportsWriter(string moduleName, Machine machine, RelocDirectory relocDirectory, Metadata metadata, PEHeaders peHeaders, Action<string, object[]> logError)
	{
		this.moduleName = moduleName;
		this.machine = machine;
		this.relocDirectory = relocDirectory;
		this.metadata = metadata;
		this.peHeaders = peHeaders;
		this.logError = logError;
		vtableFixups = new VtableFixupsChunk(this);
		stubsChunk = new StubsChunk(this);
		sdataChunk = new SdataChunk(this);
		exportDir = new ExportDir(this);
		vtables = new List<VTableInfo>();
		allMethodInfos = new List<MethodInfo>();
		sortedOrdinalMethodInfos = new List<MethodInfo>();
		sortedNameMethodInfos = new List<MethodInfo>();
		CpuArch.TryGetCpuArch(machine, out cpuArch);
	}

	internal void AddTextChunks(PESection textSection)
	{
		textSection.Add(vtableFixups, 4u);
		if (cpuArch != null)
		{
			textSection.Add(stubsChunk, cpuArch.GetStubAlignment(StubType.Export));
		}
	}

	internal void AddSdataChunks(PESection sdataSection)
	{
		sdataSection.Add(sdataChunk, 8u);
	}

	internal void InitializeChunkProperties()
	{
		if (allMethodInfos.Count != 0)
		{
			peHeaders.ExportDirectory = exportDir;
			peHeaders.ImageCor20Header.VtableFixups = vtableFixups;
		}
	}

	internal void AddExportedMethods(List<MethodDef> methods, uint timestamp)
	{
		if (methods.Count != 0)
		{
			if (cpuArch == null)
			{
				logError("The module has exported methods but the CPU architecture isn't supported: {0} (0x{1:X4})", new object[2]
				{
					machine,
					(ushort)machine
				});
			}
			else if (methods.Count > 65536)
			{
				logError("Too many methods have been exported. No more than 2^16 methods can be exported. Number of exported methods: {0}", new object[1] { methods.Count });
			}
			else
			{
				Initialize(methods, timestamp);
			}
		}
	}

	private void Initialize(List<MethodDef> methods, uint timestamp)
	{
		Dictionary<int, List<VTableInfo>> dictionary = new Dictionary<int, List<VTableInfo>>();
		VTableFlags vTableFlags = ((!Is64Bit) ? VTableFlags.Bit32 : VTableFlags.Bit64);
		uint num = 0u;
		uint stubAlignment = cpuArch.GetStubAlignment(StubType.Export);
		uint stubCodeOffset = cpuArch.GetStubCodeOffset(StubType.Export);
		uint stubSize = cpuArch.GetStubSize(StubType.Export);
		foreach (MethodDef method in methods)
		{
			MethodExportInfo exportInfo = method.ExportInfo;
			if (exportInfo != null)
			{
				VTableFlags vTableFlags2 = vTableFlags;
				if ((exportInfo.Options & MethodExportInfoOptions.FromUnmanaged) != 0)
				{
					vTableFlags2 |= VTableFlags.FromUnmanaged;
				}
				if ((exportInfo.Options & MethodExportInfoOptions.FromUnmanagedRetainAppDomain) != 0)
				{
					vTableFlags2 |= VTableFlags.FromUnmanagedRetainAppDomain;
				}
				if ((exportInfo.Options & MethodExportInfoOptions.CallMostDerived) != 0)
				{
					vTableFlags2 |= VTableFlags.CallMostDerived;
				}
				if (!dictionary.TryGetValue((int)vTableFlags2, out var value))
				{
					dictionary.Add((int)vTableFlags2, value = new List<VTableInfo>());
				}
				if (value.Count == 0 || value[value.Count - 1].Methods.Count >= 65535)
				{
					value.Add(new VTableInfo(vTableFlags2));
				}
				MethodInfo item = new MethodInfo(method, num + stubCodeOffset);
				allMethodInfos.Add(item);
				value[value.Count - 1].Methods.Add(item);
				num = (num + stubSize + stubAlignment - 1) & ~(stubAlignment - 1);
			}
		}
		foreach (KeyValuePair<int, List<VTableInfo>> item2 in dictionary)
		{
			vtables.AddRange(item2.Value);
		}
		WriteSdataBlob(timestamp);
		vtableFixups.length = (uint)(vtables.Count * 8);
		stubsChunk.length = num;
		sdataChunk.length = (uint)sdataBytesInfo.Data.Length;
		uint num2 = 0u;
		foreach (MethodInfo allMethodInfo in allMethodInfos)
		{
			uint num3 = allMethodInfo.StubChunkOffset - stubCodeOffset;
			if (num2 != num3)
			{
				throw new InvalidOperationException();
			}
			cpuArch.WriteStubRelocs(StubType.Export, relocDirectory, stubsChunk, num3);
			num2 = (num3 + stubSize + stubAlignment - 1) & ~(stubAlignment - 1);
		}
		if (num2 != num)
		{
			throw new InvalidOperationException();
		}
	}

	private void WriteSdataBlob(uint timestamp)
	{
		MemoryStream memoryStream = new MemoryStream();
		DataWriter dataWriter = new DataWriter(memoryStream);
		foreach (VTableInfo vtable in vtables)
		{
			vtable.SdataChunkOffset = (uint)dataWriter.Position;
			foreach (MethodInfo method in vtable.Methods)
			{
				method.ManagedVtblOffset = (uint)dataWriter.Position;
				dataWriter.WriteUInt32(100663296 + metadata.GetRid(method.Method));
				if ((vtable.Flags & VTableFlags.Bit64) != 0)
				{
					dataWriter.WriteUInt32(0u);
				}
			}
		}
		NamesBlob namesBlob = new NamesBlob(dummy: false);
		int num = 0;
		foreach (MethodInfo allMethodInfo in allMethodInfos)
		{
			MethodExportInfo exportInfo = allMethodInfo.Method.ExportInfo;
			string text = exportInfo.Name;
			if (text == null)
			{
				if (exportInfo.Ordinal.HasValue)
				{
					sortedOrdinalMethodInfos.Add(allMethodInfo);
					continue;
				}
				text = allMethodInfo.Method.Name;
			}
			if (string.IsNullOrEmpty(text))
			{
				logError("Exported method name is null or empty, method: {0} (0x{1:X8})", new object[2]
				{
					allMethodInfo.Method,
					allMethodInfo.Method.MDToken.Raw
				});
			}
			else
			{
				allMethodInfo.NameOffset = namesBlob.GetMethodNameOffset(text, out allMethodInfo.NameBytes);
				allMethodInfo.NameIndex = num++;
				sortedNameMethodInfos.Add(allMethodInfo);
			}
		}
		sdataBytesInfo.MethodNameOffsets = namesBlob.GetMethodNameOffsets();
		sdataBytesInfo.moduleNameOffset = namesBlob.GetOtherNameOffset(moduleName);
		sortedOrdinalMethodInfos.Sort((MethodInfo a, MethodInfo b) => a.Method.ExportInfo.Ordinal.Value.CompareTo(b.Method.ExportInfo.Ordinal.Value));
		sortedNameMethodInfos.Sort((MethodInfo a, MethodInfo b) => CompareTo(a.NameBytes, b.NameBytes));
		int num2;
		int num3;
		if (sortedOrdinalMethodInfos.Count == 0)
		{
			num2 = 0;
			num3 = 0;
		}
		else
		{
			num2 = sortedOrdinalMethodInfos[0].Method.ExportInfo.Ordinal.Value;
			num3 = sortedOrdinalMethodInfos[sortedOrdinalMethodInfos.Count - 1].Method.ExportInfo.Ordinal.Value + 1;
		}
		int num4 = num3 - num2;
		int num5 = 0;
		for (int i = 0; i < sortedOrdinalMethodInfos.Count; i++)
		{
			int num6 = sortedOrdinalMethodInfos[i].Method.ExportInfo.Ordinal.Value - num2;
			sortedOrdinalMethodInfos[i].FunctionIndex = num6;
			num5 = num6;
		}
		for (int j = 0; j < sortedNameMethodInfos.Count; j++)
		{
			num5 = num4 + j;
			sortedNameMethodInfos[j].FunctionIndex = num5;
		}
		int num7 = num5 + 1;
		if (num7 > 65536)
		{
			logError("Exported function array is too big", Array2.Empty<object>());
			return;
		}
		exportDirOffset = (uint)dataWriter.Position;
		dataWriter.WriteUInt32(0u);
		dataWriter.WriteUInt32(timestamp);
		dataWriter.WriteUInt32(0u);
		sdataBytesInfo.exportDirModuleNameStreamOffset = (uint)dataWriter.Position;
		dataWriter.WriteUInt32(0u);
		dataWriter.WriteInt32(num2);
		dataWriter.WriteUInt32((uint)num7);
		dataWriter.WriteInt32(sdataBytesInfo.MethodNameOffsets.Length);
		sdataBytesInfo.exportDirAddressOfFunctionsStreamOffset = (uint)dataWriter.Position;
		dataWriter.WriteUInt32(0u);
		dataWriter.WriteUInt32(0u);
		dataWriter.WriteUInt32(0u);
		sdataBytesInfo.addressOfFunctionsStreamOffset = (uint)dataWriter.Position;
		dataWriter.WriteZeroes(num7 * 4);
		sdataBytesInfo.addressOfNamesStreamOffset = (uint)dataWriter.Position;
		dataWriter.WriteZeroes(sdataBytesInfo.MethodNameOffsets.Length * 4);
		sdataBytesInfo.addressOfNameOrdinalsStreamOffset = (uint)dataWriter.Position;
		dataWriter.WriteZeroes(sdataBytesInfo.MethodNameOffsets.Length * 2);
		sdataBytesInfo.namesBlobStreamOffset = (uint)dataWriter.Position;
		namesBlob.Write(dataWriter);
		sdataBytesInfo.Data = memoryStream.ToArray();
	}

	private void WriteSdata(DataWriter writer)
	{
		if (sdataBytesInfo.Data != null)
		{
			PatchSdataBytesBlob();
			writer.WriteBytes(sdataBytesInfo.Data);
		}
	}

	private void PatchSdataBytesBlob()
	{
		uint rVA = (uint)sdataChunk.RVA;
		uint num = rVA + sdataBytesInfo.namesBlobStreamOffset;
		DataWriter dataWriter = new DataWriter(new MemoryStream(sdataBytesInfo.Data));
		dataWriter.Position = sdataBytesInfo.exportDirModuleNameStreamOffset;
		dataWriter.WriteUInt32(num + sdataBytesInfo.moduleNameOffset);
		dataWriter.Position = sdataBytesInfo.exportDirAddressOfFunctionsStreamOffset;
		dataWriter.WriteUInt32(rVA + sdataBytesInfo.addressOfFunctionsStreamOffset);
		if (sdataBytesInfo.MethodNameOffsets.Length != 0)
		{
			dataWriter.WriteUInt32(rVA + sdataBytesInfo.addressOfNamesStreamOffset);
			dataWriter.WriteUInt32(rVA + sdataBytesInfo.addressOfNameOrdinalsStreamOffset);
		}
		uint rVA2 = (uint)stubsChunk.RVA;
		dataWriter.Position = sdataBytesInfo.addressOfFunctionsStreamOffset;
		int num2 = 0;
		foreach (MethodInfo sortedOrdinalMethodInfo in sortedOrdinalMethodInfos)
		{
			int num3 = sortedOrdinalMethodInfo.FunctionIndex - num2;
			if (num3 < 0)
			{
				throw new InvalidOperationException();
			}
			while (num3-- > 0)
			{
				dataWriter.WriteInt32(0);
			}
			dataWriter.WriteUInt32(rVA2 + sortedOrdinalMethodInfo.StubChunkOffset);
			num2 = sortedOrdinalMethodInfo.FunctionIndex + 1;
		}
		foreach (MethodInfo sortedNameMethodInfo in sortedNameMethodInfos)
		{
			if (sortedNameMethodInfo.FunctionIndex != num2++)
			{
				throw new InvalidOperationException();
			}
			dataWriter.WriteUInt32(rVA2 + sortedNameMethodInfo.StubChunkOffset);
		}
		uint[] methodNameOffsets = sdataBytesInfo.MethodNameOffsets;
		if (methodNameOffsets.Length == 0)
		{
			return;
		}
		dataWriter.Position = sdataBytesInfo.addressOfNamesStreamOffset;
		foreach (MethodInfo sortedNameMethodInfo2 in sortedNameMethodInfos)
		{
			dataWriter.WriteUInt32(num + methodNameOffsets[sortedNameMethodInfo2.NameIndex]);
		}
		dataWriter.Position = sdataBytesInfo.addressOfNameOrdinalsStreamOffset;
		foreach (MethodInfo sortedNameMethodInfo3 in sortedNameMethodInfos)
		{
			dataWriter.WriteUInt16((ushort)sortedNameMethodInfo3.FunctionIndex);
		}
	}

	private void WriteVtableFixups(DataWriter writer)
	{
		if (vtables.Count == 0)
		{
			return;
		}
		foreach (VTableInfo vtable in vtables)
		{
			writer.WriteUInt32((uint)(sdataChunk.RVA + vtable.SdataChunkOffset));
			writer.WriteUInt16((ushort)vtable.Methods.Count);
			writer.WriteUInt16((ushort)vtable.Flags);
		}
	}

	private void WriteStubs(DataWriter writer)
	{
		if (vtables.Count == 0 || cpuArch == null)
		{
			return;
		}
		ulong imageBase = peHeaders.ImageBase;
		uint rVA = (uint)stubsChunk.RVA;
		uint rVA2 = (uint)sdataChunk.RVA;
		uint num = 0u;
		uint stubCodeOffset = cpuArch.GetStubCodeOffset(StubType.Export);
		uint stubSize = cpuArch.GetStubSize(StubType.Export);
		uint stubAlignment = cpuArch.GetStubAlignment(StubType.Export);
		int num2 = (int)(((stubSize + stubAlignment - 1) & ~(stubAlignment - 1)) - stubSize);
		foreach (MethodInfo allMethodInfo in allMethodInfos)
		{
			uint num3 = allMethodInfo.StubChunkOffset - stubCodeOffset;
			if (num != num3)
			{
				throw new InvalidOperationException();
			}
			long position = writer.Position;
			cpuArch.WriteStub(StubType.Export, writer, imageBase, rVA + num3, rVA2 + allMethodInfo.ManagedVtblOffset);
			if (position + stubSize != writer.Position)
			{
				throw new InvalidOperationException();
			}
			if (num2 != 0)
			{
				writer.WriteZeroes(num2);
			}
			num = (num3 + stubSize + stubAlignment - 1) & ~(stubAlignment - 1);
		}
		if (num == stubsChunk.length)
		{
			return;
		}
		throw new InvalidOperationException();
	}

	private static int CompareTo(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return 0;
		}
		int num = Math.Min(a.Length, b.Length);
		for (int i = 0; i < num; i++)
		{
			int num2 = a[i] - b[i];
			if (num2 != 0)
			{
				return num2;
			}
		}
		return a.Length - b.Length;
	}
}
