using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.Microsoft.Win32.Registry;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.Microsoft.Win32.Registry.SR)));

	internal static string AccessControl_InvalidHandle => GetResourceString("AccessControl_InvalidHandle");

	internal static string Arg_RegSubKeyAbsent => GetResourceString("Arg_RegSubKeyAbsent");

	internal static string Arg_RegKeyDelHive => GetResourceString("Arg_RegKeyDelHive");

	internal static string Arg_RegKeyNoRemoteConnect => GetResourceString("Arg_RegKeyNoRemoteConnect");

	internal static string Arg_RegKeyOutOfRange => GetResourceString("Arg_RegKeyOutOfRange");

	internal static string Arg_RegKeyNotFound => GetResourceString("Arg_RegKeyNotFound");

	internal static string Arg_RegKeyStrLenBug => GetResourceString("Arg_RegKeyStrLenBug");

	internal static string Arg_RegValStrLenBug => GetResourceString("Arg_RegValStrLenBug");

	internal static string Arg_RegBadKeyKind => GetResourceString("Arg_RegBadKeyKind");

	internal static string Arg_RegGetOverflowBug => GetResourceString("Arg_RegGetOverflowBug");

	internal static string Arg_RegSetMismatchedKind => GetResourceString("Arg_RegSetMismatchedKind");

	internal static string Arg_RegSetBadArrType => GetResourceString("Arg_RegSetBadArrType");

	internal static string Arg_RegSetStrArrNull => GetResourceString("Arg_RegSetStrArrNull");

	internal static string Arg_RegInvalidKeyName => GetResourceString("Arg_RegInvalidKeyName");

	internal static string Arg_DllInitFailure => GetResourceString("Arg_DllInitFailure");

	internal static string Arg_EnumIllegalVal => GetResourceString("Arg_EnumIllegalVal");

	internal static string Arg_RegSubKeyValueAbsent => GetResourceString("Arg_RegSubKeyValueAbsent");

	internal static string Argument_InvalidRegistryOptionsCheck => GetResourceString("Argument_InvalidRegistryOptionsCheck");

	internal static string Argument_InvalidRegistryViewCheck => GetResourceString("Argument_InvalidRegistryViewCheck");

	internal static string Argument_InvalidRegistryKeyPermissionCheck => GetResourceString("Argument_InvalidRegistryKeyPermissionCheck");

	internal static string InvalidOperation_RegRemoveSubKey => GetResourceString("InvalidOperation_RegRemoveSubKey");

	internal static string ObjectDisposed_RegKeyClosed => GetResourceString("ObjectDisposed_RegKeyClosed");

	internal static string PlatformNotSupported_Registry => GetResourceString("PlatformNotSupported_Registry");

	internal static string Security_RegistryPermission => GetResourceString("Security_RegistryPermission");

	internal static string UnauthorizedAccess_RegistryKeyGeneric_Key => GetResourceString("UnauthorizedAccess_RegistryKeyGeneric_Key");

	internal static string UnauthorizedAccess_RegistryNoWrite => GetResourceString("UnauthorizedAccess_RegistryNoWrite");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString = null)
	{
		if (UsingResourceKeys())
		{
			return defaultString ?? resourceKey;
		}
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(provider, resourceFormat, p1);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(provider, resourceFormat, p1, p2);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(provider, resourceFormat, p1, p2, p3);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(provider, resourceFormat, args);
		}
		return resourceFormat;
	}
}
