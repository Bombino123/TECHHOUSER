using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Mapping;

public sealed class AssociationSetModificationFunctionMapping : MappingItem
{
	private readonly AssociationSet _associationSet;

	private readonly ModificationFunctionMapping _deleteFunctionMapping;

	private readonly ModificationFunctionMapping _insertFunctionMapping;

	public AssociationSet AssociationSet => _associationSet;

	public ModificationFunctionMapping DeleteFunctionMapping => _deleteFunctionMapping;

	public ModificationFunctionMapping InsertFunctionMapping => _insertFunctionMapping;

	public AssociationSetModificationFunctionMapping(AssociationSet associationSet, ModificationFunctionMapping deleteFunctionMapping, ModificationFunctionMapping insertFunctionMapping)
	{
		Check.NotNull(associationSet, "associationSet");
		_associationSet = associationSet;
		_deleteFunctionMapping = deleteFunctionMapping;
		_insertFunctionMapping = insertFunctionMapping;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "AS{{{0}}}:{3}DFunc={{{1}}},{3}IFunc={{{2}}}", AssociationSet, DeleteFunctionMapping, InsertFunctionMapping, Environment.NewLine + "  ");
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_deleteFunctionMapping);
		MappingItem.SetReadOnly(_insertFunctionMapping);
		base.SetReadOnly();
	}
}
