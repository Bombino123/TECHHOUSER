using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal sealed class FunctionImportReturnTypeEntityTypeColumnsRenameBuilder
{
	internal Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ColumnRenameMapping;

	internal FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> isOfTypeEntityTypeColumnsRenameMapping, Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> entityTypeColumnsRenameMapping)
	{
		ColumnRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();
		foreach (EntityType key in isOfTypeEntityTypeColumnsRenameMapping.Keys)
		{
			SetStructuralTypeColumnsRename(key, isOfTypeEntityTypeColumnsRenameMapping[key], isTypeOf: true);
		}
		foreach (EntityType key2 in entityTypeColumnsRenameMapping.Keys)
		{
			SetStructuralTypeColumnsRename(key2, entityTypeColumnsRenameMapping[key2], isTypeOf: false);
		}
	}

	private void SetStructuralTypeColumnsRename(EntityType entityType, Collection<FunctionImportReturnTypePropertyMapping> columnsRenameMapping, bool isTypeOf)
	{
		foreach (FunctionImportReturnTypePropertyMapping item in columnsRenameMapping)
		{
			if (!ColumnRenameMapping.Keys.Contains(item.CMember))
			{
				ColumnRenameMapping[item.CMember] = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(item.CMember);
			}
			ColumnRenameMapping[item.CMember].AddRename(new FunctionImportReturnTypeStructuralTypeColumn(item.SColumn, entityType, isTypeOf, item.LineInfo));
		}
	}
}
