using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportReturnTypeScalarPropertyMapping : FunctionImportReturnTypePropertyMapping
{
	private readonly string _propertyName;

	private readonly string _columnName;

	public string PropertyName => _propertyName;

	internal override string CMember => PropertyName;

	public string ColumnName => _columnName;

	internal override string SColumn => ColumnName;

	public FunctionImportReturnTypeScalarPropertyMapping(string propertyName, string columnName)
		: this(Check.NotNull(propertyName, "propertyName"), Check.NotNull(columnName, "columnName"), LineInfo.Empty)
	{
	}

	internal FunctionImportReturnTypeScalarPropertyMapping(string propertyName, string columnName, LineInfo lineInfo)
		: base(lineInfo)
	{
		_propertyName = propertyName;
		_columnName = columnName;
	}
}
