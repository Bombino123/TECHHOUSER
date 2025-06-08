using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class TypedColumnMap : StructuredColumnMap
{
	internal TypedColumnMap(TypeUsage type, string name, ColumnMap[] properties)
		: base(type, name, properties)
	{
	}
}
