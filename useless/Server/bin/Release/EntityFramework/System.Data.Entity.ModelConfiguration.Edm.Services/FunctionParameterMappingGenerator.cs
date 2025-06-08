using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services;

internal class FunctionParameterMappingGenerator : StructuralTypeMappingGenerator
{
	public FunctionParameterMappingGenerator(DbProviderManifest providerManifest)
		: base(providerManifest)
	{
	}

	public IEnumerable<ModificationFunctionParameterBinding> Generate(ModificationOperator modificationOperator, IEnumerable<EdmProperty> properties, IList<ColumnMappingBuilder> columnMappings, IList<EdmProperty> propertyPath, bool useOriginalValues = false)
	{
		foreach (EdmProperty property in properties)
		{
			if (property.IsComplexType && propertyPath.Any((EdmProperty p) => p.IsComplexType && p.ComplexType == property.ComplexType))
			{
				throw Error.CircularComplexTypeHierarchy();
			}
			propertyPath.Add(property);
			if (property.IsComplexType)
			{
				foreach (ModificationFunctionParameterBinding item in Generate(modificationOperator, property.ComplexType.Properties, columnMappings, propertyPath, useOriginalValues))
				{
					yield return item;
				}
			}
			else if (property.GetStoreGeneratedPattern() != StoreGeneratedPattern.Identity || modificationOperator != ModificationOperator.Insert)
			{
				EdmProperty columnProperty = columnMappings.First((ColumnMappingBuilder cm) => cm.PropertyPath.SequenceEqual(propertyPath)).ColumnProperty;
				if (property.GetStoreGeneratedPattern() != StoreGeneratedPattern.Computed && (modificationOperator != ModificationOperator.Delete || property.IsKeyMember))
				{
					yield return new ModificationFunctionParameterBinding(new FunctionParameter(columnProperty.Name, columnProperty.TypeUsage, ParameterMode.In), new ModificationFunctionMemberPath(propertyPath, null), !useOriginalValues);
				}
				if (modificationOperator != ModificationOperator.Insert && property.ConcurrencyMode == ConcurrencyMode.Fixed)
				{
					yield return new ModificationFunctionParameterBinding(new FunctionParameter(columnProperty.Name + "_Original", columnProperty.TypeUsage, ParameterMode.In), new ModificationFunctionMemberPath(propertyPath, null), isCurrent: false);
				}
			}
			propertyPath.Remove(property);
		}
	}

	public IEnumerable<ModificationFunctionParameterBinding> Generate(IEnumerable<Tuple<ModificationFunctionMemberPath, EdmProperty>> iaFkProperties, bool useOriginalValues = false)
	{
		return from iaFkProperty in iaFkProperties
			let functionParameter = new FunctionParameter(iaFkProperty.Item2.Name, iaFkProperty.Item2.TypeUsage, ParameterMode.In)
			select new ModificationFunctionParameterBinding(functionParameter, iaFkProperty.Item1, !useOriginalValues);
	}
}
