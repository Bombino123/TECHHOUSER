using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.EntityClient;

public sealed class EntityConnectionStringBuilder : DbConnectionStringBuilder
{
	internal const string NameParameterName = "name";

	internal const string MetadataParameterName = "metadata";

	internal const string ProviderParameterName = "provider";

	internal const string ProviderConnectionStringParameterName = "provider connection string";

	internal static readonly string[] ValidKeywords = new string[4] { "name", "metadata", "provider", "provider connection string" };

	private string _namedConnectionName;

	private string _providerName;

	private string _metadataLocations;

	private string _storeProviderConnectionString;

	[DisplayName("Name")]
	[EntityResCategory("EntityDataCategory_NamedConnectionString")]
	[EntityResDescription("EntityConnectionString_Name")]
	[RefreshProperties(RefreshProperties.All)]
	public string Name
	{
		get
		{
			return _namedConnectionName ?? "";
		}
		set
		{
			_namedConnectionName = value;
			base["name"] = value;
		}
	}

	[DisplayName("Provider")]
	[EntityResCategory("EntityDataCategory_Source")]
	[EntityResDescription("EntityConnectionString_Provider")]
	[RefreshProperties(RefreshProperties.All)]
	public string Provider
	{
		get
		{
			return _providerName ?? "";
		}
		set
		{
			_providerName = value;
			base["provider"] = value;
		}
	}

	[DisplayName("Metadata")]
	[EntityResCategory("EntityDataCategory_Context")]
	[EntityResDescription("EntityConnectionString_Metadata")]
	[RefreshProperties(RefreshProperties.All)]
	public string Metadata
	{
		get
		{
			return _metadataLocations ?? "";
		}
		set
		{
			_metadataLocations = value;
			base["metadata"] = value;
		}
	}

	[DisplayName("Provider Connection String")]
	[EntityResCategory("EntityDataCategory_Source")]
	[EntityResDescription("EntityConnectionString_ProviderConnectionString")]
	[RefreshProperties(RefreshProperties.All)]
	public string ProviderConnectionString
	{
		get
		{
			return _storeProviderConnectionString ?? "";
		}
		set
		{
			_storeProviderConnectionString = value;
			base["provider connection string"] = value;
		}
	}

	public override bool IsFixedSize => true;

	public override ICollection Keys => new ReadOnlyCollection<string>(ValidKeywords);

	public override object this[string keyword]
	{
		get
		{
			Check.NotNull(keyword, "keyword");
			if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Metadata;
			}
			if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ProviderConnectionString;
			}
			if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Name;
			}
			if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Provider;
			}
			throw new ArgumentException(Strings.EntityClient_KeywordNotSupported(keyword));
		}
		set
		{
			Check.NotNull(keyword, "keyword");
			if (value == null)
			{
				Remove(keyword);
				return;
			}
			if (!(value is string text))
			{
				throw new ArgumentException(Strings.EntityClient_ValueNotString, "value");
			}
			if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
			{
				Metadata = text;
				return;
			}
			if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
			{
				ProviderConnectionString = text;
				return;
			}
			if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
			{
				Name = text;
				return;
			}
			if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
			{
				Provider = text;
				return;
			}
			throw new ArgumentException(Strings.EntityClient_KeywordNotSupported(keyword));
		}
	}

	public EntityConnectionStringBuilder()
	{
	}

	public EntityConnectionStringBuilder(string connectionString)
	{
		base.ConnectionString = connectionString;
	}

	public override void Clear()
	{
		base.Clear();
		_namedConnectionName = null;
		_providerName = null;
		_metadataLocations = null;
		_storeProviderConnectionString = null;
	}

	public override bool ContainsKey(string keyword)
	{
		Check.NotNull(keyword, "keyword");
		string[] validKeywords = ValidKeywords;
		for (int i = 0; i < validKeywords.Length; i++)
		{
			if (validKeywords[i].Equals(keyword, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public override bool TryGetValue(string keyword, out object value)
	{
		Check.NotNull(keyword, "keyword");
		if (ContainsKey(keyword))
		{
			value = this[keyword];
			return true;
		}
		value = null;
		return false;
	}

	public override bool Remove(string keyword)
	{
		if (string.Compare(keyword, "metadata", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_metadataLocations = null;
		}
		else if (string.Compare(keyword, "provider connection string", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_storeProviderConnectionString = null;
		}
		else if (string.Compare(keyword, "name", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_namedConnectionName = null;
		}
		else if (string.Compare(keyword, "provider", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_providerName = null;
		}
		return base.Remove(keyword);
	}
}
