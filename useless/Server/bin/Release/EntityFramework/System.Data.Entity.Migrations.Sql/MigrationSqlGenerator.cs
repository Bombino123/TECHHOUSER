using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Model;
using System.Linq;

namespace System.Data.Entity.Migrations.Sql;

public abstract class MigrationSqlGenerator
{
	protected DbProviderManifest ProviderManifest { get; set; }

	public abstract IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken);

	public virtual string GenerateProcedureBody(ICollection<DbModificationCommandTree> commandTrees, string rowsAffectedParameter, string providerManifestToken)
	{
		return null;
	}

	public virtual bool IsPermissionDeniedError(Exception exception)
	{
		return false;
	}

	protected virtual TypeUsage BuildStoreTypeUsage(string storeTypeName, PropertyModel propertyModel)
	{
		PrimitiveType primitiveType = ProviderManifest.GetStoreTypes().SingleOrDefault((PrimitiveType p) => string.Equals(p.Name, storeTypeName, StringComparison.OrdinalIgnoreCase));
		if (primitiveType != null)
		{
			return TypeUsage.Create(primitiveType, propertyModel.ToFacetValues());
		}
		return null;
	}
}
