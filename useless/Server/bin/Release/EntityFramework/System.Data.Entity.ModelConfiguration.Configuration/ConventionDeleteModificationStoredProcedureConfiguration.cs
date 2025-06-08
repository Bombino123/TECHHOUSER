using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionDeleteModificationStoredProcedureConfiguration : ConventionModificationStoredProcedureConfiguration
{
	private readonly Type _type;

	internal ConventionDeleteModificationStoredProcedureConfiguration(Type type)
	{
		_type = type;
	}

	public ConventionDeleteModificationStoredProcedureConfiguration HasName(string procedureName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		base.Configuration.HasName(procedureName);
		return this;
	}

	public ConventionDeleteModificationStoredProcedureConfiguration HasName(string procedureName, string schemaName)
	{
		Check.NotEmpty(procedureName, "procedureName");
		Check.NotEmpty(schemaName, "schemaName");
		base.Configuration.HasName(procedureName, schemaName);
		return this;
	}

	public ConventionDeleteModificationStoredProcedureConfiguration Parameter(string propertyName, string parameterName)
	{
		Check.NotEmpty(propertyName, "propertyName");
		Check.NotEmpty(parameterName, "parameterName");
		return Parameter(_type.GetAnyProperty(propertyName), parameterName);
	}

	public ConventionDeleteModificationStoredProcedureConfiguration Parameter(PropertyInfo propertyInfo, string parameterName)
	{
		Check.NotEmpty(parameterName, "parameterName");
		if (propertyInfo != null)
		{
			base.Configuration.Parameter(new PropertyPath(propertyInfo), parameterName);
		}
		return this;
	}

	public ConventionDeleteModificationStoredProcedureConfiguration RowsAffectedParameter(string parameterName)
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
