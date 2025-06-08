using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class MethodEqualityComparer : IEqualityComparer<IMethod>, IEqualityComparer<IMethodDefOrRef>, IEqualityComparer<MethodDef>, IEqualityComparer<MemberRef>, IEqualityComparer<MethodSpec>
{
	private readonly SigComparerOptions options;

	public static readonly MethodEqualityComparer CompareDeclaringTypes = new MethodEqualityComparer(SigComparerOptions.CompareMethodFieldDeclaringType);

	public static readonly MethodEqualityComparer DontCompareDeclaringTypes = new MethodEqualityComparer((SigComparerOptions)0u);

	public static readonly MethodEqualityComparer CaseInsensitiveCompareDeclaringTypes = new MethodEqualityComparer(SigComparerOptions.CaseInsensitiveAll | SigComparerOptions.CompareMethodFieldDeclaringType);

	public static readonly MethodEqualityComparer CaseInsensitiveDontCompareDeclaringTypes = new MethodEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public static readonly MethodEqualityComparer CompareReferenceInSameModule = new MethodEqualityComparer(SigComparerOptions.ReferenceCompareForMemberDefsInSameModule);

	public MethodEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(IMethod x, IMethod y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(IMethod obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(IMethodDefOrRef x, IMethodDefOrRef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(IMethodDefOrRef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(MethodDef x, MethodDef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(MethodDef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(MemberRef x, MemberRef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(MemberRef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(MethodSpec x, MethodSpec y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(MethodSpec obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}
}
