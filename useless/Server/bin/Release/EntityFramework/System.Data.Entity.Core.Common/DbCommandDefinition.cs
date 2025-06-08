using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common;

public class DbCommandDefinition
{
	private readonly DbCommand _prototype;

	private readonly Func<DbCommand, DbCommand> _cloneMethod;

	protected internal DbCommandDefinition(DbCommand prototype, Func<DbCommand, DbCommand> cloneMethod)
	{
		Check.NotNull(prototype, "prototype");
		Check.NotNull(cloneMethod, "cloneMethod");
		_prototype = prototype;
		_cloneMethod = cloneMethod;
	}

	protected DbCommandDefinition()
	{
	}

	public virtual DbCommand CreateCommand()
	{
		return _cloneMethod(_prototype);
	}

	internal static void PopulateParameterFromTypeUsage(DbParameter parameter, TypeUsage type, bool isOutParam)
	{
		parameter.IsNullable = TypeSemantics.IsNullable(type);
		if (Helper.IsPrimitiveType(type.EdmType) && TryGetDbTypeFromPrimitiveType((PrimitiveType)type.EdmType, out var dbType))
		{
			switch (dbType)
			{
			case DbType.Binary:
				PopulateBinaryParameter(parameter, type, dbType, isOutParam);
				break;
			case DbType.DateTime:
			case DbType.Time:
			case DbType.DateTimeOffset:
				PopulateDateTimeParameter(parameter, type, dbType);
				break;
			case DbType.Decimal:
				PopulateDecimalParameter(parameter, type, dbType);
				break;
			case DbType.String:
				PopulateStringParameter(parameter, type, isOutParam);
				break;
			default:
				parameter.DbType = dbType;
				break;
			}
		}
	}

	internal static bool TryGetDbTypeFromPrimitiveType(PrimitiveType type, out DbType dbType)
	{
		switch (type.PrimitiveTypeKind)
		{
		case PrimitiveTypeKind.Binary:
			dbType = DbType.Binary;
			return true;
		case PrimitiveTypeKind.Boolean:
			dbType = DbType.Boolean;
			return true;
		case PrimitiveTypeKind.Byte:
			dbType = DbType.Byte;
			return true;
		case PrimitiveTypeKind.DateTime:
			dbType = DbType.DateTime;
			return true;
		case PrimitiveTypeKind.Time:
			dbType = DbType.Time;
			return true;
		case PrimitiveTypeKind.DateTimeOffset:
			dbType = DbType.DateTimeOffset;
			return true;
		case PrimitiveTypeKind.Decimal:
			dbType = DbType.Decimal;
			return true;
		case PrimitiveTypeKind.Double:
			dbType = DbType.Double;
			return true;
		case PrimitiveTypeKind.Guid:
			dbType = DbType.Guid;
			return true;
		case PrimitiveTypeKind.Single:
			dbType = DbType.Single;
			return true;
		case PrimitiveTypeKind.SByte:
			dbType = DbType.SByte;
			return true;
		case PrimitiveTypeKind.Int16:
			dbType = DbType.Int16;
			return true;
		case PrimitiveTypeKind.Int32:
			dbType = DbType.Int32;
			return true;
		case PrimitiveTypeKind.Int64:
			dbType = DbType.Int64;
			return true;
		case PrimitiveTypeKind.String:
			dbType = DbType.String;
			return true;
		default:
			dbType = DbType.AnsiString;
			return false;
		}
	}

	private static void PopulateBinaryParameter(DbParameter parameter, TypeUsage type, DbType dbType, bool isOutParam)
	{
		parameter.DbType = dbType;
		SetParameterSize(parameter, type, isOutParam);
	}

	private static void PopulateDecimalParameter(DbParameter parameter, TypeUsage type, DbType dbType)
	{
		parameter.DbType = dbType;
		if (TypeHelpers.TryGetPrecision(type, out var precision))
		{
			((IDbDataParameter)parameter).Precision = precision;
		}
		if (TypeHelpers.TryGetScale(type, out var scale))
		{
			((IDbDataParameter)parameter).Scale = scale;
		}
	}

	private static void PopulateDateTimeParameter(DbParameter parameter, TypeUsage type, DbType dbType)
	{
		parameter.DbType = dbType;
		if (TypeHelpers.TryGetPrecision(type, out var precision))
		{
			((IDbDataParameter)parameter).Precision = precision;
		}
	}

	private static void PopulateStringParameter(DbParameter parameter, TypeUsage type, bool isOutParam)
	{
		bool isUnicode = true;
		bool isFixedLength = false;
		if (!TypeHelpers.TryGetIsFixedLength(type, out isFixedLength))
		{
			isFixedLength = false;
		}
		if (!TypeHelpers.TryGetIsUnicode(type, out isUnicode))
		{
			isUnicode = true;
		}
		if (isFixedLength)
		{
			parameter.DbType = (isUnicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength);
		}
		else
		{
			parameter.DbType = (isUnicode ? DbType.String : DbType.AnsiString);
		}
		SetParameterSize(parameter, type, isOutParam);
	}

	private static void SetParameterSize(DbParameter parameter, TypeUsage type, bool isOutParam)
	{
		if (type.Facets.TryGetValue("MaxLength", ignoreCase: true, out var item) && item.Value != null)
		{
			if (!Helper.IsUnboundedFacetValue(item))
			{
				parameter.Size = (int)item.Value;
			}
			else if (isOutParam)
			{
				parameter.Size = int.MaxValue;
			}
		}
	}
}
