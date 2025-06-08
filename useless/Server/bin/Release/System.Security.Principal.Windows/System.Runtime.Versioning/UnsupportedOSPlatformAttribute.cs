namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event, AllowMultiple = true, Inherited = false)]
internal sealed class UnsupportedOSPlatformAttribute : OSPlatformAttribute
{
	public UnsupportedOSPlatformAttribute(string platformName)
		: base(platformName)
	{
	}
}
