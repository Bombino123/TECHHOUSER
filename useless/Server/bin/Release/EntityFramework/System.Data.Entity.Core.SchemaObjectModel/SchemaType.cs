namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class SchemaType : SchemaElement
{
	public string Namespace => base.Schema.Namespace;

	public override string Identity => Namespace + "." + Name;

	public override string FQName => Namespace + "." + Name;

	internal SchemaType(Schema parentElement)
		: base(parentElement)
	{
	}
}
