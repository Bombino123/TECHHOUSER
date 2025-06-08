using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

public struct InformationQueueMessage
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public Guid Producer;

		public InformationQueueMessageCategory Category;

		public InformationQueueMessageSeverity Severity;

		public int Id;

		public IntPtr PDescription;

		public IntPtr DescriptionByteLength;
	}

	public Guid Producer;

	public InformationQueueMessageCategory Category;

	public InformationQueueMessageSeverity Severity;

	public int Id;

	public string PDescription;

	public PointerSize DescriptionByteLength;

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.PDescription);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Producer = @ref.Producer;
		Category = @ref.Category;
		Severity = @ref.Severity;
		Id = @ref.Id;
		PDescription = Marshal.PtrToStringAnsi(@ref.PDescription);
		DescriptionByteLength = @ref.DescriptionByteLength;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Producer = Producer;
		@ref.Category = Category;
		@ref.Severity = Severity;
		@ref.Id = Id;
		@ref.PDescription = Marshal.StringToHGlobalAnsi(PDescription);
		@ref.DescriptionByteLength = DescriptionByteLength;
	}
}
