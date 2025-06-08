using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace System.Data.SQLite;

[DefaultProperty("DataSource")]
[DefaultMember("Item")]
public sealed class SQLiteConnectionStringBuilder : DbConnectionStringBuilder
{
	private Hashtable _properties;

	[Browsable(true)]
	[DefaultValue(3)]
	public int Version
	{
		get
		{
			TryGetValue("version", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			if (value != 3)
			{
				throw new NotSupportedException();
			}
			this["version"] = value;
		}
	}

	[DisplayName("Synchronous")]
	[Browsable(true)]
	[DefaultValue(SynchronizationModes.Normal)]
	public SynchronizationModes SyncMode
	{
		get
		{
			TryGetValue("synchronous", out var value);
			if (value is string)
			{
				return (SynchronizationModes)TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(value);
			}
			return (SynchronizationModes)value;
		}
		set
		{
			this["synchronous"] = value;
		}
	}

	[DisplayName("Use UTF-16 Encoding")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool UseUTF16Encoding
	{
		get
		{
			TryGetValue("useutf16encoding", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["useutf16encoding"] = value;
		}
	}

	[Browsable(true)]
	[DefaultValue(false)]
	public bool Pooling
	{
		get
		{
			TryGetValue("pooling", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["pooling"] = value;
		}
	}

	[DisplayName("Binary GUID")]
	[Browsable(true)]
	[DefaultValue(true)]
	public bool BinaryGUID
	{
		get
		{
			TryGetValue("binaryguid", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["binaryguid"] = value;
		}
	}

	[DisplayName("Data Source")]
	[Browsable(true)]
	[DefaultValue("")]
	public string DataSource
	{
		get
		{
			TryGetValue("data source", out var value);
			return value?.ToString();
		}
		set
		{
			this["data source"] = value;
		}
	}

	[DisplayName("URI")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string Uri
	{
		get
		{
			TryGetValue("uri", out var value);
			return value?.ToString();
		}
		set
		{
			this["uri"] = value;
		}
	}

	[DisplayName("Full URI")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string FullUri
	{
		get
		{
			TryGetValue("fulluri", out var value);
			return value?.ToString();
		}
		set
		{
			this["fulluri"] = value;
		}
	}

	[DisplayName("Default Timeout")]
	[Browsable(true)]
	[DefaultValue(30)]
	public int DefaultTimeout
	{
		get
		{
			TryGetValue("default timeout", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["default timeout"] = value;
		}
	}

	[DisplayName("Busy Timeout")]
	[Browsable(true)]
	[DefaultValue(0)]
	public int BusyTimeout
	{
		get
		{
			TryGetValue("busytimeout", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["busytimeout"] = value;
		}
	}

	[DisplayName("Wait Timeout")]
	[Browsable(true)]
	[DefaultValue(30000)]
	public int WaitTimeout
	{
		get
		{
			TryGetValue("waittimeout", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["waittimeout"] = value;
		}
	}

	[DisplayName("Prepare Retries")]
	[Browsable(true)]
	[DefaultValue(3)]
	public int PrepareRetries
	{
		get
		{
			TryGetValue("prepareretries", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["prepareretries"] = value;
		}
	}

	[DisplayName("Progress Ops")]
	[Browsable(true)]
	[DefaultValue(0)]
	public int ProgressOps
	{
		get
		{
			TryGetValue("progressops", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["progressops"] = value;
		}
	}

	[Browsable(true)]
	[DefaultValue(true)]
	public bool Enlist
	{
		get
		{
			TryGetValue("enlist", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["enlist"] = value;
		}
	}

	[DisplayName("Fail If Missing")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool FailIfMissing
	{
		get
		{
			TryGetValue("failifmissing", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["failifmissing"] = value;
		}
	}

	[DisplayName("Legacy Format")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool LegacyFormat
	{
		get
		{
			TryGetValue("legacy format", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["legacy format"] = value;
		}
	}

	[DisplayName("Read Only")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool ReadOnly
	{
		get
		{
			TryGetValue("read only", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["read only"] = value;
		}
	}

	[Browsable(true)]
	[PasswordPropertyText(true)]
	[DefaultValue("")]
	public string Password
	{
		get
		{
			TryGetValue("password", out var value);
			return value?.ToString();
		}
		set
		{
			this["password"] = value;
		}
	}

	[DisplayName("Hexadecimal Password")]
	[Browsable(true)]
	[PasswordPropertyText(true)]
	[DefaultValue(null)]
	public byte[] HexPassword
	{
		get
		{
			if (TryGetValue("hexpassword", out var value))
			{
				if (value is string)
				{
					return SQLiteConnection.FromHexString((string)value);
				}
				if (value != null)
				{
					return (byte[])value;
				}
			}
			return null;
		}
		set
		{
			this["hexpassword"] = SQLiteConnection.ToHexString(value);
		}
	}

	[DisplayName("Textual Password")]
	[Browsable(true)]
	[PasswordPropertyText(true)]
	[DefaultValue(null)]
	public string TextPassword
	{
		get
		{
			if (TryGetValue("textpassword", out var value))
			{
				return value?.ToString();
			}
			return null;
		}
		set
		{
			this["textpassword"] = value;
		}
	}

	[DisplayName("Page Size")]
	[Browsable(true)]
	[DefaultValue(4096)]
	public int PageSize
	{
		get
		{
			TryGetValue("page size", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["page size"] = value;
		}
	}

	[DisplayName("Maximum Page Count")]
	[Browsable(true)]
	[DefaultValue(0)]
	public int MaxPageCount
	{
		get
		{
			TryGetValue("max page count", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["max page count"] = value;
		}
	}

	[DisplayName("Cache Size")]
	[Browsable(true)]
	[DefaultValue(-2000)]
	public int CacheSize
	{
		get
		{
			TryGetValue("cache size", out var value);
			return Convert.ToInt32(value, CultureInfo.CurrentCulture);
		}
		set
		{
			this["cache size"] = value;
		}
	}

	[DisplayName("DateTime Format")]
	[Browsable(true)]
	[DefaultValue(SQLiteDateFormats.ISO8601)]
	public SQLiteDateFormats DateTimeFormat
	{
		get
		{
			if (TryGetValue("datetimeformat", out var value))
			{
				if (value is SQLiteDateFormats)
				{
					return (SQLiteDateFormats)value;
				}
				if (value != null)
				{
					return (SQLiteDateFormats)TypeDescriptor.GetConverter(typeof(SQLiteDateFormats)).ConvertFrom(value);
				}
			}
			return SQLiteDateFormats.ISO8601;
		}
		set
		{
			this["datetimeformat"] = value;
		}
	}

	[DisplayName("DateTime Kind")]
	[Browsable(true)]
	[DefaultValue(DateTimeKind.Unspecified)]
	public DateTimeKind DateTimeKind
	{
		get
		{
			if (TryGetValue("datetimekind", out var value))
			{
				if (value is DateTimeKind)
				{
					return (DateTimeKind)value;
				}
				if (value != null)
				{
					return (DateTimeKind)TypeDescriptor.GetConverter(typeof(DateTimeKind)).ConvertFrom(value);
				}
			}
			return DateTimeKind.Unspecified;
		}
		set
		{
			this["datetimekind"] = value;
		}
	}

	[DisplayName("DateTime Format String")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string DateTimeFormatString
	{
		get
		{
			if (TryGetValue("datetimeformatstring", out var value))
			{
				if (value is string)
				{
					return (string)value;
				}
				if (value != null)
				{
					return value.ToString();
				}
			}
			return null;
		}
		set
		{
			this["datetimeformatstring"] = value;
		}
	}

	[DisplayName("Base Schema Name")]
	[Browsable(true)]
	[DefaultValue("sqlite_default_schema")]
	public string BaseSchemaName
	{
		get
		{
			if (TryGetValue("baseschemaname", out var value))
			{
				if (value is string)
				{
					return (string)value;
				}
				if (value != null)
				{
					return value.ToString();
				}
			}
			return null;
		}
		set
		{
			this["baseschemaname"] = value;
		}
	}

	[Browsable(true)]
	[DefaultValue(SQLiteJournalModeEnum.Default)]
	[DisplayName("Journal Mode")]
	public SQLiteJournalModeEnum JournalMode
	{
		get
		{
			TryGetValue("journal mode", out var value);
			if (value is string)
			{
				return (SQLiteJournalModeEnum)TypeDescriptor.GetConverter(typeof(SQLiteJournalModeEnum)).ConvertFrom(value);
			}
			return (SQLiteJournalModeEnum)value;
		}
		set
		{
			this["journal mode"] = value;
		}
	}

	[Browsable(true)]
	[DefaultValue(IsolationLevel.Serializable)]
	[DisplayName("Default Isolation Level")]
	public IsolationLevel DefaultIsolationLevel
	{
		get
		{
			TryGetValue("default isolationlevel", out var value);
			if (value is string)
			{
				return (IsolationLevel)TypeDescriptor.GetConverter(typeof(IsolationLevel)).ConvertFrom(value);
			}
			return (IsolationLevel)value;
		}
		set
		{
			this["default isolationlevel"] = value;
		}
	}

	[DisplayName("Default Database Type")]
	[Browsable(true)]
	[DefaultValue((DbType)(-1))]
	public DbType DefaultDbType
	{
		get
		{
			if (TryGetValue("defaultdbtype", out var value))
			{
				if (value is string)
				{
					return (DbType)TypeDescriptor.GetConverter(typeof(DbType)).ConvertFrom(value);
				}
				if (value != null)
				{
					return (DbType)value;
				}
			}
			return (DbType)(-1);
		}
		set
		{
			this["defaultdbtype"] = value;
		}
	}

	[DisplayName("Default Type Name")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string DefaultTypeName
	{
		get
		{
			TryGetValue("defaulttypename", out var value);
			return value?.ToString();
		}
		set
		{
			this["defaulttypename"] = value;
		}
	}

	[DisplayName("VFS Name")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string VfsName
	{
		get
		{
			TryGetValue("vfsname", out var value);
			return value?.ToString();
		}
		set
		{
			this["vfsname"] = value;
		}
	}

	[DisplayName("Foreign Keys")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool ForeignKeys
	{
		get
		{
			TryGetValue("foreign keys", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["foreign keys"] = value;
		}
	}

	[DisplayName("Recursive Triggers")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool RecursiveTriggers
	{
		get
		{
			TryGetValue("recursive triggers", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["recursive triggers"] = value;
		}
	}

	[DisplayName("ZipVFS Version")]
	[Browsable(true)]
	[DefaultValue(null)]
	public string ZipVfsVersion
	{
		get
		{
			TryGetValue("zipvfsversion", out var value);
			return value?.ToString();
		}
		set
		{
			this["zipvfsversion"] = value;
		}
	}

	[Browsable(true)]
	[DefaultValue(SQLiteConnectionFlags.Default)]
	public SQLiteConnectionFlags Flags
	{
		get
		{
			if (TryGetValue("flags", out var value))
			{
				if (value is SQLiteConnectionFlags)
				{
					return (SQLiteConnectionFlags)value;
				}
				if (value != null)
				{
					return (SQLiteConnectionFlags)TypeDescriptor.GetConverter(typeof(SQLiteConnectionFlags)).ConvertFrom(value);
				}
			}
			return SQLiteConnectionFlags.Default;
		}
		set
		{
			this["flags"] = value;
		}
	}

	[DisplayName("Set Defaults")]
	[Browsable(true)]
	[DefaultValue(true)]
	public bool SetDefaults
	{
		get
		{
			TryGetValue("setdefaults", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["setdefaults"] = value;
		}
	}

	[DisplayName("To Full Path")]
	[Browsable(true)]
	[DefaultValue(true)]
	public bool ToFullPath
	{
		get
		{
			TryGetValue("tofullpath", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["tofullpath"] = value;
		}
	}

	[DisplayName("No Default Flags")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool NoDefaultFlags
	{
		get
		{
			TryGetValue("nodefaultflags", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["nodefaultflags"] = value;
		}
	}

	[DisplayName("No Shared Flags")]
	[Browsable(true)]
	[DefaultValue(false)]
	public bool NoSharedFlags
	{
		get
		{
			TryGetValue("nosharedflags", out var value);
			return SQLiteConvert.ToBoolean(value);
		}
		set
		{
			this["nosharedflags"] = value;
		}
	}

	public SQLiteConnectionStringBuilder()
	{
		Initialize(null);
	}

	public SQLiteConnectionStringBuilder(string connectionString)
	{
		Initialize(connectionString);
	}

	private void Initialize(string cnnString)
	{
		_properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
		try
		{
			base.GetProperties(_properties);
		}
		catch (NotImplementedException)
		{
			FallbackGetProperties(_properties);
		}
		if (!string.IsNullOrEmpty(cnnString))
		{
			base.ConnectionString = cnnString;
		}
	}

	public override bool TryGetValue(string keyword, out object value)
	{
		bool flag = base.TryGetValue(keyword, out value);
		if (!_properties.ContainsKey(keyword))
		{
			return flag;
		}
		if (!(_properties[keyword] is PropertyDescriptor propertyDescriptor))
		{
			return flag;
		}
		if (flag)
		{
			if (propertyDescriptor.PropertyType == typeof(bool))
			{
				value = SQLiteConvert.ToBoolean(value);
			}
			else if (propertyDescriptor.PropertyType != typeof(byte[]))
			{
				value = TypeDescriptor.GetConverter(propertyDescriptor.PropertyType).ConvertFrom(value);
			}
		}
		else if (propertyDescriptor.Attributes[typeof(DefaultValueAttribute)] is DefaultValueAttribute defaultValueAttribute)
		{
			value = defaultValueAttribute.Value;
			flag = true;
		}
		return flag;
	}

	private void FallbackGetProperties(Hashtable propertyList)
	{
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, noCustomTypeDesc: true))
		{
			if (property.Name != "ConnectionString" && !propertyList.ContainsKey(property.DisplayName))
			{
				propertyList.Add(property.DisplayName, property);
			}
		}
	}
}
