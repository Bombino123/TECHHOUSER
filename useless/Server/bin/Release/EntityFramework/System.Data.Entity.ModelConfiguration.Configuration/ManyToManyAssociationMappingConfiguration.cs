using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public sealed class ManyToManyAssociationMappingConfiguration : AssociationMappingConfiguration
{
	private readonly List<string> _leftKeyColumnNames = new List<string>();

	private readonly List<string> _rightKeyColumnNames = new List<string>();

	private DatabaseName _tableName;

	private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

	internal ManyToManyAssociationMappingConfiguration()
	{
	}

	private ManyToManyAssociationMappingConfiguration(ManyToManyAssociationMappingConfiguration source)
	{
		_leftKeyColumnNames.AddRange(source._leftKeyColumnNames);
		_rightKeyColumnNames.AddRange(source._rightKeyColumnNames);
		_tableName = source._tableName;
		foreach (KeyValuePair<string, object> annotation in source._annotations)
		{
			_annotations.Add(annotation);
		}
	}

	internal override AssociationMappingConfiguration Clone()
	{
		return new ManyToManyAssociationMappingConfiguration(this);
	}

	public ManyToManyAssociationMappingConfiguration ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		return ToTable(tableName, null);
	}

	public ManyToManyAssociationMappingConfiguration ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		_tableName = new DatabaseName(tableName, schemaName);
		return this;
	}

	public ManyToManyAssociationMappingConfiguration HasTableAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		if (!name.IsValidUndottedName())
		{
			throw new ArgumentException(Strings.BadAnnotationName(name));
		}
		_annotations[name] = value;
		return this;
	}

	public ManyToManyAssociationMappingConfiguration MapLeftKey(params string[] keyColumnNames)
	{
		Check.NotNull(keyColumnNames, "keyColumnNames");
		_leftKeyColumnNames.Clear();
		_leftKeyColumnNames.AddRange(keyColumnNames);
		return this;
	}

	public ManyToManyAssociationMappingConfiguration MapRightKey(params string[] keyColumnNames)
	{
		Check.NotNull(keyColumnNames, "keyColumnNames");
		_rightKeyColumnNames.Clear();
		_rightKeyColumnNames.AddRange(keyColumnNames);
		return this;
	}

	internal override void Configure(AssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
	{
		EntityType table = associationSetMapping.Table;
		if (_tableName != null)
		{
			table.SetTableName(_tableName);
			table.SetConfiguration(this);
		}
		bool num = navigationProperty.IsSameAs(associationSetMapping.SourceEndMapping.AssociationEnd.GetClrPropertyInfo());
		ConfigureColumnNames(num ? _leftKeyColumnNames : _rightKeyColumnNames, associationSetMapping.SourceEndMapping.PropertyMappings.ToList());
		ConfigureColumnNames(num ? _rightKeyColumnNames : _leftKeyColumnNames, associationSetMapping.TargetEndMapping.PropertyMappings.ToList());
		foreach (KeyValuePair<string, object> annotation in _annotations)
		{
			table.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:" + annotation.Key, annotation.Value);
		}
	}

	private static void ConfigureColumnNames(ICollection<string> keyColumnNames, IList<ScalarPropertyMapping> propertyMappings)
	{
		if (keyColumnNames.Count > 0 && keyColumnNames.Count != propertyMappings.Count)
		{
			throw Error.IncorrectColumnCount(string.Join(", ", keyColumnNames));
		}
		keyColumnNames.Each(delegate(string n, int i)
		{
			propertyMappings[i].Column.Name = n;
		});
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Equals(ManyToManyAssociationMappingConfiguration other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (!object.Equals(other._tableName, _tableName))
		{
			return false;
		}
		if (object.Equals(other._tableName, _tableName) && ((_leftKeyColumnNames.SequenceEqual(other._leftKeyColumnNames) && _rightKeyColumnNames.SequenceEqual(other._rightKeyColumnNames)) || (_leftKeyColumnNames.SequenceEqual(other._rightKeyColumnNames) && _rightKeyColumnNames.SequenceEqual(other._leftKeyColumnNames))))
		{
			return _annotations.OrderBy((KeyValuePair<string, object> a) => a.Key).SequenceEqual(other._annotations.OrderBy((KeyValuePair<string, object> a) => a.Key));
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
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
		if (obj.GetType() != typeof(ManyToManyAssociationMappingConfiguration))
		{
			return false;
		}
		return Equals((ManyToManyAssociationMappingConfiguration)obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		int seed = ((_tableName != null) ? _tableName.GetHashCode() : 0) * 397;
		seed = _leftKeyColumnNames.Aggregate(seed, (int h, string v) => (h * 397) ^ v.GetHashCode());
		seed = _rightKeyColumnNames.Aggregate(seed, (int h, string v) => (h * 397) ^ v.GetHashCode());
		return _annotations.OrderBy((KeyValuePair<string, object> a) => a.Key).Aggregate(seed, (int h, KeyValuePair<string, object> v) => (h * 397) ^ v.GetHashCode());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
