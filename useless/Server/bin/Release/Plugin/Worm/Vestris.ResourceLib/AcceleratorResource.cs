using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class AcceleratorResource : Resource
{
	private List<Accelerator> _accelerators = new List<Accelerator>();

	public List<Accelerator> Accelerators
	{
		get
		{
			return _accelerators;
		}
		set
		{
			_accelerators = value;
		}
	}

	public AcceleratorResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_ACCELERATOR), null, ResourceUtil.NEUTRALLANGID, 0)
	{
	}

	public AcceleratorResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		long num = _size / Marshal.SizeOf(typeof(User32.ACCEL));
		for (long num2 = 0L; num2 < num; num2++)
		{
			Accelerator accelerator = new Accelerator();
			lpRes = accelerator.Read(lpRes);
			_accelerators.Add(accelerator);
		}
		return lpRes;
	}

	internal override void Write(BinaryWriter w)
	{
		foreach (Accelerator accelerator in _accelerators)
		{
			accelerator.Write(w);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{base.Name} ACCELERATORS");
		stringBuilder.AppendLine("BEGIN");
		foreach (Accelerator accelerator in _accelerators)
		{
			stringBuilder.AppendLine($" {accelerator}");
		}
		stringBuilder.AppendLine("END");
		return stringBuilder.ToString();
	}
}
