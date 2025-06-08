using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets;

internal static class V1ConventionSet
{
	private static readonly ConventionSet _conventions = new ConventionSet(new IConvention[16]
	{
		new NotMappedTypeAttributeConvention(),
		new ComplexTypeAttributeConvention(),
		new TableAttributeConvention(),
		new NotMappedPropertyAttributeConvention(),
		new KeyAttributeConvention(),
		new RequiredPrimitivePropertyAttributeConvention(),
		new RequiredNavigationPropertyAttributeConvention(),
		new TimestampAttributeConvention(),
		new ConcurrencyCheckAttributeConvention(),
		new DatabaseGeneratedAttributeConvention(),
		new MaxLengthAttributeConvention(),
		new StringLengthAttributeConvention(),
		new ColumnAttributeConvention(),
		new IndexAttributeConvention(),
		new InversePropertyAttributeConvention(),
		new ForeignKeyPrimitivePropertyAttributeConvention()
	}.Reverse(), new IConvention[16]
	{
		new IdKeyDiscoveryConvention(),
		new AssociationInverseDiscoveryConvention(),
		new ForeignKeyNavigationPropertyAttributeConvention(),
		new OneToOneConstraintIntroductionConvention(),
		new NavigationPropertyNameForeignKeyDiscoveryConvention(),
		new PrimaryKeyNameForeignKeyDiscoveryConvention(),
		new TypeNameForeignKeyDiscoveryConvention(),
		new ForeignKeyAssociationMultiplicityConvention(),
		new OneToManyCascadeDeleteConvention(),
		new ComplexTypeDiscoveryConvention(),
		new StoreGeneratedIdentityKeyConvention(),
		new PluralizingEntitySetNameConvention(),
		new DeclaredPropertyOrderingConvention(),
		new SqlCePropertyMaxLengthConvention(),
		new PropertyMaxLengthConvention(),
		new DecimalPropertyConvention()
	}, new IConvention[2]
	{
		new ManyToManyCascadeDeleteConvention(),
		new MappingInheritedPropertiesSupportConvention()
	}, new IConvention[3]
	{
		new PluralizingTableNameConvention(),
		new ColumnOrderingConvention(),
		new ForeignKeyIndexConvention()
	});

	public static ConventionSet Conventions => _conventions;
}
