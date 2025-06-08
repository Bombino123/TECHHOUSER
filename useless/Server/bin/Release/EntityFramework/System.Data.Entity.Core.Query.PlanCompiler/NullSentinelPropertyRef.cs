namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class NullSentinelPropertyRef : PropertyRef
{
	private static readonly NullSentinelPropertyRef _singleton = new NullSentinelPropertyRef();

	internal static NullSentinelPropertyRef Instance => _singleton;

	private NullSentinelPropertyRef()
	{
	}

	public override string ToString()
	{
		return "NULLSENTINEL";
	}
}
