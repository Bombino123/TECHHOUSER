using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet;

[DebuggerDisplay("RVA = {RVA}, Count = {VTables.Count}")]
[ComVisible(true)]
public sealed class VTableFixups : IEnumerable<VTable>, IEnumerable
{
	private RVA rva;

	private IList<VTable> vtables;

	public RVA RVA
	{
		get
		{
			return rva;
		}
		set
		{
			rva = value;
		}
	}

	public IList<VTable> VTables => vtables;

	public VTableFixups()
	{
		vtables = new List<VTable>();
	}

	public VTableFixups(ModuleDefMD module)
	{
		Initialize(module);
	}

	private void Initialize(ModuleDefMD module)
	{
		ImageDataDirectory vTableFixups = module.Metadata.ImageCor20Header.VTableFixups;
		if (vTableFixups.VirtualAddress == (RVA)0u || vTableFixups.Size == 0)
		{
			vtables = new List<VTable>();
			return;
		}
		rva = vTableFixups.VirtualAddress;
		vtables = new List<VTable>((int)vTableFixups.Size / 8);
		IPEImage pEImage = module.Metadata.PEImage;
		DataReader dataReader = pEImage.CreateReader();
		dataReader.Position = (uint)pEImage.ToFileOffset(vTableFixups.VirtualAddress);
		ulong num = (ulong)dataReader.Position + (ulong)vTableFixups.Size;
		while ((ulong)((long)dataReader.Position + 8L) <= num && dataReader.CanRead(8u))
		{
			RVA rVA = (RVA)dataReader.ReadUInt32();
			int numSlots = dataReader.ReadUInt16();
			VTableFlags flags = (VTableFlags)dataReader.ReadUInt16();
			VTable vTable = new VTable(rVA, flags, numSlots);
			vtables.Add(vTable);
			uint position = dataReader.Position;
			dataReader.Position = (uint)pEImage.ToFileOffset(rVA);
			uint num2 = (vTable.Is64Bit ? 8u : 4u);
			while (numSlots-- > 0 && dataReader.CanRead(num2))
			{
				vTable.Methods.Add(module.ResolveToken(dataReader.ReadUInt32()) as IMethod);
				if (num2 == 8)
				{
					dataReader.ReadUInt32();
				}
			}
			dataReader.Position = position;
		}
	}

	public IEnumerator<VTable> GetEnumerator()
	{
		return vtables.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
