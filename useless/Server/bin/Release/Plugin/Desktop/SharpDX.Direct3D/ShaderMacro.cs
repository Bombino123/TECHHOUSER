using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D;

public struct ShaderMacro : IEquatable<ShaderMacro>
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr Name;

		public IntPtr Definition;
	}

	public string Name;

	public string Definition;

	public ShaderMacro(string name, object definition)
	{
		Name = name;
		Definition = definition?.ToString();
	}

	public bool Equals(ShaderMacro other)
	{
		if (string.Equals(Name, other.Name))
		{
			return string.Equals(Definition, other.Definition);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is ShaderMacro)
		{
			return Equals((ShaderMacro)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((Name != null) ? Name.GetHashCode() : 0) * 397) ^ ((Definition != null) ? Definition.GetHashCode() : 0);
	}

	public static bool operator ==(ShaderMacro left, ShaderMacro right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ShaderMacro left, ShaderMacro right)
	{
		return !left.Equals(right);
	}

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.Name);
		Marshal.FreeHGlobal(@ref.Definition);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Name = Marshal.PtrToStringAnsi(@ref.Name);
		Definition = Marshal.PtrToStringAnsi(@ref.Definition);
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Name = Marshal.StringToHGlobalAnsi(Name);
		@ref.Definition = Marshal.StringToHGlobalAnsi(Definition);
	}
}
