using System.ComponentModel;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Spatial;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class StructuralTypeConfiguration<TStructuralType> where TStructuralType : class
{
	internal abstract StructuralTypeConfiguration Configuration { get; }

	public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
	{
		return new PrimitivePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
	{
		return new PrimitivePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, HierarchyId>> propertyExpression)
	{
		return new PrimitivePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, DbGeometry>> propertyExpression)
	{
		return new PrimitivePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, DbGeography>> propertyExpression)
	{
		return new PrimitivePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>(propertyExpression));
	}

	public StringPropertyConfiguration Property(Expression<Func<TStructuralType, string>> propertyExpression)
	{
		return new StringPropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration>(propertyExpression));
	}

	public BinaryPropertyConfiguration Property(Expression<Func<TStructuralType, byte[]>> propertyExpression)
	{
		return new BinaryPropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration>(propertyExpression));
	}

	public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal>> propertyExpression)
	{
		return new DecimalPropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
	}

	public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal?>> propertyExpression)
	{
		return new DecimalPropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, DateTime>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, DateTime?>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset?>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	public DateTimePropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan?>> propertyExpression)
	{
		return new DateTimePropertyConfiguration(Property<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration>(propertyExpression));
	}

	internal abstract TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(LambdaExpression lambdaExpression) where TPrimitivePropertyConfiguration : System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration, new();

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
