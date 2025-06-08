using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ManyToManyCascadeDeleteConvention : IDbMappingConvention, IConvention
{
	void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
	{
		Check.NotNull(databaseMapping, "databaseMapping");
		(from asm in databaseMapping.EntityContainerMappings.SelectMany((EntityContainerMapping ecm) => ecm.AssociationSetMappings)
			where asm.AssociationSet.ElementType.IsManyToMany() && !asm.AssociationSet.ElementType.IsSelfReferencing()
			select asm).SelectMany((AssociationSetMapping asm) => asm.Table.ForeignKeyBuilders).Each((ForeignKeyBuilder fk) => fk.DeleteAction = OperationAction.Cascade);
	}
}
