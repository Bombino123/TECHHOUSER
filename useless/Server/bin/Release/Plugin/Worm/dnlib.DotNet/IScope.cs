using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IScope
{
	ScopeType ScopeType { get; }

	string ScopeName { get; }
}
