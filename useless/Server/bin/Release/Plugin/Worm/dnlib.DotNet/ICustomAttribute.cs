using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ICustomAttribute
{
	ITypeDefOrRef AttributeType { get; }

	string TypeFullName { get; }

	IList<CANamedArgument> NamedArguments { get; }

	bool HasNamedArguments { get; }

	IEnumerable<CANamedArgument> Fields { get; }

	IEnumerable<CANamedArgument> Properties { get; }
}
