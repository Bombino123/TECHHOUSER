namespace System.Data.Entity.Core.Objects.DataClasses;

[Obsolete("This attribute has been replaced by System.Data.Entity.DbFunctionAttribute.")]
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EdmFunctionAttribute : DbFunctionAttribute
{
	public EdmFunctionAttribute(string namespaceName, string functionName)
		: base(namespaceName, functionName)
	{
	}
}
