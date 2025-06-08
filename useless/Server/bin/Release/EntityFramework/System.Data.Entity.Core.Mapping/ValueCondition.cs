namespace System.Data.Entity.Core.Mapping;

internal class ValueCondition : IEquatable<ValueCondition>
{
	internal readonly string Description;

	internal readonly bool IsSentinel;

	internal const string IsNullDescription = "NULL";

	internal const string IsNotNullDescription = "NOT NULL";

	internal const string IsOtherDescription = "OTHER";

	internal static readonly ValueCondition IsNull = new ValueCondition("NULL", isSentinel: true);

	internal static readonly ValueCondition IsNotNull = new ValueCondition("NOT NULL", isSentinel: true);

	internal static readonly ValueCondition IsOther = new ValueCondition("OTHER", isSentinel: true);

	internal bool IsNotNullCondition => this == IsNotNull;

	private ValueCondition(string description, bool isSentinel)
	{
		Description = description;
		IsSentinel = isSentinel;
	}

	internal ValueCondition(string description)
		: this(description, isSentinel: false)
	{
	}

	public bool Equals(ValueCondition other)
	{
		if (other.IsSentinel == IsSentinel)
		{
			return other.Description == Description;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Description.GetHashCode();
	}

	public override string ToString()
	{
		return Description;
	}
}
