using System.Data.Entity.Utilities;

namespace System.Data.Entity;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class DbFunctionAttribute : Attribute
{
	private readonly string _namespaceName;

	private readonly string _functionName;

	public string NamespaceName => _namespaceName;

	public string FunctionName => _functionName;

	public DbFunctionAttribute(string namespaceName, string functionName)
	{
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotEmpty(functionName, "functionName");
		_namespaceName = namespaceName;
		_functionName = functionName;
	}
}
