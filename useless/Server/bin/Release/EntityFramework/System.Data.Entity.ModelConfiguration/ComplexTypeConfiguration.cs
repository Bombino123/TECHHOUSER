using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration;

public class ComplexTypeConfiguration<TComplexType> : StructuralTypeConfiguration<TComplexType> where TComplexType : class
{
	private readonly ComplexTypeConfiguration _complexTypeConfiguration;

	internal override StructuralTypeConfiguration Configuration => _complexTypeConfiguration;

	public ComplexTypeConfiguration()
		: this(new ComplexTypeConfiguration(typeof(TComplexType)))
	{
	}

	public ComplexTypeConfiguration<TComplexType> Ignore<TProperty>(Expression<Func<TComplexType, TProperty>> propertyExpression)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Configuration.Ignore(propertyExpression.GetSimplePropertyAccess().Single());
		return this;
	}

	internal ComplexTypeConfiguration(ComplexTypeConfiguration configuration)
	{
		_complexTypeConfiguration = configuration;
	}

	internal override TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(LambdaExpression lambdaExpression)
	{
		return Configuration.Property(lambdaExpression.GetSimplePropertyAccess(), () => new TPrimitivePropertyConfiguration
		{
			OverridableConfigurationParts = OverridableConfigurationParts.OverridableInSSpace
		});
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
