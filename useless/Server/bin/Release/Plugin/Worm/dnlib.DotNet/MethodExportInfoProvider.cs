using System;
using System.Collections.Generic;
using System.IO;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet;

internal sealed class MethodExportInfoProvider
{
	private struct NameAndIndex
	{
		public string Name;

		public int Index;
	}

	private readonly Dictionary<uint, MethodExportInfo> toInfo;

	public MethodExportInfoProvider(ModuleDefMD module)
	{
		toInfo = new Dictionary<uint, MethodExportInfo>();
		try
		{
			Initialize(module);
		}
		catch (OutOfMemoryException)
		{
		}
		catch (IOException)
		{
		}
	}

	private void Initialize(ModuleDefMD module)
	{
		ImageDataDirectory vTableFixups = module.Metadata.ImageCor20Header.VTableFixups;
		if (vTableFixups.VirtualAddress == (RVA)0u || vTableFixups.Size == 0)
		{
			return;
		}
		IPEImage pEImage = module.Metadata.PEImage;
		ImageDataDirectory imageDataDirectory = pEImage.ImageNTHeaders.OptionalHeader.DataDirectories[0];
		if (imageDataDirectory.VirtualAddress == (RVA)0u || imageDataDirectory.Size < 40 || !CpuArch.TryGetCpuArch(pEImage.ImageNTHeaders.FileHeader.Machine, out var cpuArch))
		{
			return;
		}
		DataReader reader = pEImage.CreateReader();
		Dictionary<uint, MethodExportInfo> offsetToExportInfoDictionary = GetOffsetToExportInfoDictionary(ref reader, pEImage, imageDataDirectory, cpuArch);
		reader.Position = (uint)pEImage.ToFileOffset(vTableFixups.VirtualAddress);
		ulong num = (ulong)reader.Position + (ulong)vTableFixups.Size;
		while ((ulong)((long)reader.Position + 8L) <= num && reader.CanRead(8u))
		{
			RVA rva = (RVA)reader.ReadUInt32();
			int num2 = reader.ReadUInt16();
			ushort num3 = reader.ReadUInt16();
			bool flag = (num3 & 2) != 0;
			MethodExportInfoOptions options = ToMethodExportInfoOptions((VTableFlags)num3);
			uint position = reader.Position;
			reader.Position = (uint)pEImage.ToFileOffset(rva);
			uint num4 = (flag ? 8u : 4u);
			while (num2-- > 0 && reader.CanRead(num4))
			{
				uint position2 = reader.Position;
				uint key = reader.ReadUInt32();
				if (offsetToExportInfoDictionary.TryGetValue(position2, out var value))
				{
					toInfo[key] = new MethodExportInfo(value.Name, value.Ordinal, options);
				}
				if (num4 == 8)
				{
					reader.ReadUInt32();
				}
			}
			reader.Position = position;
		}
	}

	private static MethodExportInfoOptions ToMethodExportInfoOptions(VTableFlags flags)
	{
		MethodExportInfoOptions methodExportInfoOptions = MethodExportInfoOptions.None;
		if ((flags & VTableFlags.FromUnmanaged) != 0)
		{
			methodExportInfoOptions |= MethodExportInfoOptions.FromUnmanaged;
		}
		if ((flags & VTableFlags.FromUnmanagedRetainAppDomain) != 0)
		{
			methodExportInfoOptions |= MethodExportInfoOptions.FromUnmanagedRetainAppDomain;
		}
		if ((flags & VTableFlags.CallMostDerived) != 0)
		{
			methodExportInfoOptions |= MethodExportInfoOptions.CallMostDerived;
		}
		return methodExportInfoOptions;
	}

	private static Dictionary<uint, MethodExportInfo> GetOffsetToExportInfoDictionary(ref DataReader reader, IPEImage peImage, ImageDataDirectory exportHdr, CpuArch cpuArch)
	{
		reader.Position = (uint)peImage.ToFileOffset(exportHdr.VirtualAddress);
		reader.Position += 16u;
		uint num = reader.ReadUInt32();
		int num2 = reader.ReadInt32();
		int numNames = reader.ReadInt32();
		uint position = (uint)peImage.ToFileOffset((RVA)reader.ReadUInt32());
		uint offsetOfNames = (uint)peImage.ToFileOffset((RVA)reader.ReadUInt32());
		uint offsetOfNameIndexes = (uint)peImage.ToFileOffset((RVA)reader.ReadUInt32());
		NameAndIndex[] array = ReadNames(ref reader, peImage, numNames, offsetOfNames, offsetOfNameIndexes);
		reader.Position = position;
		MethodExportInfo[] array2 = new MethodExportInfo[num2];
		Dictionary<uint, MethodExportInfo> dictionary = new Dictionary<uint, MethodExportInfo>(num2);
		for (int i = 0; i < array2.Length; i++)
		{
			uint position2 = reader.Position + 4;
			uint funcRva = 0u;
			RVA rVA = (RVA)reader.ReadUInt32();
			reader.Position = (uint)peImage.ToFileOffset(rVA);
			uint num3 = (uint)((rVA != 0 && cpuArch.TryGetExportedRvaFromStub(ref reader, peImage, out funcRva)) ? peImage.ToFileOffset((RVA)funcRva) : ((FileOffset)0u));
			MethodExportInfo methodExportInfo = new MethodExportInfo((ushort)(num + (uint)i));
			if (num3 != 0)
			{
				dictionary[num3] = methodExportInfo;
			}
			array2[i] = methodExportInfo;
			reader.Position = position2;
		}
		NameAndIndex[] array3 = array;
		for (int j = 0; j < array3.Length; j++)
		{
			NameAndIndex nameAndIndex = array3[j];
			int index = nameAndIndex.Index;
			if ((uint)index < (uint)num2)
			{
				array2[index].Ordinal = null;
				array2[index].Name = nameAndIndex.Name;
			}
		}
		return dictionary;
	}

	private static NameAndIndex[] ReadNames(ref DataReader reader, IPEImage peImage, int numNames, uint offsetOfNames, uint offsetOfNameIndexes)
	{
		NameAndIndex[] array = new NameAndIndex[numNames];
		reader.Position = offsetOfNameIndexes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Index = reader.ReadUInt16();
		}
		uint num = offsetOfNames;
		int num2 = 0;
		while (num2 < array.Length)
		{
			reader.Position = num;
			uint offset = (uint)peImage.ToFileOffset((RVA)reader.ReadUInt32());
			array[num2].Name = ReadMethodNameASCIIZ(ref reader, offset);
			num2++;
			num += 4;
		}
		return array;
	}

	private static string ReadMethodNameASCIIZ(ref DataReader reader, uint offset)
	{
		reader.Position = offset;
		return reader.TryReadZeroTerminatedUtf8String() ?? string.Empty;
	}

	public MethodExportInfo GetMethodExportInfo(uint token)
	{
		if (toInfo.Count == 0)
		{
			return null;
		}
		if (toInfo.TryGetValue(token, out var value))
		{
			return new MethodExportInfo(value.Name, value.Ordinal, value.Options);
		}
		return null;
	}
}
