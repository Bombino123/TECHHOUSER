using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class TypeResolver
{
	private sealed class TypeUsageStructuralComparer : IEqualityComparer<TypeUsage>
	{
		private static readonly TypeUsageStructuralComparer _instance = new TypeUsageStructuralComparer();

		public static TypeUsageStructuralComparer Instance => _instance;

		private TypeUsageStructuralComparer()
		{
		}

		public bool Equals(TypeUsage x, TypeUsage y)
		{
			return TypeSemantics.IsStructurallyEqual(x, y);
		}

		public int GetHashCode(TypeUsage obj)
		{
			return 0;
		}
	}

	private readonly Perspective _perspective;

	private readonly ParserOptions _parserOptions;

	private readonly Dictionary<string, MetadataNamespace> _aliasedNamespaces;

	private readonly HashSet<MetadataNamespace> _namespaces;

	private readonly Dictionary<string, List<InlineFunctionInfo>> _functionDefinitions;

	private bool _includeInlineFunctions;

	private bool _resolveLeftMostUnqualifiedNameAsNamespaceOnly;

	internal Perspective Perspective => _perspective;

	internal ICollection<MetadataNamespace> NamespaceImports => _namespaces;

	internal static TypeUsage StringType => MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.String);

	internal static TypeUsage BooleanType => MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean);

	internal static TypeUsage Int64Type => MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int64);

	internal TypeResolver(Perspective perspective, ParserOptions parserOptions)
	{
		_perspective = perspective;
		_parserOptions = parserOptions;
		_aliasedNamespaces = new Dictionary<string, MetadataNamespace>(parserOptions.NameComparer);
		_namespaces = new HashSet<MetadataNamespace>(MetadataMember.CreateMetadataMemberNameEqualityComparer(parserOptions.NameComparer));
		_functionDefinitions = new Dictionary<string, List<InlineFunctionInfo>>(parserOptions.NameComparer);
		_includeInlineFunctions = true;
		_resolveLeftMostUnqualifiedNameAsNamespaceOnly = false;
	}

	internal void AddAliasedNamespaceImport(string alias, MetadataNamespace @namespace, ErrorContext errCtx)
	{
		if (_aliasedNamespaces.ContainsKey(alias))
		{
			string errorMessage = Strings.NamespaceAliasAlreadyUsed(alias);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		_aliasedNamespaces.Add(alias, @namespace);
	}

	internal void AddNamespaceImport(MetadataNamespace @namespace, ErrorContext errCtx)
	{
		if (_namespaces.Contains(@namespace))
		{
			string errorMessage = Strings.NamespaceAlreadyImported(@namespace.Name);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		_namespaces.Add(@namespace);
	}

	internal void DeclareInlineFunction(string name, InlineFunctionInfo functionInfo)
	{
		if (!_functionDefinitions.TryGetValue(name, out var value))
		{
			value = new List<InlineFunctionInfo>();
			_functionDefinitions.Add(name, value);
		}
		if (value.Exists((InlineFunctionInfo overload) => overload.Parameters.Select((DbVariableReferenceExpression p) => p.ResultType).SequenceEqual(functionInfo.Parameters.Select((DbVariableReferenceExpression p) => p.ResultType), TypeUsageStructuralComparer.Instance)))
		{
			ErrorContext errCtx = functionInfo.FunctionDefAst.ErrCtx;
			string errorMessage = Strings.DuplicatedInlineFunctionOverload(name);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
		value.Add(functionInfo);
	}

	internal IDisposable EnterFunctionNameResolution(bool includeInlineFunctions)
	{
		bool savedIncludeInlineFunctions = _includeInlineFunctions;
		_includeInlineFunctions = includeInlineFunctions;
		return new Disposer(delegate
		{
			_includeInlineFunctions = savedIncludeInlineFunctions;
		});
	}

	internal IDisposable EnterBackwardCompatibilityResolution()
	{
		_resolveLeftMostUnqualifiedNameAsNamespaceOnly = true;
		return new Disposer(delegate
		{
			_resolveLeftMostUnqualifiedNameAsNamespaceOnly = false;
		});
	}

	internal MetadataMember ResolveMetadataMemberName(string[] name, ErrorContext errCtx)
	{
		if (name.Length == 1)
		{
			return ResolveUnqualifiedName(name[0], partOfQualifiedName: false, errCtx);
		}
		return ResolveFullyQualifiedName(name, name.Length, errCtx);
	}

	internal MetadataMember ResolveMetadataMemberAccess(MetadataMember qualifier, string name, ErrorContext errCtx)
	{
		string fullName = GetFullName(qualifier.Name, name);
		if (qualifier.MetadataMemberClass == MetadataMemberClass.Namespace)
		{
			if (TryGetTypeFromMetadata(fullName, out var type))
			{
				return type;
			}
			if (TryGetFunctionFromMetadata(qualifier.Name, name, out var functionGroup))
			{
				return functionGroup;
			}
			return new MetadataNamespace(fullName);
		}
		if (qualifier.MetadataMemberClass == MetadataMemberClass.Type)
		{
			MetadataType metadataType = (MetadataType)qualifier;
			if (TypeSemantics.IsEnumerationType(metadataType.TypeUsage))
			{
				if (_perspective.TryGetEnumMember((EnumType)metadataType.TypeUsage.EdmType, name, _parserOptions.NameComparisonCaseInsensitive, out var outMember))
				{
					return new MetadataEnumMember(fullName, metadataType.TypeUsage, outMember);
				}
				string errorMessage = Strings.NotAMemberOfType(name, qualifier.Name);
				throw EntitySqlException.Create(errCtx, errorMessage, null);
			}
		}
		string errorMessage2 = Strings.InvalidMetadataMemberClassResolution(qualifier.Name, qualifier.MetadataMemberClassName, MetadataNamespace.NamespaceClassName);
		throw EntitySqlException.Create(errCtx, errorMessage2, null);
	}

	internal MetadataMember ResolveUnqualifiedName(string name, bool partOfQualifiedName, ErrorContext errCtx)
	{
		bool flag = partOfQualifiedName && _resolveLeftMostUnqualifiedNameAsNamespaceOnly;
		bool flag2 = !partOfQualifiedName;
		if (!flag && flag2 && TryGetInlineFunction(name, out var inlineFunctionGroup))
		{
			return inlineFunctionGroup;
		}
		if (_aliasedNamespaces.TryGetValue(name, out var value))
		{
			return value;
		}
		if (!flag)
		{
			MetadataType type = null;
			MetadataFunctionGroup functionGroup = null;
			if (!TryGetTypeFromMetadata(name, out type) && flag2)
			{
				string[] array = name.Split(new char[1] { '.' });
				if (array.Length > 1 && array.All((string p) => p.Length > 0))
				{
					string text = array[^1];
					string namespaceName = name.Substring(0, name.Length - text.Length - 1);
					TryGetFunctionFromMetadata(namespaceName, text, out functionGroup);
				}
			}
			MetadataNamespace ns = null;
			foreach (MetadataNamespace @namespace in _namespaces)
			{
				string fullName = GetFullName(@namespace.Name, name);
				if (TryGetTypeFromMetadata(fullName, out var type2))
				{
					if (type != null || functionGroup != null)
					{
						throw AmbiguousMetadataMemberName(errCtx, name, @namespace, ns);
					}
					type = type2;
					ns = @namespace;
				}
				if (flag2 && TryGetFunctionFromMetadata(@namespace.Name, name, out var functionGroup2))
				{
					if (type != null || functionGroup != null)
					{
						throw AmbiguousMetadataMemberName(errCtx, name, @namespace, ns);
					}
					functionGroup = functionGroup2;
					ns = @namespace;
				}
			}
			if (type != null)
			{
				return type;
			}
			if (functionGroup != null)
			{
				return functionGroup;
			}
		}
		return new MetadataNamespace(name);
	}

	private MetadataMember ResolveFullyQualifiedName(string[] name, int length, ErrorContext errCtx)
	{
		MetadataMember qualifier = ((length != 2) ? ResolveFullyQualifiedName(name, length - 1, errCtx) : ResolveUnqualifiedName(name[0], partOfQualifiedName: true, errCtx));
		string name2 = name[length - 1];
		return ResolveMetadataMemberAccess(qualifier, name2, errCtx);
	}

	private static Exception AmbiguousMetadataMemberName(ErrorContext errCtx, string name, MetadataNamespace ns1, MetadataNamespace ns2)
	{
		string errorMessage = Strings.AmbiguousMetadataMemberName(name, ns1.Name, ns2?.Name);
		throw EntitySqlException.Create(errCtx, errorMessage, null);
	}

	private bool TryGetTypeFromMetadata(string typeFullName, out MetadataType type)
	{
		if (_perspective.TryGetTypeByName(typeFullName, _parserOptions.NameComparisonCaseInsensitive, out var typeUsage))
		{
			type = new MetadataType(typeFullName, typeUsage);
			return true;
		}
		type = null;
		return false;
	}

	internal bool TryGetFunctionFromMetadata(string namespaceName, string functionName, out MetadataFunctionGroup functionGroup)
	{
		if (_perspective.TryGetFunctionByName(namespaceName, functionName, _parserOptions.NameComparisonCaseInsensitive, out var functionOverloads))
		{
			functionGroup = new MetadataFunctionGroup(GetFullName(namespaceName, functionName), functionOverloads);
			return true;
		}
		functionGroup = null;
		return false;
	}

	private bool TryGetInlineFunction(string functionName, out InlineFunctionGroup inlineFunctionGroup)
	{
		if (_includeInlineFunctions && _functionDefinitions.TryGetValue(functionName, out var value))
		{
			inlineFunctionGroup = new InlineFunctionGroup(functionName, value);
			return true;
		}
		inlineFunctionGroup = null;
		return false;
	}

	internal static string GetFullName(params string[] names)
	{
		return string.Join(".", names);
	}
}
