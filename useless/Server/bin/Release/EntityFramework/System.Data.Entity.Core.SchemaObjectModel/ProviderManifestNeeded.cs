using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal delegate DbProviderManifest ProviderManifestNeeded(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError);
