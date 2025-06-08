using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Data.SQLite.EF6;

internal sealed class SQLiteProviderManifest : DbXmlEnabledProviderManifest
{
	private static class TypeHelpers
	{
		public static bool TryGetPrecision(TypeUsage tu, out byte precision)
		{
			precision = 0;
			Facet val = default(Facet);
			if (tu.Facets.TryGetValue("Precision", false, ref val) && !val.IsUnbounded && val.Value != null)
			{
				precision = (byte)val.Value;
				return true;
			}
			return false;
		}

		public static bool TryGetMaxLength(TypeUsage tu, out int maxLength)
		{
			maxLength = 0;
			Facet val = default(Facet);
			if (tu.Facets.TryGetValue("MaxLength", false, ref val) && !val.IsUnbounded && val.Value != null)
			{
				maxLength = (int)val.Value;
				return true;
			}
			return false;
		}

		public static bool TryGetScale(TypeUsage tu, out byte scale)
		{
			scale = 0;
			Facet val = default(Facet);
			if (tu.Facets.TryGetValue("Scale", false, ref val) && !val.IsUnbounded && val.Value != null)
			{
				scale = (byte)val.Value;
				return true;
			}
			return false;
		}
	}

	internal SQLiteDateFormats _dateTimeFormat;

	internal DateTimeKind _dateTimeKind;

	internal string _dateTimeFormatString;

	internal bool _binaryGuid;

	public SQLiteProviderManifest(string manifestToken)
		: base(GetProviderManifest())
	{
		SetFromOptions(ParseProviderManifestToken(GetProviderManifestToken(manifestToken)));
	}

	private static XmlReader GetProviderManifest()
	{
		return GetXmlResource("System.Data.SQLite.SQLiteProviderServices.ProviderManifest.xml");
	}

	private static string GetProviderManifestToken(string manifestToken)
	{
		string settingValue = UnsafeNativeMethods.GetSettingValue("AppendManifestToken_SQLiteProviderManifest", (string)null);
		if (string.IsNullOrEmpty(settingValue))
		{
			return manifestToken;
		}
		int num = settingValue.Length;
		if (manifestToken != null)
		{
			num += manifestToken.Length;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		stringBuilder.Append(manifestToken);
		stringBuilder.Append(settingValue);
		return stringBuilder.ToString();
	}

	private static SortedList<string, string> ParseProviderManifestToken(string manifestToken)
	{
		return SQLiteConnection.ParseConnectionString(manifestToken, false, true);
	}

	internal void SetFromOptions(SortedList<string, string> opts)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		_dateTimeFormat = (SQLiteDateFormats)1;
		_dateTimeKind = DateTimeKind.Unspecified;
		_dateTimeFormatString = null;
		_binaryGuid = false;
		if (opts == null)
		{
			return;
		}
		string[] names = Enum.GetNames(typeof(SQLiteDateFormats));
		foreach (string text in names)
		{
			if (!string.IsNullOrEmpty(text) && SQLiteConnection.FindKey(opts, text, (string)null) != null)
			{
				_dateTimeFormat = (SQLiteDateFormats)Enum.Parse(typeof(SQLiteDateFormats), text, ignoreCase: true);
			}
		}
		object obj = SQLiteConnection.TryParseEnum(typeof(SQLiteDateFormats), SQLiteConnection.FindKey(opts, "DateTimeFormat", (string)null), true);
		if (obj is SQLiteDateFormats)
		{
			_dateTimeFormat = (SQLiteDateFormats)obj;
		}
		obj = SQLiteConnection.TryParseEnum(typeof(DateTimeKind), SQLiteConnection.FindKey(opts, "DateTimeKind", (string)null), true);
		if (obj is DateTimeKind)
		{
			_dateTimeKind = (DateTimeKind)obj;
		}
		string text2 = SQLiteConnection.FindKey(opts, "DateTimeFormatString", (string)null);
		if (text2 != null)
		{
			_dateTimeFormatString = text2;
		}
		text2 = SQLiteConnection.FindKey(opts, "BinaryGUID", (string)null);
		if (text2 != null)
		{
			_binaryGuid = SQLiteConvert.ToBoolean(text2);
		}
	}

	protected override XmlReader GetDbInformation(string informationType)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		return informationType switch
		{
			"StoreSchemaDefinition" => GetStoreSchemaDescription(), 
			"StoreSchemaMapping" => GetStoreSchemaMapping(), 
			"ConceptualSchemaDefinition" => null, 
			_ => throw new ProviderIncompatibleException($"SQLite does not support this information type '{informationType}'."), 
		};
	}

	public override TypeUsage GetEdmType(TypeUsage storeType)
	{
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Invalid comparison between Unknown and I4
		if (storeType == null)
		{
			throw new ArgumentNullException("storeType");
		}
		string text = storeType.EdmType.Name.ToLowerInvariant();
		if (!((DbXmlEnabledProviderManifest)this).StoreTypeNameToEdmPrimitiveType.TryGetValue(text, out var value))
		{
			throw new ArgumentException($"SQLite does not support the type '{text}'.");
		}
		int maxLength = 0;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = true;
		PrimitiveTypeKind val;
		switch (text)
		{
		case "tinyint":
		case "smallint":
		case "integer":
		case "bit":
		case "uniqueidentifier":
		case "int":
		case "float":
		case "real":
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)value);
		case "varchar":
			val = (PrimitiveTypeKind)12;
			flag3 = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
			flag = false;
			flag2 = false;
			break;
		case "char":
			val = (PrimitiveTypeKind)12;
			flag3 = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
			flag = false;
			flag2 = true;
			break;
		case "nvarchar":
			val = (PrimitiveTypeKind)12;
			flag3 = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
			flag = true;
			flag2 = false;
			break;
		case "nchar":
			val = (PrimitiveTypeKind)12;
			flag3 = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
			flag = true;
			flag2 = true;
			break;
		case "blob":
			val = (PrimitiveTypeKind)0;
			flag3 = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
			flag2 = false;
			break;
		case "decimal":
		{
			if (TypeHelpers.TryGetPrecision(storeType, out var precision) && TypeHelpers.TryGetScale(storeType, out var scale))
			{
				return TypeUsage.CreateDecimalTypeUsage(value, precision, scale);
			}
			return TypeUsage.CreateDecimalTypeUsage(value);
		}
		case "datetime":
			return TypeUsage.CreateDateTimeTypeUsage(value, (byte?)null);
		default:
			throw new NotSupportedException($"SQLite does not support the type '{text}'.");
		}
		if ((int)val != 0)
		{
			if ((int)val == 12)
			{
				if (!flag3)
				{
					return TypeUsage.CreateStringTypeUsage(value, flag, flag2, maxLength);
				}
				return TypeUsage.CreateStringTypeUsage(value, flag, flag2);
			}
			throw new NotSupportedException($"SQLite does not support the type '{text}'.");
		}
		if (!flag3)
		{
			return TypeUsage.CreateBinaryTypeUsage(value, flag2, maxLength);
		}
		return TypeUsage.CreateBinaryTypeUsage(value, flag2);
	}

	public override TypeUsage GetStoreType(TypeUsage edmType)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected I4, but got Unknown
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		if (edmType == null)
		{
			throw new ArgumentNullException("edmType");
		}
		EdmType edmType2 = edmType.EdmType;
		PrimitiveType val = (PrimitiveType)(object)((edmType2 is PrimitiveType) ? edmType2 : null);
		if (val == null)
		{
			throw new ArgumentException($"SQLite does not support the type '{edmType}'.");
		}
		ReadOnlyMetadataCollection<Facet> facets = edmType.Facets;
		PrimitiveTypeKind primitiveTypeKind = val.PrimitiveTypeKind;
		switch ((int)primitiveTypeKind)
		{
		case 1:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["bit"]);
		case 2:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["tinyint"]);
		case 9:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["smallint"]);
		case 10:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["int"]);
		case 11:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["integer"]);
		case 6:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["uniqueidentifier"]);
		case 5:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["float"]);
		case 7:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["real"]);
		case 4:
		{
			if (!TypeHelpers.TryGetPrecision(edmType, out var precision))
			{
				precision = 18;
			}
			if (!TypeHelpers.TryGetScale(edmType, out var scale))
			{
				scale = 0;
			}
			return TypeUsage.CreateDecimalTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
		}
		case 0:
		{
			bool num2 = facets["FixedLength"].Value != null && (bool)facets["FixedLength"].Value;
			Facet val3 = facets["MaxLength"];
			bool flag4 = val3.IsUnbounded || val3.Value == null || (int)val3.Value > int.MaxValue;
			int num3 = ((!flag4) ? ((int)val3.Value) : int.MinValue);
			if (num2)
			{
				return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["blob"], true, num3);
			}
			if (flag4)
			{
				return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["blob"], false);
			}
			return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["blob"], false, num3);
		}
		case 12:
		{
			bool flag = facets["Unicode"].Value == null || (bool)facets["Unicode"].Value;
			bool flag2 = facets["FixedLength"].Value != null && (bool)facets["FixedLength"].Value;
			Facet val2 = facets["MaxLength"];
			bool flag3 = val2.IsUnbounded || val2.Value == null || (int)val2.Value > (flag ? int.MaxValue : int.MaxValue);
			int num = ((!flag3) ? ((int)val2.Value) : int.MinValue);
			if (flag)
			{
				if (flag2)
				{
					return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nchar"], true, true, num);
				}
				if (flag3)
				{
					return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nvarchar"], true, false);
				}
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, num);
			}
			if (flag2)
			{
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["char"], false, true, num);
			}
			if (flag3)
			{
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varchar"], false, false);
			}
			return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varchar"], false, false, num);
		}
		case 3:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["datetime"]);
		default:
			throw new NotSupportedException($"There is no store type corresponding to the EDM type '{edmType}' of primitive type '{val.PrimitiveTypeKind}'.");
		}
	}

	private XmlReader GetStoreSchemaMapping()
	{
		return GetXmlResource("System.Data.SQLite.SQLiteProviderServices.StoreSchemaMapping.msl");
	}

	private XmlReader GetStoreSchemaDescription()
	{
		return GetXmlResource("System.Data.SQLite.SQLiteProviderServices.StoreSchemaDefinition.ssdl");
	}

	internal static XmlReader GetXmlResource(string resourceName)
	{
		return XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
	}
}
