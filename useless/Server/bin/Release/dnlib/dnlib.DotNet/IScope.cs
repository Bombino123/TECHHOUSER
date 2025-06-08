namespace dnlib.DotNet;

public interface IScope
{
	ScopeType ScopeType { get; }

	string ScopeName { get; }
}
