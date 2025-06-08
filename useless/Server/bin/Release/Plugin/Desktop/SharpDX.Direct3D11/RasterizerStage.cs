using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da")]
public class RasterizerStage : CppObject
{
	public RasterizerState State
	{
		get
		{
			GetState(out var rasterizerStateOut);
			return rasterizerStateOut;
		}
		set
		{
			SetState(value);
		}
	}

	public T[] GetViewports<T>() where T : struct
	{
		if (Utilities.SizeOf<T>() != Utilities.SizeOf<RawViewportF>())
		{
			throw new ArgumentException("Type T must have same size and layout as RawViewPortF", "viewports");
		}
		int numViewportsRef = 0;
		GetViewports(ref numViewportsRef, IntPtr.Zero);
		T[] array = new T[numViewportsRef];
		GetViewports(array);
		return array;
	}

	public unsafe void GetViewports<T>(T[] viewports) where T : struct
	{
		if (Utilities.SizeOf<T>() != Utilities.SizeOf<RawViewportF>())
		{
			throw new ArgumentException("Type T must have same size and layout as RawViewPortF", "viewports");
		}
		int numViewportsRef = viewports.Length;
		fixed (T* ptr = &viewports[0])
		{
			void* value = ptr;
			GetViewports(ref numViewportsRef, new IntPtr(value));
		}
	}

	public T[] GetScissorRectangles<T>() where T : struct
	{
		int numRectsRef = 0;
		GetScissorRects(ref numRectsRef, IntPtr.Zero);
		T[] array = new T[numRectsRef];
		GetScissorRectangles(array);
		return array;
	}

	public unsafe void GetScissorRectangles<T>(T[] scissorRectangles) where T : struct
	{
		if (Utilities.SizeOf<T>() != Utilities.SizeOf<RawRectangle>())
		{
			throw new ArgumentException("Type T must have same size and layout as RawRectangle", "scissorRectangles");
		}
		int numRectsRef = scissorRectangles.Length;
		fixed (T* ptr = &scissorRectangles[0])
		{
			void* value = ptr;
			GetScissorRects(ref numRectsRef, new IntPtr(value));
		}
	}

	public unsafe void SetScissorRectangle(int left, int top, int right, int bottom)
	{
		RawRectangle rawRectangle = default(RawRectangle);
		rawRectangle.Left = left;
		rawRectangle.Top = top;
		rawRectangle.Right = right;
		rawRectangle.Bottom = bottom;
		RawRectangle rawRectangle2 = rawRectangle;
		SetScissorRects(1, new IntPtr(&rawRectangle2));
	}

	public unsafe void SetScissorRectangles<T>(params T[] scissorRectangles) where T : struct
	{
		//IL_0035->IL0035: Incompatible stack types: I vs Ref
		//The blocks IL_0035, IL_003d, IL_0045, IL_0046 are reachable both inside and outside the pinned region starting at IL_002c. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		if (Utilities.SizeOf<T>() != Utilities.SizeOf<RawRectangle>())
		{
			throw new ArgumentException("Type T must have same size and layout as RawRectangle", "viewports");
		}
		ref _003F reference;
		RasterizerStage rasterizerStage;
		int numRects;
		void* ptr2;
		if (scissorRectangles != null)
		{
			fixed (T* ptr = &scissorRectangles[0])
			{
				reference = ref *(_003F*)ptr;
				ptr2 = System.Runtime.CompilerServices.Unsafe.AsPointer(ref reference);
				rasterizerStage = this;
				numRects = ((scissorRectangles != null) ? scissorRectangles.Length : 0);
				rasterizerStage.SetScissorRects(numRects, (IntPtr)ptr2);
				return;
			}
		}
		reference = ref *(_003F*)null;
		ptr2 = System.Runtime.CompilerServices.Unsafe.AsPointer(ref reference);
		rasterizerStage = this;
		numRects = ((scissorRectangles != null) ? scissorRectangles.Length : 0);
		rasterizerStage.SetScissorRects(numRects, (IntPtr)ptr2);
	}

	public unsafe void SetViewport(float x, float y, float width, float height, float minZ = 0f, float maxZ = 1f)
	{
		RawViewportF rawViewportF = default(RawViewportF);
		rawViewportF.X = x;
		rawViewportF.Y = y;
		rawViewportF.Width = width;
		rawViewportF.Height = height;
		rawViewportF.MinDepth = minZ;
		rawViewportF.MaxDepth = maxZ;
		RawViewportF rawViewportF2 = rawViewportF;
		SetViewports(1, new IntPtr(&rawViewportF2));
	}

	public unsafe void SetViewport(RawViewportF viewport)
	{
		fixed (RawViewportF* value = &System.Runtime.CompilerServices.Unsafe.AsRef<RawViewportF>(&viewport))
		{
			SetViewports(1, new IntPtr(value));
		}
	}

	public unsafe void SetViewports(RawViewportF[] viewports, int count = 0)
	{
		//IL_0016->IL0016: Incompatible stack types: I vs Ref
		//The blocks IL_0016, IL_001e, IL_0025, IL_002b, IL_0033, IL_0034 are reachable both inside and outside the pinned region starting at IL_000d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		ref _003F reference;
		RasterizerStage rasterizerStage;
		int numViewports;
		void* ptr2;
		if (viewports != null)
		{
			fixed (RawViewportF* ptr = &viewports[0])
			{
				reference = ref *(_003F*)ptr;
				ptr2 = System.Runtime.CompilerServices.Unsafe.AsPointer(ref reference);
				rasterizerStage = this;
				numViewports = ((viewports != null) ? ((count <= 0) ? viewports.Length : count) : 0);
				rasterizerStage.SetViewports(numViewports, (IntPtr)ptr2);
				return;
			}
		}
		reference = ref *(_003F*)null;
		ptr2 = System.Runtime.CompilerServices.Unsafe.AsPointer(ref reference);
		rasterizerStage = this;
		numViewports = ((viewports != null) ? ((count <= 0) ? viewports.Length : count) : 0);
		rasterizerStage.SetViewports(numViewports, (IntPtr)ptr2);
	}

	public unsafe void SetViewports(RawViewportF* viewports, int count = 0)
	{
		SetViewports(count, (IntPtr)viewports);
	}

	public RasterizerStage(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RasterizerStage(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RasterizerStage(nativePtr);
		}
		return null;
	}

	internal unsafe void SetState(RasterizerState rasterizerStateRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RasterizerState>(rasterizerStateRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void SetViewports(int numViewports, IntPtr viewportsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, numViewports, (void*)viewportsRef);
	}

	internal unsafe void SetScissorRects(int numRects, IntPtr rectsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, numRects, (void*)rectsRef);
	}

	internal unsafe void GetState(out RasterizerState rasterizerStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)94 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			rasterizerStateOut = new RasterizerState(zero);
		}
		else
		{
			rasterizerStateOut = null;
		}
	}

	internal unsafe void GetViewports(ref int numViewportsRef, IntPtr viewportsRef)
	{
		fixed (int* ptr = &numViewportsRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)95 * (nint)sizeof(void*))))(_nativePointer, ptr2, (void*)viewportsRef);
		}
	}

	internal unsafe void GetScissorRects(ref int numRectsRef, IntPtr rectsRef)
	{
		fixed (int* ptr = &numRectsRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)96 * (nint)sizeof(void*))))(_nativePointer, ptr2, (void*)rectsRef);
		}
	}
}
