using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("D67441C7-672A-476f-9E82-CD55B44949CE")]
public class InfoQueue : ComObject
{
	public static InfoQueue TryCreate()
	{
		if (!DebugInterface.TryCreateComPtr<InfoQueue>(out var comPtr))
		{
			return null;
		}
		return new InfoQueue(comPtr);
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

	public unsafe void SetMessageCountLimit(Guid producer, long messageCountLimit)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, producer, messageCountLimit)).CheckError();
	}

	public unsafe void ClearStoredMessages(Guid producer)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void GetMessage(Guid producer, long messageIndex, InformationQueueMessage[] messageRef, ref PointerSize messageByteLengthRef)
	{
		InformationQueueMessage.__Native[] array = ((messageRef == null) ? null : new InformationQueueMessage.__Native[messageRef.Length]);
		Result result;
		fixed (PointerSize* ptr = &messageByteLengthRef)
		{
			void* ptr2 = ptr;
			fixed (InformationQueueMessage.__Native* ptr3 = array)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, Guid, long, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, producer, messageIndex, ptr4, ptr2);
			}
		}
		if (messageRef != null)
		{
			for (int i = 0; i < messageRef.Length; i++)
			{
				messageRef?[i].__MarshalFrom(ref array[i]);
			}
		}
		result.CheckError();
	}

	public unsafe long GetNumStoredMessagesAllowedByRetrievalFilters(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe long GetNumStoredMessages(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe long GetNumMessagesDiscardedByMessageCountLimit(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe long GetMessageCountLimit(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe long GetNumMessagesAllowedByStorageFilter(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe long GetNumMessagesDeniedByStorageFilter(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void AddStorageFilterEntries(Guid producer, ref InfoQueueFilter filterRef)
	{
		Result result;
		fixed (InfoQueueFilter* ptr = &filterRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, producer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetStorageFilter(Guid producer, InfoQueueFilter[] filterRef, ref PointerSize filterByteLengthRef)
	{
		Result result;
		fixed (PointerSize* ptr = &filterByteLengthRef)
		{
			void* ptr2 = ptr;
			fixed (InfoQueueFilter* ptr3 = filterRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, producer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void ClearStorageFilter(Guid producer)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void PushEmptyStorageFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushDenyAllStorageFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushCopyOfStorageFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushStorageFilter(Guid producer, ref InfoQueueFilter filterRef)
	{
		Result result;
		fixed (InfoQueueFilter* ptr = &filterRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, producer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void PopStorageFilter(Guid producer)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe int GetStorageFilterStackSize(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void AddRetrievalFilterEntries(Guid producer, ref InfoQueueFilter filterRef)
	{
		Result result;
		fixed (InfoQueueFilter* ptr = &filterRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, producer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetRetrievalFilter(Guid producer, InfoQueueFilter[] filterRef, ref PointerSize filterByteLengthRef)
	{
		Result result;
		fixed (PointerSize* ptr = &filterByteLengthRef)
		{
			void* ptr2 = ptr;
			fixed (InfoQueueFilter* ptr3 = filterRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, producer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void ClearRetrievalFilter(Guid producer)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void PushEmptyRetrievalFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushDenyAllRetrievalFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushCopyOfRetrievalFilter(Guid producer)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, producer)).CheckError();
	}

	public unsafe void PushRetrievalFilter(Guid producer, ref InfoQueueFilter filterRef)
	{
		Result result;
		fixed (InfoQueueFilter* ptr = &filterRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, Guid, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, producer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void PopRetrievalFilter(Guid producer)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe int GetRetrievalFilterStackSize(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, producer);
	}

	public unsafe void AddMessage(Guid producer, InformationQueueMessageCategory category, InformationQueueMessageSeverity severity, int id, string descriptionRef)
	{
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(descriptionRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, Guid, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, producer, (int)category, (int)severity, id, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void AddApplicationMessage(InformationQueueMessageSeverity severity, string descriptionRef)
	{
		IntPtr intPtr = Marshal.StringToHGlobalAnsi(descriptionRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, (int)severity, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void SetBreakOnCategory(Guid producer, InformationQueueMessageCategory category, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, producer, (int)category, bEnable)).CheckError();
	}

	public unsafe void SetBreakOnSeverity(Guid producer, InformationQueueMessageSeverity severity, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, producer, (int)severity, bEnable)).CheckError();
	}

	public unsafe void SetBreakOnID(Guid producer, int id, RawBool bEnable)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, producer, id, bEnable)).CheckError();
	}

	public unsafe RawBool GetBreakOnCategory(Guid producer, InformationQueueMessageCategory category)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, producer, (int)category);
	}

	public unsafe RawBool GetBreakOnSeverity(Guid producer, InformationQueueMessageSeverity severity)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, producer, (int)severity);
	}

	public unsafe RawBool GetBreakOnID(Guid producer, int id)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, producer, id);
	}

	public unsafe void SetMuteDebugOutput(Guid producer, RawBool bMute)
	{
		((delegate* unmanaged[Stdcall]<void*, Guid, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, producer, bMute);
	}

	public unsafe RawBool GetMuteDebugOutput(Guid producer)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Guid, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, producer);
	}
}
