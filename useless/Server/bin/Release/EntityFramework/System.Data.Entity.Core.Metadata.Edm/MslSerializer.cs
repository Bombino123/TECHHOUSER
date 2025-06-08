using System.Data.Entity.Utilities;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MslSerializer
{
	public virtual bool Serialize(DbDatabaseMapping databaseMapping, XmlWriter xmlWriter)
	{
		Check.NotNull(databaseMapping, "databaseMapping");
		Check.NotNull(xmlWriter, "xmlWriter");
		new MslXmlSchemaWriter(xmlWriter, databaseMapping.Model.SchemaVersion).WriteSchema(databaseMapping);
		return true;
	}
}
