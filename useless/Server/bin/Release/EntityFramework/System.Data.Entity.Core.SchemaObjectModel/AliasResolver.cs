using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class AliasResolver
{
	private enum NameKind
	{
		Alias,
		Namespace
	}

	private readonly Dictionary<string, string> _aliasToNamespaceMap = new Dictionary<string, string>(StringComparer.Ordinal);

	private readonly List<UsingElement> _usingElementCollection = new List<UsingElement>();

	private readonly Schema _definingSchema;

	public AliasResolver(Schema schema)
	{
		_definingSchema = schema;
		if (!string.IsNullOrEmpty(schema.Alias))
		{
			_aliasToNamespaceMap.Add(schema.Alias, schema.Namespace);
		}
	}

	public void Add(UsingElement usingElement)
	{
		string text = usingElement.NamespaceName;
		string text2 = usingElement.Alias;
		if (CheckForSystemNamespace(usingElement, text2, NameKind.Alias))
		{
			text2 = null;
		}
		if (CheckForSystemNamespace(usingElement, text, NameKind.Namespace))
		{
			text = null;
		}
		if (text2 != null && _aliasToNamespaceMap.ContainsKey(text2))
		{
			usingElement.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.AliasNameIsAlreadyDefined(text2));
			text2 = null;
		}
		if (text2 != null)
		{
			_aliasToNamespaceMap.Add(text2, text);
			_usingElementCollection.Add(usingElement);
		}
	}

	public bool TryResolveAlias(string alias, out string namespaceName)
	{
		return _aliasToNamespaceMap.TryGetValue(alias, out namespaceName);
	}

	public void ResolveNamespaces()
	{
		foreach (UsingElement item in _usingElementCollection)
		{
			if (!_definingSchema.SchemaManager.IsValidNamespaceName(item.NamespaceName))
			{
				item.AddError(ErrorCode.InvalidNamespaceInUsing, EdmSchemaErrorSeverity.Error, Strings.InvalidNamespaceInUsing(item.NamespaceName));
			}
		}
	}

	private bool CheckForSystemNamespace(UsingElement refSchema, string name, NameKind nameKind)
	{
		if (EdmItemCollection.IsSystemNamespace(_definingSchema.ProviderManifest, name))
		{
			if (nameKind == NameKind.Alias)
			{
				refSchema.AddError(ErrorCode.CannotUseSystemNamespaceAsAlias, EdmSchemaErrorSeverity.Error, Strings.CannotUseSystemNamespaceAsAlias(name));
			}
			else
			{
				refSchema.AddError(ErrorCode.NeedNotUseSystemNamespaceInUsing, EdmSchemaErrorSeverity.Error, Strings.NeedNotUseSystemNamespaceInUsing(name));
			}
			return true;
		}
		return false;
	}
}
