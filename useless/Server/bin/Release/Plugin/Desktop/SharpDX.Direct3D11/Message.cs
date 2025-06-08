using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct Message
{
	internal struct __Native
	{
		public MessageCategory Category;

		public MessageSeverity Severity;

		public MessageId Id;

		public IntPtr PDescription;

		public PointerSize DescriptionByteLength;
	}

	public MessageCategory Category;

	public MessageSeverity Severity;

	public MessageId Id;

	public string Description;

	internal PointerSize DescriptionByteLength;

	internal void __MarshalFrom(ref __Native @ref)
	{
		Category = @ref.Category;
		Severity = @ref.Severity;
		Id = @ref.Id;
		Description = ((@ref.PDescription == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(@ref.PDescription, @ref.DescriptionByteLength));
		DescriptionByteLength = @ref.DescriptionByteLength;
	}

	public override string ToString()
	{
		return $"[{Id}] [{Severity}] [{Category}] : {Description}";
	}
}
