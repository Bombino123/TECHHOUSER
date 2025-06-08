namespace dnlib.DotNet;

public interface ICorLibTypes
{
	CorLibTypeSig Void { get; }

	CorLibTypeSig Boolean { get; }

	CorLibTypeSig Char { get; }

	CorLibTypeSig SByte { get; }

	CorLibTypeSig Byte { get; }

	CorLibTypeSig Int16 { get; }

	CorLibTypeSig UInt16 { get; }

	CorLibTypeSig Int32 { get; }

	CorLibTypeSig UInt32 { get; }

	CorLibTypeSig Int64 { get; }

	CorLibTypeSig UInt64 { get; }

	CorLibTypeSig Single { get; }

	CorLibTypeSig Double { get; }

	CorLibTypeSig String { get; }

	CorLibTypeSig TypedReference { get; }

	CorLibTypeSig IntPtr { get; }

	CorLibTypeSig UIntPtr { get; }

	CorLibTypeSig Object { get; }

	AssemblyRef AssemblyRef { get; }

	TypeRef GetTypeRef(string @namespace, string name);
}
