using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class SimpleColumnMap : ColumnMap
{
	internal SimpleColumnMap(TypeUsage type, string name)
		: base(type, name)
	{
	}
}
