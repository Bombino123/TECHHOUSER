using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class FieldEqualityComparer : IEqualityComparer<IField>, IEqualityComparer<FieldDef>, IEqualityComparer<MemberRef>
{
	private readonly SigComparerOptions options;

	public static readonly FieldEqualityComparer CompareDeclaringTypes = new FieldEqualityComparer(SigComparerOptions.CompareMethodFieldDeclaringType);

	public static readonly FieldEqualityComparer DontCompareDeclaringTypes = new FieldEqualityComparer((SigComparerOptions)0u);

	public static readonly FieldEqualityComparer CaseInsensitiveCompareDeclaringTypes = new FieldEqualityComparer(SigComparerOptions.CaseInsensitiveAll | SigComparerOptions.CompareMethodFieldDeclaringType);

	public static readonly FieldEqualityComparer CaseInsensitiveDontCompareDeclaringTypes = new FieldEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public static readonly FieldEqualityComparer CompareReferenceInSameModule = new FieldEqualityComparer(SigComparerOptions.ReferenceCompareForMemberDefsInSameModule);

	public FieldEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(IField x, IField y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(IField obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(FieldDef x, FieldDef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(FieldDef obj)
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
}
