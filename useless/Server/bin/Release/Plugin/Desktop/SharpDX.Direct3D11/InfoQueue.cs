using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("6543dbb6-1b48-42f5-ab82-e97ec74326f6")]
public class InfoQueue : ComObject
{
	public long MessageCountLimit
	{
		get
		{
			return GetMessageCountLimit();
		}
		set
		{
			SetMessageCountLimit(value);
		}
	}

	public long NumMessagesAllowedByStorageFilter => GetNumMessagesAllowedByStorageFilter();

	public long NumMessagesDeniedByStorageFilter => GetNumMessagesDeniedByStorageFilter();

	public long NumStoredMessages => GetNumStoredMessages();

	public long NumStoredMessagesAllowedByRetrievalFilter => GetNumStoredMessagesAllowedByRetrievalFilter();

	public long NumMessagesDiscardedByMessageCountLimit => GetNumMessagesDiscardedByMessageCountLimit();

	public int StorageFilterStackSize => GetStorageFilterStackSize();

	public int RetrievalFilterStackSize => GetRetrievalFilterStackSize();

	public RawBool MuteDebugOutput
	{
		get
		{
			return GetMuteDebugOutput();
		}
		set
		{
			SetMuteDebugOutput(value);
		}
	}

	public unsafe Message GetMessage(long messageIndex)
	{
		PointerSize messageByteLengthRef = 0;
		GetMessage(messageIndex, IntPtr.Zero, ref messageByteLengthRef);
		if (messageByteLengthRef == 0)
		{
			return default(Message);
		}
		byte* ptr = stackalloc byte[(int)(uint)(int)messageByteLengthRef];
		GetMessage(messageIndex, new IntPtr(ptr), ref messageByteLengthRef);
		Message result = default(Message);
		result.__MarshalFrom(ref *(Message.__Native*)ptr);
		return result;
	}

	public unsafe InfoQueueFilter GetStorageFilter()
	{
		PointerSize filterByteLengthRef = PointerSize.Zero;
		GetStorageFilter(IntPtr.Zero, ref filterByteLengthRef);
		if (filterByteLengthRef == 0)
		{
			return null;
		}
		byte* ptr = stackalloc byte[(int)(uint)(int)filterByteLengthRef];
		GetStorageFilter((IntPtr)ptr, ref filterByteLengthRef);
		InfoQueueFilter infoQueueFilter = new InfoQueueFilter();
		infoQueueFilter.__MarshalFrom(ref *(InfoQueueFilter.__Native*)ptr);
		return infoQueueFilter;
	}

	public unsafe InfoQueueFilter GetRetrievalFilter()
	{
		PointerSize filterByteLengthRef = PointerSize.Zero;
		GetRetrievalFilter(IntPtr.Zero, ref filterByteLengthRef);
		if (filterByteLengthRef == 0)
		{
			return null;
		}
		byte* ptr = stackalloc byte[(int)(uint)(int)filterByteLengthRef];
		GetRetrievalFilter((IntPtr)ptr, ref filterByteLengthRef);
		InfoQueueFilter infoQueueFilter = new InfoQueueFilter();
		infoQueueFilter.__MarshalFrom(ref *(InfoQueueFilter.__Native*)ptr);
		return infoQueueFilter;
	}

	public InfoQueue(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InfoQueue(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InfoQueue(nativePtr);
		}
		return null;
	}

	internal unsafe void SetMessageCountLimit(long messageCountLimit)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, messageCountLimit)).CheckError();
	}

	public unsafe void ClearStoredMessages()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetMessage(long messageIndex, IntPtr messageRef, ref PointerSize messageByteLengthRef)
	{
		Result result;
		fixed (PointerSize* ptr = &messageByteLengthRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, long, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, messageIndex, (void*)messageRef, ptr2);
		}
		result.CheckError();
	}

	internal unsafe long GetNumMessagesAllowedByStorageFilter()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetNumMessagesDeniedByStorageFilter()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetNumStoredMessages()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetNumStoredMessagesAllowedByRetrievalFilter()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetNumMessagesDiscardedByMessageCountLimit()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetMessageCountLimit()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void AddStorageFilterEntries(InfoQueueFilter filterRef)
	{
		InfoQueueFilter.__Native @ref = default(InfoQueueFilter.__Native);
		filterRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		filterRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetStorageFilter(IntPtr filterRef, ref PointerSize filterByteLengthRef)
	{
		Result result;
		fixed (PointerSize* ptr = &filterByteLengthRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)filterRef, ptr2);
		}
		result.CheckError();
	}

	public unsafe void ClearStorageFilter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void PushEmptyStorageFilter()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void PushCopyOfStorageFilter()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void PushStorageFilter(InfoQueueFilter filterRef)
	{
		InfoQueueFilter.__Native @ref = default(InfoQueueFilter.__Native);
		filterRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		filterRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe void PopStorageFilter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetStorageFilterStackSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void AddRetrievalFilterEntries(InfoQueueFilter filterRef)
	{
		InfoQueueFilter.__Native @ref = default(InfoQueueFilter.__Native);
		filterRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		filterRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetRetrievalFilter(IntPtr filterRef, ref PointerSize filterByteLengthRef)
	{
		Result result;
		fixed (PointerSize* ptr = &filterByteLengthRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)filterRef, ptr2);
		}
		result.CheckError();
	}

	public unsafe void ClearRetrievalFilter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void PushEmptyRetrievalFilter()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void PushCopyOfRetrievalFilter()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void PushRetrievalFilter(InfoQueueFilter filterRef)
	{
		InfoQueueFilter.__Native @ref = default(InfoQueueFilter.__Native);
		filterRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		filterRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe void PopRetrievalFilter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetRetrievalFilterStackSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void AddMessage(MessageCategory category, MessageSeverity severity, MessageId id, string descriptionRef)
	{
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(descriptionRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (int)category, (int)severity, (int)id, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void AddApplicationMessage(MessageSeverity severity, string descriptionRef)
	{
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(descriptionRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, (int)severity, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void SetBreakOnCategory(MessageCategory category, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (int)category, bEnable)).CheckError();
	}

	public unsafe void SetBreakOnSeverity(MessageSeverity severity, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, (int)severity, bEnable)).CheckError();
	}

	public unsafe void SetBreakOnID(MessageId id, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)id, bEnable)).CheckError();
	}

	public unsafe RawBool GetBreakOnCategory(MessageCategory category)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (int)category);
	}

	public unsafe RawBool GetBreakOnSeverity(MessageSeverity severity)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (int)severity);
	}

	public unsafe RawBool GetBreakOnID(MessageId id)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, (int)id);
	}

	internal unsafe void SetMuteDebugOutput(RawBool bMute)
	{
		((delegate* unmanaged[Stdcall]<void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, bMute);
	}

	internal unsafe RawBool GetMuteDebugOutput()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer);
	}
}
