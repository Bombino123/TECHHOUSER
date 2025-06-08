using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ModificationStoredProcedureConfiguration
{
	private sealed class ParameterKey
	{
		private readonly PropertyPath _propertyPath;

		private readonly bool _rightKey;

		public PropertyPath PropertyPath => _propertyPath;

		public bool IsRightKey => _rightKey;

		public ParameterKey(PropertyPath propertyPath, bool rightKey)
		{
			_propertyPath = propertyPath;
			_rightKey = rightKey;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			ParameterKey parameterKey = (ParameterKey)obj;
			if (_propertyPath.Equals(parameterKey._propertyPath))
			{
				bool rightKey = _rightKey;
				return rightKey.Equals(parameterKey._rightKey);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = _propertyPath.GetHashCode() * 397;
			bool rightKey = _rightKey;
			return num ^ rightKey.GetHashCode();
		}
	}

	private readonly Dictionary<ParameterKey, Tuple<string, string>> _parameterNames = new Dictionary<ParameterKey, Tuple<string, string>>();

	private readonly Dictionary<PropertyInfo, string> _resultBindings = new Dictionary<PropertyInfo, string>();

	private string _name;

	private string _schema;

	private string _rowsAffectedParameter;

	private List<FunctionParameter> _configuredParameters;

	public string Name => _name;

	public string Schema => _schema;

	public string RowsAffectedParameterName => _rowsAffectedParameter;

	public IEnumerable<Tuple<string, string>> ParameterNames => _parameterNames.Values;

	public Dictionary<PropertyInfo, string> ResultBindings => _resultBindings;

	public ModificationStoredProcedureConfiguration()
	{
	}

	private ModificationStoredProcedureConfiguration(ModificationStoredProcedureConfiguration source)
	{
		_name = source._name;
		_schema = source._schema;
		_rowsAffectedParameter = source._rowsAffectedParameter;
		source._parameterNames.Each(delegate(KeyValuePair<ParameterKey, Tuple<string, string>> c)
		{
			_parameterNames.Add(c.Key, Tuple.Create(c.Value.Item1, c.Value.Item2));
		});
		source._resultBindings.Each(delegate(KeyValuePair<PropertyInfo, string> r)
		{
			_resultBindings.Add(r.Key, r.Value);
		});
	}

	public virtual ModificationStoredProcedureConfiguration Clone()
	{
		return new ModificationStoredProcedureConfiguration(this);
	}

	public void HasName(string name)
	{
		DatabaseName databaseName = DatabaseName.Parse(name);
		_name = databaseName.Name;
		_schema = databaseName.Schema;
	}

	public void HasName(string name, string schema)
	{
		_name = name;
		_schema = schema;
	}

	public void RowsAffectedParameter(string name)
	{
		_rowsAffectedParameter = name;
	}

	public void ClearParameterNames()
	{
		_parameterNames.Clear();
	}

	public void Parameter(PropertyPath propertyPath, string parameterName, string originalValueParameterName = null, bool rightKey = false)
	{
		_parameterNames[new ParameterKey(propertyPath, rightKey)] = Tuple.Create(parameterName, originalValueParameterName);
	}

	public void Result(PropertyPath propertyPath, string columnName)
	{
		_resultBindings[propertyPath.Single()] = columnName;
	}

	public virtual void Configure(ModificationFunctionMapping modificationStoredProcedureMapping, DbProviderManifest providerManifest)
	{
		_configuredParameters = new List<FunctionParameter>();
		ConfigureName(modificationStoredProcedureMapping);
		ConfigureSchema(modificationStoredProcedureMapping);
		ConfigureRowsAffectedParameter(modificationStoredProcedureMapping, providerManifest);
		ConfigureParameters(modificationStoredProcedureMapping);
		ConfigureResultBindings(modificationStoredProcedureMapping);
	}

	private void ConfigureName(ModificationFunctionMapping modificationStoredProcedureMapping)
	{
		if (!string.IsNullOrWhiteSpace(_name))
		{
			modificationStoredProcedureMapping.Function.StoreFunctionNameAttribute = _name;
		}
	}

	private void ConfigureSchema(ModificationFunctionMapping modificationStoredProcedureMapping)
	{
		if (!string.IsNullOrWhiteSpace(_schema))
		{
			modificationStoredProcedureMapping.Function.Schema = _schema;
		}
	}

	private void ConfigureRowsAffectedParameter(ModificationFunctionMapping modificationStoredProcedureMapping, DbProviderManifest providerManifest)
	{
		if (!string.IsNullOrWhiteSpace(_rowsAffectedParameter))
		{
			if (modificationStoredProcedureMapping.RowsAffectedParameter == null)
			{
				FunctionParameter functionParameter = new FunctionParameter("_RowsAffected_", providerManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))), ParameterMode.Out);
				modificationStoredProcedureMapping.Function.AddParameter(functionParameter);
				modificationStoredProcedureMapping.RowsAffectedParameter = functionParameter;
			}
			modificationStoredProcedureMapping.RowsAffectedParameter.Name = _rowsAffectedParameter;
			_configuredParameters.Add(modificationStoredProcedureMapping.RowsAffectedParameter);
		}
	}

	private void ConfigureParameters(ModificationFunctionMapping modificationStoredProcedureMapping)
	{
		foreach (KeyValuePair<ParameterKey, Tuple<string, string>> parameterName in _parameterNames)
		{
			PropertyPath propertyPath = parameterName.Key.PropertyPath;
			string item = parameterName.Value.Item1;
			string item2 = parameterName.Value.Item2;
			List<ModificationFunctionParameterBinding> list = modificationStoredProcedureMapping.ParameterBindings.Where((ModificationFunctionParameterBinding pb) => ((pb.MemberPath.AssociationSetEnd == null || pb.MemberPath.AssociationSetEnd.ParentAssociationSet.ElementType.IsManyToMany()) && propertyPath.Equals(new PropertyPath(from m in pb.MemberPath.Members.OfType<EdmProperty>()
				select m.GetClrPropertyInfo()))) || (propertyPath.Count == 2 && pb.MemberPath.AssociationSetEnd != null && pb.MemberPath.Members.First().GetClrPropertyInfo().IsSameAs(propertyPath.Last()) && (from ae in pb.MemberPath.AssociationSetEnd.ParentAssociationSet.AssociationSetEnds
				select ae.CorrespondingAssociationEndMember.GetClrPropertyInfo() into pi
				where pi != null
				select pi).Any((PropertyInfo pi) => pi.IsSameAs(propertyPath.First())))).ToList();
			if (list.Count == 1)
			{
				ModificationFunctionParameterBinding modificationFunctionParameterBinding = list.Single();
				if (!string.IsNullOrWhiteSpace(item2) && modificationFunctionParameterBinding.IsCurrent)
				{
					throw Error.ModificationFunctionParameterNotFoundOriginal(propertyPath, modificationStoredProcedureMapping.Function.FunctionName);
				}
				modificationFunctionParameterBinding.Parameter.Name = item;
				_configuredParameters.Add(modificationFunctionParameterBinding.Parameter);
				continue;
			}
			if (list.Count == 2)
			{
				ModificationFunctionParameterBinding modificationFunctionParameterBinding2 = ((list.Select((ModificationFunctionParameterBinding pb) => pb.IsCurrent).Distinct().Count() != 1 || !list.All((ModificationFunctionParameterBinding pb) => pb.MemberPath.AssociationSetEnd != null)) ? list.Single((ModificationFunctionParameterBinding pb) => pb.IsCurrent) : ((!parameterName.Key.IsRightKey) ? list.First() : list.Last()));
				modificationFunctionParameterBinding2.Parameter.Name = item;
				_configuredParameters.Add(modificationFunctionParameterBinding2.Parameter);
				if (!string.IsNullOrWhiteSpace(item2))
				{
					modificationFunctionParameterBinding2 = list.Single((ModificationFunctionParameterBinding pb) => !pb.IsCurrent);
					modificationFunctionParameterBinding2.Parameter.Name = item2;
					_configuredParameters.Add(modificationFunctionParameterBinding2.Parameter);
				}
				continue;
			}
			throw Error.ModificationFunctionParameterNotFound(propertyPath, modificationStoredProcedureMapping.Function.FunctionName);
		}
		foreach (FunctionParameter item3 in modificationStoredProcedureMapping.Function.Parameters.Except(_configuredParameters))
		{
			item3.Name = modificationStoredProcedureMapping.Function.Parameters.Except(new FunctionParameter[1] { item3 }).UniquifyName(item3.Name);
		}
	}

	private void ConfigureResultBindings(ModificationFunctionMapping modificationStoredProcedureMapping)
	{
		foreach (KeyValuePair<PropertyInfo, string> resultBinding in _resultBindings)
		{
			PropertyInfo propertyInfo = resultBinding.Key;
			string value = resultBinding.Value;
			IEnumerable<ModificationFunctionResultBinding> resultBindings = modificationStoredProcedureMapping.ResultBindings;
			((resultBindings ?? Enumerable.Empty<ModificationFunctionResultBinding>()).SingleOrDefault((ModificationFunctionResultBinding rb) => propertyInfo.IsSameAs(rb.Property.GetClrPropertyInfo())) ?? throw Error.ResultBindingNotFound(propertyInfo.Name, modificationStoredProcedureMapping.Function.FunctionName)).ColumnName = value;
		}
	}

	public bool IsCompatibleWith(ModificationStoredProcedureConfiguration other)
	{
		if (_name != null && other._name != null && !string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (_schema != null && other._schema != null && !string.Equals(_schema, other._schema, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return !(from kv1 in _parameterNames
			join kv2 in other._parameterNames on kv1.Key equals kv2.Key
			select !object.Equals(kv1.Value, kv2.Value)).Any((bool j) => j);
	}

	public void Merge(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration, bool allowOverride)
	{
		if (allowOverride || string.IsNullOrWhiteSpace(_name))
		{
			_name = modificationStoredProcedureConfiguration.Name ?? _name;
		}
		if (allowOverride || string.IsNullOrWhiteSpace(_schema))
		{
			_schema = modificationStoredProcedureConfiguration.Schema ?? _schema;
		}
		if (allowOverride || string.IsNullOrWhiteSpace(_rowsAffectedParameter))
		{
			_rowsAffectedParameter = modificationStoredProcedureConfiguration.RowsAffectedParameterName ?? _rowsAffectedParameter;
		}
		foreach (KeyValuePair<ParameterKey, Tuple<string, string>> item in modificationStoredProcedureConfiguration._parameterNames.Where((KeyValuePair<ParameterKey, Tuple<string, string>> parameterName) => allowOverride || !_parameterNames.ContainsKey(parameterName.Key)))
		{
			_parameterNames[item.Key] = item.Value;
		}
		foreach (KeyValuePair<PropertyInfo, string> item2 in modificationStoredProcedureConfiguration.ResultBindings.Where((KeyValuePair<PropertyInfo, string> resultBinding) => allowOverride || !_resultBindings.ContainsKey(resultBinding.Key)))
		{
			_resultBindings[item2.Key] = item2.Value;
		}
	}
}
