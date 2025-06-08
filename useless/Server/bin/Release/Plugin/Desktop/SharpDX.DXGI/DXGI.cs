using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

internal static class DXGI
{
	public const int CreateFactoryDebug = 1;

	public unsafe static void CreateDXGIFactory1(Guid riid, out IntPtr factoryOut)
	{
		Result result;
		fixed (IntPtr* ptr = &factoryOut)
		{
			void* param = ptr;
			result = CreateDXGIFactory1_(&riid, param);
		}
		result.CheckError();
	}

	[DllImport("dxgi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateDXGIFactory1")]
	private unsafe static extern int CreateDXGIFactory1_(void* param0, void* param1);

	public unsafe static void CreateDXGIFactory2(int flags, Guid riid, out IntPtr factoryOut)
	{
		Result result;
		fixed (IntPtr* ptr = &factoryOut)
		{
			void* param = ptr;
			result = CreateDXGIFactory2_(flags, &riid, param);
		}
		result.CheckError();
	}

	[DllImport("dxgi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateDXGIFactory2")]
	private unsafe static extern int CreateDXGIFactory2_(int param0, void* param1, void* param2);
}
