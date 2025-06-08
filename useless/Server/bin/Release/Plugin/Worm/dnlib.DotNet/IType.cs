using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IType : IFullName, IOwnerModule, ICodedToken, IMDTokenProvider, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter
{
	bool IsValueType { get; }

	string TypeName { get; }

	string ReflectionName { get; }

	string Namespace { get; }

	string ReflectionNamespace { get; }

	string ReflectionFullName { get; }

	string AssemblyQualifiedName { get; }

	IAssembly DefinitionAssembly { get; }

	IScope Scope { get; }

	ITypeDefOrRef ScopeType { get; }

	bool IsPrimitive { get; }
}
