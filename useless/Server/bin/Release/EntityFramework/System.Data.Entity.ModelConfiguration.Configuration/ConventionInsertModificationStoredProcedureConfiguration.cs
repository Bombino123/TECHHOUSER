using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionInsertModificationStoredProcedureConfiguration : ConventionModificationStoredProcedureConfiguration
{
	private readonly Type _type;

	internal ConventionInsertModificationStoredProcedureConfiguration(Type type)
	{
		_type = type;
	}

	public ConventionInsertModificationStoredProcedureConfiguration HasName(string procedureName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		base.Configuration.HasName(procedureName);
		return this;
	}

	public ConventionInsertModificationStoredProcedureConfiguration HasName(string procedureName, string schemaName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		Check.NotEmpty(schemaName, "schemaName");
		base.Configuration.HasName(procedureName, schemaName);
		return this;
	}

	public ConventionInsertModificationStoredProcedureConfiguration Parameter(string propertyName, string parameterName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(parameterName, "parameterName");
		return Parameter(_type.GetAnyProperty(propertyName), parameterName);
	}

	public ConventionInsertModificationStoredProcedureConfiguration Parameter(PropertyInfo propertyInfo, string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		if (propertyInfo != null)
		{
			base.Configuration.Parameter(new PropertyPath(propertyInfo), parameterName);
		}
		return this;
	}

	public ConventionInsertModificationStoredProcedureConfiguration Result(string propertyName, string columnName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(new PropertyPath(_type.GetAnyProperty(propertyName)), columnName);
		return this;
	}

	public ConventionInsertModificationStoredProcedureConfiguration Result(PropertyInfo propertyInfo, string columnName)
	{
		Check.NotNull(propertyInfo, "propertyInfo");
		Check.NotEmpty(columnName, "columnName");
		base.Configuration.Result(new PropertyPath(propertyInfo), columnName);
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
