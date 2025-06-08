using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm.Provider;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class CacheForPrimitiveTypes
{
	private readonly List<PrimitiveType>[] _primitiveTypeMap = new List<PrimitiveType>[32];

	internal void Add(PrimitiveType type)
	{
		List<PrimitiveType> list = EntityUtil.CheckArgumentOutOfRange(_primitiveTypeMap, (int)type.PrimitiveTypeKind, "primitiveTypeKind");
		if (list == null)
		{
			list = new List<PrimitiveType>();
			list.Add(type);
			_primitiveTypeMap[(int)type.PrimitiveTypeKind] = list;
		}
		else
		{
			list.Add(type);
		}
	}

	internal bool TryGetType(PrimitiveTypeKind primitiveTypeKind, IEnumerable<Facet> facets, out PrimitiveType type)
	{
		type = null;
		List<PrimitiveType> list = EntityUtil.CheckArgumentOutOfRange(_primitiveTypeMap, (int)primitiveTypeKind, "primitiveTypeKind");
		if (list != null && 0 < list.Count)
		{
			if (list.Count == 1)
			{
				type = list[0];
				return true;
			}
			if (facets == null)
			{
				FacetDescription[] initialFacetDescriptions = EdmProviderManifest.GetInitialFacetDescriptions(primitiveTypeKind);
				if (initialFacetDescriptions == null)
				{
					type = list[0];
					return true;
				}
				facets = CreateInitialFacets(initialFacetDescriptions);
			}
			bool flag = false;
			foreach (Facet facet in facets)
			{
				if ((primitiveTypeKind == PrimitiveTypeKind.String || primitiveTypeKind == PrimitiveTypeKind.Binary) && facet.Value != null && facet.Name == "MaxLength" && Helper.IsUnboundedFacetValue(facet))
				{
					flag = true;
				}
			}
			int num = 0;
			foreach (PrimitiveType item in list)
			{
				if (flag)
				{
					if (type == null)
					{
						type = item;
						num = Helper.GetFacet(item.FacetDescriptions, "MaxLength").MaxValue.Value;
						continue;
					}
					int value = Helper.GetFacet(item.FacetDescriptions, "MaxLength").MaxValue.Value;
					if (value > num)
					{
						type = item;
						num = value;
					}
					continue;
				}
				type = item;
				break;
			}
			return true;
		}
		return false;
	}

	private static Facet[] CreateInitialFacets(FacetDescription[] facetDescriptions)
	{
		Facet[] array = new Facet[facetDescriptions.Length];
		for (int i = 0; i < facetDescriptions.Length; i++)
		{
			switch (facetDescriptions[i].FacetName)
			{
			case "MaxLength":
				array[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultMaxLengthFacetValue);
				break;
			case "Unicode":
				array[i] = Facet.Create(facetDescriptions[i], true);
				break;
			case "FixedLength":
				array[i] = Facet.Create(facetDescriptions[i], false);
				break;
			case "Precision":
				array[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultPrecisionFacetValue);
				break;
			case "Scale":
				array[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultScaleFacetValue);
				break;
			}
		}
		return array;
	}

	internal ReadOnlyCollection<PrimitiveType> GetTypes()
	{
		List<PrimitiveType> list = new List<PrimitiveType>();
		List<PrimitiveType>[] primitiveTypeMap = _primitiveTypeMap;
		foreach (List<PrimitiveType> list2 in primitiveTypeMap)
		{
			if (list2 != null)
			{
				list.AddRange(list2);
			}
		}
		return new ReadOnlyCollection<PrimitiveType>(list);
	}
}
