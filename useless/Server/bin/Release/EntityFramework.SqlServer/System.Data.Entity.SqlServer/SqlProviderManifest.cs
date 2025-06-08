using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.Entity.SqlServer;

internal class SqlProviderManifest : DbXmlEnabledProviderManifest
{
	internal const string TokenSql8 = "2000";

	internal const string TokenSql9 = "2005";

	internal const string TokenSql10 = "2008";

	internal const string TokenSql11 = "2012";

	internal const string TokenAzure11 = "2012.Azure";

	internal const char LikeEscapeChar = '~';

	internal const string LikeEscapeCharToString = "~";

	private readonly SqlVersion _version = SqlVersion.Sql9;

	private const int varcharMaxSize = 8000;

	private const int nvarcharMaxSize = 4000;

	private const int binaryMaxSize = 8000;

	private ReadOnlyCollection<PrimitiveType> _primitiveTypes;

	private ReadOnlyCollection<EdmFunction> _functions;

	internal SqlVersion SqlVersion => _version;

	public SqlProviderManifest(string manifestToken)
		: base(GetProviderManifest())
	{
		_version = SqlVersionUtils.GetSqlVersion(manifestToken);
		Initialize();
	}

	private void Initialize()
	{
		if (_version == SqlVersion.Sql10 || _version == SqlVersion.Sql11)
		{
			_primitiveTypes = ((DbXmlEnabledProviderManifest)this).GetStoreTypes();
			_functions = ((DbXmlEnabledProviderManifest)this).GetStoreFunctions();
			return;
		}
		List<PrimitiveType> list = new List<PrimitiveType>(((DbXmlEnabledProviderManifest)this).GetStoreTypes());
		list.RemoveAll((PrimitiveType primitiveType) => ((EdmType)primitiveType).Name.Equals("time", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("date", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("datetime2", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("datetimeoffset", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("hierarchyid", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("geography", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.Equals("geometry", StringComparison.OrdinalIgnoreCase));
		if (_version == SqlVersion.Sql8)
		{
			list.RemoveAll((PrimitiveType primitiveType) => ((EdmType)primitiveType).Name.Equals("xml", StringComparison.OrdinalIgnoreCase) || ((EdmType)primitiveType).Name.EndsWith("(max)", StringComparison.OrdinalIgnoreCase));
		}
		_primitiveTypes = new ReadOnlyCollection<PrimitiveType>(list);
		IEnumerable<EdmFunction> source = from f in ((DbXmlEnabledProviderManifest)this).GetStoreFunctions()
			where !IsKatmaiOrNewer(f)
			select f;
		if (_version == SqlVersion.Sql8)
		{
			source = source.Where((EdmFunction f) => !IsYukonOrNewer(f));
		}
		_functions = new ReadOnlyCollection<EdmFunction>(source.ToList());
	}

	private static XmlReader GetXmlResource(string resourceName)
	{
		return XmlReader.Create(typeof(SqlProviderManifest).Assembly().GetManifestResourceStream(resourceName));
	}

	internal static XmlReader GetProviderManifest()
	{
		return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices.ProviderManifest.xml");
	}

	internal static XmlReader GetStoreSchemaMapping(string mslName)
	{
		return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + mslName + ".msl");
	}

	internal XmlReader GetStoreSchemaDescription(string ssdlName)
	{
		if (_version == SqlVersion.Sql8)
		{
			return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + "_Sql8.ssdl");
		}
		return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + ".ssdl");
	}

	internal static string EscapeLikeText(string text, bool alwaysEscapeEscapeChar, out bool usedEscapeChar)
	{
		usedEscapeChar = false;
		if (!text.Contains("%") && !text.Contains("_") && !text.Contains("[") && !text.Contains("^") && (!alwaysEscapeEscapeChar || !text.Contains("~")))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		foreach (char c in text)
		{
			if (c == '%' || c == '_' || c == '[' || c == '^' || c == '~')
			{
				stringBuilder.Append('~');
				usedEscapeChar = true;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	protected override XmlReader GetDbInformation(string informationType)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		switch (informationType)
		{
		case "StoreSchemaDefinitionVersion3":
		case "StoreSchemaDefinition":
			return GetStoreSchemaDescription(informationType);
		case "StoreSchemaMappingVersion3":
		case "StoreSchemaMapping":
			return GetStoreSchemaMapping(informationType);
		case "ConceptualSchemaDefinitionVersion3":
		case "ConceptualSchemaDefinition":
			return null;
		default:
			throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForGetDbInformation(informationType));
		}
	}

	public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
	{
		return _primitiveTypes;
	}

	public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
	{
		return _functions;
	}

	private static bool IsKatmaiOrNewer(EdmFunction edmFunction)
	{
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		if ((edmFunction.ReturnParameter != null && edmFunction.ReturnParameter.TypeUsage.IsSpatialType()) || ((IEnumerable<FunctionParameter>)edmFunction.Parameters).Any((FunctionParameter p) => p.TypeUsage.IsSpatialType()))
		{
			return true;
		}
		ReadOnlyMetadataCollection<FunctionParameter> parameters = edmFunction.Parameters;
		switch (((EdmType)edmFunction).Name.ToUpperInvariant())
		{
		case "COUNT":
		case "COUNT_BIG":
		case "MAX":
		case "MIN":
		{
			string name2 = ((CollectionType)((ReadOnlyCollection<FunctionParameter>)(object)parameters)[0].TypeUsage.EdmType).TypeUsage.EdmType.Name;
			if (!name2.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
			{
				return name2.Equals("Time", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		case "DAY":
		case "MONTH":
		case "YEAR":
		case "DATALENGTH":
		case "CHECKSUM":
		{
			string name3 = ((ReadOnlyCollection<FunctionParameter>)(object)parameters)[0].TypeUsage.EdmType.Name;
			if (!name3.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
			{
				return name3.Equals("Time", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		case "DATEADD":
		case "DATEDIFF":
		{
			string name4 = ((ReadOnlyCollection<FunctionParameter>)(object)parameters)[1].TypeUsage.EdmType.Name;
			string name5 = ((ReadOnlyCollection<FunctionParameter>)(object)parameters)[2].TypeUsage.EdmType.Name;
			if (!name4.Equals("Time", StringComparison.OrdinalIgnoreCase) && !name5.Equals("Time", StringComparison.OrdinalIgnoreCase) && !name4.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
			{
				return name5.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		case "DATENAME":
		case "DATEPART":
		{
			string name = ((ReadOnlyCollection<FunctionParameter>)(object)parameters)[1].TypeUsage.EdmType.Name;
			if (!name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
			{
				return name.Equals("Time", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		case "SYSUTCDATETIME":
		case "SYSDATETIME":
		case "SYSDATETIMEOFFSET":
			return true;
		default:
			return false;
		}
	}

	private static bool IsYukonOrNewer(EdmFunction edmFunction)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyMetadataCollection<FunctionParameter> parameters = edmFunction.Parameters;
		if (parameters == null || ((ReadOnlyCollection<FunctionParameter>)(object)parameters).Count == 0)
		{
			return false;
		}
		switch (((EdmType)edmFunction).Name.ToUpperInvariant())
		{
		case "COUNT":
		case "COUNT_BIG":
			return ((CollectionType)((ReadOnlyCollection<FunctionParameter>)(object)parameters)[0].TypeUsage.EdmType).TypeUsage.EdmType.Name.Equals("Guid", StringComparison.OrdinalIgnoreCase);
		case "CHARINDEX":
		{
			Enumerator<FunctionParameter> enumerator = parameters.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.TypeUsage.EdmType.Name.Equals("Int64", StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			break;
		}
		}
		return false;
	}

	public override TypeUsage GetEdmType(TypeUsage storeType)
	{
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0747: Unknown result type (might be due to invalid IL or missing references)
		//IL_074b: Invalid comparison between Unknown and I4
		Check.NotNull<TypeUsage>(storeType, "storeType");
		string text = storeType.EdmType.Name.ToLowerInvariant();
		if (!((DbXmlEnabledProviderManifest)this).StoreTypeNameToEdmPrimitiveType.ContainsKey(text))
		{
			throw new ArgumentException(Strings.ProviderDoesNotSupportType(text));
		}
		PrimitiveType val = ((DbXmlEnabledProviderManifest)this).StoreTypeNameToEdmPrimitiveType[text];
		int maxLength = 0;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = true;
		PrimitiveTypeKind val2;
		switch (text)
		{
		case "tinyint":
		case "smallint":
		case "bigint":
		case "bit":
		case "uniqueidentifier":
		case "hierarchyid":
		case "int":
		case "geography":
		case "geometry":
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)val);
		case "varchar":
			val2 = (PrimitiveTypeKind)12;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag = false;
			flag2 = false;
			break;
		case "char":
			val2 = (PrimitiveTypeKind)12;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag = false;
			flag2 = true;
			break;
		case "nvarchar":
			val2 = (PrimitiveTypeKind)12;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag = true;
			flag2 = false;
			break;
		case "nchar":
			val2 = (PrimitiveTypeKind)12;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag = true;
			flag2 = true;
			break;
		case "varchar(max)":
		case "text":
			val2 = (PrimitiveTypeKind)12;
			flag3 = true;
			flag = false;
			flag2 = false;
			break;
		case "nvarchar(max)":
		case "ntext":
		case "xml":
			val2 = (PrimitiveTypeKind)12;
			flag3 = true;
			flag = true;
			flag2 = false;
			break;
		case "binary":
			val2 = (PrimitiveTypeKind)0;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag2 = true;
			break;
		case "varbinary":
			val2 = (PrimitiveTypeKind)0;
			flag3 = !storeType.TryGetMaxLength(out maxLength);
			flag2 = false;
			break;
		case "varbinary(max)":
		case "image":
			val2 = (PrimitiveTypeKind)0;
			flag3 = true;
			flag2 = false;
			break;
		case "timestamp":
		case "rowversion":
			return TypeUsage.CreateBinaryTypeUsage(val, true, 8);
		case "float":
		case "real":
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)val);
		case "decimal":
		case "numeric":
		{
			if (storeType.TryGetPrecision(out var precision) && storeType.TryGetScale(out var scale))
			{
				return TypeUsage.CreateDecimalTypeUsage(val, precision, scale);
			}
			return TypeUsage.CreateDecimalTypeUsage(val);
		}
		case "money":
			return TypeUsage.CreateDecimalTypeUsage(val, (byte)19, (byte)4);
		case "smallmoney":
			return TypeUsage.CreateDecimalTypeUsage(val, (byte)10, (byte)4);
		case "datetime":
		case "datetime2":
		case "smalldatetime":
			return TypeUsage.CreateDateTimeTypeUsage(val, (byte?)null);
		case "date":
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)val);
		case "time":
			return TypeUsage.CreateTimeTypeUsage(val, (byte?)null);
		case "datetimeoffset":
			return TypeUsage.CreateDateTimeOffsetTypeUsage(val, (byte?)null);
		default:
			throw new NotSupportedException(Strings.ProviderDoesNotSupportType(text));
		}
		if ((int)val2 != 0)
		{
			if ((int)val2 == 12)
			{
				if (!flag3)
				{
					return TypeUsage.CreateStringTypeUsage(val, flag, flag2, maxLength);
				}
				return TypeUsage.CreateStringTypeUsage(val, flag, flag2);
			}
			throw new NotSupportedException(Strings.ProviderDoesNotSupportType(text));
		}
		if (!flag3)
		{
			return TypeUsage.CreateBinaryTypeUsage(val, flag2, maxLength);
		}
		return TypeUsage.CreateBinaryTypeUsage(val, flag2);
	}

	public override TypeUsage GetStoreType(TypeUsage edmType)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected I4, but got Unknown
		//IL_054b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<TypeUsage>(edmType, "edmType");
		EdmType edmType2 = edmType.EdmType;
		PrimitiveType val = (PrimitiveType)(object)((edmType2 is PrimitiveType) ? edmType2 : null);
		if (val == null)
		{
			throw new ArgumentException(Strings.ProviderDoesNotSupportType(edmType.EdmType.Name));
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
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["bigint"]);
		case 16:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
			return GetStorePrimitiveTypeIfPostSql9("geography", edmType.EdmType.Name, val.PrimitiveTypeKind);
		case 15:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
			return GetStorePrimitiveTypeIfPostSql9("geometry", edmType.EdmType.Name, val.PrimitiveTypeKind);
		case 6:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["uniqueidentifier"]);
		case 31:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["hierarchyid"]);
		case 5:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["float"]);
		case 7:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["real"]);
		case 4:
		{
			if (!edmType.TryGetPrecision(out var precision))
			{
				precision = 18;
			}
			if (!edmType.TryGetScale(out var scale))
			{
				scale = 0;
			}
			return TypeUsage.CreateDecimalTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
		}
		case 0:
		{
			bool num2 = facets["FixedLength"].Value != null && (bool)facets["FixedLength"].Value;
			Facet val3 = facets["MaxLength"];
			bool flag4 = val3.IsUnbounded || val3.Value == null || (int)val3.Value > 8000;
			int num3 = ((!flag4) ? ((int)val3.Value) : int.MinValue);
			if (num2)
			{
				return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["binary"], true, flag4 ? 8000 : num3);
			}
			if (flag4)
			{
				if (_version != SqlVersion.Sql8)
				{
					return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varbinary(max)"], false);
				}
				return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varbinary"], false, 8000);
			}
			return TypeUsage.CreateBinaryTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varbinary"], false, num3);
		}
		case 12:
		{
			bool flag = facets["Unicode"].Value == null || (bool)facets["Unicode"].Value;
			bool flag2 = facets["FixedLength"].Value != null && (bool)facets["FixedLength"].Value;
			Facet val2 = facets["MaxLength"];
			bool flag3 = val2.IsUnbounded || val2.Value == null || (int)val2.Value > (flag ? 4000 : 8000);
			int num = ((!flag3) ? ((int)val2.Value) : int.MinValue);
			if (flag)
			{
				if (flag2)
				{
					return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nchar"], true, true, flag3 ? 4000 : num);
				}
				if (flag3)
				{
					if (_version != SqlVersion.Sql8)
					{
						return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nvarchar(max)"], true, false);
					}
					return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, 4000);
				}
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, num);
			}
			if (flag2)
			{
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["char"], false, true, flag3 ? 8000 : num);
			}
			if (flag3)
			{
				if (_version != SqlVersion.Sql8)
				{
					return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varchar(max)"], false, false);
				}
				return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varchar"], false, false, 8000);
			}
			return TypeUsage.CreateStringTypeUsage(((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["varchar"], false, false, num);
		}
		case 3:
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType["datetime"]);
		case 14:
			return GetStorePrimitiveTypeIfPostSql9("datetimeoffset", edmType.EdmType.Name, val.PrimitiveTypeKind);
		case 13:
			return GetStorePrimitiveTypeIfPostSql9("time", edmType.EdmType.Name, val.PrimitiveTypeKind);
		default:
			throw new NotSupportedException(Strings.NoStoreTypeForEdmType(edmType.EdmType.Name, val.PrimitiveTypeKind));
		}
	}

	private TypeUsage GetStorePrimitiveTypeIfPostSql9(string storeTypeName, string nameForException, PrimitiveTypeKind primitiveTypeKind)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (SqlVersion != SqlVersion.Sql8 && SqlVersion != SqlVersion.Sql9)
		{
			return TypeUsage.CreateDefaultTypeUsage((EdmType)(object)((DbXmlEnabledProviderManifest)this).StoreTypeNameToStorePrimitiveType[storeTypeName]);
		}
		throw new NotSupportedException(Strings.NoStoreTypeForEdmType(nameForException, primitiveTypeKind));
	}

	public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
	{
		escapeCharacter = '~';
		return true;
	}

	public override string EscapeLikeArgument(string argument)
	{
		Check.NotNull(argument, "argument");
		bool usedEscapeChar;
		return EscapeLikeText(argument, alwaysEscapeEscapeChar: true, out usedEscapeChar);
	}

	public override bool SupportsParameterOptimizationInSchemaQueries()
	{
		return true;
	}

	public override bool SupportsInExpression()
	{
		return true;
	}

	public override bool SupportsIntersectAndUnionAllFlattening()
	{
		return true;
	}
}
