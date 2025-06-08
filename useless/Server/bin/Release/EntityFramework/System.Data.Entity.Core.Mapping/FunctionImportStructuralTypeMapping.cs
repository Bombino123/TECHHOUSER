using System.Collections.ObjectModel;

namespace System.Data.Entity.Core.Mapping;

public abstract class FunctionImportStructuralTypeMapping : MappingItem
{
	internal readonly LineInfo LineInfo;

	internal readonly Collection<FunctionImportReturnTypePropertyMapping> ColumnsRenameList;

	public ReadOnlyCollection<FunctionImportReturnTypePropertyMapping> PropertyMappings => new ReadOnlyCollection<FunctionImportReturnTypePropertyMapping>(ColumnsRenameList);

	internal FunctionImportStructuralTypeMapping(Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
	{
		ColumnsRenameList = columnsRenameList;
		LineInfo = lineInfo;
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(ColumnsRenameList);
		base.SetReadOnly();
	}
}
