using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionUpdateModificationStoredProcedureConfiguration : ConventionModificationStoredProcedureConfiguration
{
	private readonly Type _type;

	internal ConventionUpdateModificationStoredProcedureConfiguration(Type type)
	{
		_type = type;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration HasName(string procedureName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		base.Configuration.HasName(procedureName);
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration HasName(string procedureName, string schemaName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		Check.NotEmpty(schemaName, "schemaName");
		base.Configuration.HasName(procedureName, schemaName);
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Parameter(string propertyName, string parameterName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(parameterName, "parameterName");
		return Parameter(_type.GetAnyProperty(propertyName), parameterName);
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Parameter(PropertyInfo propertyInfo, string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		if (propertyInfo != null)
		{
			base.Configuration.Parameter(new PropertyPath(propertyInfo), parameterName);
		}
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Parameter(string propertyName, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		return Parameter(_type.GetAnyProperty(propertyName), currentValueParameterName, originalValueParameterName);
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Parameter(PropertyInfo propertyInfo, string currentValueParameterName, string originalValueParameterName)
	{
		Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
		Check.NotEmpty(originalValueParameterName, "originalValueParameterName");
		if (propertyInfo != null)
		{
			base.Configuration.Parameter(new PropertyPath(propertyInfo), currentValueParameterName, originalValueParameterName);
		}
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Result(string propertyName, string columnName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(new PropertyPath(_type.GetAnyProperty(propertyName)), columnName);
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration Result(PropertyInfo propertyInfo, string columnName)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(new PropertyPath(propertyInfo), columnName);
		return this;
	}

	public ConventionUpdateModificationStoredProcedureConfiguration RowsAffectedParameter(string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		base.Configuration.RowsAffectedParameter(parameterName);
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
