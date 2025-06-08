using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal interface IDbMappingConvention : IConvention
{
	void Apply(DbDatabaseMapping databaseMapping);
}
