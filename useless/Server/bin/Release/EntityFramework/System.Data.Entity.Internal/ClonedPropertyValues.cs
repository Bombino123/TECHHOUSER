using System.Collections.Generic;
using System.Data.Common;

namespace System.Data.Entity.Internal;

internal class ClonedPropertyValues : InternalPropertyValues
{
	private readonly ISet<string> _propertyNames;

	private readonly IDictionary<string, ClonedPropertyValuesItem> _propertyValues;

	public override ISet<string> PropertyNames => _propertyNames;

	internal ClonedPropertyValues(InternalPropertyValues original, DbDataRecord valuesRecord = null)
		: base(original.InternalContext, original.ObjectType, original.IsEntityValues)
	{
		_propertyNames = original.PropertyNames;
		_propertyValues = new Dictionary<string, ClonedPropertyValuesItem>(_propertyNames.Count);
		foreach (string propertyName in _propertyNames)
		{
			IPropertyValuesItem item = original.GetItem(propertyName);
			object obj = item.Value;
			if (obj is InternalPropertyValues original2)
			{
				DbDataRecord valuesRecord2 = ((valuesRecord == null) ? null : ((DbDataRecord)valuesRecord[propertyName]));
				obj = new ClonedPropertyValues(original2, valuesRecord2);
			}
			else if (valuesRecord != null)
			{
				obj = valuesRecord[propertyName];
				if (obj == DBNull.Value)
				{
					obj = null;
				}
			}
			_propertyValues[propertyName] = new ClonedPropertyValuesItem(propertyName, obj, item.Type, item.IsComplex);
		}
	}

	protected override IPropertyValuesItem GetItemImpl(string propertyName)
	{
		return _propertyValues[propertyName];
	}
}
