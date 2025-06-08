using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class PrimitiveSchema : Schema
{
	internal override string Alias => base.ProviderManifest.NamespaceName;

	internal override string Namespace
	{
		get
		{
			if (base.ProviderManifest != null)
			{
				return base.ProviderManifest.NamespaceName;
			}
			return string.Empty;
		}
	}

	public PrimitiveSchema(SchemaManager schemaManager)
		: base(schemaManager)
	{
		base.Schema = this;
		DbProviderManifest providerManifest = base.ProviderManifest;
		if (providerManifest == null)
		{
			AddError(new EdmSchemaError(Strings.FailedToRetrieveProviderManifest, 168, EdmSchemaErrorSeverity.Error));
			return;
		}
		IList<PrimitiveType> list = providerManifest.GetStoreTypes();
		if (schemaManager.DataModel == SchemaDataModelOption.EntityDataModel && schemaManager.SchemaVersion < 3.0)
		{
			list = list.Where((PrimitiveType t) => !Helper.IsSpatialType(t)).ToList();
		}
		foreach (PrimitiveType item in list)
		{
			TryAddType(new ScalarType(this, item.Name, item), doNotAddErrorForEmptyName: false);
		}
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		return false;
	}
}
