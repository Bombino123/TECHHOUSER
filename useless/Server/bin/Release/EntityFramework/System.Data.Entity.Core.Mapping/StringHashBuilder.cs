using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace System.Data.Entity.Core.Mapping;

internal class StringHashBuilder
{
	private readonly HashAlgorithm _hashAlgorithm;

	private const string NewLine = "\n";

	private readonly List<string> _strings = new List<string>();

	private int _totalLength;

	private byte[] _cachedBuffer;

	internal int CharCount => _totalLength;

	internal StringHashBuilder(HashAlgorithm hashAlgorithm)
	{
		_hashAlgorithm = hashAlgorithm;
	}

	internal StringHashBuilder(HashAlgorithm hashAlgorithm, int startingBufferSize)
		: this(hashAlgorithm)
	{
		_cachedBuffer = new byte[startingBufferSize];
	}

	internal virtual void Append(string s)
	{
		InternalAppend(s);
	}

	internal virtual void AppendLine(string s)
	{
		InternalAppend(s);
		InternalAppend("\n");
	}

	private void InternalAppend(string s)
	{
		if (s.Length != 0)
		{
			_strings.Add(s);
			_totalLength += s.Length;
		}
	}

	internal string ComputeHash()
	{
		int byteCount = GetByteCount();
		if (_cachedBuffer == null)
		{
			_cachedBuffer = new byte[byteCount];
		}
		else if (_cachedBuffer.Length < byteCount)
		{
			int num = Math.Max(_cachedBuffer.Length + _cachedBuffer.Length / 2, byteCount);
			_cachedBuffer = new byte[num];
		}
		int num2 = 0;
		foreach (string @string in _strings)
		{
			num2 += Encoding.Unicode.GetBytes(@string, 0, @string.Length, _cachedBuffer, num2);
		}
		return ConvertHashToString(_hashAlgorithm.ComputeHash(_cachedBuffer, 0, byteCount));
	}

	internal void Clear()
	{
		_strings.Clear();
		_totalLength = 0;
	}

	public override string ToString()
	{
		StringBuilder builder = new StringBuilder();
		_strings.Each((string s) => builder.Append(s));
		return builder.ToString();
	}

	private int GetByteCount()
	{
		int num = 0;
		foreach (string @string in _strings)
		{
			num += Encoding.Unicode.GetByteCount(@string);
		}
		return num;
	}

	private static string ConvertHashToString(byte[] hash)
	{
		StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
		for (int i = 0; i < hash.Length; i++)
		{
			stringBuilder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	public static string ComputeHash(HashAlgorithm hashAlgorithm, string source)
	{
		StringHashBuilder stringHashBuilder = new StringHashBuilder(hashAlgorithm);
		stringHashBuilder.Append(source);
		return stringHashBuilder.ComputeHash();
	}
}
