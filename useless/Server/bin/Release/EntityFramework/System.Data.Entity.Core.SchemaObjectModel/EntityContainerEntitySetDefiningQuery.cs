using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class EntityContainerEntitySetDefiningQuery : SchemaElement
{
	private string _query;

	public string Query => _query;

	public EntityContainerEntitySetDefiningQuery(EntityContainerEntitySet parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleText(XmlReader reader)
	{
		_query = reader.Value;
		return true;
	}

	internal override void Validate()
	{
		base.Validate();
		if (string.IsNullOrEmpty(_query))
		{
			AddError(ErrorCode.EmptyDefiningQuery, EdmSchemaErrorSeverity.Error, Strings.EmptyDefiningQuery);
		}
	}
}
