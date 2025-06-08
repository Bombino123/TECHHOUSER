namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

[Flags]
internal enum OverridableConfigurationParts
{
	None = 0,
	OverridableInCSpace = 1,
	OverridableInSSpace = 2
}
