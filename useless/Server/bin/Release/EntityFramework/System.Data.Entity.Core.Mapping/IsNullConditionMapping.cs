using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public class IsNullConditionMapping : ConditionPropertyMapping
{
	public new bool IsNull => base.IsNull.Value;

	public IsNullConditionMapping(EdmProperty propertyOrColumn, bool isNull)
		: base(Check.NotNull(propertyOrColumn, "propertyOrColumn"), null, isNull)
	{
	}
}
