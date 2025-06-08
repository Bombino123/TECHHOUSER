using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class BooleanFacetDescriptionElement : FacetDescriptionElement
{
	public override EdmType FacetType => MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean);

	public BooleanFacetDescriptionElement(TypeElement type, string name)
		: base(type, name)
	{
	}

	protected override void HandleDefaultAttribute(XmlReader reader)
	{
		bool field = false;
		if (HandleBoolAttribute(reader, ref field))
		{
			base.DefaultValue = field;
		}
	}
}
