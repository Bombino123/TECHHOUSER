namespace System.Data.Entity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DbModelBuilderVersionAttribute : Attribute
{
	public DbModelBuilderVersion Version { get; private set; }

	public DbModelBuilderVersionAttribute(DbModelBuilderVersion version)
	{
		if (!Enum.IsDefined(typeof(DbModelBuilderVersion), version))
		{
			throw new ArgumentOutOfRangeException("version");
		}
		Version = version;
	}
}
