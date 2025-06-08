namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class PropertyRefElement : SchemaElement
{
	private StructuredProperty _property;

	public StructuredProperty Property => _property;

	public PropertyRefElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	internal override void ResolveTopLevelNames()
	{
	}

	internal bool ResolveNames(SchemaEntityType entityType)
	{
		if (string.IsNullOrEmpty(Name))
		{
			return true;
		}
		_property = entityType.FindProperty(Name);
		return _property != null;
	}
}
