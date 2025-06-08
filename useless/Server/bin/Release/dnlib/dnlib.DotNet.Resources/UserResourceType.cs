namespace dnlib.DotNet.Resources;

public sealed class UserResourceType
{
	private readonly string name;

	private readonly ResourceTypeCode code;

	public string Name => name;

	public ResourceTypeCode Code => code;

	public UserResourceType(string name, ResourceTypeCode code)
	{
		this.name = name;
		this.code = code;
	}

	public override string ToString()
	{
		return $"{(int)code:X2} {name}";
	}
}
