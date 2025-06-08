namespace Plugin.Methods;

internal interface Method
{
	string Name { get; }

	void Run(string host, int port);
}
