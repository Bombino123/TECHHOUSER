using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal delegate void AttributeValueNotification(string token, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError);
