using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

public abstract class FunctionImportMapping : MappingItem
{
	private readonly EdmFunction _functionImport;

	private readonly EdmFunction _targetFunction;

	public EdmFunction FunctionImport => _functionImport;

	public EdmFunction TargetFunction => _targetFunction;

	internal FunctionImportMapping(EdmFunction functionImport, EdmFunction targetFunction)
	{
		_functionImport = functionImport;
		_targetFunction = targetFunction;
	}
}
