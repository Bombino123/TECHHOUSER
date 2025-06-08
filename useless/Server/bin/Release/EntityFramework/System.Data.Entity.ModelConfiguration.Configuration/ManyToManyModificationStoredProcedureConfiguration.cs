using System.ComponentModel;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> : ModificationStoredProcedureConfigurationBase where TEntityType : class where TTargetEntityType : class
{
	internal ManyToManyModificationStoredProcedureConfiguration()
	{
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> HasName(string procedureName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		base.Configuration.HasName(procedureName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> HasName(string procedureName, string schemaName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		Check.NotEmpty(schemaName, "schemaName");
		base.Configuration.HasName(procedureName, schemaName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(Expression<Func<TTargetEntityType, TProperty>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, null, rightKey: true);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(Expression<Func<TTargetEntityType, TProperty?>> propertyExpression, string parameterName) where TProperty : struct
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, null, rightKey: true);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(Expression<Func<TTargetEntityType, string>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, null, rightKey: true);
		return this;
	}

	public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(Expression<Func<TTargetEntityType, byte[]>> propertyExpression, string parameterName)
	{
		Check.NotNull(propertyExpression, "propertyExpression");
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, null, rightKey: true);
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
