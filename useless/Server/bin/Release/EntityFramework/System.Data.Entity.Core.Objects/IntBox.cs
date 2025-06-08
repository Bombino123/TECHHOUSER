namespace System.Data.Entity.Core.Objects;

internal sealed class IntBox
{
	internal int Value { get; set; }

	internal IntBox(int val)
	{
		Value = val;
	}
}
