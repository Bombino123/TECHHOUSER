using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ComplexTypeMaterializer
{
	private sealed class Plan
	{
		internal readonly TypeUsage Key;

		internal readonly Func<object> ClrType;

		internal readonly PlanEdmProperty[] Properties;

		internal Plan(TypeUsage key, ObjectTypeMapping mapping, ReadOnlyCollection<FieldMetadata> fields)
		{
			Key = key;
			ClrType = DelegateFactory.GetConstructorDelegateForType((ClrComplexType)mapping.ClrType);
			Properties = new PlanEdmProperty[fields.Count];
			int num = -1;
			for (int i = 0; i < Properties.Length; i++)
			{
				FieldMetadata fieldMetadata = fields[i];
				num = fieldMetadata.Ordinal;
				Properties[i] = new PlanEdmProperty(num, mapping.GetPropertyMap(fieldMetadata.FieldType.Name).ClrProperty);
			}
		}
	}

	private struct PlanEdmProperty
	{
		internal readonly int Ordinal;

		internal readonly Func<object, object> GetExistingComplex;

		internal readonly Action<object, object> ClrProperty;

		internal PlanEdmProperty(int ordinal, EdmProperty property)
		{
			Ordinal = ordinal;
			GetExistingComplex = (Helper.IsComplexType(property.TypeUsage.EdmType) ? DelegateFactory.GetGetterDelegateForProperty(property) : null);
			ClrProperty = DelegateFactory.GetSetterDelegateForProperty(property);
		}
	}

	private readonly MetadataWorkspace _workspace;

	private const int MaxPlanCount = 4;

	private Plan[] _lastPlans;

	private int _lastPlanIndex;

	internal ComplexTypeMaterializer(MetadataWorkspace workspace)
	{
		_workspace = workspace;
	}

	internal object CreateComplex(IExtendedDataRecord record, DataRecordInfo recordInfo, object result)
	{
		Plan plan = GetPlan(recordInfo);
		if (result == null)
		{
			result = plan.ClrType();
		}
		SetProperties(record, result, plan.Properties);
		return result;
	}

	private void SetProperties(IExtendedDataRecord record, object result, PlanEdmProperty[] properties)
	{
		for (int i = 0; i < properties.Length; i++)
		{
			if (properties[i].GetExistingComplex != null)
			{
				object obj = properties[i].GetExistingComplex(result);
				object arg = CreateComplexRecursive(record.GetValue(properties[i].Ordinal), obj);
				if (obj == null)
				{
					properties[i].ClrProperty(result, arg);
				}
			}
			else
			{
				properties[i].ClrProperty(result, ConvertDBNull(record.GetValue(properties[i].Ordinal)));
			}
		}
	}

	private static object ConvertDBNull(object value)
	{
		if (DBNull.Value == value)
		{
			return null;
		}
		return value;
	}

	private object CreateComplexRecursive(object record, object existing)
	{
		if (DBNull.Value == record)
		{
			return existing;
		}
		return CreateComplexRecursive((IExtendedDataRecord)record, existing);
	}

	private object CreateComplexRecursive(IExtendedDataRecord record, object existing)
	{
		return CreateComplex(record, record.DataRecordInfo, existing);
	}

	private Plan GetPlan(DataRecordInfo recordInfo)
	{
		Plan[] array = _lastPlans ?? (_lastPlans = new Plan[4]);
		int num = _lastPlanIndex - 1;
		for (int i = 0; i < 4; i++)
		{
			num = (num + 1) % 4;
			if (array[num] == null)
			{
				break;
			}
			if (array[num].Key == recordInfo.RecordType)
			{
				_lastPlanIndex = num;
				return array[num];
			}
		}
		ObjectTypeMapping objectMapping = Util.GetObjectMapping(recordInfo.RecordType.EdmType, _workspace);
		_lastPlanIndex = num;
		array[num] = new Plan(recordInfo.RecordType, objectMapping, recordInfo.FieldMetadata);
		return array[num];
	}
}
