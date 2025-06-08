using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ByteFacetDescriptionElement : FacetDescriptionElement
{
	public override EdmType FacetType => MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte);

	public ByteFacetDescriptionElement(TypeElement type, string name)
		: base(type, name)
	{
	}

	protected override void HandleDefaultAttribute(XmlReader reader)
	{
		byte field = 0;
		if (HandleByteAttribute(reader, ref field))
		{
			base.DefaultValue = field;
		}
	}
}
