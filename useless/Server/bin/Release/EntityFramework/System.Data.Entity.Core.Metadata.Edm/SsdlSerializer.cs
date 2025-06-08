using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

public class SsdlSerializer
{
	public event EventHandler<DataModelErrorEventArgs> OnError;

	public virtual bool Serialize(EdmModel dbDatabase, string provider, string providerManifestToken, XmlWriter xmlWriter, bool serializeDefaultNullability = true)
	{
		Check.NotNull(dbDatabase, "dbDatabase");
		Check.NotEmpty(provider, "provider");
		Check.NotEmpty(providerManifestToken, "providerManifestToken");
		Check.NotNull(xmlWriter, "xmlWriter");
		if (ValidateModel(dbDatabase))
		{
			CreateVisitor(xmlWriter, dbDatabase, serializeDefaultNullability).Visit(dbDatabase, provider, providerManifestToken);
			return true;
		}
		return false;
	}

	public virtual bool Serialize(EdmModel dbDatabase, string namespaceName, string provider, string providerManifestToken, XmlWriter xmlWriter, bool serializeDefaultNullability = true)
	{
		Check.NotNull(dbDatabase, "dbDatabase");
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotEmpty(provider, "provider");
		Check.NotEmpty(providerManifestToken, "providerManifestToken");
		Check.NotNull(xmlWriter, "xmlWriter");
		if (ValidateModel(dbDatabase))
		{
			CreateVisitor(xmlWriter, dbDatabase, serializeDefaultNullability).Visit(dbDatabase, namespaceName, provider, providerManifestToken);
			return true;
		}
		return false;
	}

	private bool ValidateModel(EdmModel model)
	{
		bool modelIsValid = true;
		Action<DataModelErrorEventArgs> onErrorAction = delegate(DataModelErrorEventArgs e)
		{
			MetadataItem item = e.Item;
			if (item == null || !MetadataItemHelper.IsInvalid(item))
			{
				modelIsValid = false;
				if (this.OnError != null)
				{
					this.OnError(this, e);
				}
			}
		};
		if (model.NamespaceNames.Count() > 1 || model.Containers.Count() != 1)
		{
			onErrorAction(new DataModelErrorEventArgs
			{
				ErrorMessage = Strings.Serializer_OneNamespaceAndOneContainer
			});
		}
		DataModelValidator dataModelValidator = new DataModelValidator();
		dataModelValidator.OnError += delegate(object _, DataModelErrorEventArgs e)
		{
			onErrorAction(e);
		};
		dataModelValidator.Validate(model, validateSyntax: true);
		return modelIsValid;
	}

	private static EdmSerializationVisitor CreateVisitor(XmlWriter xmlWriter, EdmModel dbDatabase, bool serializeDefaultNullability)
	{
		return new EdmSerializationVisitor(xmlWriter, dbDatabase.SchemaVersion, serializeDefaultNullability);
	}
}
