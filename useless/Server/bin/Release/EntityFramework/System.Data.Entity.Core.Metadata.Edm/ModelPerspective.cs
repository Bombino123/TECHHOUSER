using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ModelPerspective : Perspective
{
	internal ModelPerspective(MetadataWorkspace metadataWorkspace)
		: base(metadataWorkspace, DataSpace.CSpace)
	{
	}

	internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage)
	{
		Check.NotEmpty(fullName, "fullName");
		typeUsage = null;
		EdmType item = null;
		if (base.MetadataWorkspace.TryGetItem<EdmType>(fullName, ignoreCase, base.TargetDataspace, out item))
		{
			if (Helper.IsPrimitiveType(item))
			{
				typeUsage = MetadataWorkspace.GetCanonicalModelTypeUsage(((PrimitiveType)item).PrimitiveTypeKind);
			}
			else
			{
				typeUsage = TypeUsage.Create(item);
			}
		}
		return typeUsage != null;
	}
}
