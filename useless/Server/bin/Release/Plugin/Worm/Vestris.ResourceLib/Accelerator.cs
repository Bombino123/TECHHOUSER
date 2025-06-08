using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class Accelerator
{
	private User32.ACCEL _accel;

	public string Key
	{
		get
		{
			string name = Enum.GetName(typeof(User32.VirtualKeys), _accel.key);
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
			char key = (char)_accel.key;
			return key.ToString();
		}
	}

	public uint Command
	{
		get
		{
			return _accel.cmd;
		}
		set
		{
			_accel.cmd = value;
		}
	}

	internal IntPtr Read(IntPtr lpRes)
	{
		_accel = (User32.ACCEL)Marshal.PtrToStructure(lpRes, typeof(User32.ACCEL));
		return new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_accel));
	}

	internal void Write(BinaryWriter w)
	{
		w.Write(_accel.fVirt);
		w.Write(_accel.key);
		w.Write(_accel.cmd);
		ResourceUtil.PadToWORD(w);
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, {2}", Key, Command, ResourceUtil.FlagsToString<User32.AcceleratorVirtualKey>(_accel.fVirt).Replace(" |", ","));
	}
}
