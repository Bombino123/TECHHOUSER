using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class PropertyEqualityComparer : IEqualityComparer<PropertyDef>
{
	private readonly SigComparerOptions options;

	public static readonly PropertyEqualityComparer CompareDeclaringTypes = new PropertyEqualityComparer(SigComparerOptions.ComparePropertyDeclaringType);

	public static readonly PropertyEqualityComparer DontCompareDeclaringTypes = new PropertyEqualityComparer((SigComparerOptions)0u);

	public static readonly PropertyEqualityComparer CaseInsensitiveCompareDeclaringTypes = new PropertyEqualityComparer(SigComparerOptions.CaseInsensitiveAll | SigComparerOptions.ComparePropertyDeclaringType);

	public static readonly PropertyEqualityComparer CaseInsensitiveDontCompareDeclaringTypes = new PropertyEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public static readonly PropertyEqualityComparer CompareReferenceInSameModule = new PropertyEqualityComparer(SigComparerOptions.ReferenceCompareForMemberDefsInSameModule);

	public PropertyEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(PropertyDef x, PropertyDef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(PropertyDef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}
}
