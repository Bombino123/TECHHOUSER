namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class EntitySetIdPropertyRef : PropertyRef
{
	internal static EntitySetIdPropertyRef Instance = new EntitySetIdPropertyRef();

	private EntitySetIdPropertyRef()
	{
	}

	public override string ToString()
	{
		return "ENTITYSETID";
	}
}
