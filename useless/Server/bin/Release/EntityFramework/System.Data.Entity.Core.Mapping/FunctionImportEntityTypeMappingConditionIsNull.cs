using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportEntityTypeMappingConditionIsNull : FunctionImportEntityTypeMappingCondition
{
	private readonly bool _isNull;

	public bool IsNull => _isNull;

	internal override ValueCondition ConditionValue
	{
		get
		{
			if (!IsNull)
			{
				return ValueCondition.IsNotNull;
			}
			return ValueCondition.IsNull;
		}
	}

	public FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull)
		: this(Check.NotNull(columnName, "columnName"), isNull, LineInfo.Empty)
	{
	}

	internal FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull, LineInfo lineInfo)
		: base(columnName, lineInfo)
	{
		_isNull = isNull;
	}

	internal override bool ColumnValueMatchesCondition(object columnValue)
	{
		return (columnValue == null || Convert.IsDBNull(columnValue)) == IsNull;
	}
}
