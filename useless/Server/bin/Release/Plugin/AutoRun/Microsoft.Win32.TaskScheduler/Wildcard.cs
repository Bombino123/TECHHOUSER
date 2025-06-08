using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public class Wildcard : Regex
{
	public Wildcard([NotNull] string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)
		: base(WildcardToRegex(pattern), options)
	{
	}

	public static string WildcardToRegex([NotNull] string pattern)
	{
		string input = Regex.Escape(pattern);
		input = Regex.Replace(input, "(?<!\\\\)\\\\\\*", ".*");
		input = Regex.Replace(input, "\\\\\\\\\\\\\\*", "\\*");
		input = Regex.Replace(input, "(?<!\\\\)\\\\\\?", ".");
		input = Regex.Replace(input, "\\\\\\\\\\\\\\?", "\\?");
		return "^" + Regex.Replace(input, "\\\\\\\\\\\\\\\\", "\\\\") + "$";
	}
}
