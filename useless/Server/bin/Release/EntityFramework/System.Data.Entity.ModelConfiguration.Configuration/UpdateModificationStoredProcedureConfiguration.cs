using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class UpdateModificationStoredProcedureConfiguration<TEntityType> : ModificationStoredProcedureConfigurationBase where TEntityType : class
{
	internal UpdateModificationStoredProcedureConfiguration()
	{
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> HasName(string procedureName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		base.Configuration.HasName(procedureName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> HasName(string procedureName, string schemaName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		Check.NotEmpty(schemaName, "schemaName");
		base.Configuration.HasName(procedureName, schemaName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, DbGeography>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, DbGeometry>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression, string currentValueParameterName, string originalValueParameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression, string currentValueParameterName, string originalValueParameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, string>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, byte[]>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, DbGeography>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Parameter(Expression<Func<TEntityType, DbGeometry>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		base.Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression, string columnName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression, string columnName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result(Expression<Func<TEntityType, string>> propertyExpression, string columnName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result(Expression<Func<TEntityType, byte[]>> propertyExpression, string columnName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result(Expression<Func<TEntityType, DbGeography>> propertyExpression, string columnName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Result(Expression<Func<TEntityType, DbGeometry>> propertyExpression, string columnName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> RowsAffectedParameter(string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.RowsAffectedParameter(parameterName);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Navigation<TPrincipalEntityType>(Expression<Func<TPrincipalEntityType, TEntityType>> navigationPropertyExpression, Action<AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType>> associationModificationStoredProcedureConfigurationAction) where TPrincipalEntityType : class
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		Check.NotNull(associationModificationStoredProcedureConfigurationAction, "associationModificationStoredProcedureConfigurationAction");
		AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType> obj = new AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType>(navigationPropertyExpression.GetSimplePropertyAccess().Single(), base.Configuration);
		associationModificationStoredProcedureConfigurationAction(obj);
		return this;
	}

	public UpdateModificationStoredProcedureConfiguration<TEntityType> Navigation<TPrincipalEntityType>(Expression<Func<TPrincipalEntityType, ICollection<TEntityType>>> navigationPropertyExpression, Action<AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType>> associationModificationStoredProcedureConfigurationAction) where TPrincipalEntityType : class
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		Check.NotNull(associationModificationStoredProcedureConfigurationAction, "associationModificationStoredProcedureConfigurationAction");
		AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType> obj = new AssociationModificationStoredProcedureConfiguration<TPrincipalEntityType>(navigationPropertyExpression.GetSimplePropertyAccess().Single(), base.Configuration);
		associationModificationStoredProcedureConfigurationAction(obj);
		return this;
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
