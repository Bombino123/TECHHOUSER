using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public sealed class FunctionImportResultMapping : MappingItem
{
	private readonly List<FunctionImportStructuralTypeMapping> _typeMappings = new List<FunctionImportStructuralTypeMapping>();

	public ReadOnlyCollection<FunctionImportStructuralTypeMapping> TypeMappings => new ReadOnlyCollection<FunctionImportStructuralTypeMapping>(_typeMappings);

	internal List<FunctionImportStructuralTypeMapping> SourceList => _typeMappings;

	public void AddTypeMapping(FunctionImportStructuralTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_typeMappings.Add(typeMapping);
	}

	public void RemoveTypeMapping(FunctionImportStructuralTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_typeMappings.Remove(typeMapping);
	}

	internal override void SetReadOnly()
	{
		_typeMappings.TrimExcess();
		MappingItem.SetReadOnly(_typeMappings);
		base.SetReadOnly();
	}
}
