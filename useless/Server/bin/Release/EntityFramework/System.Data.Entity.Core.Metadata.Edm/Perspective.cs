using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class Perspective
{
	private readonly MetadataWorkspace _metadataWorkspace;

	private readonly DataSpace _targetDataspace;

	internal MetadataWorkspace MetadataWorkspace => _metadataWorkspace;

	internal DataSpace TargetDataspace => _targetDataspace;

	internal Perspective(MetadataWorkspace metadataWorkspace, DataSpace targetDataspace)
	{
		_metadataWorkspace = metadataWorkspace;
		_targetDataspace = targetDataspace;
	}

	internal virtual bool TryGetMember(StructuralType type, string memberName, bool ignoreCase, out EdmMember outMember)
	{
		Check.NotEmpty(memberName, "memberName");
		outMember = null;
		return type.Members.TryGetValue(memberName, ignoreCase, out outMember);
	}

	internal virtual bool TryGetEnumMember(EnumType type, string memberName, bool ignoreCase, out EnumMember outMember)
	{
		Check.NotEmpty(memberName, "memberName");
		outMember = null;
		return type.Members.TryGetValue(memberName, ignoreCase, out outMember);
	}

	internal virtual bool TryGetExtent(EntityContainer entityContainer, string extentName, bool ignoreCase, out EntitySetBase outSet)
	{
		return entityContainer.BaseEntitySets.TryGetValue(extentName, ignoreCase, out outSet);
	}

	internal virtual bool TryGetFunctionImport(EntityContainer entityContainer, string functionImportName, bool ignoreCase, out EdmFunction functionImport)
	{
		functionImport = null;
		if (ignoreCase)
		{
			functionImport = entityContainer.FunctionImports.Where((EdmFunction fi) => string.Equals(fi.Name, functionImportName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
		}
		else
		{
			functionImport = entityContainer.FunctionImports.Where((EdmFunction fi) => fi.Name == functionImportName).SingleOrDefault();
		}
		return functionImport != null;
	}

	internal virtual EntityContainer GetDefaultContainer()
	{
		return null;
	}

	internal virtual bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
	{
		return MetadataWorkspace.TryGetEntityContainer(name, ignoreCase, TargetDataspace, out entityContainer);
	}

	internal abstract bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage);

	internal bool TryGetFunctionByName(string namespaceName, string functionName, bool ignoreCase, out IList<EdmFunction> functionOverloads)
	{
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotEmpty(functionName, "functionName");
		string functionName2 = namespaceName + "." + functionName;
		ItemCollection itemCollection = _metadataWorkspace.GetItemCollection(_targetDataspace);
		IList<EdmFunction> list = ((_targetDataspace == DataSpace.SSpace) ? ((StoreItemCollection)itemCollection).GetCTypeFunctions(functionName2, ignoreCase) : itemCollection.GetFunctions(functionName2, ignoreCase));
		if (_targetDataspace == DataSpace.CSpace)
		{
			if ((list == null || list.Count == 0) && TryGetEntityContainer(namespaceName, ignoreCase: false, out var entityContainer) && TryGetFunctionImport(entityContainer, functionName, ignoreCase: false, out var functionImport))
			{
				list = new EdmFunction[1] { functionImport };
			}
			if ((list == null || list.Count == 0) && _metadataWorkspace.TryGetItemCollection(DataSpace.SSpace, out var collection))
			{
				list = ((StoreItemCollection)collection).GetCTypeFunctions(functionName2, ignoreCase);
			}
		}
		functionOverloads = ((list != null && list.Count > 0) ? list : null);
		return functionOverloads != null;
	}

	internal virtual bool TryGetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, out PrimitiveType primitiveType)
	{
		primitiveType = _metadataWorkspace.GetMappedPrimitiveType(primitiveTypeKind, DataSpace.CSpace);
		return primitiveType != null;
	}
}
