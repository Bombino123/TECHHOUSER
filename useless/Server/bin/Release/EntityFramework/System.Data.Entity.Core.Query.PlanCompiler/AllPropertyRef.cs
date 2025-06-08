namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class AllPropertyRef : PropertyRef
{
	internal static AllPropertyRef Instance = new AllPropertyRef();

	private AllPropertyRef()
	{
	}

	internal override PropertyRef CreateNestedPropertyRef(PropertyRef p)
	{
		return p;
	}

	public override string ToString()
	{
		return "ALL";
	}
}
