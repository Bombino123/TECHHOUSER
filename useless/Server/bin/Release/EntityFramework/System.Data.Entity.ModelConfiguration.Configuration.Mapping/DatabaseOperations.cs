using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class DatabaseOperations
{
	public static void AddTypeConstraint(EdmModel database, EntityType entityType, EntityType principalTable, EntityType dependentTable, bool isSplitting)
	{
		ForeignKeyBuilder foreignKeyBuilder = new ForeignKeyBuilder(database, string.Format(CultureInfo.InvariantCulture, "{0}_TypeConstraint_From_{1}_To_{2}", new object[3] { entityType.Name, principalTable.Name, dependentTable.Name }))
		{
			PrincipalTable = principalTable
		};
		dependentTable.AddForeignKey(foreignKeyBuilder);
		if (isSplitting)
		{
			foreignKeyBuilder.SetIsSplitConstraint();
		}
		else
		{
			foreignKeyBuilder.SetIsTypeConstraint();
		}
		foreignKeyBuilder.DependentColumns = dependentTable.Properties.Where((EdmProperty c) => c.IsPrimaryKeyColumn);
		dependentTable.Properties.Where((EdmProperty c) => c.IsPrimaryKeyColumn).Each(delegate(EdmProperty c)
		{
			c.RemoveStoreGeneratedIdentityPattern();
		});
	}
}
