using System.Data.Entity.SqlServer.Resources;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class StringExtensions
{
	private const string StartCharacterExp = "[\\p{L}\\p{Nl}_]";

	private const string OtherCharacterExp = "[\\p{L}\\p{Nl}\\p{Nd}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Cf}]";

	private const string NameExp = "[\\p{L}\\p{Nl}_][\\p{L}\\p{Nl}\\p{Nd}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Cf}]{0,}";

	private static readonly Regex _undottedNameValidator = new Regex("^[\\p{L}\\p{Nl}_][\\p{L}\\p{Nl}\\p{Nd}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Cf}]{0,}$", RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly Regex _migrationIdPattern = new Regex("\\d{15}_.+");

	private static readonly string[] _lineEndings = new string[2] { "\r\n", "\n" };

	public static bool EqualsIgnoreCase(this string s1, string s2)
	{
		return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
	}

	internal static bool EqualsOrdinal(this string s1, string s2)
	{
		return string.Equals(s1, s2, StringComparison.Ordinal);
	}

	public static string MigrationName(this string migrationId)
	{
		return migrationId.Substring(16);
	}

	public static string RestrictTo(this string s, int size)
	{
		if (string.IsNullOrEmpty(s) || s.Length <= size)
		{
			return s;
		}
		return s.Substring(0, size);
	}

	public static void EachLine(this string s, Action<string> action)
	{
		s.Split(_lineEndings, StringSplitOptions.None).Each<string>(action);
	}

	public static bool IsValidMigrationId(this string migrationId)
	{
		if (!_migrationIdPattern.IsMatch(migrationId))
		{
			return migrationId == "0";
		}
		return true;
	}

	public static bool IsAutomaticMigration(this string migrationId)
	{
		return migrationId.EndsWith(Strings.AutomaticMigration, StringComparison.Ordinal);
	}

	public static string ToAutomaticMigrationId(this string migrationId)
	{
		return Convert.ToInt64(migrationId.Substring(0, 15), CultureInfo.InvariantCulture) - 1 + migrationId.Substring(15) + "_" + Strings.AutomaticMigration;
	}

	public static bool IsValidUndottedName(this string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return _undottedNameValidator.IsMatch(name);
		}
		return false;
	}
}
