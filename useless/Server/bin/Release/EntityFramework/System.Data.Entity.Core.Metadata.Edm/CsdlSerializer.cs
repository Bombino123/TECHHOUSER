using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

public class CsdlSerializer
{
	public event EventHandler<DataModelErrorEventArgs> OnError;

	public bool Serialize(EdmModel model, XmlWriter xmlWriter, string modelNamespace = null)
	{
		Check.NotNull(model, "model");
		Check.NotNull(xmlWriter, "xmlWriter");
		bool modelIsValid = true;
		Action<DataModelErrorEventArgs> onErrorAction = delegate(DataModelErrorEventArgs e)
		{
			modelIsValid = false;
			if (this.OnError != null)
			{
				this.OnError(this, e);
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
		if (modelIsValid)
		{
			new EdmSerializationVisitor(xmlWriter, model.SchemaVersion).Visit(model, modelNamespace);
			return true;
		}
		return false;
	}
}
