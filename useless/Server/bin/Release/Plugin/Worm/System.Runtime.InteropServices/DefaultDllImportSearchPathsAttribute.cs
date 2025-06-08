namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method, AllowMultiple = false)]
internal sealed class DefaultDllImportSearchPathsAttribute : Attribute
{
	internal DllImportSearchPath _paths;

	public DllImportSearchPath Paths => _paths;

	public DefaultDllImportSearchPathsAttribute(DllImportSearchPath paths)
	{
		_paths = paths;
	}
}
