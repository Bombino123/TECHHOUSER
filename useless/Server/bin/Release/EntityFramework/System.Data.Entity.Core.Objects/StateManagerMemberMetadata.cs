using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects;

internal class StateManagerMemberMetadata
{
	private readonly EdmProperty _clrProperty;

	private readonly EdmProperty _edmProperty;

	private readonly bool _isPartOfKey;

	private readonly bool _isComplexType;

	internal string CLayerName => _edmProperty.Name;

	internal Type ClrType => _clrProperty.TypeUsage.EdmType.ClrType;

	internal virtual bool IsComplex => _isComplexType;

	internal virtual EdmProperty CdmMetadata => _edmProperty;

	internal EdmProperty ClrMetadata => _clrProperty;

	internal bool IsPartOfKey => _isPartOfKey;

	internal StateManagerMemberMetadata()
	{
	}

	internal StateManagerMemberMetadata(ObjectPropertyMapping memberMap, EdmProperty memberMetadata, bool isPartOfKey)
	{
		_clrProperty = memberMap.ClrProperty;
		_edmProperty = memberMetadata;
		_isPartOfKey = isPartOfKey;
		_isComplexType = Helper.IsEntityType(_edmProperty.TypeUsage.EdmType) || Helper.IsComplexType(_edmProperty.TypeUsage.EdmType);
	}

	public virtual object GetValue(object userObject)
	{
		return DelegateFactory.GetValue(_clrProperty, userObject);
	}

	public void SetValue(object userObject, object value)
	{
		if (DBNull.Value == value)
		{
			value = null;
		}
		if (IsComplex && value == null)
		{
			throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(CLayerName));
		}
		DelegateFactory.SetValue(_clrProperty, userObject, value);
	}
}
