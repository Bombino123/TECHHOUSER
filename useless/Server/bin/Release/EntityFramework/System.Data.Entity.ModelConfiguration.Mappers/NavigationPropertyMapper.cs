using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Mappers;

internal sealed class NavigationPropertyMapper
{
	private readonly TypeMapper _typeMapper;

	public NavigationPropertyMapper(TypeMapper typeMapper)
	{
		_typeMapper = typeMapper;
	}

	public void Map(PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
	{
		Type elementType = propertyInfo.PropertyType;
		RelationshipMultiplicity relationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
		if (elementType.IsCollection(out elementType))
		{
			relationshipMultiplicity = RelationshipMultiplicity.Many;
		}
		EntityType entityType2 = _typeMapper.MapEntityType(elementType);
		if (entityType2 != null)
		{
			RelationshipMultiplicity sourceAssociationEndKind = ((!relationshipMultiplicity.IsMany()) ? RelationshipMultiplicity.Many : RelationshipMultiplicity.ZeroOrOne);
			AssociationType associationType = _typeMapper.MappingContext.Model.AddAssociationType(entityType.Name + "_" + propertyInfo.Name, entityType, sourceAssociationEndKind, entityType2, relationshipMultiplicity, _typeMapper.MappingContext.ModelConfiguration.ModelNamespace);
			associationType.SourceEnd.SetClrPropertyInfo(propertyInfo);
			_typeMapper.MappingContext.Model.AddAssociationSet(associationType.Name, associationType);
			NavigationProperty navigationProperty = entityType.AddNavigationProperty(propertyInfo.Name, associationType);
			navigationProperty.SetClrPropertyInfo(propertyInfo);
			_typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, () => entityTypeConfiguration().Navigation(propertyInfo), _typeMapper.MappingContext.ModelConfiguration);
			new AttributeMapper(_typeMapper.MappingContext.AttributeProvider).Map(propertyInfo, navigationProperty.GetMetadataProperties());
		}
	}
}
