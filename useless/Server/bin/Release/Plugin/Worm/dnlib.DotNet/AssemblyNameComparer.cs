using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public readonly struct AssemblyNameComparer : IEqualityComparer<IAssembly>
{
	public static readonly AssemblyNameComparer CompareAll = new AssemblyNameComparer(AssemblyNameComparerFlags.All);

	public static readonly AssemblyNameComparer NameAndPublicKeyTokenOnly = new AssemblyNameComparer(AssemblyNameComparerFlags.Name | AssemblyNameComparerFlags.PublicKeyToken);

	public static readonly AssemblyNameComparer NameOnly = new AssemblyNameComparer(AssemblyNameComparerFlags.Name);

	private readonly AssemblyNameComparerFlags flags;

	public bool CompareName => (flags & AssemblyNameComparerFlags.Name) != 0;

	public bool CompareVersion => (flags & AssemblyNameComparerFlags.Version) != 0;

	public bool ComparePublicKeyToken => (flags & AssemblyNameComparerFlags.PublicKeyToken) != 0;

	public bool CompareCulture => (flags & AssemblyNameComparerFlags.Culture) != 0;

	public bool CompareContentType => (flags & AssemblyNameComparerFlags.ContentType) != 0;

	public AssemblyNameComparer(AssemblyNameComparerFlags flags)
	{
		this.flags = flags;
	}

	public int CompareTo(IAssembly a, IAssembly b)
	{
		if (a == b)
		{
			return 0;
		}
		if (a == null)
		{
			return -1;
		}
		if (b == null)
		{
			return 1;
		}
		int result;
		if (CompareName && (result = UTF8String.CaseInsensitiveCompareTo(a.Name, b.Name)) != 0)
		{
			return result;
		}
		if (CompareVersion && (result = Utils.CompareTo(a.Version, b.Version)) != 0)
		{
			return result;
		}
		if (ComparePublicKeyToken && (result = PublicKeyBase.TokenCompareTo(a.PublicKeyOrToken, b.PublicKeyOrToken)) != 0)
		{
			return result;
		}
		if (CompareCulture && (result = Utils.LocaleCompareTo(a.Culture, b.Culture)) != 0)
		{
			return result;
		}
		if (CompareContentType && (result = a.ContentType.CompareTo(b.ContentType)) != 0)
		{
			return result;
		}
		return 0;
	}

	public bool Equals(IAssembly a, IAssembly b)
	{
		return CompareTo(a, b) == 0;
	}

	public int CompareClosest(IAssembly requested, IAssembly a, IAssembly b)
	{
		if (a == b)
		{
			return 0;
		}
		if (a == null)
		{
			if (CompareName)
			{
				return UTF8String.CaseInsensitiveEquals(requested.Name, b.Name) ? 1 : 0;
			}
			return 1;
		}
		if (b == null)
		{
			if (CompareName)
			{
				return (!UTF8String.CaseInsensitiveEquals(requested.Name, a.Name)) ? 1 : 0;
			}
			return 0;
		}
		if (CompareName)
		{
			bool flag = UTF8String.CaseInsensitiveEquals(requested.Name, a.Name);
			bool flag2 = UTF8String.CaseInsensitiveEquals(requested.Name, b.Name);
			if (flag && !flag2)
			{
				return 0;
			}
			if (!flag && flag2)
			{
				return 1;
			}
			if (!flag && !flag2)
			{
				return -1;
			}
		}
		if (ComparePublicKeyToken)
		{
			bool flag3;
			bool flag4;
			if (PublicKeyBase.IsNullOrEmpty2(requested.PublicKeyOrToken))
			{
				flag3 = PublicKeyBase.IsNullOrEmpty2(a.PublicKeyOrToken);
				flag4 = PublicKeyBase.IsNullOrEmpty2(b.PublicKeyOrToken);
			}
			else
			{
				flag3 = PublicKeyBase.TokenEquals(requested.PublicKeyOrToken, a.PublicKeyOrToken);
				flag4 = PublicKeyBase.TokenEquals(requested.PublicKeyOrToken, b.PublicKeyOrToken);
			}
			if (flag3 && !flag4)
			{
				return 0;
			}
			if (!flag3 && flag4)
			{
				return 1;
			}
		}
		if (CompareVersion && !Utils.Equals(a.Version, b.Version))
		{
			Version version = Utils.CreateVersionWithNoUndefinedValues(requested.Version);
			if (version == new Version(0, 0, 0, 0))
			{
				version = new Version(65535, 65535, 65535, 65535);
			}
			int num = Utils.CompareTo(a.Version, version);
			int num2 = Utils.CompareTo(b.Version, version);
			if (num == 0)
			{
				return 0;
			}
			if (num2 == 0)
			{
				return 1;
			}
			if (num > 0 && num2 < 0)
			{
				return 0;
			}
			if (num < 0 && num2 > 0)
			{
				return 1;
			}
			if (num > 0)
			{
				return (Utils.CompareTo(a.Version, b.Version) >= 0) ? 1 : 0;
			}
			return (Utils.CompareTo(a.Version, b.Version) <= 0) ? 1 : 0;
		}
		if (CompareCulture)
		{
			bool flag5 = Utils.LocaleEquals(requested.Culture, a.Culture);
			bool flag6 = Utils.LocaleEquals(requested.Culture, b.Culture);
			if (flag5 && !flag6)
			{
				return 0;
			}
			if (!flag5 && flag6)
			{
				return 1;
			}
		}
		if (CompareContentType)
		{
			bool flag7 = requested.ContentType == a.ContentType;
			bool flag8 = requested.ContentType == b.ContentType;
			if (flag7 && !flag8)
			{
				return 0;
			}
			if (!flag7 && flag8)
			{
				return 1;
			}
		}
		return -1;
	}

	public int GetHashCode(IAssembly a)
	{
		if (a == null)
		{
			return 0;
		}
		int num = 0;
		if (CompareName)
		{
			num += UTF8String.GetHashCode(a.Name);
		}
		if (CompareVersion)
		{
			num += Utils.CreateVersionWithNoUndefinedValues(a.Version).GetHashCode();
		}
		if (ComparePublicKeyToken)
		{
			num += PublicKeyBase.GetHashCodeToken(a.PublicKeyOrToken);
		}
		if (CompareCulture)
		{
			num += Utils.GetHashCodeLocale(a.Culture);
		}
		if (CompareContentType)
		{
			num += (int)a.ContentType;
		}
		return num;
	}
}
