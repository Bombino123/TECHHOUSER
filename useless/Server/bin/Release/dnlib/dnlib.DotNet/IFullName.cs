namespace dnlib.DotNet;

public interface IFullName
{
	string FullName { get; }

	UTF8String Name { get; set; }
}
