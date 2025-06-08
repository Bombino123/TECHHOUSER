using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal class MetadataMappingHasherVisitor : BaseMetadataMappingVisitor
{
	private CompressingHashBuilder m_hashSourceBuilder;

	private Dictionary<object, int> m_itemsAlreadySeen = new Dictionary<object, int>();

	private int m_instanceNumber;

	private EdmItemCollection m_EdmItemCollection;

	private double m_MappingVersion;

	internal string HashValue => m_hashSourceBuilder.ComputeHash();

	private MetadataMappingHasherVisitor(double mappingVersion, bool sortSequence)
		: base(sortSequence)
	{
		m_MappingVersion = mappingVersion;
		m_hashSourceBuilder = new CompressingHashBuilder(MetadataHelper.CreateMetadataHashAlgorithm(m_MappingVersion));
	}

	protected override void Visit(EntityContainerMapping entityContainerMapping)
	{
		m_MappingVersion = entityContainerMapping.StorageMappingItemCollection.MappingVersion;
		m_EdmItemCollection = entityContainerMapping.StorageMappingItemCollection.EdmItemCollection;
		if (AddObjectToSeenListAndHashBuilder(entityContainerMapping, out var instanceIndex))
		{
			if (m_itemsAlreadySeen.Count > 1)
			{
				Clean();
				Visit(entityContainerMapping);
				return;
			}
			AddObjectStartDumpToHashBuilder(entityContainerMapping, instanceIndex);
			AddObjectContentToHashBuilder(entityContainerMapping.Identity);
			AddV2ObjectContentToHashBuilder(entityContainerMapping.GenerateUpdateViews, m_MappingVersion);
			base.Visit(entityContainerMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EntityContainer entityContainer)
	{
		if (AddObjectToSeenListAndHashBuilder(entityContainer, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(entityContainer, instanceIndex);
			AddObjectContentToHashBuilder(entityContainer.Identity);
			base.Visit(entityContainer);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EntitySetBaseMapping setMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(setMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(setMapping, instanceIndex);
			base.Visit(setMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(TypeMapping typeMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(typeMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(typeMapping, instanceIndex);
			base.Visit(typeMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(MappingFragment mappingFragment)
	{
		if (AddObjectToSeenListAndHashBuilder(mappingFragment, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(mappingFragment, instanceIndex);
			AddV2ObjectContentToHashBuilder(mappingFragment.IsSQueryDistinct, m_MappingVersion);
			base.Visit(mappingFragment);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(PropertyMapping propertyMapping)
	{
		base.Visit(propertyMapping);
	}

	protected override void Visit(ComplexPropertyMapping complexPropertyMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(complexPropertyMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(complexPropertyMapping, instanceIndex);
			base.Visit(complexPropertyMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(ComplexTypeMapping complexTypeMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(complexTypeMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(complexTypeMapping, instanceIndex);
			base.Visit(complexTypeMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(ConditionPropertyMapping conditionPropertyMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(conditionPropertyMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(conditionPropertyMapping, instanceIndex);
			AddObjectContentToHashBuilder(conditionPropertyMapping.IsNull);
			AddObjectContentToHashBuilder(conditionPropertyMapping.Value);
			base.Visit(conditionPropertyMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(ScalarPropertyMapping scalarPropertyMapping)
	{
		if (AddObjectToSeenListAndHashBuilder(scalarPropertyMapping, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(scalarPropertyMapping, instanceIndex);
			base.Visit(scalarPropertyMapping);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EntitySetBase entitySetBase)
	{
		base.Visit(entitySetBase);
	}

	protected override void Visit(EntitySet entitySet)
	{
		if (!AddObjectToSeenListAndHashBuilder(entitySet, out var instanceIndex))
		{
			return;
		}
		AddObjectStartDumpToHashBuilder(entitySet, instanceIndex);
		AddObjectContentToHashBuilder(entitySet.Name);
		AddObjectContentToHashBuilder(entitySet.Schema);
		AddObjectContentToHashBuilder(entitySet.Table);
		base.Visit(entitySet);
		IEnumerable<EdmType> sequence = from type in MetadataHelper.GetTypeAndSubtypesOf(entitySet.ElementType, m_EdmItemCollection, includeAbstractTypes: false)
			where type != entitySet.ElementType
			select type;
		foreach (EdmType item in GetSequence(sequence, (EdmType it) => it.Identity))
		{
			Visit(item);
		}
		AddObjectEndDumpToHashBuilder();
	}

	protected override void Visit(AssociationSet associationSet)
	{
		if (AddObjectToSeenListAndHashBuilder(associationSet, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(associationSet, instanceIndex);
			AddObjectContentToHashBuilder(associationSet.Identity);
			AddObjectContentToHashBuilder(associationSet.Schema);
			AddObjectContentToHashBuilder(associationSet.Table);
			base.Visit(associationSet);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EntityType entityType)
	{
		if (AddObjectToSeenListAndHashBuilder(entityType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(entityType, instanceIndex);
			AddObjectContentToHashBuilder(entityType.Abstract);
			AddObjectContentToHashBuilder(entityType.Identity);
			base.Visit(entityType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(AssociationSetEnd associationSetEnd)
	{
		if (AddObjectToSeenListAndHashBuilder(associationSetEnd, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(associationSetEnd, instanceIndex);
			AddObjectContentToHashBuilder(associationSetEnd.Identity);
			base.Visit(associationSetEnd);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(AssociationType associationType)
	{
		if (AddObjectToSeenListAndHashBuilder(associationType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(associationType, instanceIndex);
			AddObjectContentToHashBuilder(associationType.Abstract);
			AddObjectContentToHashBuilder(associationType.Identity);
			base.Visit(associationType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EdmProperty edmProperty)
	{
		if (AddObjectToSeenListAndHashBuilder(edmProperty, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(edmProperty, instanceIndex);
			AddObjectContentToHashBuilder(edmProperty.DefaultValue);
			AddObjectContentToHashBuilder(edmProperty.Identity);
			AddObjectContentToHashBuilder(edmProperty.IsStoreGeneratedComputed);
			AddObjectContentToHashBuilder(edmProperty.IsStoreGeneratedIdentity);
			AddObjectContentToHashBuilder(edmProperty.Nullable);
			base.Visit(edmProperty);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(NavigationProperty navigationProperty)
	{
	}

	protected override void Visit(EdmMember edmMember)
	{
		if (AddObjectToSeenListAndHashBuilder(edmMember, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(edmMember, instanceIndex);
			AddObjectContentToHashBuilder(edmMember.Identity);
			AddObjectContentToHashBuilder(edmMember.IsStoreGeneratedComputed);
			AddObjectContentToHashBuilder(edmMember.IsStoreGeneratedIdentity);
			base.Visit(edmMember);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(AssociationEndMember associationEndMember)
	{
		if (AddObjectToSeenListAndHashBuilder(associationEndMember, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(associationEndMember, instanceIndex);
			AddObjectContentToHashBuilder(associationEndMember.DeleteBehavior);
			AddObjectContentToHashBuilder(associationEndMember.Identity);
			AddObjectContentToHashBuilder(associationEndMember.IsStoreGeneratedComputed);
			AddObjectContentToHashBuilder(associationEndMember.IsStoreGeneratedIdentity);
			AddObjectContentToHashBuilder(associationEndMember.RelationshipMultiplicity);
			base.Visit(associationEndMember);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(ReferentialConstraint referentialConstraint)
	{
		if (AddObjectToSeenListAndHashBuilder(referentialConstraint, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(referentialConstraint, instanceIndex);
			AddObjectContentToHashBuilder(referentialConstraint.Identity);
			base.Visit(referentialConstraint);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(RelationshipEndMember relationshipEndMember)
	{
		if (AddObjectToSeenListAndHashBuilder(relationshipEndMember, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(relationshipEndMember, instanceIndex);
			AddObjectContentToHashBuilder(relationshipEndMember.DeleteBehavior);
			AddObjectContentToHashBuilder(relationshipEndMember.Identity);
			AddObjectContentToHashBuilder(relationshipEndMember.IsStoreGeneratedComputed);
			AddObjectContentToHashBuilder(relationshipEndMember.IsStoreGeneratedIdentity);
			AddObjectContentToHashBuilder(relationshipEndMember.RelationshipMultiplicity);
			base.Visit(relationshipEndMember);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(TypeUsage typeUsage)
	{
		if (AddObjectToSeenListAndHashBuilder(typeUsage, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(typeUsage, instanceIndex);
			base.Visit(typeUsage);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(RelationshipType relationshipType)
	{
		base.Visit(relationshipType);
	}

	protected override void Visit(EdmType edmType)
	{
		base.Visit(edmType);
	}

	protected override void Visit(EnumType enumType)
	{
		if (AddObjectToSeenListAndHashBuilder(enumType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(enumType, instanceIndex);
			AddObjectContentToHashBuilder(enumType.Identity);
			Visit(enumType.UnderlyingType);
			base.Visit(enumType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EnumMember enumMember)
	{
		if (AddObjectToSeenListAndHashBuilder(enumMember, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(enumMember, instanceIndex);
			AddObjectContentToHashBuilder(enumMember.Name);
			AddObjectContentToHashBuilder(enumMember.Value);
			base.Visit(enumMember);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(CollectionType collectionType)
	{
		if (AddObjectToSeenListAndHashBuilder(collectionType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(collectionType, instanceIndex);
			AddObjectContentToHashBuilder(collectionType.Identity);
			base.Visit(collectionType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(RefType refType)
	{
		if (AddObjectToSeenListAndHashBuilder(refType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(refType, instanceIndex);
			AddObjectContentToHashBuilder(refType.Identity);
			base.Visit(refType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EntityTypeBase entityTypeBase)
	{
		base.Visit(entityTypeBase);
	}

	protected override void Visit(Facet facet)
	{
		if (!(facet.Name != "Nullable") && AddObjectToSeenListAndHashBuilder(facet, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(facet, instanceIndex);
			AddObjectContentToHashBuilder(facet.Identity);
			AddObjectContentToHashBuilder(facet.Value);
			base.Visit(facet);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(EdmFunction edmFunction)
	{
	}

	protected override void Visit(ComplexType complexType)
	{
		if (AddObjectToSeenListAndHashBuilder(complexType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(complexType, instanceIndex);
			AddObjectContentToHashBuilder(complexType.Abstract);
			AddObjectContentToHashBuilder(complexType.Identity);
			base.Visit(complexType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(PrimitiveType primitiveType)
	{
		if (AddObjectToSeenListAndHashBuilder(primitiveType, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(primitiveType, instanceIndex);
			AddObjectContentToHashBuilder(primitiveType.Name);
			AddObjectContentToHashBuilder(primitiveType.NamespaceName);
			base.Visit(primitiveType);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(FunctionParameter functionParameter)
	{
		if (AddObjectToSeenListAndHashBuilder(functionParameter, out var instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(functionParameter, instanceIndex);
			AddObjectContentToHashBuilder(functionParameter.Identity);
			AddObjectContentToHashBuilder(functionParameter.Mode);
			base.Visit(functionParameter);
			AddObjectEndDumpToHashBuilder();
		}
	}

	protected override void Visit(DbProviderManifest providerManifest)
	{
	}

	private void Clean()
	{
		m_hashSourceBuilder = new CompressingHashBuilder(MetadataHelper.CreateMetadataHashAlgorithm(m_MappingVersion));
		m_instanceNumber = 0;
		m_itemsAlreadySeen = new Dictionary<object, int>();
	}

	private bool TryAddSeenItem(object o, out int indexSeen)
	{
		if (!m_itemsAlreadySeen.TryGetValue(o, out indexSeen))
		{
			m_itemsAlreadySeen.Add(o, m_instanceNumber);
			indexSeen = m_instanceNumber;
			m_instanceNumber++;
			return true;
		}
		return false;
	}

	private bool AddObjectToSeenListAndHashBuilder(object o, out int instanceIndex)
	{
		if (o == null)
		{
			instanceIndex = -1;
			return false;
		}
		if (!TryAddSeenItem(o, out instanceIndex))
		{
			AddObjectStartDumpToHashBuilder(o, instanceIndex);
			AddSeenObjectToHashBuilder(instanceIndex);
			AddObjectEndDumpToHashBuilder();
			return false;
		}
		return true;
	}

	private void AddSeenObjectToHashBuilder(int instanceIndex)
	{
		m_hashSourceBuilder.AppendLine("Instance Reference: " + instanceIndex);
	}

	private void AddObjectStartDumpToHashBuilder(object o, int objectIndex)
	{
		m_hashSourceBuilder.AppendObjectStartDump(o, objectIndex);
	}

	private void AddObjectEndDumpToHashBuilder()
	{
		m_hashSourceBuilder.AppendObjectEndDump();
	}

	private void AddObjectContentToHashBuilder(object content)
	{
		if (content != null)
		{
			if (content is IFormattable formattable)
			{
				m_hashSourceBuilder.AppendLine(formattable.ToString(null, CultureInfo.InvariantCulture));
			}
			else
			{
				m_hashSourceBuilder.AppendLine(content.ToString());
			}
		}
		else
		{
			m_hashSourceBuilder.AppendLine("NULL");
		}
	}

	private void AddV2ObjectContentToHashBuilder(object content, double version)
	{
		if (version >= 2.0)
		{
			AddObjectContentToHashBuilder(content);
		}
	}

	internal static string GetMappingClosureHash(double mappingVersion, EntityContainerMapping entityContainerMapping, bool sortSequence = true)
	{
		MetadataMappingHasherVisitor metadataMappingHasherVisitor = new MetadataMappingHasherVisitor(mappingVersion, sortSequence);
		metadataMappingHasherVisitor.Visit(entityContainerMapping);
		return metadataMappingHasherVisitor.HashValue;
	}
}
