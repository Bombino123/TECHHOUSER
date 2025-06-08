using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects.DataClasses;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public sealed class EdmSchemaAttribute : Attribute
{
	public EdmSchemaAttribute()
	{
	}

	public EdmSchemaAttribute(string assemblyGuid)
	{
		Check.NotNull(assemblyGuid, "assemblyGuid");
	}
}
