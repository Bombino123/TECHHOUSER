using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class TphColumnFixer
{
	private readonly IList<ColumnMappingBuilder> _columnMappings;

	private readonly EntityType _table;

	private readonly EdmModel _storeModel;

	public TphColumnFixer(IEnumerable<ColumnMappingBuilder> columnMappings, EntityType table, EdmModel storeModel)
	{
		_columnMappings = columnMappings.OrderBy((ColumnMappingBuilder m) => m.ColumnProperty.Name).ToList();
		_table = table;
		_storeModel = storeModel;
	}

	public void RemoveDuplicateTphColumns()
	{
		int num = 0;
		while (num < _columnMappings.Count - 1)
		{
			StructuralType declaringType = _columnMappings[num].PropertyPath[0].DeclaringType;
			EdmProperty column = _columnMappings[num].ColumnProperty;
			int i;
			EdmType commonBaseType;
			for (i = num + 1; i < _columnMappings.Count && column.Name == _columnMappings[i].ColumnProperty.Name && declaringType != _columnMappings[i].PropertyPath[0].DeclaringType && TypeSemantics.TryGetCommonBaseType(declaringType, _columnMappings[i].PropertyPath[0].DeclaringType, out commonBaseType); i++)
			{
			}
			System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration primitivePropertyConfiguration = column.GetConfiguration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;
			for (int j = num + 1; j < i; j++)
			{
				ColumnMappingBuilder toFixup = _columnMappings[j];
				System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration primitivePropertyConfiguration2 = toFixup.ColumnProperty.GetConfiguration() as System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;
				if (primitivePropertyConfiguration == null || primitivePropertyConfiguration.IsCompatible(primitivePropertyConfiguration2, inCSpace: false, out var errorMessage))
				{
					primitivePropertyConfiguration2?.Configure(column, _table, _storeModel.ProviderManifest);
					column.Nullable = true;
					AssociationType[] array = (from a in _storeModel.AssociationTypes
						where a.Constraint != null
						let p = a.Constraint.ToProperties
						where p.Contains(column) || p.Contains(toFixup.ColumnProperty)
						select a).ToArray();
					foreach (AssociationType associationType in array)
					{
						_storeModel.RemoveAssociationType(associationType);
					}
					if (toFixup.ColumnProperty.DeclaringType.HasMember(toFixup.ColumnProperty))
					{
						toFixup.ColumnProperty.DeclaringType.RemoveMember(toFixup.ColumnProperty);
					}
					toFixup.ColumnProperty = column;
					continue;
				}
				throw new MappingException(Strings.BadTphMappingToSharedColumn(string.Join(".", _columnMappings[num].PropertyPath.Select((EdmProperty p) => p.Name)), declaringType.Name, string.Join(".", toFixup.PropertyPath.Select((EdmProperty p) => p.Name)), toFixup.PropertyPath[0].DeclaringType.Name, column.Name, column.DeclaringType.Name, errorMessage));
			}
			num = i;
		}
	}
}
