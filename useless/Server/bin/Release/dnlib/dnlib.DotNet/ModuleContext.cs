using System.Threading;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet;

public class ModuleContext
{
	private IAssemblyResolver assemblyResolver;

	private IResolver resolver;

	private readonly OpCode[][] experimentalOpCodes = new OpCode[12][];

	public IAssemblyResolver AssemblyResolver
	{
		get
		{
			if (assemblyResolver == null)
			{
				Interlocked.CompareExchange(ref assemblyResolver, NullResolver.Instance, null);
			}
			return assemblyResolver;
		}
		set
		{
			assemblyResolver = value;
		}
	}

	public IResolver Resolver
	{
		get
		{
			if (resolver == null)
			{
				Interlocked.CompareExchange(ref resolver, NullResolver.Instance, null);
			}
			return resolver;
		}
		set
		{
			resolver = value;
		}
	}

	public ModuleContext()
	{
	}

	public ModuleContext(IAssemblyResolver assemblyResolver)
		: this(assemblyResolver, new Resolver(assemblyResolver))
	{
	}

	public ModuleContext(IResolver resolver)
		: this(null, resolver)
	{
	}

	public ModuleContext(IAssemblyResolver assemblyResolver, IResolver resolver)
	{
		this.assemblyResolver = assemblyResolver;
		this.resolver = resolver;
		if (resolver == null && assemblyResolver != null)
		{
			this.resolver = new Resolver(assemblyResolver);
		}
	}

	public void RegisterExperimentalOpCode(OpCode opCode)
	{
		byte num = (byte)((ushort)opCode.Value >> 8);
		byte b = (byte)opCode.Value;
		OpCode[][] array = experimentalOpCodes;
		int num2 = num - 240;
		(array[num2] ?? (array[num2] = new OpCode[256]))[b] = opCode;
	}

	public void ClearExperimentalOpCode(byte high, byte low)
	{
		OpCode[] array = experimentalOpCodes[high - 240];
		if (array != null)
		{
			array[low] = null;
		}
	}

	public OpCode GetExperimentalOpCode(byte high, byte low)
	{
		OpCode[] obj = experimentalOpCodes[high - 240];
		if (obj == null)
		{
			return null;
		}
		return obj[low];
	}
}
