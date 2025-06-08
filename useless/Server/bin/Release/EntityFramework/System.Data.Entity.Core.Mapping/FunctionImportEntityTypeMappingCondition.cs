namespace System.Data.Entity.Core.Mapping;

public abstract class FunctionImportEntityTypeMappingCondition : MappingItem
{
	private readonly string _columnName;

	internal readonly LineInfo LineInfo;

	public string ColumnName => _columnName;

	internal abstract ValueCondition ConditionValue { get; }

	internal FunctionImportEntityTypeMappingCondition(string columnName, LineInfo lineInfo)
	{
		_columnName = columnName;
		LineInfo = lineInfo;
	}

	internal abstract bool ColumnValueMatchesCondition(object columnValue);

	public override string ToString()
	{
		return ConditionValue.ToString();
	}
}
