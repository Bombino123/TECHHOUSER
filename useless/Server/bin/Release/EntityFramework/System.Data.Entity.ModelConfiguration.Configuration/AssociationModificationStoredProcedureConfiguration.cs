using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class AssociationModificationStoredProcedureConfiguration<TEntityType> where TEntityType : class
{
	private readonly PropertyInfo _navigationPropertyInfo;

	private readonly ModificationStoredProcedureConfiguration _configuration;

	internal AssociationModificationStoredProcedureConfiguration(PropertyInfo navigationPropertyInfo, ModificationStoredProcedureConfiguration configuration)
	{
		_navigationPropertyInfo = navigationPropertyInfo;
		_configuration = configuration;
	}

	public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		_configuration.Parameter(new PropertyPath(new PropertyInfo[1] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())), parameterName);
		return this;
	}

	public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		_configuration.Parameter(new PropertyPath(new PropertyInfo[1] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())), parameterName);
		return this;
	}

	public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		_configuration.Parameter(new PropertyPath(new PropertyInfo[1] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())), parameterName);
		return this;
	}

	public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		_configuration.Parameter(new PropertyPath(new PropertyInfo[1] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())), parameterName);
		return this;
	}
}
