using dnlib.PE;

namespace dnlib.DotNet.Emit;

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
