namespace System.Data.SQLite;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblySourceIdAttribute : Attribute
{
	private string sourceId;

	public string SourceId => sourceId;

	public AssemblySourceIdAttribute(string value)
	{
		sourceId = value;
	}
}
