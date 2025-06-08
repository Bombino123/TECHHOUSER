namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class TypeIdPropertyRef : PropertyRef
{
	internal static TypeIdPropertyRef Instance = new TypeIdPropertyRef();

	private TypeIdPropertyRef()
	{
	}

	public override string ToString()
	{
		return "TYPEID";
	}
}
