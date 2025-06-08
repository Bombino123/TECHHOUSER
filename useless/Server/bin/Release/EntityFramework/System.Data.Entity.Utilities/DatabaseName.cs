using System.Data.Entity.Resources;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Utilities;

internal class DatabaseName
{
	private const string NamePartRegex = "(?:(?:\\[(?<part{0}>(?:(?:\\]\\])|[^\\]])+)\\])|(?<part{0}>[^\\.\\[\\]]+))";

	private static readonly Regex _partExtractor = new Regex(string.Format(CultureInfo.InvariantCulture, "^{0}(?:\\.{1})?$", new object[2]
	{
		string.Format(CultureInfo.InvariantCulture, "(?:(?:\\[(?<part{0}>(?:(?:\\]\\])|[^\\]])+)\\])|(?<part{0}>[^\\.\\[\\]]+))", new object[1] { 1 }),
		string.Format(CultureInfo.InvariantCulture, "(?:(?:\\[(?<part{0}>(?:(?:\\]\\])|[^\\]])+)\\])|(?<part{0}>[^\\.\\[\\]]+))", new object[1] { 2 })
	}), RegexOptions.Compiled);

	private readonly string _name;

	private readonly string _schema;

	public string Name => _name;

	public string Schema => _schema;

	public static DatabaseName Parse(string name)
	{
		Match match = _partExtractor.Match(name.Trim());
		if (!match.Success)
		{
			throw Error.InvalidDatabaseName(name);
		}
		string text = match.Groups["part1"].Value.Replace("]]", "]");
		string text2 = match.Groups["part2"].Value.Replace("]]", "]");
		if (string.IsNullOrWhiteSpace(text2))
		{
			return new DatabaseName(text);
		}
		return new DatabaseName(text2, text);
	}

	public DatabaseName(string name)
		: this(name, null)
	{
	}

	public DatabaseName(string name, string schema)
	{
		_name = name;
		_schema = ((!string.IsNullOrEmpty(schema)) ? schema : null);
	}

	public override string ToString()
	{
		string text = Escape(_name);
		if (_schema != null)
		{
			text = Escape(_schema) + "." + text;
		}
		return text;
	}

	private static string Escape(string name)
	{
		if (name.IndexOfAny(new char[3] { ']', '[', '.' }) == -1)
		{
			return name;
		}
		return "[" + name.Replace("]", "]]") + "]";
	}

	public bool Equals(DatabaseName other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (string.Equals(other._name, _name, StringComparison.Ordinal))
		{
			return string.Equals(other._schema, _schema, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == typeof(DatabaseName))
		{
			return Equals((DatabaseName)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (_name.GetHashCode() * 397) ^ ((_schema != null) ? _schema.GetHashCode() : 0);
	}
}
