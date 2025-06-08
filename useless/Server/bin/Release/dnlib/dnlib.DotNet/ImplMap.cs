using System;
using System.Diagnostics;
using System.IO;
using dnlib.DotNet.MD;

namespace dnlib.DotNet;

[DebuggerDisplay("{Module} {Name}")]
public abstract class ImplMap : IMDTokenProvider
{
	protected uint rid;

	protected int attributes;

	protected UTF8String name;

	protected ModuleRef module;

	private static readonly char[] trimChars = new char[1] { ' ' };

	public MDToken MDToken => new MDToken(Table.ImplMap, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public PInvokeAttributes Attributes
	{
		get
		{
			return (PInvokeAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public ModuleRef Module
	{
		get
		{
			return module;
		}
		set
		{
			module = value;
		}
	}

	public bool IsNoMangle
	{
		get
		{
			return ((ushort)attributes & 1) != 0;
		}
		set
		{
			ModifyAttributes(value, PInvokeAttributes.NoMangle);
		}
	}

	public PInvokeAttributes CharSet
	{
		get
		{
			return (PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask;
		}
		set
		{
			ModifyAttributes(~PInvokeAttributes.CharSetMask, value & PInvokeAttributes.CharSetMask);
		}
	}

	public bool IsCharSetNotSpec => ((ushort)attributes & 6) == 0;

	public bool IsCharSetAnsi => ((ushort)attributes & 6) == 2;

	public bool IsCharSetUnicode => ((ushort)attributes & 6) == 4;

	public bool IsCharSetAuto => ((ushort)attributes & 6) == 6;

	public PInvokeAttributes BestFit
	{
		get
		{
			return (PInvokeAttributes)attributes & PInvokeAttributes.BestFitMask;
		}
		set
		{
			ModifyAttributes(~PInvokeAttributes.BestFitMask, value & PInvokeAttributes.BestFitMask);
		}
	}

	public bool IsBestFitUseAssem => ((ushort)attributes & 0x30) == 0;

	public bool IsBestFitEnabled => ((ushort)attributes & 0x30) == 16;

	public bool IsBestFitDisabled => ((ushort)attributes & 0x30) == 32;

	public PInvokeAttributes ThrowOnUnmappableChar
	{
		get
		{
			return (PInvokeAttributes)attributes & PInvokeAttributes.ThrowOnUnmappableCharMask;
		}
		set
		{
			ModifyAttributes(~PInvokeAttributes.ThrowOnUnmappableCharMask, value & PInvokeAttributes.ThrowOnUnmappableCharMask);
		}
	}

	public bool IsThrowOnUnmappableCharUseAssem => ((ushort)attributes & 0x3000) == 0;

	public bool IsThrowOnUnmappableCharEnabled => ((ushort)attributes & 0x3000) == 4096;

	public bool IsThrowOnUnmappableCharDisabled => ((ushort)attributes & 0x3000) == 8192;

	public bool SupportsLastError
	{
		get
		{
			return ((ushort)attributes & 0x40) != 0;
		}
		set
		{
			ModifyAttributes(value, PInvokeAttributes.SupportsLastError);
		}
	}

	public PInvokeAttributes CallConv
	{
		get
		{
			return (PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask;
		}
		set
		{
			ModifyAttributes(~PInvokeAttributes.CallConvMask, value & PInvokeAttributes.CallConvMask);
		}
	}

	public bool IsCallConvWinapi => ((ushort)attributes & 0x700) == 256;

	public bool IsCallConvCdecl => ((ushort)attributes & 0x700) == 512;

	public bool IsCallConvStdcall => ((ushort)attributes & 0x700) == 768;

	public bool IsCallConvThiscall => ((ushort)attributes & 0x700) == 1024;

	public bool IsCallConvFastcall => ((ushort)attributes & 0x700) == 1280;

	private void ModifyAttributes(PInvokeAttributes andMask, PInvokeAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, PInvokeAttributes flags)
	{
		if (set)
		{
			attributes |= (int)flags;
		}
		else
		{
			attributes &= (int)(~(uint)flags);
		}
	}

	public bool IsPinvokeMethod(string dllName, string funcName)
	{
		return IsPinvokeMethod(dllName, funcName, IsWindows());
	}

	public bool IsPinvokeMethod(string dllName, string funcName, bool treatAsWindows)
	{
		if (name != funcName)
		{
			return false;
		}
		ModuleRef moduleRef = module;
		if (moduleRef == null)
		{
			return false;
		}
		return GetDllName(dllName, treatAsWindows).Equals(GetDllName(moduleRef.Name, treatAsWindows), StringComparison.OrdinalIgnoreCase);
	}

	private static string GetDllName(string dllName, bool treatAsWindows)
	{
		if (treatAsWindows)
		{
			dllName = dllName.TrimEnd(trimChars);
		}
		if (dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
		{
			return dllName.Substring(0, dllName.Length - 4);
		}
		return dllName;
	}

	private static bool IsWindows()
	{
		if (Path.DirectorySeparatorChar != '\\')
		{
			return Path.AltDirectorySeparatorChar == '\\';
		}
		return true;
	}
}
