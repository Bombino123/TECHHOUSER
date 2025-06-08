using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class EventEqualityComparer : IEqualityComparer<EventDef>
{
	private readonly SigComparerOptions options;

	public static readonly EventEqualityComparer CompareDeclaringTypes = new EventEqualityComparer(SigComparerOptions.CompareEventDeclaringType);

	public static readonly EventEqualityComparer DontCompareDeclaringTypes = new EventEqualityComparer((SigComparerOptions)0u);

	public static readonly EventEqualityComparer CaseInsensitiveCompareDeclaringTypes = new EventEqualityComparer(SigComparerOptions.CaseInsensitiveAll | SigComparerOptions.CompareEventDeclaringType);

	public static readonly EventEqualityComparer CaseInsensitiveDontCompareDeclaringTypes = new EventEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public static readonly EventEqualityComparer CompareReferenceInSameModule = new EventEqualityComparer(SigComparerOptions.ReferenceCompareForMemberDefsInSameModule);

	public EventEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(EventDef x, EventDef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(EventDef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}
}
