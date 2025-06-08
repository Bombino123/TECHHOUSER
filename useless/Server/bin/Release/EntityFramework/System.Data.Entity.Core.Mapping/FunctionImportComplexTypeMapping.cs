using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportComplexTypeMapping : FunctionImportStructuralTypeMapping
{
	private readonly ComplexType _returnType;

	public ComplexType ReturnType => _returnType;

	public FunctionImportComplexTypeMapping(ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> properties)
		: this(Check.NotNull(returnType, "returnType"), Check.NotNull(properties, "properties"), LineInfo.Empty)
	{
	}

	internal FunctionImportComplexTypeMapping(ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> properties, LineInfo lineInfo)
		: base(properties, lineInfo)
	{
		_returnType = returnType;
	}
}
