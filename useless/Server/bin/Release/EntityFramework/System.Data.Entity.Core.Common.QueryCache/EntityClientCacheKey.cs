using System.Collections.Generic;
using System.Data.Entity.Core.Common.Internal;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Common.QueryCache;

internal sealed class EntityClientCacheKey : QueryCacheKey
{
	private readonly CommandType _commandType;

	private readonly string _eSqlStatement;

	private readonly string _parametersToken;

	private readonly int _parameterCount;

	private readonly int _hashCode;

	internal EntityClientCacheKey(EntityCommand entityCommand)
	{
		_commandType = entityCommand.CommandType;
		_eSqlStatement = entityCommand.CommandText;
		_parametersToken = GetParametersToken(entityCommand);
		_parameterCount = entityCommand.Parameters.Count;
		_hashCode = _commandType.GetHashCode() ^ _eSqlStatement.GetHashCode() ^ _parametersToken.GetHashCode();
	}

	public override bool Equals(object otherObject)
	{
		if (typeof(EntityClientCacheKey) != otherObject.GetType())
		{
			return false;
		}
		EntityClientCacheKey entityClientCacheKey = (EntityClientCacheKey)otherObject;
		if (_commandType == entityClientCacheKey._commandType && _parameterCount == entityClientCacheKey._parameterCount && Equals(entityClientCacheKey._eSqlStatement, _eSqlStatement))
		{
			return Equals(entityClientCacheKey._parametersToken, _parametersToken);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	private static string GetTypeUsageToken(TypeUsage type)
	{
		string text = null;
		if (type == DbTypeMap.AnsiString)
		{
			return "AnsiString";
		}
		if (type == DbTypeMap.AnsiStringFixedLength)
		{
			return "AnsiStringFixedLength";
		}
		if (type == DbTypeMap.String)
		{
			return "String";
		}
		if (type == DbTypeMap.StringFixedLength)
		{
			return "StringFixedLength";
		}
		if (type == DbTypeMap.Xml)
		{
			return "String";
		}
		if (TypeSemantics.IsEnumerationType(type))
		{
			return type.EdmType.FullName;
		}
		return type.EdmType.Name;
	}

	private static string GetParametersToken(EntityCommand entityCommand)
	{
		if (entityCommand.Parameters == null || entityCommand.Parameters.Count == 0)
		{
			return "@@0";
		}
		Dictionary<string, TypeUsage> parameterTypeUsage = entityCommand.GetParameterTypeUsage();
		if (1 == parameterTypeUsage.Count)
		{
			return "@@1:" + entityCommand.Parameters[0].ParameterName + ":" + GetTypeUsageToken(parameterTypeUsage[entityCommand.Parameters[0].ParameterName]);
		}
		StringBuilder stringBuilder = new StringBuilder(entityCommand.Parameters.Count * 20);
		stringBuilder.Append("@@");
		stringBuilder.Append(entityCommand.Parameters.Count);
		stringBuilder.Append(":");
		string value = "";
		foreach (KeyValuePair<string, TypeUsage> item in parameterTypeUsage)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(item.Key);
			stringBuilder.Append(":");
			stringBuilder.Append(GetTypeUsageToken(item.Value));
			value = ";";
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return string.Join("|", Enum.GetName(typeof(CommandType), _commandType), _eSqlStatement, _parametersToken);
	}
}
