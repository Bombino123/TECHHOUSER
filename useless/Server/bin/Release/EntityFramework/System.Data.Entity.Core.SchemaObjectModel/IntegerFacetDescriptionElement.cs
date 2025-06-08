using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class IntegerFacetDescriptionElement : FacetDescriptionElement
{
	public override EdmType FacetType => MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32);

	public IntegerFacetDescriptionElement(TypeElement type, string name)
		: base(type, name)
	{
	}

	protected override void HandleDefaultAttribute(XmlReader reader)
	{
		int field = -1;
		if (HandleIntAttribute(reader, ref field))
		{
			base.DefaultValue = field;
		}
	}
}
