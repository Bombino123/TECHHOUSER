namespace System.Data.Entity.Core.Objects.DataClasses;

public abstract class EdmTypeAttribute : Attribute
{
	public string Name { get; set; }

	public string NamespaceName { get; set; }

	internal EdmTypeAttribute()
	{
	}
}
