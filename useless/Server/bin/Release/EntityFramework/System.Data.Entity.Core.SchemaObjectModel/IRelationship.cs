using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal interface IRelationship
{
	string Name { get; }

	string FQName { get; }

	IList<IRelationshipEnd> Ends { get; }

	IList<ReferentialConstraint> Constraints { get; }

	RelationshipKind RelationshipKind { get; }

	bool IsForeignKey { get; }

	bool TryGetEnd(string roleName, out IRelationshipEnd end);
}
