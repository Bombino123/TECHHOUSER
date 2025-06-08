using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.Mapping;

internal class FunctionImportReturnTypeStructuralTypeColumnRenameMapping
{
	private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForType;

	private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForIsTypeOfType;

	private readonly string _defaultMemberName;

	private readonly Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn> _renameCache;

	internal FunctionImportReturnTypeStructuralTypeColumnRenameMapping(string defaultMemberName)
	{
		_defaultMemberName = defaultMemberName;
		_columnListForType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
		_columnListForIsTypeOfType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
		_renameCache = new Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn>(GetRename, EqualityComparer<StructuralType>.Default);
	}

	internal string GetRename(EdmType type)
	{
		IXmlLineInfo lineInfo;
		return GetRename(type, out lineInfo);
	}

	internal string GetRename(EdmType type, out IXmlLineInfo lineInfo)
	{
		FunctionImportReturnTypeStructuralTypeColumn functionImportReturnTypeStructuralTypeColumn = _renameCache.Evaluate(type as StructuralType);
		lineInfo = functionImportReturnTypeStructuralTypeColumn.LineInfo;
		return functionImportReturnTypeStructuralTypeColumn.ColumnName;
	}

	private FunctionImportReturnTypeStructuralTypeColumn GetRename(StructuralType typeForRename)
	{
		FunctionImportReturnTypeStructuralTypeColumn functionImportReturnTypeStructuralTypeColumn = _columnListForType.FirstOrDefault((FunctionImportReturnTypeStructuralTypeColumn t) => t.Type == typeForRename);
		if (functionImportReturnTypeStructuralTypeColumn != null)
		{
			return functionImportReturnTypeStructuralTypeColumn;
		}
		FunctionImportReturnTypeStructuralTypeColumn functionImportReturnTypeStructuralTypeColumn2 = _columnListForIsTypeOfType.Where((FunctionImportReturnTypeStructuralTypeColumn t) => t.Type == typeForRename).LastOrDefault();
		if (functionImportReturnTypeStructuralTypeColumn2 != null)
		{
			return functionImportReturnTypeStructuralTypeColumn2;
		}
		IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> enumerable = _columnListForIsTypeOfType.Where((FunctionImportReturnTypeStructuralTypeColumn t) => t.Type.IsAssignableFrom(typeForRename));
		if (enumerable.Count() == 0)
		{
			return new FunctionImportReturnTypeStructuralTypeColumn(_defaultMemberName, typeForRename, isTypeOf: false, null);
		}
		return GetLowestParentInHierarchy(enumerable);
	}

	private static FunctionImportReturnTypeStructuralTypeColumn GetLowestParentInHierarchy(IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> nodesInHierarchy)
	{
		FunctionImportReturnTypeStructuralTypeColumn functionImportReturnTypeStructuralTypeColumn = null;
		foreach (FunctionImportReturnTypeStructuralTypeColumn item in nodesInHierarchy)
		{
			if (functionImportReturnTypeStructuralTypeColumn == null)
			{
				functionImportReturnTypeStructuralTypeColumn = item;
			}
			else if (functionImportReturnTypeStructuralTypeColumn.Type.IsAssignableFrom(item.Type))
			{
				functionImportReturnTypeStructuralTypeColumn = item;
			}
		}
		return functionImportReturnTypeStructuralTypeColumn;
	}

	internal void AddRename(FunctionImportReturnTypeStructuralTypeColumn renamedColumn)
	{
		if (!renamedColumn.IsTypeOf)
		{
			_columnListForType.Add(renamedColumn);
		}
		else
		{
			_columnListForIsTypeOfType.Add(renamedColumn);
		}
	}
}
