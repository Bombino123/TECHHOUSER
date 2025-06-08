namespace System.Data.Entity.Core.Mapping;

public abstract class FunctionImportReturnTypePropertyMapping : MappingItem
{
	internal readonly LineInfo LineInfo;

	internal abstract string CMember { get; }

	internal abstract string SColumn { get; }

	internal FunctionImportReturnTypePropertyMapping(LineInfo lineInfo)
	{
		LineInfo = lineInfo;
	}
}
