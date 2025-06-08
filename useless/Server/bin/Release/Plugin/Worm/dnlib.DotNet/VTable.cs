using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.PE;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class VTable : IEnumerable<IMethod>, IEnumerable
{
	private RVA rva;

	private VTableFlags flags;

	private readonly IList<IMethod> methods;

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

	public VTableFlags Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public bool Is32Bit => (flags & VTableFlags.Bit32) != 0;

	public bool Is64Bit => (flags & VTableFlags.Bit64) != 0;

	public IList<IMethod> Methods => methods;

	public VTable()
	{
		methods = new List<IMethod>();
	}

	public VTable(VTableFlags flags)
	{
		this.flags = flags;
		methods = new List<IMethod>();
	}

	public VTable(RVA rva, VTableFlags flags, int numSlots)
	{
		this.rva = rva;
		this.flags = flags;
		methods = new List<IMethod>(numSlots);
	}

	public VTable(RVA rva, VTableFlags flags, IEnumerable<IMethod> methods)
	{
		this.rva = rva;
		this.flags = flags;
		this.methods = new List<IMethod>(methods);
	}

	public IEnumerator<IMethod> GetEnumerator()
	{
		return methods.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		if (methods.Count == 0)
		{
			return $"{methods.Count} {(uint)rva:X8}";
		}
		return $"{methods.Count} {(uint)rva:X8} {methods[0]}";
	}
}
