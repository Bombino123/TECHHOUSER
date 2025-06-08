using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ExpensiveOSpaceLoader
{
	public virtual Dictionary<string, EdmType> LoadTypesExpensiveWay(Assembly assembly)
	{
		KnownAssembliesSet knownAssemblies = new KnownAssembliesSet();
		AssemblyCache.LoadAssembly(assembly, loadReferencedAssemblies: false, knownAssemblies, out var typesInLoading, out var errors);
		if (errors.Count != 0)
		{
			throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(errors));
		}
		return typesInLoading;
	}

	public virtual AssociationType GetRelationshipTypeExpensiveWay(Type entityClrType, string relationshipName)
	{
		Dictionary<string, EdmType> dictionary = LoadTypesExpensiveWay(entityClrType.Assembly());
		if (dictionary != null && dictionary.TryGetValue(relationshipName, out var value) && Helper.IsRelationshipType(value))
		{
			return (AssociationType)value;
		}
		return null;
	}

	public virtual IEnumerable<AssociationType> GetAllRelationshipTypesExpensiveWay(Assembly assembly)
	{
		Dictionary<string, EdmType> dictionary = LoadTypesExpensiveWay(assembly);
		if (dictionary == null)
		{
			yield break;
		}
		foreach (EdmType value in dictionary.Values)
		{
			if (Helper.IsAssociationType(value))
			{
				yield return (AssociationType)value;
			}
		}
	}
}
