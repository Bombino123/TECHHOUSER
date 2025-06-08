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

public sealed class ForeignKeyAssociationMappingConfiguration : AssociationMappingConfiguration
{
	private readonly List<string> _keyColumnNames = new List<string>();

	private readonly IDictionary<Tuple<string, string>, object> _annotations = new Dictionary<Tuple<string, string>, object>();

	private DatabaseName _tableName;

	internal ForeignKeyAssociationMappingConfiguration()
	{
	}

	private ForeignKeyAssociationMappingConfiguration(ForeignKeyAssociationMappingConfiguration source)
	{
		_keyColumnNames.AddRange(source._keyColumnNames);
		_tableName = source._tableName;
		foreach (KeyValuePair<Tuple<string, string>, object> annotation in source._annotations)
		{
			_annotations.Add(annotation);
		}
	}

	internal override AssociationMappingConfiguration Clone()
	{
		return new ForeignKeyAssociationMappingConfiguration(this);
	}

	public ForeignKeyAssociationMappingConfiguration MapKey(params string[] keyColumnNames)
	{
		Check.NotNull(keyColumnNames, "keyColumnNames");
		_keyColumnNames.Clear();
		_keyColumnNames.AddRange(keyColumnNames);
		return this;
	}

	public ForeignKeyAssociationMappingConfiguration HasColumnAnnotation(string keyColumnName, string annotationName, object value)
	{
		Check.NotEmpty(keyColumnName, "keyColumnName");
		Check.NotEmpty(annotationName, "annotationName");
		_annotations[Tuple.Create(keyColumnName, annotationName)] = value;
		return this;
	}

	public ForeignKeyAssociationMappingConfiguration ToTable(string tableName)
	{
		Check.NotEmpty(tableName, "tableName");
		return ToTable(tableName, null);
	}

	public ForeignKeyAssociationMappingConfiguration ToTable(string tableName, string schemaName)
	{
		Check.NotEmpty(tableName, "tableName");
		_tableName = new DatabaseName(tableName, schemaName);
		return this;
	}

	internal override void Configure(AssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
	{
		List<ScalarPropertyMapping> propertyMappings = associationSetMapping.SourceEndMapping.PropertyMappings.ToList();
		if (_tableName != null)
		{
			EntityType targetTable = (from t in database.EntityTypes
				let n = t.GetTableName()
				where n != null && n.Equals(_tableName)
				select t).SingleOrDefault() ?? (from es in database.GetEntitySets()
				where string.Equals(es.Table, _tableName.Name, StringComparison.Ordinal)
				select es.ElementType).SingleOrDefault();
			if (targetTable == null)
			{
				throw Error.TableNotFound(_tableName);
			}
			EntityType sourceTable = associationSetMapping.Table;
			if (sourceTable != targetTable)
			{
				ForeignKeyBuilder foreignKeyBuilder = sourceTable.ForeignKeyBuilders.Single((ForeignKeyBuilder fk) => fk.DependentColumns.SequenceEqual(propertyMappings.Select((ScalarPropertyMapping pm) => pm.Column)));
				sourceTable.RemoveForeignKey(foreignKeyBuilder);
				targetTable.AddForeignKey(foreignKeyBuilder);
				foreignKeyBuilder.DependentColumns.Each(delegate(EdmProperty c)
				{
					bool isPrimaryKeyColumn = c.IsPrimaryKeyColumn;
					sourceTable.RemoveMember(c);
					targetTable.AddMember(c);
					if (isPrimaryKeyColumn)
					{
						targetTable.AddKeyMember(c);
					}
				});
				associationSetMapping.StoreEntitySet = database.GetEntitySet(targetTable);
			}
		}
		if (_keyColumnNames.Count > 0 && _keyColumnNames.Count != propertyMappings.Count())
		{
			throw Error.IncorrectColumnCount(string.Join(", ", _keyColumnNames));
		}
		_keyColumnNames.Each(delegate(string n, int i)
		{
			propertyMappings[i].Column.Name = n;
		});
		foreach (KeyValuePair<Tuple<string, string>, object> annotation in _annotations)
		{
			int num = _keyColumnNames.IndexOf(annotation.Key.Item1);
			if (num == -1)
			{
				throw new InvalidOperationException(Strings.BadKeyNameForAnnotation(annotation.Key.Item1, annotation.Key.Item2));
			}
			propertyMappings[num].Column.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:" + annotation.Key.Item2, annotation.Value);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Equals(ForeignKeyAssociationMappingConfiguration other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(other._tableName, _tableName) && other._keyColumnNames.SequenceEqual(_keyColumnNames))
		{
			return other._annotations.OrderBy((KeyValuePair<Tuple<string, string>, object> a) => a.Key).SequenceEqual(_annotations.OrderBy((KeyValuePair<Tuple<string, string>, object> a) => a.Key));
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
		if (obj.GetType() != typeof(ForeignKeyAssociationMappingConfiguration))
		{
			return false;
		}
		return Equals((ForeignKeyAssociationMappingConfiguration)obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		int seed = ((_tableName != null) ? _tableName.GetHashCode() : 0) * 397;
		seed = _keyColumnNames.Aggregate(seed, (int h, string v) => (h * 397) ^ v.GetHashCode());
		return _annotations.OrderBy((KeyValuePair<Tuple<string, string>, object> a) => a.Key).Aggregate(seed, (int h, KeyValuePair<Tuple<string, string>, object> v) => (h * 397) ^ v.GetHashCode());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
