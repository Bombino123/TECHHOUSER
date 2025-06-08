using System.Runtime.InteropServices;
using dnlib.PE;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public sealed class NativeMethodBody : MethodBody
{
	private RVA rva;

	public RVA RVA
	{
		get
		{
			return rva;
		}
		set
		{
			rva = value;
		}
	}

	public NativeMethodBody()
	{
	}

	public NativeMethodBody(RVA rva)
	{
		this.rva = rva;
	}
}
