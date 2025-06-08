using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal static class Utils
{
	internal static void ExtractNamespaceAndName(string qualifiedTypeName, out string namespaceName, out string name)
	{
		GetBeforeAndAfterLastPeriod(qualifiedTypeName, out namespaceName, out name);
	}

	internal static string ExtractTypeName(string qualifiedTypeName)
	{
		return GetEverythingAfterLastPeriod(qualifiedTypeName);
	}

	private static void GetBeforeAndAfterLastPeriod(string qualifiedTypeName, out string before, out string after)
	{
		int num = qualifiedTypeName.LastIndexOf('.');
		if (num < 0)
		{
			before = null;
			after = qualifiedTypeName;
		}
		else
		{
			before = qualifiedTypeName.Substring(0, num);
			after = qualifiedTypeName.Substring(num + 1);
		}
	}

	internal static string GetEverythingBeforeLastPeriod(string qualifiedTypeName)
	{
		int num = qualifiedTypeName.LastIndexOf('.');
		if (num < 0)
		{
			return null;
		}
		return qualifiedTypeName.Substring(0, num);
	}

	private static string GetEverythingAfterLastPeriod(string qualifiedTypeName)
	{
		int num = qualifiedTypeName.LastIndexOf('.');
		if (num < 0)
		{
			return qualifiedTypeName;
		}
		return qualifiedTypeName.Substring(num + 1);
	}

	public static bool GetString(Schema schema, XmlReader reader, out string value)
	{
		if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
		{
			value = null;
			return false;
		}
		value = reader.Value;
		if (string.IsNullOrEmpty(value))
		{
			schema.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidName(value, reader.Name));
			return false;
		}
		return true;
	}

	public static bool GetDottedName(Schema schema, XmlReader reader, out string name)
	{
		if (!GetString(schema, reader, out name))
		{
			return false;
		}
		return ValidateDottedName(schema, reader, name);
	}

	internal static bool ValidateDottedName(Schema schema, XmlReader reader, string name)
	{
		if (schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			string[] array = name.Split(new char[1] { '.' });
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].IsValidUndottedName())
				{
					schema.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidName(name, reader.Name));
					return false;
				}
			}
		}
		return true;
	}

	public static bool GetUndottedName(Schema schema, XmlReader reader, out string name)
	{
		if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
		{
			name = null;
			return false;
		}
		name = reader.Value;
		if (string.IsNullOrEmpty(name))
		{
			schema.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader, Strings.EmptyName(reader.Name));
			return false;
		}
		if (schema.DataModel == SchemaDataModelOption.EntityDataModel && !name.IsValidUndottedName())
		{
			schema.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidName(name, reader.Name));
			return false;
		}
		return true;
	}

	public static bool GetBool(Schema schema, XmlReader reader, out bool value)
	{
		if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
		{
			value = true;
			return false;
		}
		try
		{
			value = reader.ReadContentAsBoolean();
			return true;
		}
		catch (XmlException)
		{
			schema.AddError(ErrorCode.BoolValueExpected, EdmSchemaErrorSeverity.Error, reader, Strings.ValueNotUnderstood(reader.Value, reader.Name));
		}
		value = true;
		return false;
	}

	public static bool GetInt(Schema schema, XmlReader reader, out int value)
	{
		if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
		{
			value = 0;
			return false;
		}
		string value2 = reader.Value;
		value = int.MinValue;
		if (int.TryParse(value2, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
		{
			return true;
		}
		schema.AddError(ErrorCode.IntegerExpected, EdmSchemaErrorSeverity.Error, reader, Strings.ValueNotUnderstood(reader.Value, reader.Name));
		return false;
	}

	public static bool GetByte(Schema schema, XmlReader reader, out byte value)
	{
		if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
		{
			value = 0;
			return false;
		}
		string value2 = reader.Value;
		value = 0;
		if (byte.TryParse(value2, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
		{
			return true;
		}
		schema.AddError(ErrorCode.ByteValueExpected, EdmSchemaErrorSeverity.Error, reader, Strings.ValueNotUnderstood(reader.Value, reader.Name));
		return false;
	}

	public static int CompareNames(string lhsName, string rhsName)
	{
		return string.Compare(lhsName, rhsName, StringComparison.Ordinal);
	}
}
