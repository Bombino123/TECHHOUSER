namespace dnlib.DotNet.Resources;

public sealed class ResourceElement
{
	public string Name { get; set; }

	public IResourceData ResourceData { get; set; }

	public override string ToString()
	{
		return $"N: {Name}, V: {ResourceData}";
	}
}
