using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class SignatureEqualityComparer : IEqualityComparer<CallingConventionSig>, IEqualityComparer<MethodBaseSig>, IEqualityComparer<MethodSig>, IEqualityComparer<PropertySig>, IEqualityComparer<FieldSig>, IEqualityComparer<LocalSig>, IEqualityComparer<GenericInstMethodSig>
{
	private readonly SigComparerOptions options;

	public static readonly SignatureEqualityComparer Instance = new SignatureEqualityComparer((SigComparerOptions)0u);

	public static readonly SignatureEqualityComparer CaseInsensitive = new SignatureEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public SignatureEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(CallingConventionSig x, CallingConventionSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(CallingConventionSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(MethodBaseSig x, MethodBaseSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(MethodBaseSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(MethodSig x, MethodSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(MethodSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(PropertySig x, PropertySig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(PropertySig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(FieldSig x, FieldSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(FieldSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(LocalSig x, LocalSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(LocalSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(GenericInstMethodSig x, GenericInstMethodSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(GenericInstMethodSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}
}
