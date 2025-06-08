namespace System.Data.Entity.Core.Metadata.Edm;

internal struct FacetValueContainer<T>
{
	private T _value;

	private bool _hasValue;

	private bool _isUnbounded;

	internal T Value
	{
		set
		{
			_isUnbounded = false;
			_hasValue = true;
			_value = value;
		}
	}

	internal bool HasValue => _hasValue;

	private void SetUnbounded()
	{
		_isUnbounded = true;
		_hasValue = true;
	}

	public static implicit operator FacetValueContainer<T>(EdmConstants.Unbounded unbounded)
	{
		FacetValueContainer<T> result = default(FacetValueContainer<T>);
		result.SetUnbounded();
		return result;
	}

	public static implicit operator FacetValueContainer<T>(T value)
	{
		FacetValueContainer<T> result = default(FacetValueContainer<T>);
		result.Value = value;
		return result;
	}

	internal object GetValueAsObject()
	{
		if (_isUnbounded)
		{
			return EdmConstants.UnboundedValue;
		}
		return _value;
	}
}
