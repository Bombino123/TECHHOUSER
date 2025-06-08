namespace System.Data.Entity.Utilities;

internal static class BoolExtensions
{
	internal static bool? Not(this bool? operand)
	{
		if (!operand.HasValue)
		{
			return null;
		}
		return !operand.Value;
	}

	internal static bool? And(this bool? left, bool? right)
	{
		bool? result;
		if (left.HasValue && right.HasValue)
		{
			result = left.Value && right.Value;
		}
		else
		{
			if (left.HasValue || right.HasValue)
			{
				if (left.HasValue)
				{
					return left.Value ? null : new bool?(false);
				}
				return right.Value ? null : new bool?(false);
			}
			result = null;
		}
		return result;
	}

	internal static bool? Or(this bool? left, bool? right)
	{
		bool? result;
		if (left.HasValue && right.HasValue)
		{
			result = left.Value || right.Value;
		}
		else
		{
			if (left.HasValue || right.HasValue)
			{
				if (left.HasValue)
				{
					return left.Value ? new bool?(true) : null;
				}
				return right.Value ? new bool?(true) : null;
			}
			result = null;
		}
		return result;
	}
}
