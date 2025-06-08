using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal interface IRelationshipEnd
{
	string Name { get; }

	SchemaEntityType Type { get; }

	RelationshipMultiplicity? Multiplicity { get; set; }

	ICollection<OnOperation> Operations { get; }
}
