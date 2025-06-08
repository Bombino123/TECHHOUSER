using System.Collections.Generic;

namespace dnlib.DotNet;

public interface ICustomAttribute
{
	ITypeDefOrRef AttributeType { get; }

	string TypeFullName { get; }

	IList<CANamedArgument> NamedArguments { get; }

	bool HasNamedArguments { get; }

	IEnumerable<CANamedArgument> Fields { get; }

	IEnumerable<CANamedArgument> Properties { get; }
}
