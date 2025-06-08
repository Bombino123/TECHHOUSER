using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public class ValueConditionMapping : ConditionPropertyMapping
{
	public new object Value => base.Value;

	public ValueConditionMapping(EdmProperty propertyOrColumn, object value)
		: base(Check.NotNull(propertyOrColumn, "propertyOrColumn"), Check.NotNull(value, "value"), null)
	{
	}
}
