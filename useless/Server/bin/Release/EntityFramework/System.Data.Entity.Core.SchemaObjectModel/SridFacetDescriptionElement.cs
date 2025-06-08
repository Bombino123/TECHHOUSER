using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class SridFacetDescriptionElement : FacetDescriptionElement
{
	public override EdmType FacetType => MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32);

	public SridFacetDescriptionElement(TypeElement type, string name)
		: base(type, name)
	{
	}

	protected override void HandleDefaultAttribute(XmlReader reader)
	{
		if (reader.Value.Trim() == "Variable")
		{
			base.DefaultValue = EdmConstants.VariableValue;
			return;
		}
		int field = -1;
		if (HandleIntAttribute(reader, ref field))
		{
			base.DefaultValue = field;
		}
	}
}
